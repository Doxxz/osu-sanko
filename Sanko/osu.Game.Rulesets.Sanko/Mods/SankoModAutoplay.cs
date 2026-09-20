// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Sanko.Replays;

namespace osu.Game.Rulesets.Sanko.Mods
{
    public class SankoModAutoplay : ModAutoplay
    {
        public override ModReplayData CreateReplayData(IBeatmap beatmap, IReadOnlyList<Mod> mods)
            => new ModReplayData(new SankoAutoGenerator(beatmap).Generate(), new ModCreatedUser { Username = "mekkadosu!" });

        public override Type[] IncompatibleMods => base.IncompatibleMods.Concat(new[] { typeof(SankoModSingleTap) }).ToArray();
    }
}
