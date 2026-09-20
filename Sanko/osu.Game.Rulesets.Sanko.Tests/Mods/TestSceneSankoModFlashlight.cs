// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Sanko.Mods;
using osu.Game.Rulesets.Sanko.UI;
using osuTK;

namespace osu.Game.Rulesets.Sanko.Tests.Mods
{
    public partial class TestSceneSankoModFlashlight : SankoModTestScene
    {
        [Test]
        public void TestAspectRatios([Values] bool withClassicMod)
        {
            if (withClassicMod)
                CreateModTest(new ModTestData { Mods = new Mod[] { new SankoModFlashlight(), new SankoModClassic() }, PassCondition = () => true });
            else
                CreateModTest(new ModTestData { Mod = new SankoModFlashlight(), PassCondition = () => true });

            AddStep("clear dim", () => LocalConfig.SetValue(OsuSetting.DimLevel, 0.0));

            AddStep("reset", () => Stack.FillMode = FillMode.Stretch);
            AddStep("set to 16:9", () =>
            {
                Stack.FillAspectRatio = 16 / 9f;
                Stack.FillMode = FillMode.Fit;
            });
            AddStep("set to 4:3", () =>
            {
                Stack.FillAspectRatio = 4 / 3f;
                Stack.FillMode = FillMode.Fit;
            });
            AddSliderStep("aspect ratio", 0.01f, 5f, 1f, v =>
            {
                Stack.FillAspectRatio = v;
                Stack.FillMode = FillMode.Fit;
            });
        }

        [TestCase(1f)]
        [TestCase(0.5f)]
        [TestCase(1.25f)]
        [TestCase(1.5f)]
        public void TestSizeMultiplier(float sizeMultiplier) => CreateModTest(new ModTestData { Mod = new SankoModFlashlight { SizeMultiplier = { Value = sizeMultiplier } }, PassCondition = () => true });

        [Test]
        public void TestComboBasedSize([Values] bool comboBasedSize) => CreateModTest(new ModTestData { Mod = new SankoModFlashlight { ComboBasedSize = { Value = comboBasedSize } }, PassCondition = () => true });

        [Test]
        public void TestFlashlightAlwaysHasNonZeroSize()
        {
            bool failed = false;

            CreateModTest(new ModTestData
            {
                Mod = new TestSankoModFlashlight { ComboBasedSize = { Value = true } },
                Autoplay = false,
                PassCondition = () =>
                {
                    failed |= this.ChildrenOfType<TestSankoModFlashlight.TestSankoFlashlight>().SingleOrDefault()?.FlashlightSize.Y == 0;
                    return !failed;
                }
            });
        }

        private partial class TestSankoModFlashlight : SankoModFlashlight
        {
            protected override Flashlight CreateFlashlight() => new TestSankoFlashlight(this, DrawableRuleset);

            public partial class TestSankoFlashlight : SankoFlashlight
            {
                public TestSankoFlashlight(SankoModFlashlight modFlashlight, DrawableSankoRuleset drawableRuleset)
                    : base(modFlashlight, drawableRuleset)
                {
                }

                public new Vector2 FlashlightSize => base.FlashlightSize;
            }
        }
    }
}
