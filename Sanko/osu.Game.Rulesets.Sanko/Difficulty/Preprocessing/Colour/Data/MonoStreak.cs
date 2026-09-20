// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Sanko.Objects;
using System.Collections.Generic;

namespace osu.Game.Rulesets.Sanko.Difficulty.Preprocessing.Colour.Data
{
    /// <summary>
    /// Encode colour information for a sequence of <see cref="SankoDifficultyHitObject"/>s. Consecutive <see cref="SankoDifficultyHitObject"/>s
    /// of the same <see cref="Objects.HitType"/> are encoded within the same <see cref="MonoStreak"/>.
    /// </summary>
    public class MonoStreak
    {
        /// <summary>
        /// List of <see cref="DifficultyHitObject"/>s that are encoded within this <see cref="MonoStreak"/>.
        /// </summary>
        public List<SankoDifficultyHitObject> HitObjects { get; private set; } = new List<SankoDifficultyHitObject>();

        /// <summary>
        /// The parent <see cref="AlternatingMonoPattern"/> that contains this <see cref="MonoStreak"/>
        /// </summary>
        public AlternatingMonoPattern Parent = null!;

        /// <summary>
        /// Index of this <see cref="MonoStreak"/> within it's parent <see cref="AlternatingMonoPattern"/>
        /// </summary>
        public int Index;

        /// <summary>
        /// The first <see cref="SankoDifficultyHitObject"/> in this <see cref="MonoStreak"/>.
        /// </summary>
        public SankoDifficultyHitObject FirstHitObject => HitObjects[0];

        /// <summary>
        /// The last <see cref="SankoDifficultyHitObject"/> in this <see cref="MonoStreak"/>.
        /// </summary>
        public SankoDifficultyHitObject LastHitObject => HitObjects[^1];

        /// <summary>
        /// The hit type of all objects encoded within this <see cref="MonoStreak"/>
        /// </summary>
        public HitType? HitType => (HitObjects[0].BaseObject as Hit)?.Type;

        /// <summary>
        /// How long the mono pattern encoded within is
        /// </summary>
        public int RunLength => HitObjects.Count;
    }
}
