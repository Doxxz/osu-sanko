// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sanko.Judgements;

namespace osu.Game.Rulesets.Sanko.Objects
{
    /// <summary>
    /// Base type for nested strong hits.
    /// Used by <see cref="SankoStrongableHitObject"/>s to represent their strong bonus scoring portions.
    /// </summary>
    public abstract class StrongNestedHitObject : SankoHitObject
    {
        public readonly SankoHitObject Parent;

        protected StrongNestedHitObject(SankoHitObject parent)
        {
            Parent = parent;
        }

        public override Judgement CreateJudgement() => new SankoStrongJudgement();

        protected override HitWindows CreateHitWindows() => HitWindows.Empty;
    }
}
