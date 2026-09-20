// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Sanko.Beatmaps;
using osu.Game.Rulesets.Sanko.Mods;
using osu.Game.Rulesets.Sanko.Objects;

namespace osu.Game.Rulesets.Sanko.Tests
{
    public class SankoTsuConversionTest
    {
        [Test]
        public void TestIsolatedNoteRequiresBothNeighbours()
        {
            // In dkd only the middle note is surrounded by the opposite colour.
            var beatmap = createBeatmap(HitType.Centre, HitType.Rim, HitType.Centre);

            SankoTsuConversion.Apply(beatmap, 1, 0, 0, new Random(0));

            Assert.That(typesOf(beatmap), Is.EqualTo(new[] { HitType.Centre, HitType.Tsu, HitType.Centre }));
        }

        [Test]
        public void TestIsolatedNoteAtBoundaryIsNotConverted()
        {
            var beatmap = createBeatmap(HitType.Centre, HitType.Rim);

            SankoTsuConversion.Apply(beatmap, 1, 0, 0, new Random(0));

            Assert.That(typesOf(beatmap), Is.EqualTo(new[] { HitType.Centre, HitType.Rim }));
        }

        [Test]
        public void TestIsolatedKat()
        {
            var beatmap = createBeatmap(HitType.Rim, HitType.Centre, HitType.Rim);

            SankoTsuConversion.Apply(beatmap, 1, 0, 0, new Random(0));

            Assert.That(typesOf(beatmap), Is.EqualTo(new[] { HitType.Rim, HitType.Tsu, HitType.Rim }));
        }

        [Test]
        public void TestNonIsolatedSequencesOnly()
        {
            // The first sequence converts, which blocks the following one.
            var beatmap = createBeatmap(HitType.Centre, HitType.Centre, HitType.Rim, HitType.Rim, HitType.Rim);

            SankoTsuConversion.Apply(beatmap, 0, 1, 0, new Random(0));

            Assert.That(typesOf(beatmap), Is.EqualTo(new[] { HitType.Tsu, HitType.Tsu, HitType.Rim, HitType.Rim, HitType.Rim }));
        }

        [Test]
        public void TestZeroChanceConvertsNothing()
        {
            var beatmap = createBeatmap(HitType.Centre, HitType.Centre, HitType.Rim);

            SankoTsuConversion.Apply(beatmap, 0, 0, 0, new Random(0));

            Assert.That(typesOf(beatmap), Is.EqualTo(new[] { HitType.Centre, HitType.Centre, HitType.Rim }));
        }

        [Test]
        public void TestConsecutiveSequencesAreSeparatedByAnUnconvertedOne()
        {
            // ddkkdd: the leading don sequence converts, the kat sequence is blocked by it, and the
            // trailing don sequence converts again because the kat sequence did not.
            var beatmap = createBeatmap(HitType.Centre, HitType.Centre, HitType.Rim, HitType.Rim, HitType.Centre, HitType.Centre);

            SankoTsuConversion.Apply(beatmap, 0, 1, 0, new Random(0));

            Assert.That(typesOf(beatmap), Is.EqualTo(new[] { HitType.Tsu, HitType.Tsu, HitType.Rim, HitType.Rim, HitType.Tsu, HitType.Tsu }));
        }

        [Test]
        public void TestSequenceConvertsWhenPreviousUnitDidNot()
        {
            // The big don sequence at the start can never convert (there is no previous note), so it does
            // not block the following kat sequence.
            var beatmap = createBeatmap(HitType.Centre, HitType.Centre, HitType.Rim, HitType.Rim);
            markStrong(beatmap, 0, 1);

            SankoTsuConversion.Apply(beatmap, 0, 1, 1, new Random(0));

            Assert.That(typesOf(beatmap), Is.EqualTo(new[] { HitType.Centre, HitType.Centre, HitType.Tsu, HitType.Tsu }));
        }

        [Test]
        public void TestBigSequenceConvertsWhenPreviousNoteIsDifferentColour()
        {
            var beatmap = createBeatmap(HitType.Rim, HitType.Centre, HitType.Centre);
            markStrong(beatmap, 1, 2);

            // The sequence chance and isolated chance are zero, so this can only use the special chance.
            SankoTsuConversion.Apply(beatmap, 0, 0, 1, new Random(0));

            Assert.That(typesOf(beatmap), Is.EqualTo(new[] { HitType.Rim, HitType.Tsu, HitType.Tsu }));
        }

