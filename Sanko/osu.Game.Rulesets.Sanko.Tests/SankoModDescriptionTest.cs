// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Reflection;
using NUnit.Framework;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Rulesets.Sanko.Mods;

namespace osu.Game.Rulesets.Sanko.Tests
{
    public class SankoModDescriptionTest
    {
        [Test]
        public void TestModDescriptions()
        {
            Assert.Multiple(() =>
            {
                Assert.That(new SankoModTsu().Description.ToString(), Is.EqualTo("Modify the chance of notes to become tsu."));
                Assert.That(new SankoModTsuRandom().Description.ToString(), Is.EqualTo("Randomly turn notes into tsu, without changing dons into kats, or kats into dons."));
            });
        }

        [Test]
        public void TestTsuConversionSettingLabels()
        {
            Assert.Multiple(() =>
            {
                Assert.That(settingLabel(nameof(SankoModTsu.IsolatedChance)).ToString(), Is.EqualTo("Isolated notes"));
                Assert.That(settingLabel(nameof(SankoModTsu.NonIsolatedChance)).ToString(), Is.EqualTo("Sequences"));
                Assert.That(settingLabel(nameof(SankoModTsu.BigChance)).ToString(), Is.EqualTo("Big notes"));
            });
        }

        [Test]
        public void TestTsuConversionSettingDescriptions()
        {
            Assert.Multiple(() =>
            {
                Assert.That(settingDescription(nameof(SankoModTsu.IsolatedChance)).ToString(), Is.EqualTo("The chance for an isolated note to become a tsu, as a percentage."));
                Assert.That(settingDescription(nameof(SankoModTsu.NonIsolatedChance)).ToString(), Is.EqualTo("The chance for a sequence of notes to become tsus, as a percentage."));
                Assert.That(settingDescription(nameof(SankoModTsu.BigChance)).ToString(), Is.EqualTo("The chance for an isolated big note or a sequence to become tsus, as a percentage."));
            });
        }

        private static LocalisableString settingLabel(string propertyName)
            => settingSource(propertyName).Label;

        private static LocalisableString settingDescription(string propertyName)
            => settingSource(propertyName).Description;

        private static SettingSourceAttribute settingSource(string propertyName)
            => typeof(SankoModTsu).GetProperty(propertyName)!.GetCustomAttribute<SettingSourceAttribute>()!;
    }
}
