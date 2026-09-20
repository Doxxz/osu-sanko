// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Sanko.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Utils;
using System.Threading;
using osu.Game.Audio;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Objects.Legacy;

namespace osu.Game.Rulesets.Sanko.Beatmaps
{
    internal class SankoBeatmapConverter : BeatmapConverter<SankoHitObject>
    {
        /// <summary>
        /// A speed multiplier applied globally to osu!sanko.
        /// </summary>
        /// <remarks>
        /// osu! is generally slower than sanko, so a factor was historically added to increase speed for converts.
        /// This must be used everywhere slider length or beat length is used in sanko.
        ///
        /// Of note, this has never been exposed to the end user, and is considered a hidden internal multiplier.
        /// </remarks>
        public const float VELOCITY_MULTIPLIER = 1.4f;

        /// <summary>
        /// The short name of the legacy ruleset whose conversion behaviour sanko is based on.
        /// </summary>
        /// <remarks>
        /// Sanko is a clone of taiko, so it reuses taiko's legacy beat length precision adjustments.
        /// </remarks>
        private const string legacy_ruleset_short_name = "taiko";

        /// <summary>
        /// Because swells are easier in sanko than spinners are in osu!,
        /// legacy sanko multiplies a factor when converting the number of required hits.
        /// </summary>
        private const float swell_hit_multiplier = 1.65f;

        /// <summary>
        /// Base osu! slider scoring distance.
        /// </summary>
        private const float osu_base_scoring_distance = 100;

        private readonly bool isForCurrentRuleset;

        public SankoBeatmapConverter(IBeatmap beatmap, Ruleset ruleset)
            : base(beatmap, ruleset)
        {
            isForCurrentRuleset = beatmap.BeatmapInfo.Ruleset.Equals(ruleset.RulesetInfo);
        }

        public override bool CanConvert() => true;

        protected override Beatmap<SankoHitObject> ConvertBeatmap(IBeatmap original, CancellationToken cancellationToken)
        {
            Beatmap<SankoHitObject> converted = base.ConvertBeatmap(original, cancellationToken);

            if (original.BeatmapInfo.Ruleset.OnlineID == 0)
            {
                // Post processing step to transform standard slider velocity changes into scroll speed changes
                double lastScrollSpeed = 1;

                foreach (HitObject hitObject in original.HitObjects)
                {
                    if (hitObject is not IHasSliderVelocity hasSliderVelocity) continue;

                    double nextScrollSpeed = hasSliderVelocity.SliderVelocityMultiplier;
                    EffectControlPoint currentEffectPoint = converted.ControlPointInfo.EffectPointAt(hitObject.StartTime);

                    if (!Precision.AlmostEquals(lastScrollSpeed, nextScrollSpeed, acceptableDifference: currentEffectPoint.ScrollSpeedBindable.Precision))
                    {
                        converted.ControlPointInfo.Add(hitObject.StartTime, new EffectControlPoint
                        {
                            KiaiMode = currentEffectPoint.KiaiMode,
                            ScrollSpeed = lastScrollSpeed = nextScrollSpeed,
                        });
                    }
                }
            }

            if (original.BeatmapInfo.Ruleset.OnlineID == 3)
            {
                // Post processing step to transform mania hit objects with the same start time into strong hits
                converted.HitObjects = converted.HitObjects.GroupBy(t => t.StartTime).Select(x =>
                {
                    SankoHitObject first = x.First();
                    if (x.Skip(1).Any() && first is SankoStrongableHitObject strong)
                        strong.IsStrong = true;
                    return first;
                }).ToList();
            }

            // TODO: stable makes the last tick of a drumroll non-required when the next object is too close.
            // This probably needs to be reimplemented:
            //
            // List<HitObject> hitobjects = hitObjectManager.hitObjects;
            // int ind = hitobjects.IndexOf(this);
            // if (i < hitobjects.Count - 1 && hitobjects[i + 1].HittableStartTime - (EndTime + (int)TickSpacing) <= (int)TickSpacing)
            //     lastTickHittable = false;

            // Convert don/kat notes into tsu notes by default when converting from another (installed) ruleset.
            // Native sanko beatmaps keep their authored notes, and beatmaps without a known ruleset are left alone.
            if (!isForCurrentRuleset && original.BeatmapInfo.Ruleset.OnlineID >= 0)
            {
                SankoTsuConversion.Apply(converted, SankoTsuConversion.DEFAULT_ISOLATED_CHANCE, SankoTsuConversion.DEFAULT_NON_ISOLATED_CHANCE, SankoTsuConversion.DEFAULT_BIG_CHANCE,
                    SankoTsuConversion.CreateContentRandom(converted));
            }

            return converted;
        }

