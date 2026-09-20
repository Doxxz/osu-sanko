// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Sanko.Difficulty.Preprocessing.Colour.Data;

namespace osu.Game.Rulesets.Sanko.Difficulty.Preprocessing.Colour
{
    /// <summary>
    /// Stores colour compression information for a <see cref="SankoDifficultyHitObject"/>.
    /// </summary>
    public class SankoColourData
    {
        /// <summary>
        /// The <see cref="MonoStreak"/> that encodes this note.
        /// </summary>
        public MonoStreak? MonoStreak;

        /// <summary>
        /// The <see cref="AlternatingMonoPattern"/> that encodes this note.
        /// </summary>
        public AlternatingMonoPattern? AlternatingMonoPattern;

        /// <summary>
        /// The <see cref="RepeatingHitPattern"/> that encodes this note.
        /// </summary>
        public RepeatingHitPatterns? RepeatingHitPattern;

        /// <summary>
        /// The closest past <see cref="SankoDifficultyHitObject"/> that's not the same colour.
        /// </summary>
        public SankoDifficultyHitObject? PreviousColourChange => MonoStreak?.FirstHitObject.PreviousNote(0);

        /// <summary>
        /// The closest future <see cref="SankoDifficultyHitObject"/> that's not the same colour.
        /// </summary>
        public SankoDifficultyHitObject? NextColourChange => MonoStreak?.LastHitObject.NextNote(0);
    }
}
