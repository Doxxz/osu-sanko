// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Sanko.Objects;

namespace osu.Game.Rulesets.Sanko.Tests
{
    public class TsuHitTest
    {
        [Test]
        public void TestDefaultTypeIsCentre()
        {
            Assert.That(new Hit().Type, Is.EqualTo(HitType.Centre));
        }

        [Test]
        public void TestColourIsGreen()
        {
            Assert.That(new Hit { Type = HitType.Tsu }.DisplayColour.Value, Is.EqualTo(Hit.COLOUR_TSU));
        }

        [Test]
        public void TestTsuIsMarkedWithWhistle()
        {
            var hit = new Hit { Type = HitType.Tsu };

            Assert.That(hit.Samples.Any(s => s.Name == HitSampleInfo.HIT_WHISTLE), Is.True);
            Assert.That(hit.Samples.Any(s => s.Name == HitSampleInfo.HIT_CLAP), Is.False);
        }

        [Test]
        public void TestRimIsMarkedWithClap()
        {
            var hit = new Hit { Type = HitType.Rim };

            Assert.That(hit.Samples.Any(s => s.Name == HitSampleInfo.HIT_CLAP), Is.True);
            Assert.That(hit.Samples.Any(s => s.Name == HitSampleInfo.HIT_WHISTLE), Is.False);
        }

        [Test]
        public void TestTsuKeepsItsUnderlyingKatMarker()
        {
            var hit = new Hit { Type = HitType.Rim };
            hit.Type = HitType.Tsu;

            Assert.That(hit.Type, Is.EqualTo(HitType.Tsu));
            Assert.That(hit.OriginalType, Is.EqualTo(HitType.Rim));
        }

        [Test]
        public void TestTsuFromDonHasNoUnderlyingMarker()
        {
            Assert.That(new Hit { Type = HitType.Tsu }.OriginalType, Is.EqualTo(HitType.Centre));
        }

        [Test]
        public void TestTsuRoundTripsThroughSamples()
        {
            var hit = new Hit { Type = HitType.Tsu };

            // Simulate a beatmap reload: a fresh hit built from the serialised samples.
            var reloaded = new Hit();

            foreach (var sample in hit.Samples)
                reloaded.Samples.Add(sample);

            Assert.That(reloaded.Type, Is.EqualTo(HitType.Tsu));
        }

        [Test]
        public void TestChangingTypeRemovesTsuSample()
        {
            var hit = new Hit { Type = HitType.Tsu };
            hit.Type = HitType.Rim;

            Assert.That(hit.Type, Is.EqualTo(HitType.Rim));
            Assert.That(hit.Samples.Any(s => s.Name == HitSampleInfo.HIT_WHISTLE), Is.False);
        }

        [Test]
        public void TestWhistleEncodedSourceKatConvertsToKat()
        {
            // osu! encodes a kat with either a clap or a whistle, but sanko reserves the whistle for tsu,
            // so a source kat authored with a whistle must still convert to a kat.
            var converted = convertObjectWithSamples(new HitSampleInfo(HitSampleInfo.HIT_WHISTLE));

            Assert.That(converted.Type, Is.EqualTo(HitType.Rim));
            Assert.That(converted.Samples.Any(s => s.Name == HitSampleInfo.HIT_CLAP), Is.True);
            Assert.That(converted.Samples.Any(s => s.Name == HitSampleInfo.HIT_WHISTLE), Is.False);
        }

        [Test]
        public void TestClapEncodedSourceKatConvertsToKat()
        {
            Assert.That(convertObjectWithSamples(new HitSampleInfo(HitSampleInfo.HIT_CLAP)).Type, Is.EqualTo(HitType.Rim));
        }

        [Test]
        public void TestUnmarkedSourceNoteConvertsToDon()
        {
            Assert.That(convertObjectWithSamples().Type, Is.EqualTo(HitType.Centre));
        }

        [Test]
        public void TestWhistleToggleMakesNoteTsu()
        {
            // The editor's sample toggles work by adding samples, so a whistle toggled onto a note must make it a tsu.
            var hit = new Hit();
            hit.Samples.Add(new HitSampleInfo(HitSampleInfo.HIT_WHISTLE));

            Assert.That(hit.Type, Is.EqualTo(HitType.Tsu));
        }

        [Test]
        public void TestClapToggleMakesNoteKat()
        {
            var hit = new Hit();
            hit.Samples.Add(new HitSampleInfo(HitSampleInfo.HIT_CLAP));

            Assert.That(hit.Type, Is.EqualTo(HitType.Rim));
        }

        [Test]
        public void TestWhistleToggleRemovedRestoresUnderlyingColour()
        {
            var hit = new Hit();
            hit.Samples.Add(new HitSampleInfo(HitSampleInfo.HIT_CLAP));
            hit.Samples.Add(new HitSampleInfo(HitSampleInfo.HIT_WHISTLE));

            Assert.That(hit.Type, Is.EqualTo(HitType.Tsu));

            hit.SamplesBindable.RemoveAll(s => s.Name == HitSampleInfo.HIT_WHISTLE);

            Assert.That(hit.Type, Is.EqualTo(HitType.Rim));
        }

        private static Hit convertObjectWithSamples(params HitSampleInfo[] samples)
        {
            var cpi = new ControlPointInfo();
            cpi.Add(0, new TimingControlPoint { BeatLength = 500 });

            var beatmap = new Beatmap
            {
                ControlPointInfo = cpi,
                Difficulty = new BeatmapDifficulty(),
                BeatmapInfo = { Ruleset = new RulesetInfo("taiko", "osu!taiko", string.Empty, 1) },
            };

            var hitObject = new HitObject { StartTime = 0 };

            foreach (var sample in samples)
                hitObject.Samples.Add(sample);

            beatmap.HitObjects.Add(hitObject);

            return (Hit)new SankoRuleset().CreateBeatmapConverter(beatmap).Convert().HitObjects.Single();
        }
    }
}
