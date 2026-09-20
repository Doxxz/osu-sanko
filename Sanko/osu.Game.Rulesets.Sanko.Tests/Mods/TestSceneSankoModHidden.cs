// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Configuration;
using osu.Game.Rulesets.Sanko.Mods;
using osu.Game.Rulesets.Sanko.Objects;
using osu.Game.Rulesets.Sanko.Objects.Drawables;

namespace osu.Game.Rulesets.Sanko.Tests.Mods
{
    public partial class TestSceneSankoModHidden : SankoModTestScene
    {
        private Func<bool> checkAllMaxResultJudgements(int count) => ()
            => Player.ScoreProcessor.JudgedHits >= count
               && Player.Results.All(result => result.Type == result.Judgement.MaxResult);

        [Test]
        public void TestDefaultBeatmapTest() => CreateModTest(new ModTestData
        {
            Mod = new SankoModHidden(),
            Autoplay = true,
            PassCondition = checkAllMaxResultJudgements(4),
        });

        [Test]
        public void TestHitTwoNotesWithinShortPeriod()
        {
            const double hit_time = 1;

            CreateModTest(new ModTestData
            {
                Mod = new SankoModHidden(),
                Autoplay = true,
                PassCondition = checkAllMaxResultJudgements(2),
                CreateBeatmap = () =>
                {
                    var beatmap = new Beatmap<SankoHitObject>
                    {
                        HitObjects = new List<SankoHitObject>
                        {
                            new Hit
                            {
                                Type = HitType.Rim,
                                StartTime = hit_time,
                            },
                            new Hit
                            {
                                Type = HitType.Centre,
                                StartTime = hit_time * 2,
                            },
                        },
                        BeatmapInfo =
                        {
                            Difficulty = new BeatmapDifficulty
                            {
                                SliderTickRate = 4,
                                OverallDifficulty = 0,
                            },
                            Ruleset = new SankoRuleset().RulesetInfo
                        },
                    };

                    beatmap.ControlPointInfo.Add(0, new EffectControlPoint { ScrollSpeed = 0.1f });
                    return beatmap;
                },
            });
        }

        [Test]
        public void TestIncreasedVisibilityOnFirstObject()
        {
            bool firstHitNeverFadedOut = true;
            AddStep("enable increased visibility", () => LocalConfig.SetValue(OsuSetting.IncreaseFirstObjectVisibility, true));
            CreateModTest(new ModTestData
            {
                Mod = new SankoModHidden(),
                Autoplay = true,
                PassCondition = () =>
                {
                    var firstHit = this.ChildrenOfType<DrawableHit>().FirstOrDefault(h => h.HitObject.StartTime == 100);

                    if (firstHit?.Alpha < 1 && !firstHit.IsHit)
                        firstHitNeverFadedOut = false;

                    return firstHitNeverFadedOut && checkAllMaxResultJudgements(2).Invoke();
                },
                CreateBeatmap = () =>
                {
                    var beatmap = new Beatmap<SankoHitObject>
                    {
                        HitObjects = new List<SankoHitObject>
                        {
                            new Hit
                            {
                                Type = HitType.Rim,
                                StartTime = 100,
                            },
                            new Hit
                            {
                                Type = HitType.Centre,
                                StartTime = 200,
                            },
                        },
                        BeatmapInfo =
                        {
                            Difficulty = new BeatmapDifficulty
                            {
                                SliderTickRate = 4,
                                OverallDifficulty = 0,
                            },
                            Ruleset = new SankoRuleset().RulesetInfo
                        },
                    };

                    beatmap.ControlPointInfo.Add(0, new EffectControlPoint { ScrollSpeed = 0.1f });
                    return beatmap;
                },
            });
        }

        [Test]
        public void TestNoIncreasedVisibilityOnFirstObject()
        {
            bool firstHitFadedOut = true;
            AddStep("enable increased visibility", () => LocalConfig.SetValue(OsuSetting.IncreaseFirstObjectVisibility, false));
            CreateModTest(new ModTestData
            {
                Mod = new SankoModHidden(),
                Autoplay = true,
                PassCondition = () =>
                {
                    var firstHit = this.ChildrenOfType<DrawableHit>().FirstOrDefault(h => h.HitObject.StartTime == 100);
                    firstHitFadedOut |= firstHit?.IsHit == false && firstHit.Alpha < 1;
                    return firstHitFadedOut && checkAllMaxResultJudgements(2).Invoke();
                },
                CreateBeatmap = () =>
                {
                    var beatmap = new Beatmap<SankoHitObject>
                    {
                        HitObjects = new List<SankoHitObject>
                        {
                            new Hit
                            {
                                Type = HitType.Rim,
                                StartTime = 100,
                            },
                            new Hit
                            {
                                Type = HitType.Centre,
                                StartTime = 200,
                            },
                        },
                        BeatmapInfo =
                        {
                            Difficulty = new BeatmapDifficulty
                            {
                                SliderTickRate = 4,
                                OverallDifficulty = 0,
                            },
                            Ruleset = new SankoRuleset().RulesetInfo
                        },
                    };

                    beatmap.ControlPointInfo.Add(0, new EffectControlPoint { ScrollSpeed = 0.1f });
                    return beatmap;
                },
            });
        }
    }
}
