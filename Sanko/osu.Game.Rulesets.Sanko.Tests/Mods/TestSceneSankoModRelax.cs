// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Game.Rulesets.Sanko.Beatmaps;
using osu.Game.Rulesets.Sanko.Mods;
using osu.Game.Rulesets.Sanko.Objects;
using osu.Game.Rulesets.Sanko.Replays;

namespace osu.Game.Rulesets.Sanko.Tests.Mods
{
    public partial class TestSceneSankoModRelax : SankoModTestScene
    {
        [Test]
        public void TestRelax()
        {
            var beatmapForReplay = createBeatmap();

            foreach (var ho in beatmapForReplay.HitObjects)
                ho.ApplyDefaults(beatmapForReplay.ControlPointInfo, beatmapForReplay.Difficulty);

            var replay = new SankoAutoGenerator(beatmapForReplay).Generate();

            foreach (var frame in replay.Frames.OfType<SankoReplayFrame>().Where(r => r.Actions.Any()))
                frame.Actions = [SankoAction.LeftCentre];

            CreateModTest(new ModTestData
            {
                Mod = new SankoModRelax(),
                CreateBeatmap = createBeatmap,
                ReplayFrames = replay.Frames,
                Autoplay = false,
                PassCondition = () => Player.ScoreProcessor.HasCompleted.Value && Player.ScoreProcessor.Accuracy.Value == 1,
            });

            SankoBeatmap createBeatmap() => new SankoBeatmap
            {
                HitObjects =
                {
                    new Hit { StartTime = 0, Type = HitType.Centre, },
                    new Hit { StartTime = 250, Type = HitType.Rim, },
                    new DrumRoll { StartTime = 500, Duration = 500, },
                    new Swell { StartTime = 1250, Duration = 500 },
                }
            };
        }
    }
}
