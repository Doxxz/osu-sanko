// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Utils;
using osu.Game.Rulesets.Sanko.Difficulty.Preprocessing;
using osu.Game.Rulesets.Sanko.Difficulty.Preprocessing.Colour;
using osu.Game.Rulesets.Sanko.Difficulty.Preprocessing.Colour.Data;
using osu.Game.Rulesets.Sanko.Objects;

namespace osu.Game.Rulesets.Sanko.Difficulty.Evaluators
{
    public static class ColourEvaluator
    {
        /// <summary>
        /// Calculates a consistency penalty based on the number of consecutive consistent intervals,
        /// considering the delta time between each colour sequence.
        /// </summary>
        /// <param name="hitObject">The current hitObject to consider.</param>
        /// <param name="threshold"> The allowable margin of error for determining whether ratios are consistent.</param>
        /// <param name="maxObjectsToCheck">The maximum objects to check per count of consistent ratio.</param>
        private static double consistentRatioPenalty(SankoDifficultyHitObject hitObject, double threshold = 0.01, int maxObjectsToCheck = 64)
        {
            int consistentRatioCount = 0;
            double totalRatioCount = 0.0;

            List<double> recentRatios = new List<double>();
            SankoDifficultyHitObject current = hitObject;
            var previousHitObject = (SankoDifficultyHitObject)current.Previous(1);

            for (int i = 0; i < maxObjectsToCheck; i++)
            {
                // Break if there is no valid previous object
                if (current.Index <= 1)
                    break;

                double currentRatio = current.RhythmData.Ratio;
                double previousRatio = previousHitObject.RhythmData.Ratio;

                recentRatios.Add(currentRatio);

                // A consistent interval is defined as the percentage difference between the two rhythmic ratios with the margin of error.
                if (Math.Abs(1 - currentRatio / previousRatio) <= threshold)
                {
                    consistentRatioCount++;
                    totalRatioCount += currentRatio;
                    break;
                }

                current = previousHitObject;
            }

            // Ensure no division by zero
            if (consistentRatioCount > 0)
                return 1 - totalRatioCount / (consistentRatioCount + 1) * 0.80;

            if (recentRatios.Count <= 1) return 1.0;

            // As a fallback, calculate the maximum deviation from the average of the recent ratios to ensure slightly off-snapped objects don't bypass the penalty.
            double maxRatioDeviation = recentRatios.Max(r => Math.Abs(r - recentRatios.Average()));

            double consistentRatioPenalty = 0.7 + 0.3 * DiffUtils.Smootherstep(maxRatioDeviation, 0.0, 1.0);

            return consistentRatioPenalty;
        }

        /// <summary>
        /// Evaluate the difficulty of the first hitobject within a colour streak.
        /// </summary>
        public static double EvaluateDifficultyOf(DifficultyHitObject hitObject)
        {
            var sankoObject = (SankoDifficultyHitObject)hitObject;
            SankoColourData colourData = sankoObject.ColourData;
            double difficulty = 0.0d;

            if (colourData.MonoStreak?.FirstHitObject == hitObject) // Difficulty for MonoStreak
                difficulty += evaluateMonoStreakDifficulty(colourData.MonoStreak);

            if (colourData.AlternatingMonoPattern?.FirstHitObject == hitObject) // Difficulty for AlternatingMonoPattern
                difficulty += evaluateAlternatingMonoPatternDifficulty(colourData.AlternatingMonoPattern);

            if (colourData.RepeatingHitPattern?.FirstHitObject == hitObject) // Difficulty for RepeatingHitPattern
                difficulty += evaluateRepeatingHitPatternsDifficulty(colourData.RepeatingHitPattern);

            double consistencyPenalty = consistentRatioPenalty(sankoObject);
            difficulty *= consistencyPenalty;

            // Reading a burst which mixes all three colours is additional difficulty on top of the run-length and
            // repetition analysis, so it is added after (and independently of) the rhythm consistency penalty above.
            difficulty += EvaluateThreeColourBurst(hitObject);

            return difficulty;
        }