        [Test]
        public void TestBigSequenceDoesNotConvertWhenPreviousNoteMatches()
        {
            var beatmap = createBeatmap(HitType.Centre, HitType.Centre, HitType.Centre);
            markStrong(beatmap, 1, 2);

            SankoTsuConversion.Apply(beatmap, 0, 0, 1, new Random(0));

            Assert.That(typesOf(beatmap), Is.EqualTo(new[] { HitType.Centre, HitType.Centre, HitType.Centre }));
        }

        [Test]
        public void TestBigSequenceAtStartIsNotConverted()
        {
            var beatmap = createBeatmap(HitType.Centre, HitType.Centre);
            markStrong(beatmap, 0, 1);

            SankoTsuConversion.Apply(beatmap, 1, 1, 1, new Random(0));

            Assert.That(typesOf(beatmap), Is.EqualTo(new[] { HitType.Centre, HitType.Centre }));
        }

        [Test]
        public void TestSmallNoteNextToSameColourBigNoteIsNotIsolated()
        {
            // The small don joins the big don sequence rather than being isolated.
            var beatmap = createBeatmap(HitType.Rim, HitType.Centre, HitType.Centre);
            markStrong(beatmap, 2);

            SankoTsuConversion.Apply(beatmap, 0, 0, 1, new Random(0));

            Assert.That(typesOf(beatmap), Is.EqualTo(new[] { HitType.Rim, HitType.Tsu, HitType.Tsu }));
        }

        [Test]
        public void TestBigSequenceIsBlockedByPreviousConvertedSequence()
        {
            // kk [big d d]: the kat sequence converts, which blocks the big don sequence.
            var beatmap = createBeatmap(HitType.Rim, HitType.Rim, HitType.Centre, HitType.Centre);
            markStrong(beatmap, 3);

            SankoTsuConversion.Apply(beatmap, 0, 1, 1, new Random(0));

            Assert.That(typesOf(beatmap), Is.EqualTo(new[] { HitType.Tsu, HitType.Tsu, HitType.Centre, HitType.Centre }));
        }

        [Test]
        public void TestBigSequenceConvertsWhenPreviousSequenceDidNot()
        {
            var beatmap = createBeatmap(HitType.Rim, HitType.Rim, HitType.Centre, HitType.Centre);
            markStrong(beatmap, 3);

            SankoTsuConversion.Apply(beatmap, 0, 0, 1, new Random(0));

            Assert.That(typesOf(beatmap), Is.EqualTo(new[] { HitType.Rim, HitType.Rim, HitType.Tsu, HitType.Tsu }));
        }

        [Test]
        public void TestIsolatedNoteIsBlockedByPreviousConvertedSequence()
        {
            // dd k d: the don sequence converts, which blocks the isolated kat at index 2.
            var beatmap = createBeatmap(HitType.Centre, HitType.Centre, HitType.Rim, HitType.Centre);

            SankoTsuConversion.Apply(beatmap, 1, 1, 0, new Random(0));

            Assert.That(typesOf(beatmap), Is.EqualTo(new[] { HitType.Tsu, HitType.Tsu, HitType.Rim, HitType.Centre }));
        }

        [Test]
        public void TestIsolatedNoteConvertsWhenPreviousSequenceDidNot()
        {
            var beatmap = createBeatmap(HitType.Centre, HitType.Centre, HitType.Rim, HitType.Centre);

            SankoTsuConversion.Apply(beatmap, 1, 0, 0, new Random(0));

            Assert.That(typesOf(beatmap), Is.EqualTo(new[] { HitType.Centre, HitType.Centre, HitType.Tsu, HitType.Centre }));
        }

        [Test]
        public void TestIsolatedNoteIsBlockedByPreviousConvertedBigSequence()
        {
            // k [big d d] k d: the big don sequence converts, which blocks the isolated kat at index 3.
            var beatmap = createBeatmap(HitType.Rim, HitType.Centre, HitType.Centre, HitType.Rim, HitType.Centre);
            markStrong(beatmap, 1, 2);

            SankoTsuConversion.Apply(beatmap, 1, 0, 1, new Random(0));

            Assert.That(typesOf(beatmap), Is.EqualTo(new[] { HitType.Rim, HitType.Tsu, HitType.Tsu, HitType.Rim, HitType.Centre }));
        }

