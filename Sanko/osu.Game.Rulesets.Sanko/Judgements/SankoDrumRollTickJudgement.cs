// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Sanko.Judgements
{
    public class SankoDrumRollTickJudgement : SankoJudgement
    {
        public override HitResult MaxResult => HitResult.SmallBonus;

        protected override double HealthIncreaseFor(HitResult result) => 0;
    }
}
