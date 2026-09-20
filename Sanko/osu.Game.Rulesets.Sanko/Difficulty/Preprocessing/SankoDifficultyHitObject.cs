// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Sanko.Difficulty.Evaluators;
using osu.Game.Rulesets.Sanko.Difficulty.Preprocessing.Colour;
using osu.Game.Rulesets.Sanko.Difficulty.Preprocessing.Rhythm;
using osu.Game.Rulesets.Sanko.Difficulty.Utils;
using osu.Game.Rulesets.Sanko.Objects;

namespace osu.Game.Rulesets.Sanko.Difficulty.Preprocessing
{
    /// <summary>
    /// Represents a single hit object in sanko difficulty calculation.
    /// </summary>
    public class SankoDifficultyHitObject : DifficultyHitObject, IHasInterval
    {
        /// <summary>
        /// The list of all <see cref="SankoDifficultyHitObject"/> of the same colour as this <see cref="SankoDifficultyHitObject"/> in the beatmap.
        /// </summary>
        private readonly IReadOnlyList<SankoDifficultyHitObject>? monoDifficultyHitObjects;

        /// <summary>
        /// The index of this <see cref="SankoDifficultyHitObject"/> in <see cref="monoDifficultyHitObjects"/>.
        /// </summary>
        public readonly int MonoIndex;

        /// <summary>
        /// The list of all <see cref="SankoDifficultyHitObject"/> that is either a regular note or finisher in the beatmap
        /// </summary>
        private readonly IReadOnlyList<SankoDifficultyHitObject> noteDifficultyHitObjects;

        /// <summary>
        /// The index of this <see cref="SankoDifficultyHitObject"/> in <see cref="noteDifficultyHitObjects"/>.
        /// </summary>
        public readonly int NoteIndex;

        /// <summary>
        /// Rhythm data used by <see cref="RhythmEvaluator"/>.
        /// This is populated via <see cref="SankoRhythmDifficultyPreprocessor"/>.
        /// </summary>
        public readonly SankoRhythmData RhythmData;

        /// <summary>
        /// Colour data used by <see cref="ColourEvaluator"/> and <see cref="StaminaEvaluator"/>.
        /// This is populated via <see cref="SankoColourDifficultyPreprocessor"/>.
        /// </summary>
        public readonly SankoColourData ColourData;

        /// <summary>
        /// The adjusted BPM of this hit object, based on its slider velocity and scroll speed.
        /// </summary>
        public double EffectiveBPM;

        /// <summary>
        /// Creates a new difficulty hit object.
        /// </summary>
        /// <param name="hitObject">The gameplay <see cref="HitObject"/> associated with this difficulty object.</param>
        /// <param name="lastObject">The gameplay <see cref="HitObject"/> preceding <paramref name="hitObject"/>.</param>
        /// <param name="clockRate">The rate of the gameplay clock. Modified by speed-changing mods.</param>
        /// <param name="objects">The list of all <see cref="DifficultyHitObject"/>s in the current beatmap.</param>
        /// <param name="centreHitObjects">The list of centre (don) <see cref="DifficultyHitObject"/>s in the current beatmap.</param>
        /// <param name="rimHitObjects">The list of rim (kat) <see cref="DifficultyHitObject"/>s in the current beatmap.</param>
        /// <param name="noteObjects">The list of <see cref="DifficultyHitObject"/>s that is a hit (i.e. not a drumroll or swell) in the current beatmap.</param>
        /// <param name="index">The position of this <see cref="DifficultyHitObject"/> in the <paramref name="objects"/> list.</param>
        /// <param name="controlPointInfo">The control point info of the beatmap.</param>
        /// <param name="globalSliderVelocity">The global slider velocity of the beatmap.</param>
        public SankoDifficultyHitObject(HitObject hitObject, HitObject lastObject, double clockRate,
                                        List<DifficultyHitObject> objects,
                                        List<SankoDifficultyHitObject> centreHitObjects,
                                        List<SankoDifficultyHitObject> rimHitObjects,
                                        List<SankoDifficultyHitObject> noteObjects, int index,
                                        ControlPointInfo controlPointInfo,
                                        double globalSliderVelocity)
            : base(hitObject, lastObject, clockRate, objects, index)
        {
            noteDifficultyHitObjects = noteObjects;

            ColourData = new SankoColourData();
            RhythmData = new SankoRhythmData(this);

            if (hitObject is Hit hit)
            {
                switch (hit.OriginalType)
                {
                    case HitType.Rim:
                        MonoIndex = rimHitObjects.Count;
                        rimHitObjects.Add(this);
                        monoDifficultyHitObjects = rimHitObjects;
                        break;

                    default:
                        MonoIndex = centreHitObjects.Count;
                        centreHitObjects.Add(this);
                        monoDifficultyHitObjects = centreHitObjects;
                        break;
                }

                NoteIndex = noteObjects.Count;
                noteObjects.Add(this);
            }

            // Using `hitObject.StartTime` causes floating point error differences
            double normalisedStartTime = StartTime * clockRate;

            // Retrieve the timing point at the note's start time
            TimingControlPoint currentControlPoint = controlPointInfo.TimingPointAt(normalisedStartTime);

            // Calculate the slider velocity at the note's start time.
            double currentSliderVelocity = calculateSliderVelocity(controlPointInfo, globalSliderVelocity, normalisedStartTime, clockRate);

            EffectiveBPM = currentControlPoint.BPM * currentSliderVelocity;
        }

        /// <summary>
        /// Calculates the slider velocity based on control point info and clock rate.
        /// </summary>
        private static double calculateSliderVelocity(ControlPointInfo controlPointInfo, double globalSliderVelocity, double startTime, double clockRate)
        {
            var activeEffectControlPoint = controlPointInfo.EffectPointAt(startTime);
            return globalSliderVelocity * (activeEffectControlPoint.ScrollSpeed) * clockRate;
        }

        public SankoDifficultyHitObject? PreviousMono(int backwardsIndex) => monoDifficultyHitObjects?.ElementAtOrDefault(MonoIndex - (backwardsIndex + 1));

        public SankoDifficultyHitObject? NextMono(int forwardsIndex) => monoDifficultyHitObjects?.ElementAtOrDefault(MonoIndex + (forwardsIndex + 1));

        public SankoDifficultyHitObject? PreviousNote(int backwardsIndex) => noteDifficultyHitObjects.ElementAtOrDefault(NoteIndex - (backwardsIndex + 1));

        public SankoDifficultyHitObject? NextNote(int forwardsIndex) => noteDifficultyHitObjects.ElementAtOrDefault(NoteIndex + (forwardsIndex + 1));

        public double Interval => DeltaTime;
    }
}