        protected override IEnumerable<SankoHitObject> ConvertHitObject(HitObject obj, IBeatmap beatmap, CancellationToken cancellationToken)
        {
            // Old osu! used hit sounding to determine various hit type information
            IList<HitSampleInfo> samples = obj.Samples;

            switch (obj)
            {
                case IHasPath pathData:
                {
                    // A taiko drumroll is encoded as a slider in the legacy format, but it is not an osu! slider.
                    // osu!taiko carries its own drumrolls over untouched, so sanko does the same rather than running
                    // the osu! slider conversion (which would split the drumroll into hits).
                    if (beatmap.BeatmapInfo.Ruleset.OnlineID == 1)
                    {
                        yield return new DrumRoll
                        {
                            StartTime = obj.StartTime,
                            Samples = obj.Samples,
                            Duration = pathData.Duration,
                        };

                        break;
                    }

                    if (shouldConvertSliderToHits(obj, beatmap, pathData, out int sankoDuration, out double tickSpacing))
                    {
                        IList<IList<HitSampleInfo>> allSamples = obj is IHasPathWithRepeats curveData ? curveData.NodeSamples : new List<IList<HitSampleInfo>>(new[] { samples });

                        int i = 0;

                        for (double j = obj.StartTime; j <= obj.StartTime + sankoDuration + tickSpacing / 8; j += tickSpacing)
                        {
                            IList<HitSampleInfo> currentSamples = allSamples[i];

                            var hit = new Hit
                            {
                                StartTime = j,
                                Samples = currentSamples,
                            };

                            applySourceColour(hit, currentSamples);
                            yield return hit;

                            i = (i + 1) % allSamples.Count;

                            if (Precision.AlmostEquals(0, tickSpacing))
                                break;
                        }
                    }
                    else
                    {
                        yield return new DrumRoll
                        {
                            StartTime = obj.StartTime,
                            Samples = obj.Samples,
                            Duration = sankoDuration,
                        };
                    }

                    break;
                }

                case IHasDuration endTimeData:
                {
                    double hitMultiplier = RequiredSwellHitsPerSecond(beatmap.Difficulty.OverallDifficulty);

                    yield return new Swell
                    {
                        StartTime = obj.StartTime,
                        Samples = obj.Samples,
                        Duration = endTimeData.Duration,
                        RequiredHits = (int)Math.Max(1, endTimeData.Duration / 1000 * hitMultiplier)
                    };

                    break;
                }

                default:
                {
                    var hit = new Hit
                    {
                        StartTime = obj.StartTime,
                        Samples = samples,
                    };

                    applySourceColour(hit, samples);
                    yield return hit;

                    break;
                }
            }
        }

        /// <summary>
        /// Applies the colour encoded by a source hit object's samples to a converted sanko hit.
        /// </summary>
        /// <remarks>
        /// osu! and osu!taiko encode a kat with either a clap or a whistle, whereas sanko reserves the whistle for tsu
        /// notes. Setting the colour explicitly makes the hit re-encode its own samples, so a source kat authored with
        /// a whistle becomes a clap kat rather than being misread as a tsu.
        /// </remarks>
        /// <param name="hit">The converted hit to assign a colour to.</param>
        /// <param name="sourceSamples">The samples of the source hit object.</param>
        private static void applySourceColour(Hit hit, IList<HitSampleInfo> sourceSamples)
        {
            bool isKat = sourceSamples.Any(s => s.Name == HitSampleInfo.HIT_CLAP || s.Name == HitSampleInfo.HIT_WHISTLE);
            hit.Type = isKat ? HitType.Rim : HitType.Centre;
        }

        public static double RequiredSwellHitsPerSecond(double overallDifficulty)
            => IBeatmapDifficultyInfo.DifficultyRange(overallDifficulty, 3, 5, 7.5) * swell_hit_multiplier;

        private bool shouldConvertSliderToHits(HitObject obj, IBeatmap beatmap, IHasPath pathData, out int sankoDuration, out double tickSpacing)
        {
            // DO NOT CHANGE OR REFACTOR ANYTHING IN HERE WITHOUT TESTING AGAINST _ALL_ BEATMAPS.
            // Some of these calculations look redundant, but they are not - extremely small floating point errors are introduced to maintain 1:1 compatibility with stable.
            // Rounding cannot be used as an alternative since the error deltas have been observed to be between 1e-2 and 1e-6.

            // The true distance, accounting for any repeats. This ends up being the drum roll distance later
            int spans = (obj as IHasRepeats)?.SpanCount() ?? 1;
            double distance = pathData.Path.ExpectedDistance.Value ?? 0;

            // Do not combine the following two lines!
            distance *= VELOCITY_MULTIPLIER;
            distance *= spans;

            TimingControlPoint timingPoint = beatmap.ControlPointInfo.TimingPointAt(obj.StartTime);

            double beatLength;

            if (obj is IHasSliderVelocity hasSliderVelocity)
                beatLength = LegacyRulesetExtensions.GetPrecisionAdjustedBeatLength(hasSliderVelocity, timingPoint, legacy_ruleset_short_name);
            else
                beatLength = timingPoint.BeatLength;

            double sliderScoringPointDistance = osu_base_scoring_distance * (beatmap.Difficulty.SliderMultiplier * VELOCITY_MULTIPLIER) / beatmap.Difficulty.SliderTickRate;

            // The velocity and duration of the sanko hit object - calculated as the velocity of a drum roll.
            double sankoVelocity = sliderScoringPointDistance * beatmap.Difficulty.SliderTickRate;
            sankoDuration = (int)(distance / sankoVelocity * beatLength);

            if (isForCurrentRuleset)
            {
                tickSpacing = 0;
                return false;
            }

            double osuVelocity = sankoVelocity * (1000f / beatLength);

            // osu-stable always uses the speed-adjusted beatlength to determine the osu! velocity, but only uses it for conversion if beatmap version < 8
            if (beatmap.BeatmapVersion >= 8)
                beatLength = timingPoint.BeatLength;

            // If the drum roll is to be split into hit circles, assume the ticks are 1/8 spaced within the duration of one beat
            tickSpacing = Math.Min(beatLength / beatmap.Difficulty.SliderTickRate, (double)sankoDuration / spans);

            return tickSpacing > 0
                   && distance / osuVelocity * 1000 < 2 * beatLength;
        }

        protected override Beatmap<SankoHitObject> CreateBeatmap() => new SankoBeatmap();
    }
}
