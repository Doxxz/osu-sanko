// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Sanko.UI;
using osuTK;

namespace osu.Game.Rulesets.Sanko.Tests.Skinning
{
    [TestFixture]
    public partial class TestSceneInputDrum : SankoSkinnableTestScene
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            var playfield = new SankoPlayfield();

            var beatmap = CreateWorkingBeatmap(new SankoRuleset().RulesetInfo).GetPlayableBeatmap(new SankoRuleset().RulesetInfo);

            foreach (var h in beatmap.HitObjects)
                playfield.Add(h);

            SetContents(_ => new SankoInputManager(new SankoRuleset().RulesetInfo)
            {
                RelativeSizeAxes = Axes.Both,
                Child = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(180f, 200f),
                    Child = new InputDrum
                    {
                        RelativeSizeAxes = Axes.Both,
                    }
                }
            });
        }
    }
}
