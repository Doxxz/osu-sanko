// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Bindings;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Localisation;
using osu.Game.Localisation.Taiko;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Configuration;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Replays.Types;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Scoring.Legacy;
using osu.Game.Rulesets.Sanko.Beatmaps;
using osu.Game.Rulesets.Sanko.Configuration;
using osu.Game.Rulesets.Sanko.Difficulty;
using osu.Game.Rulesets.Sanko.Edit;
using osu.Game.Rulesets.Sanko.Edit.Setup;
using osu.Game.Rulesets.Sanko.Mods;
using osu.Game.Rulesets.Sanko.Objects;
using osu.Game.Rulesets.Sanko.Replays;
using osu.Game.Rulesets.Sanko.Scoring;
using osu.Game.Rulesets.Sanko.Skinning.Argon;
using osu.Game.Rulesets.Sanko.Skinning.Default;
using osu.Game.Rulesets.Sanko.Skinning.Legacy;
using osu.Game.Rulesets.Sanko.UI;
using osu.Game.Rulesets.UI;
using osu.Game.Scoring;
using osu.Game.Screens.Edit.Setup;
using osu.Game.Screens.Ranking.Statistics;
using osu.Game.Skinning;
using osu.Game.Utils;
using osuTK;

namespace osu.Game.Rulesets.Sanko
{
    public class SankoRuleset : Ruleset, ILegacyRuleset
    {
        public override DrawableRuleset CreateDrawableRulesetWith(IBeatmap beatmap, IReadOnlyList<Mod>? mods = null) => new DrawableSankoRuleset(this, beatmap, mods);

        public override ScoreProcessor CreateScoreProcessor() => new SankoScoreProcessor();

        public override HealthProcessor CreateHealthProcessor(double drainStartTime) => new SankoHealthProcessor();

        public override IBeatmapConverter CreateBeatmapConverter(IBeatmap beatmap) => new SankoBeatmapConverter(beatmap, this);

        public override ISkin? CreateSkinTransformer(ISkin skin, IBeatmap beatmap)
        {
            switch (skin)
            {
                case ArgonSkin:
                    return new SankoArgonSkinTransformer(skin);

                case TrianglesSkin:
                    return new SankoTrianglesSkinTransformer(skin);

                case LegacySkin:
                    return new SankoLegacySkinTransformer(skin);
            }

            return null;
        }

        public const string SHORT_NAME = "sanko";

        public override string RulesetAPIVersionSupported => CURRENT_RULESET_API_VERSION;

        public override IEnumerable<KeyBinding> GetDefaultKeyBindings(int variant = 0)
        {
            switch (variant)
            {
                default:
                    // The order of these bindings determines the order the actions are listed in the key binding
                    // settings, so they are grouped left-to-right and green/blue/red on each side.
                    return new[]
                    {
                        new KeyBinding(InputKey.S, SankoAction.LeftTsu),
                        new KeyBinding(InputKey.D, SankoAction.LeftRim),
                        new KeyBinding(InputKey.MouseRight, SankoAction.LeftRim),
                        new KeyBinding(InputKey.F, SankoAction.LeftCentre),
                        new KeyBinding(InputKey.MouseLeft, SankoAction.LeftCentre),
                        new KeyBinding(InputKey.J, SankoAction.RightCentre),
                        new KeyBinding(InputKey.None, SankoAction.RightCentre),
                        new KeyBinding(InputKey.K, SankoAction.RightRim),
                        new KeyBinding(InputKey.None, SankoAction.RightRim),
                        new KeyBinding(InputKey.L, SankoAction.RightTsu),
                    };

                case EDITOR_VARIANT:
                    return
                    [
                        new KeyBinding(InputKey.Number2, SankoAction.EditorHitTool),
                        new KeyBinding(InputKey.Number3, SankoAction.EditorDrumRollTool),
                        new KeyBinding(InputKey.Number4, SankoAction.EditorSwellTool),
                    ];
            }
        }

        public override IEnumerable<Mod> ConvertFromLegacyMods(LegacyMods mods)
        {
            if (mods.HasFlag(LegacyMods.Nightcore))
                yield return new SankoModNightcore();
            else if (mods.HasFlag(LegacyMods.DoubleTime))
                yield return new SankoModDoubleTime();

            if (mods.HasFlag(LegacyMods.Perfect))
                yield return new SankoModPerfect();
            else if (mods.HasFlag(LegacyMods.SuddenDeath))
                yield return new SankoModSuddenDeath();

            if (mods.HasFlag(LegacyMods.Cinema))
                yield return new SankoModCinema();
            else if (mods.HasFlag(LegacyMods.Autoplay))
                yield return new SankoModAutoplay();

            if (mods.HasFlag(LegacyMods.Easy))
                yield return new SankoModEasy();

            if (mods.HasFlag(LegacyMods.Flashlight))
                yield return new SankoModFlashlight();

            if (mods.HasFlag(LegacyMods.HalfTime))
                yield return new SankoModHalfTime();

            if (mods.HasFlag(LegacyMods.HardRock))
                yield return new SankoModHardRock();

            if (mods.HasFlag(LegacyMods.Hidden))
                yield return new SankoModHidden();

            if (mods.HasFlag(LegacyMods.NoFail))
                yield return new SankoModNoFail();

            if (mods.HasFlag(LegacyMods.Relax))
                yield return new SankoModRelax();

            if (mods.HasFlag(LegacyMods.ScoreV2))
                yield return new ModScoreV2();
        }

