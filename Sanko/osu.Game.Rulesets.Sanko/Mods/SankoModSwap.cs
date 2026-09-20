// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Sanko.Beatmaps;
using osu.Game.Rulesets.Sanko.Objects;

namespace osu.Game.Rulesets.Sanko.Mods
{
    public class SankoModSwap : Mod, IApplicableToBeatmap
    {
        public override string Name => "Swap";
        public override string Acronym => "SW";
        public override LocalisableString Description => @"Dons become kats, kats become tsu, tsu become dons";
        public override IconUsage? Icon => OsuIcon.ModSwap;
        public override ModType Type => ModType.Conversion;
        public override Type[] IncompatibleMods => base.IncompatibleMods.Append(typeof(ModRandom)).ToArray();
        public override bool Ranked => true;

        public void ApplyToBeatmap(IBeatmap beatmap)
        {
            var sankoBeatmap = (SankoBeatmap)beatmap;

            foreach (var obj in sankoBeatmap.HitObjects)
            {
                if (obj is Hit hit)
                    hit.Type = hit.Type switch
                    {
                        HitType.Centre => HitType.Rim,
                        HitType.Rim => HitType.Tsu,
                        HitType.Tsu => HitType.Centre,
                        _ => hit.Type,
                    };
            }
        }
    }
}
