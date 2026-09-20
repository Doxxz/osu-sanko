// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sanko.Mods;
using osu.Game.Rulesets.Sanko.Objects;
using osu.Game.Rulesets.Sanko.Scoring;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Sanko.Tests.Mods
{
    public partial class TestSceneSankoModPerfect : ModFailConditionTestScene
    {
        protected override Ruleset CreatePlayerRuleset() => new TestSankoRuleset();

        public TestSceneSankoModPerfect()
            : base(new SankoModPerfect())
        {
        }

        [TestCase(false)]
        [TestCase(true)]
        public void TestHit(bool shouldMiss) => CreateHitObjectTest(new HitObjectTestData(new Hit { StartTime = 1000, Type = HitType.Centre }), shouldMiss);

        [TestCase(false)]
        [TestCase(true)]
        public void TestDrumRoll(bool shouldMiss) => CreateHitObjectTest(new HitObjectTestData(new DrumRoll { StartTime = 1000, EndTime = 3000 }, false), shouldMiss);

        [TestCase(false)]
        [TestCase(true)]
        public void TestSwell(bool shouldMiss) => CreateHitObjectTest(new HitObjectTestData(new Swell { StartTime = 1000, EndTime = 3000 }, false), shouldMiss);

        private partial class TestSankoRuleset : SankoRuleset
        {
            public override HealthProcessor CreateHealthProcessor(double drainStartTime) => new TestSankoHealthProcessor();

            private partial class TestSankoHealthProcessor : SankoHealthProcessor
            {
                protected override void Reset(bool storeResults)
                {
                    base.Reset(storeResults);

                    Health.Value = 1; // Don't care about the health condition (only the mod condition)
                }
            }
        }
    }
}