        [Test]
        public void TestAlternatingSequenceConvertsASingleColour()
        {
            var beatmap = createBeatmap(HitType.Centre, HitType.Rim, HitType.Centre, HitType.Rim, HitType.Centre, HitType.Rim);

            // Alternating sequences share the non-isolated sequence chance; the isolated and big chances are zero.
            SankoTsuConversion.Apply(beatmap, 0, 1, 0, new Random(0));

            var result = typesOf(beatmap);

            bool donsConverted = result.SequenceEqual(new[] { HitType.Tsu, HitType.Rim, HitType.Tsu, HitType.Rim, HitType.Tsu, HitType.Rim });
            bool katsConverted = result.SequenceEqual(new[] { HitType.Centre, HitType.Tsu, HitType.Centre, HitType.Tsu, HitType.Centre, HitType.Tsu });

            Assert.That(donsConverted || katsConverted, Is.True, $"Unexpected result: {string.Join(", ", result)}");
        }

        [Test]
        public void TestAlternatingSequenceIsNotAffectedByBigChance()
        {
            var beatmap = createBeatmap(HitType.Centre, HitType.Rim, HitType.Centre, HitType.Rim, HitType.Centre, HitType.Rim);

            // The big note chance must not touch an alternating sequence.
            SankoTsuConversion.Apply(beatmap, 0, 0, 1, new Random(0));

            Assert.That(typesOf(beatmap), Is.EqualTo(new[] { HitType.Centre, HitType.Rim, HitType.Centre, HitType.Rim, HitType.Centre, HitType.Rim }));
        }

        [Test]
        public void TestAlternatingSequenceIsBlockedByPreviousConvertedSequence()
        {
            // dd followed by kdkdk: the don sequence converts, which blocks the alternating sequence.
            var beatmap = createBeatmap(HitType.Centre, HitType.Centre, HitType.Rim, HitType.Centre, HitType.Rim, HitType.Centre, HitType.Rim);

            SankoTsuConversion.Apply(beatmap, 0, 1, 0, new Random(0));

            Assert.That(typesOf(beatmap), Is.EqualTo(new[] { HitType.Tsu, HitType.Tsu, HitType.Rim, HitType.Centre, HitType.Rim, HitType.Centre, HitType.Rim }));
        }

        [Test]
        public void TestAlternatingSequenceRequiresFourNotes()
        {
            var beatmap = createBeatmap(HitType.Centre, HitType.Rim, HitType.Centre);

            // dkd is too short to be an alternating sequence, so the sequence chance does not apply.
            SankoTsuConversion.Apply(beatmap, 0, 1, 0, new Random(0));

            Assert.That(typesOf(beatmap), Is.EqualTo(new[] { HitType.Centre, HitType.Rim, HitType.Centre }));
        }

        [Test]
        public void TestAlternatingSequenceIsBrokenBySameColourSequence()
        {
            // dkd followed by kkk: the same-colour sequence breaks the alternation, leaving a three-note
            // alternation (isolated, chance zero) plus a same-colour sequence.
            var beatmap = createBeatmap(HitType.Centre, HitType.Rim, HitType.Centre, HitType.Rim, HitType.Rim, HitType.Rim);

            SankoTsuConversion.Apply(beatmap, 0, 1, 0, new Random(0));

            Assert.That(typesOf(beatmap), Is.EqualTo(new[] { HitType.Centre, HitType.Rim, HitType.Centre, HitType.Tsu, HitType.Tsu, HitType.Tsu }));
        }

        [Test]
        public void TestAlternatingSequenceIsBrokenByBigNotes()
        {
            // dkd [big K] dkd: the big note splits the alternation into two three-note blocks (isolated, chance zero).
            var beatmap = createBeatmap(HitType.Centre, HitType.Rim, HitType.Centre, HitType.Rim, HitType.Centre, HitType.Rim, HitType.Centre);
            markStrong(beatmap, 3);

            SankoTsuConversion.Apply(beatmap, 0, 0, 1, new Random(0));

            Assert.That(typesOf(beatmap), Is.EqualTo(new[] { HitType.Centre, HitType.Rim, HitType.Centre, HitType.Tsu, HitType.Centre, HitType.Rim, HitType.Centre }));
        }

