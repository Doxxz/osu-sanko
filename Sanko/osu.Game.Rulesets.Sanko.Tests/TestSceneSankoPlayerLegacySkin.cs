// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Sanko.Mods;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Sanko.Tests
{
    public partial class TestSceneSankoPlayerLegacySkin : LegacySkinPlayerTestScene
    {
        protected override Ruleset CreatePlayerRuleset() => new SankoRuleset();

        protected override TestPlayer CreatePlayer(Ruleset ruleset)
        {
            SelectedMods.Value = new[] { new SankoModClassic() };
            return base.CreatePlayer(ruleset);
        }
    }
}
