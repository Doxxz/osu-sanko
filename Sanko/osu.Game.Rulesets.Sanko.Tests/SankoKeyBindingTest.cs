// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Extensions;
using osu.Framework.Localisation;
using osu.Game.Rulesets.Sanko;

namespace osu.Game.Rulesets.Sanko.Tests
{
    public class SankoKeyBindingTest
    {
        [Test]
        public void TestKeyBindingOrder()
        {
            // The order of the defaults determines the order the actions are listed in the key binding settings.
            var actions = new SankoRuleset().GetDefaultKeyBindings()
                                            .Select(b => (SankoAction)b.Action)
                                            .Distinct()
                                            .ToArray();

            Assert.That(actions, Is.EqualTo(new[]
            {
                SankoAction.LeftTsu,
                SankoAction.LeftRim,
                SankoAction.LeftCentre,
                SankoAction.RightCentre,
                SankoAction.RightRim,
                SankoAction.RightTsu,
            }));
        }

        [Test]
        public void TestKeyBindingNames()
        {
            Assert.Multiple(() =>
            {
                Assert.That(SankoAction.LeftTsu.GetLocalisableDescription().ToString(), Is.EqualTo("Left (green)"));
                Assert.That(SankoAction.LeftRim.GetLocalisableDescription().ToString(), Is.EqualTo("Left (blue)"));
                Assert.That(SankoAction.LeftCentre.GetLocalisableDescription().ToString(), Is.EqualTo("Left (red)"));
                Assert.That(SankoAction.RightCentre.GetLocalisableDescription().ToString(), Is.EqualTo("Right (red)"));
                Assert.That(SankoAction.RightRim.GetLocalisableDescription().ToString(), Is.EqualTo("Right (blue)"));
                Assert.That(SankoAction.RightTsu.GetLocalisableDescription().ToString(), Is.EqualTo("Right (green)"));
            });
        }
    }
}
