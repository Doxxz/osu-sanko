// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sanko.Judgements;
using osu.Game.Rulesets.Sanko.Scoring;

namespace osu.Game.Rulesets.Sanko.Objects
{
    public abstract class SankoHitObject : HitObject
    {
        /// <summary>
        /// Default size of a drawable sanko hit object.
        /// </summary>
        public const float DEFAULT_SIZE = 0.475f;

        public override Judgement CreateJudgement() => new SankoJudgement();

        protected override HitWindows CreateHitWindows() => new SankoHitWindows();
    }
}