        [Test]
        public void TestResetRestoresOriginalColours()
        {
            var beatmap = createBeatmap(HitType.Centre, HitType.Centre, HitType.Rim, HitType.Rim);

            // The don sequence converts and blocks the kat sequence.
            SankoTsuConversion.Apply(beatmap, 1, 1, 1, new Random(0));
            Assert.That(typesOf(beatmap), Is.EqualTo(new[] { HitType.Tsu, HitType.Tsu, HitType.Rim, HitType.Rim }));

            SankoTsuConversion.Reset(beatmap);

            Assert.That(typesOf(beatmap), Is.EqualTo(new[] { HitType.Centre, HitType.Centre, HitType.Rim, HitType.Rim }));
        }

        [Test]
        public void TestDefaultChances()
        {
            Assert.That(SankoTsuConversion.DEFAULT_ISOLATED_CHANCE, Is.EqualTo(0.60));
            Assert.That(SankoTsuConversion.DEFAULT_NON_ISOLATED_CHANCE, Is.EqualTo(0.40));
            Assert.That(SankoTsuConversion.DEFAULT_BIG_CHANCE, Is.EqualTo(0.55));

            var mod = new SankoModTsu();

            Assert.That(mod.IsolatedChance.Value, Is.EqualTo(60));
            Assert.That(mod.NonIsolatedChance.Value, Is.EqualTo(40));
            Assert.That(mod.BigChance.Value, Is.EqualTo(55));
        }

        [Test]
        public void TestModChances()
        {
            var beatmap = createBeatmap(HitType.Centre, HitType.Centre, HitType.Rim, HitType.Rim);
            var mod = new SankoModTsu();

            mod.IsolatedChance.Value = 100;
            mod.NonIsolatedChance.Value = 100;
            mod.BigChance.Value = 100;
            mod.ApplyToBeatmap(beatmap);

            // The don sequence converts and blocks the kat sequence.
            Assert.That(typesOf(beatmap), Is.EqualTo(new[] { HitType.Tsu, HitType.Tsu, HitType.Rim, HitType.Rim }));

            mod.IsolatedChance.Value = 0;
            mod.NonIsolatedChance.Value = 0;
            mod.BigChance.Value = 0;
            mod.ApplyToBeatmap(beatmap);
            Assert.That(typesOf(beatmap), Is.EqualTo(new[] { HitType.Centre, HitType.Centre, HitType.Rim, HitType.Rim }));
        }

        [Test]
        public void TestRandomModChance()
        {
            var beatmap = createBeatmap(HitType.Centre, HitType.Rim, HitType.Centre);
            var mod = new SankoModTsuRandom();

            mod.Chance.Value = 100;
            mod.ApplyToBeatmap(beatmap);
            Assert.That(typesOf(beatmap), Is.All.EqualTo(HitType.Tsu));

            mod.Chance.Value = 0;
            mod.ApplyToBeatmap(beatmap);
            Assert.That(typesOf(beatmap), Is.EqualTo(new[] { HitType.Centre, HitType.Rim, HitType.Centre }));
        }

        [Test]
        public void TestModsAreIncompatible()
        {
            Assert.That(new SankoModTsu().IncompatibleMods, Does.Contain(typeof(SankoModTsuRandom)));
            Assert.That(new SankoModTsuRandom().IncompatibleMods, Does.Contain(typeof(SankoModTsu)));
        }

        [Test]
        public void TestModIsDeterministicWithoutSeed()
        {
            // Mods are cloned for every attempt, so a fresh instance must reproduce the same conversion.
            var first = createManySequenceBeatmap();
            var second = createManySequenceBeatmap();

            new SankoModTsu().ApplyToBeatmap(first);
            new SankoModTsu().ApplyToBeatmap(second);

            Assert.That(typesOf(first), Is.EqualTo(typesOf(second)));
            Assert.That(typesOf(first), Does.Contain(HitType.Tsu));
        }

        [Test]
        public void TestModUsesCustomSeed()
        {
            var first = createManySequenceBeatmap();
            var second = createManySequenceBeatmap();

            var mod = new SankoModTsu();
            mod.Seed.Value = 12345;
            mod.ApplyToBeatmap(first);

            var other = new SankoModTsu();
            other.Seed.Value = 12345;
            other.ApplyToBeatmap(second);

            Assert.That(typesOf(first), Is.EqualTo(typesOf(second)));
        }

        [Test]
        public void TestModDefaultsMatchDefaultConversion()
        {
            var source = createManySequenceBeatmap();
            source.BeatmapInfo.Ruleset = new RulesetInfo { OnlineID = 1 };

            var converted = (SankoBeatmap)new SankoRuleset().CreateBeatmapConverter(source).Convert();

            var modded = createManySequenceBeatmap();
            new SankoModTsu().ApplyToBeatmap(modded);

            Assert.That(typesOf(modded), Is.EqualTo(typesOf(converted)));
        }

