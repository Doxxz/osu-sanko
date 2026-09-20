// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Localisation;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Sanko.Beatmaps;
using osu.Game.Rulesets.Sanko.Objects;

namespace osu.Game.Rulesets.Sanko.Mods
{
    public class SankoModRandom : ModRandom, IApplicableToBeatmap
    {
        public override LocalisableString Description => @"Shuffle around the colours!";
        public override Type[] IncompatibleMods => base.IncompatibleMods.Append(typeof(SankoModSwap)).ToArray();

        public void ApplyToBeatmap(IBeatmap beatmap)
        {
            var sankoBeatmap = (SankoBeatmap)beatmap;

            Seed.Value ??= RNG.Next();
            var rng = new Random((int)Seed.Value);

            foreach (var obj in sankoBeatmap.HitObjects)
            {
                if (obj is Hit hit)
                    hit.Type = (HitType)rng.Next(3);
            }
        }
    }
}
