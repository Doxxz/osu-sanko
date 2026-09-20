// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Rulesets.Sanko.Replays;

namespace osu.Game.Rulesets.Sanko.Tests
{
    public class SankoReplayFrameTest
    {
        // Note: the tsu actions have no stable mouse equivalent, so they rely on the extra legacy button bits.
        private static readonly SankoAction[] all_actions =
        {
            SankoAction.LeftRim,
            SankoAction.LeftCentre,
            SankoAction.RightCentre,
            SankoAction.RightRim,
            SankoAction.LeftTsu,
            SankoAction.RightTsu,
        };

        [Test]
        public void TestAllActionsRoundTripThroughLegacy()
        {
            var restored = roundTrip(new SankoReplayFrame(1234, all_actions));

            Assert.That(restored.Actions, Is.EquivalentTo(all_actions));
        }

        [TestCase(SankoAction.LeftTsu)]
        [TestCase(SankoAction.RightTsu)]
        public void TestTsuActionSurvivesLegacyConversion(SankoAction action)
        {
            var restored = roundTrip(new SankoReplayFrame(1000, action));

            Assert.That(restored.Actions, Is.EqualTo(new[] { action }));
        }

        [TestCase(SankoAction.LeftTsu)]
        [TestCase(SankoAction.RightTsu)]
        public void TestTsuActionDoesNotBecomeADonOrKat(SankoAction action)
        {
            var restored = roundTrip(new SankoReplayFrame(1000, action));

            Assert.That(restored.Actions, Has.No.Member(SankoAction.LeftCentre));
            Assert.That(restored.Actions, Has.No.Member(SankoAction.RightCentre));
            Assert.That(restored.Actions, Has.No.Member(SankoAction.LeftRim));
            Assert.That(restored.Actions, Has.No.Member(SankoAction.RightRim));
        }

        [Test]
        public void TestTsuActionsAreIndependentOfEachOther()
        {
            var restored = roundTrip(new SankoReplayFrame(1000, SankoAction.LeftTsu));

            Assert.That(restored.Actions, Has.No.Member(SankoAction.RightTsu));
        }

        [Test]
        public void TestStrongTsuRoundTripsThroughLegacy()
        {
            var restored = roundTrip(new SankoReplayFrame(1000, SankoAction.LeftTsu, SankoAction.RightTsu));

            Assert.That(restored.Actions, Is.EquivalentTo(new[] { SankoAction.LeftTsu, SankoAction.RightTsu }));
        }

        private static SankoReplayFrame roundTrip(SankoReplayFrame frame)
        {
            var restored = new SankoReplayFrame();
            restored.FromLegacy(frame.ToLegacy(null!), null!);
            return restored;
        }
    }
}