        /// <summary>
        /// The maximum number of notes (including the current note) considered part of a single colour burst.
        /// </summary>
        private const int burst_max_notes = 6;

        /// <summary>
        /// The time span (in milliseconds) at or above which notes stop being read as a single burst of colour.
        /// </summary>
        private const double burst_max_span = 600;

        /// <summary>
        /// The time span (in milliseconds) at or below which a three-colour burst is considered maximally difficult.
        /// </summary>
        private const double burst_tight_span = 120;

        /// <summary>
        /// Scales the difficulty contributed by reading three colours within a single burst.
        /// </summary>
        private const double three_colour_scale = 9.25;

        /// <summary>
        /// Calculates the difficulty of reading three distinct note colours within a short window of notes.
        /// </summary>
        /// <remarks>
        /// The rest of the colour skill is derived from osu!taiko, where only two colours exist, and measures the
        /// lengths of same-colour runs and the repetition of those run-length patterns. That can not distinguish a
        /// two-colour alternation from a burst which genuinely mixes all three colours, which is where sanko's extra
        /// reading difficulty comes from. This evaluates that directly: difficulty is only awarded when all three
        /// colours appear within the burst, and is scaled by how tightly the burst is packed and how much it alternates.
        /// </remarks>
        public static double EvaluateThreeColourBurst(DifficultyHitObject hitObject)
        {
            if (hitObject is not SankoDifficultyHitObject sankoObject || sankoObject.BaseObject is not Hit)
                return 0;

            var burst = new List<SankoDifficultyHitObject> { sankoObject };
            SankoDifficultyHitObject? previous = sankoObject;

            while (burst.Count < burst_max_notes)
            {
                previous = previous.PreviousNote(0);

                if (previous?.BaseObject is not Hit || sankoObject.StartTime - previous.StartTime > burst_max_span)
                    break;

                burst.Add(previous);
            }

            if (burst.Count < 3)
                return 0;

            int colourSwitches = 0;
            bool hasCentre = false;
            bool hasRim = false;
            bool hasTsu = false;

            for (int i = 0; i < burst.Count; i++)
            {
                HitType type = ((Hit)burst[i].BaseObject).Type;

                switch (type)
                {
                    case HitType.Rim:
                        hasRim = true;
                        break;

                    case HitType.Tsu:
                        hasTsu = true;
                        break;

                    default:
                        hasCentre = true;
                        break;
                }

                if (i > 0 && ((Hit)burst[i - 1].BaseObject).Type != type)
                    colourSwitches++;
            }

            // Reading a burst is only harder than osu!taiko when a third colour is actually present in it.
            if (!hasCentre || !hasRim || !hasTsu)
                return 0;

            double span = sankoObject.StartTime - burst[^1].StartTime;
            double tightness = DiffUtils.ReverseLerp(span, burst_max_span, burst_tight_span);

            return three_colour_scale * tightness * colourSwitches;
        }

        private static double evaluateMonoStreakDifficulty(MonoStreak monoStreak) =>
            DiffUtils.Logistic(exponent: Math.E * monoStreak.Index - 2 * Math.E) * evaluateAlternatingMonoPatternDifficulty(monoStreak.Parent) * 0.5;

        private static double evaluateAlternatingMonoPatternDifficulty(AlternatingMonoPattern alternatingMonoPattern) =>
            DiffUtils.Logistic(exponent: Math.E * alternatingMonoPattern.Index - 2 * Math.E) * evaluateRepeatingHitPatternsDifficulty(alternatingMonoPattern.Parent);

        private static double evaluateRepeatingHitPatternsDifficulty(RepeatingHitPatterns repeatingHitPattern) =>
            2 * (1 - DiffUtils.Logistic(exponent: Math.E * repeatingHitPattern.RepetitionInterval - 2 * Math.E));
    }
}
