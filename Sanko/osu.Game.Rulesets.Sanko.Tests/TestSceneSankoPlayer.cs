// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Sanko.Tests
{
    public partial class TestSceneSankoPlayer : PlayerTestScene
    {
        protected override Ruleset CreatePlayerRuleset() => new SankoRuleset();
    }
}
