// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sanko.Mods;
using osu.Game.Scoring;

namespace osu.Game.Rulesets.Sanko.Scoring
{
    public class SankoScoreMultiplierCalculator : ScoreMultiplierCalculator
    {
        public SankoScoreMultiplierCalculator(ScoreMultiplierContext context)
            : base(context)
        {
            #region Difficulty Reduction

            Single<SankoModEasy>(hasMultiplier: 0.5);
            Single<SankoModNoFail>(hasMultiplier: 0.5);
            Single<SankoModHalfTime>(hasMultiplier: halfTime => rateAdjustMultiplier(halfTime.SpeedChange.Value));
            Single<SankoModDaycore>(hasMultiplier: daycore => rateAdjustMultiplier(daycore.SpeedChange.Value));
            Single<SankoModSimplifiedRhythm>(hasMultiplier: 0.6);

            #endregion

            #region Difficulty Increase

            Single<SankoModHardRock>(hasMultiplier: hardRock => hardRock.UsesDefaultConfiguration ? 1.06 : 1);
            // Sudden Death
            // Perfect
            Single<SankoModDoubleTime>(hasMultiplier: doubleTime => rateAdjustMultiplier(doubleTime.SpeedChange.Value));
            Single<SankoModNightcore>(hasMultiplier: nightcore => rateAdjustMultiplier(nightcore.SpeedChange.Value));
            Single<SankoModHidden>(hasMultiplier: hidden => hidden.UsesDefaultConfiguration ? 1.06 : 1);
            Single<SankoModFlashlight>(hasMultiplier: flashlight => flashlight.UsesDefaultConfiguration ? 1.12 : 1);
            // Accuracy Challenge

            #endregion

            #region Conversion

            // Random
            Single<SankoModDifficultyAdjust>(hasMultiplier: 0.5);
            Single<SankoModClassic>(hasMultiplier: _ => classicMultiplier(Context.Score));
            // Swap
            // Single Tap
            Single<SankoModConstantSpeed>(hasMultiplier: 0.9);

            #endregion

            #region Automation

            // Autoplay
            // Cinema
            Single<SankoModRelax>(hasMultiplier: 0.1);

            #endregion

            #region Fun

            Single<ModWindUp>(hasMultiplier: 0.5);
            Single<ModWindDown>(hasMultiplier: 0.5);
            // Muted
            Single<ModAdaptiveSpeed>(hasMultiplier: 0.5);

            #endregion

            #region System

            // Score V2

            #endregion
        }

        private static double rateAdjustMultiplier(double speedChange)
        {
            // Round to the nearest multiple of 0.1.
            double value = (int)(speedChange * 10) / 10.0;

            // Offset back to 0.
            value -= 1;

            if (speedChange >= 1)
                return 1 + value / 5;
            else
                return 0.6 + value;
        }

        private static double classicMultiplier(ScoreInfo? score)
        {
            if (score != null && score.TotalScoreVersion < 30000017)
                return 0.96;

            return 1;
        }
    }
}
