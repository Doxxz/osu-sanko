// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Sanko.Tests.Skinning
{
    public abstract partial class SankoSkinnableTestScene : SkinnableTestScene
    {
        protected override Ruleset CreateRulesetForSkinProvider() => new SankoRuleset();
    }
}
