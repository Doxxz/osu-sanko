// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Sanko.Tests
{
    [TestFixture]
    public partial class SankoRulesetIconTest : OsuTestScene
    {
        [Test]
        public void TestIconTextureIsBundled()
        {
            Assert.That(new SankoRuleset().CreateResourceStore().Get("Textures/RulesetIcon.png"), Is.Not.Null.And.Not.Empty);
        }

        [Test]
        public void TestIconTextureLoads()
        {
            Sprite icon = null!;

            AddStep("create ruleset icon", () => Add(icon = (Sprite)new SankoRuleset().CreateIcon()));
            AddAssert("texture was loaded", () => icon.Texture != null);
        }
    }
}