        public override IEnumerable<Mod> GetModsFor(ModType type)
        {
            switch (type)
            {
                case ModType.DifficultyReduction:
                    return new Mod[]
                    {
                        new SankoModEasy(),
                        new SankoModNoFail(),
                        new MultiMod(new SankoModHalfTime(), new SankoModDaycore()),
                        new SankoModSimplifiedRhythm(),
                    };

                case ModType.DifficultyIncrease:
                    return new Mod[]
                    {
                        new SankoModHardRock(),
                        new MultiMod(new SankoModSuddenDeath(), new SankoModPerfect()),
                        new MultiMod(new SankoModDoubleTime(), new SankoModNightcore()),
                        new SankoModHidden(),
                        new SankoModFlashlight(),
                        new ModAccuracyChallenge(),
                    };

                case ModType.Conversion:
                    return new Mod[]
                    {
                        new SankoModRandom(),
                        new SankoModDifficultyAdjust(),
                        new SankoModClassic(),
                        new SankoModSwap(),
                        new SankoModSingleTap(),
                        new SankoModConstantSpeed(),
                        new SankoModTsu(),
                        new SankoModTsuRandom(),
                    };

                case ModType.Automation:
                    return new Mod[]
                    {
                        new MultiMod(new SankoModAutoplay(), new SankoModCinema()),
                        new SankoModRelax(),
                    };

                case ModType.Fun:
                    return new Mod[]
                    {
                        new MultiMod(new ModWindUp(), new ModWindDown()),
                        new SankoModMuted(),
                        new ModAdaptiveSpeed()
                    };

                case ModType.System:
                    return new Mod[]
                    {
                        new ModScoreV2(),
                    };

                default:
                    return Array.Empty<Mod>();
            }
        }

        public override ScoreMultiplierCalculator CreateScoreMultiplierCalculator(ScoreMultiplierContext context) => new SankoScoreMultiplierCalculator(context);

        public override string Description => "osu!sanko";

        public override string ShortName => SHORT_NAME;

        public override string PlayingVerb => "Bashing drums";

        public override Drawable CreateIcon() => new SankoRulesetIcon(this);

        public override HitObjectComposer CreateHitObjectComposer() => new SankoHitObjectComposer(this);

        public override IEnumerable<Drawable> CreateEditorSetupSections() =>
        [
            new MetadataSection(),
            new SankoDifficultySection(),
            new ResourcesSection(),
            new DesignSection(),
        ];

        public override IBeatmapVerifier CreateBeatmapVerifier() => new SankoBeatmapVerifier();

        public override bool AllowConversionFrom(IRulesetInfo sourceRuleset) => sourceRuleset.OnlineID == 1;

        public override DifficultyCalculator CreateDifficultyCalculator(IWorkingBeatmap beatmap) => new SankoDifficultyCalculator(RulesetInfo, beatmap);

        public override PerformanceCalculator CreatePerformanceCalculator() => new SankoPerformanceCalculator();

        /// <summary>
        /// Sanko reuses taiko's stable-format behaviour, but must not claim taiko's legacy ruleset ID (1),
        /// otherwise the two rulesets collide in the ruleset store. It therefore uses an ID past the
        /// official range.
        /// </summary>
        public int LegacyID => ILegacyRuleset.SANKO_RULESET_ID;

        public ILegacyScoreSimulator CreateLegacyScoreSimulator() => new SankoLegacyScoreSimulator();

        public override IConvertibleReplayFrame CreateConvertibleReplayFrame() => new SankoReplayFrame();

        public override IRulesetConfigManager CreateConfig(SettingsStore? settings) => new SankoRulesetConfigManager(settings, RulesetInfo);

        public override RulesetSettingsSubsection CreateSettings() => new SankoSettingsSubsection(this);

        public override IEnumerable<HitResult> GetValidHitResults()
        {
            return new[]
            {
                HitResult.Great,
                HitResult.Ok,
                HitResult.Miss,

                HitResult.SmallBonus,
                HitResult.LargeBonus,
                HitResult.IgnoreHit,
                HitResult.IgnoreMiss,
            };
        }