        [Test]
        public void TestConverterAppliesTsuWhenConvertingFromAnotherRuleset()
        {
            foreach (int rulesetId in new[] { 0, 1 })
            {
                var beatmap = createManySequenceBeatmap();
                beatmap.BeatmapInfo.Ruleset = new RulesetInfo { OnlineID = rulesetId };

                var converted = (SankoBeatmap)new SankoRuleset().CreateBeatmapConverter(beatmap).Convert();

                Assert.That(typesOf(converted), Does.Contain(HitType.Tsu), $"ruleset {rulesetId}");
            }
        }

        [Test]
        public void TestConverterLeavesNativeBeatmapsAlone()
        {
            var beatmap = createManySequenceBeatmap();
            beatmap.BeatmapInfo.Ruleset = new SankoRuleset().RulesetInfo;

            var converted = (SankoBeatmap)new SankoRuleset().CreateBeatmapConverter(beatmap).Convert();

            Assert.That(typesOf(converted), Does.Not.Contain(HitType.Tsu));
        }

        [Test]
        public void TestConverterLeavesUnknownRulesetsAlone()
        {
            var beatmap = createManySequenceBeatmap();
            beatmap.BeatmapInfo.Ruleset = new RulesetInfo { OnlineID = -1 };

            var converted = (SankoBeatmap)new SankoRuleset().CreateBeatmapConverter(beatmap).Convert();

            Assert.That(typesOf(converted), Does.Not.Contain(HitType.Tsu));
        }

        [Test]
        public void TestTaikoBeatmapsAreAvailableForConversion()
        {
            var sanko = new SankoRuleset().RulesetInfo;
            var beatmap = new BeatmapInfo { Ruleset = new RulesetInfo("taiko", "osu!taiko", string.Empty, 1) };

            Assert.That(beatmap.AllowGameplayWithRuleset(sanko, true), Is.True);
            Assert.That(beatmap.AllowGameplayWithRuleset(sanko, false), Is.False);
        }

        [Test]
        public void TestOsuBeatmapsAreAvailableForConversion()
        {
            var sanko = new SankoRuleset().RulesetInfo;
            var beatmap = new BeatmapInfo { Ruleset = new RulesetInfo("osu", "osu!", string.Empty, 0) };

            Assert.That(beatmap.AllowGameplayWithRuleset(sanko, true), Is.True);
        }

        [Test]
        public void TestOtherRulesetsRemainUnavailableForConversion()
        {
            var sanko = new SankoRuleset().RulesetInfo;
            var beatmap = new BeatmapInfo { Ruleset = new RulesetInfo("fruits", "osu!catch", string.Empty, 2) };

            Assert.That(beatmap.AllowGameplayWithRuleset(sanko, true), Is.False);
        }

        private static SankoBeatmap createBeatmap(params HitType[] types)
        {
            var beatmap = new SankoBeatmap();

            for (int i = 0; i < types.Length; i++)
                beatmap.HitObjects.Add(new Hit { StartTime = i * 100, Type = types[i] });

            return beatmap;
        }

        private static SankoBeatmap createManySequenceBeatmap()
        {
            var beatmap = new SankoBeatmap();

            // Many independent same-colour sequences, so at least one conversion is overwhelmingly likely.
            for (int i = 0; i < 50; i++)
            {
                beatmap.HitObjects.Add(new Hit { StartTime = i * 1000, Type = HitType.Centre });
                beatmap.HitObjects.Add(new Hit { StartTime = i * 1000 + 100, Type = HitType.Centre });
                beatmap.HitObjects.Add(new Hit { StartTime = i * 1000 + 200, Type = HitType.Rim });
                beatmap.HitObjects.Add(new Hit { StartTime = i * 1000 + 300, Type = HitType.Rim });
            }

            return beatmap;
        }

        private static void markStrong(SankoBeatmap beatmap, params int[] indices)
        {
            var hits = beatmap.HitObjects.OfType<Hit>().ToList();

            foreach (int index in indices)
                hits[index].IsStrong = true;
        }

        private static HitType[] typesOf(SankoBeatmap beatmap) => beatmap.HitObjects.OfType<Hit>().Select(h => h.Type).ToArray();
    }
}
