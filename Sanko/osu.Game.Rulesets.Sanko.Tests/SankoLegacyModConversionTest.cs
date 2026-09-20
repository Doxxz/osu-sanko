// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Sanko.Mods;
using osu.Game.Tests.Beatmaps;

namespace osu.Game.Rulesets.Sanko.Tests
{
    [TestFixture]
    public class SankoLegacyModConversionTest : LegacyModConversionTest
    {
        private static readonly object[][] sanko_mod_mapping =
        {
            new object[] { LegacyMods.NoFail, new[] { typeof(SankoModNoFail) } },
            new object[] { LegacyMods.Easy, new[] { typeof(SankoModEasy) } },
            new object[] { LegacyMods.Hidden, new[] { typeof(SankoModHidden) } },
            new object[] { LegacyMods.HardRock, new[] { typeof(SankoModHardRock) } },
            new object[] { LegacyMods.SuddenDeath, new[] { typeof(SankoModSuddenDeath) } },
            new object[] { LegacyMods.DoubleTime, new[] { typeof(SankoModDoubleTime) } },
            new object[] { LegacyMods.Relax, new[] { typeof(SankoModRelax) } },
            new object[] { LegacyMods.HalfTime, new[] { typeof(SankoModHalfTime) } },
            new object[] { LegacyMods.Flashlight, new[] { typeof(SankoModFlashlight) } },
            new object[] { LegacyMods.Autoplay, new[] { typeof(SankoModAutoplay) } },
            new object[] { LegacyMods.HardRock | LegacyMods.DoubleTime, new[] { typeof(SankoModHardRock), typeof(SankoModDoubleTime) } },
            new object[] { LegacyMods.ScoreV2, new[] { typeof(ModScoreV2) } },
        };

        [TestCaseSource(nameof(sanko_mod_mapping))]
        [TestCase(LegacyMods.Cinema, new[] { typeof(SankoModCinema) })]
        [TestCase(LegacyMods.Cinema | LegacyMods.Autoplay, new[] { typeof(SankoModCinema) })]
        [TestCase(LegacyMods.Nightcore, new[] { typeof(SankoModNightcore) })]
        [TestCase(LegacyMods.Nightcore | LegacyMods.DoubleTime, new[] { typeof(SankoModNightcore) })]
        [TestCase(LegacyMods.Perfect, new[] { typeof(SankoModPerfect) })]
        [TestCase(LegacyMods.Perfect | LegacyMods.SuddenDeath, new[] { typeof(SankoModPerfect) })]
        public new void TestFromLegacy(LegacyMods legacyMods, Type[] expectedMods) => base.TestFromLegacy(legacyMods, expectedMods);

        [TestCaseSource(nameof(sanko_mod_mapping))]
        [TestCase(LegacyMods.Cinema | LegacyMods.Autoplay, new[] { typeof(SankoModCinema) })]
        [TestCase(LegacyMods.Nightcore | LegacyMods.DoubleTime, new[] { typeof(SankoModNightcore) })]
        [TestCase(LegacyMods.Perfect | LegacyMods.SuddenDeath, new[] { typeof(SankoModPerfect) })]
        public new void TestToLegacy(LegacyMods legacyMods, Type[] givenMods) => base.TestToLegacy(legacyMods, givenMods);

        protected override Ruleset CreateRuleset() => new SankoRuleset();
    }
}
