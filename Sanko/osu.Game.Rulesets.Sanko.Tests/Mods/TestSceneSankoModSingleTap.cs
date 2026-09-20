// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Timing;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.Sanko.Mods;
using osu.Game.Rulesets.Sanko.Objects;
using osu.Game.Rulesets.Sanko.Replays;

namespace osu.Game.Rulesets.Sanko.Tests.Mods
{
    public partial class TestSceneSankoModSingleTap : SankoModTestScene
    {
        [Test]
        public void TestInputAlternate() => CreateModTest(new ModTestData
        {
            Mod = new SankoModSingleTap(),
            Autoplay = false,
            CreateBeatmap = () => new Beatmap
            {
                BeatmapInfo = { Ruleset = new SankoRuleset().RulesetInfo },
                HitObjects = new List<HitObject>
                {
                    new Hit
                    {
                        StartTime = 100,
                        Type = HitType.Rim
                    },
                    new Hit
                    {
                        StartTime = 300,
                        Type = HitType.Rim
                    },
                    new Hit
                    {
                        StartTime = 500,
                        Type = HitType.Rim
                    },
                    new Hit
                    {
                        StartTime = 700,
                        Type = HitType.Rim
                    },
                },
            },
            ReplayFrames = new List<ReplayFrame>
            {
                new SankoReplayFrame(100, SankoAction.RightRim),
                new SankoReplayFrame(120),
                new SankoReplayFrame(300, SankoAction.LeftRim),
                new SankoReplayFrame(320),
                new SankoReplayFrame(500, SankoAction.RightRim),
                new SankoReplayFrame(520),
                new SankoReplayFrame(700, SankoAction.LeftRim),
                new SankoReplayFrame(720),
            },
            PassCondition = () => Player.ScoreProcessor.Combo.Value == 0 && Player.ScoreProcessor.HighestCombo.Value == 1
        });

        [Test]
        public void TestInputSameKey() => CreateModTest(new ModTestData
        {
            Mod = new SankoModSingleTap(),
            Autoplay = false,
            CreateBeatmap = () => new Beatmap
            {
                BeatmapInfo = { Ruleset = new SankoRuleset().RulesetInfo },
                HitObjects = new List<HitObject>
                {
                    new Hit
                    {
                        StartTime = 100,
                        Type = HitType.Rim
                    },
                    new Hit
                    {
                        StartTime = 300,
                        Type = HitType.Rim
                    },
                    new Hit
                    {
                        StartTime = 500,
                        Type = HitType.Rim
                    },
                    new Hit
                    {
                        StartTime = 700,
                        Type = HitType.Rim
                    },
                },
            },
            ReplayFrames = new List<ReplayFrame>
            {
                new SankoReplayFrame(100, SankoAction.RightRim),
                new SankoReplayFrame(120),
                new SankoReplayFrame(300, SankoAction.RightRim),
                new SankoReplayFrame(320),
                new SankoReplayFrame(500, SankoAction.RightRim),
                new SankoReplayFrame(520),
                new SankoReplayFrame(700, SankoAction.RightRim),
                new SankoReplayFrame(720),
            },
            PassCondition = () => Player.ScoreProcessor.Combo.Value == 4
        });

        [Test]
        public void TestInputIntro() => CreateModTest(new ModTestData
        {
            Mod = new SankoModSingleTap(),
            Autoplay = false,
            CreateBeatmap = () => new Beatmap
            {
                BeatmapInfo = { Ruleset = new SankoRuleset().RulesetInfo },
                HitObjects = new List<HitObject>
                {
                    new Hit
                    {
                        StartTime = 100,
                        Type = HitType.Rim
                    },
                },
            },
            ReplayFrames = new List<ReplayFrame>
            {
                new SankoReplayFrame(0, SankoAction.RightRim),
                new SankoReplayFrame(20),
                new SankoReplayFrame(100, SankoAction.LeftRim),
                new SankoReplayFrame(120),
            },
            PassCondition = () => Player.ScoreProcessor.Combo.Value == 1
        });

        [Test]
        public void TestInputStrong() => CreateModTest(new ModTestData
        {
            Mod = new SankoModSingleTap(),
            Autoplay = false,
            CreateBeatmap = () => new Beatmap
            {
                BeatmapInfo = { Ruleset = new SankoRuleset().RulesetInfo },
                HitObjects = new List<HitObject>
                {
                    new Hit
                    {
                        StartTime = 100,
                        Type = HitType.Rim
                    },
                    new Hit
                    {
                        StartTime = 300,
                        Type = HitType.Rim,
                        IsStrong = true
                    },
                    new Hit
                    {
                        StartTime = 500,
                        Type = HitType.Rim,
                    },
                },
            },
            ReplayFrames = new List<ReplayFrame>
            {
                new SankoReplayFrame(100, SankoAction.RightRim),
                new SankoReplayFrame(120),
                new SankoReplayFrame(300, SankoAction.LeftRim),
                new SankoReplayFrame(320),
                new SankoReplayFrame(500, SankoAction.LeftRim),
                new SankoReplayFrame(520),
            },
            PassCondition = () => Player.ScoreProcessor.Combo.Value == 0 && Player.ScoreProcessor.HighestCombo.Value == 2
        });

        [Test]
        public void TestInputBreaks() => CreateModTest(new ModTestData
        {
            Mod = new SankoModSingleTap(),
            Autoplay = false,
            CreateBeatmap = () => new Beatmap
            {
                BeatmapInfo = { Ruleset = new SankoRuleset().RulesetInfo },
                Breaks =
                {
                    new BreakPeriod(100, 1600),
                },
                HitObjects = new List<HitObject>
                {
                    new Hit
                    {
                        StartTime = 100,
                        Type = HitType.Rim
                    },
                    new Hit
                    {
                        StartTime = 2000,
                        Type = HitType.Rim,
                    },
                },
            },
            ReplayFrames = new List<ReplayFrame>
            {
                new SankoReplayFrame(100, SankoAction.RightRim),
                new SankoReplayFrame(120),
                // Press different key after break but before hit object.
                new SankoReplayFrame(1900, SankoAction.LeftRim),
                new SankoReplayFrame(1820),
                // Press original key at second hitobject and ensure it has been hit.
                new SankoReplayFrame(2000, SankoAction.RightRim),
                new SankoReplayFrame(2020),
            },
            PassCondition = () => Player.ScoreProcessor.Combo.Value == 2
        });
    }
}