        public override LocalisableString GetDisplayNameForHitResult(HitResult result)
        {
            switch (result)
            {
                case HitResult.SmallBonus:
                    return "drum tick";

                case HitResult.LargeBonus:
                    return "bonus";
            }

            return base.GetDisplayNameForHitResult(result);
        }

        public override StatisticItem[] CreateStatisticsForScore(ScoreInfo score, IBeatmap playableBeatmap)
        {
            var timedHitEvents = score.HitEvents.Where(e => e.HitObject is Hit).ToList();

            return new[]
            {
                new StatisticItem("Performance Breakdown", () => new PerformanceBreakdownChart(score)
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y
                }),
                new StatisticItem("Timing Distribution", () => new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Spacing = new Vector2(15),
                    Children = new Drawable[]
                    {
                        new HitEventTimingDistributionGraph(timedHitEvents)
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = 150
                        },
                        new SimpleStatisticTable(1, new SimpleStatisticItem[]
                        {
                            new AverageHitError(timedHitEvents),
                            new UnstableRate(timedHitEvents)
                        })
                    }
                }, true)
            };
        }

        /// <seealso cref="SankoHitWindows"/>
        public override BeatmapDifficulty GetAdjustedDisplayDifficulty(IBeatmapInfo beatmapInfo, IReadOnlyCollection<Mod> mods)
        {
            BeatmapDifficulty adjustedDifficulty = base.GetAdjustedDisplayDifficulty(beatmapInfo, mods);
            double rate = ModUtils.CalculateRateWithMods(mods);

            double greatHitWindow = IBeatmapDifficultyInfo.DifficultyRange(adjustedDifficulty.OverallDifficulty, SankoHitWindows.GREAT_WINDOW_RANGE);
            greatHitWindow /= rate;
            adjustedDifficulty.OverallDifficulty = (float)IBeatmapDifficultyInfo.InverseDifficultyRange(greatHitWindow, SankoHitWindows.GREAT_WINDOW_RANGE);

            return adjustedDifficulty;
        }

        public override IEnumerable<RulesetBeatmapAttribute> GetBeatmapAttributesForDisplay(IBeatmapInfo beatmapInfo, IReadOnlyCollection<Mod> mods)
        {
            var originalDifficulty = beatmapInfo.Difficulty;
            // `modAdjustedDifficulty` contains only the direct effect of mods.
            // `effectiveDifficulty` contains the "perceived" effect of rate-adjusting mods on OD and AR.
            // we make a distinction here, because some of the calculations below will require very careful maneuvering between the two for correct results.
            var modAdjustedDifficulty = base.GetAdjustedDisplayDifficulty(beatmapInfo, mods);
            var effectiveDifficulty = GetAdjustedDisplayDifficulty(beatmapInfo, mods);
            var colours = new OsuColour();

            // when displaying hit window ranges with rate-changing mods active, we will want to adjust for rate ourselves, as `effectiveDifficulty` may not be accurate
            // because `SankoHitWindows` applies a floor-and-round operation that will result in inaccurate results
            // (the floor-and-round needs to happen *before* rate is taken into account, not after).
            var hitWindows = new SankoHitWindows();
            hitWindows.SetDifficulty(modAdjustedDifficulty.OverallDifficulty);
            double rate = ModUtils.CalculateRateWithMods(mods);
            yield return new RulesetBeatmapAttribute(SongSelectStrings.Accuracy, @"OD", originalDifficulty.OverallDifficulty, effectiveDifficulty.OverallDifficulty, 10)
            {
                Description = TaikoRulesetStrings.AccuracyDescription,
                AdditionalMetrics = hitWindows.GetAllAvailableWindows()
                                              .Reverse()
                                              .Select(window => new RulesetBeatmapAttribute.AdditionalMetric(
                                                  SongSelectStrings.HitResultWindow(window.result.GetDescription().ToUpperInvariant()),
                                                  LocalisableString.Interpolate($@"±{hitWindows.WindowFor(window.result) / rate:0.##} ms"),
                                                  colours.ForHitResult(window.result)
                                              ))
                                              .Append(new RulesetBeatmapAttribute.AdditionalMetric(TaikoRulesetStrings.HitsPerSecondRequiredToClearSwells, LocalisableString.Interpolate($@"{SankoBeatmapConverter.RequiredSwellHitsPerSecond(modAdjustedDifficulty.OverallDifficulty):0.#}")))
                                              .ToArray()
            };

            yield return new RulesetBeatmapAttribute(SongSelectStrings.HPDrain, @"HP", originalDifficulty.DrainRate, effectiveDifficulty.DrainRate, 10)
            {
                Description = SongSelectStrings.HPDrainDescription
            };

            yield return new RulesetBeatmapAttribute(SongSelectStrings.ScrollSpeed, @"SS", 1f, (float)(effectiveDifficulty.SliderMultiplier / originalDifficulty.SliderMultiplier), 4)
            {
                Description = TaikoRulesetStrings.ScrollSpeedDescription
            };
        }
    }
}
