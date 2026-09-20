// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Localisation;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Sanko.Beatmaps;
using osu.Game.Rulesets.Sanko.Objects;

namespace osu.Game.Rulesets.Sanko.Mods
{
    /// <summary>
    /// Randomly turns notes into tsu notes, without ever changing a don into a kat (or vice versa).
    /// </summary>
    public class SankoModTsuRandom : ModRandom, IApplicableToBeatmap
    {
        public override string Name => "Random Tsu";
        public override string Acronym => "RT";
        public override LocalisableString Description => @"Randomly turn notes into tsu, without changing dons into kats, or kats into dons.";
        public override Type[] IncompatibleMods => base.IncompatibleMods
                                                       .Append(typeof(SankoModTsu))
                                                       .Append(typeof(SankoModRandom))
                                                       .Append(typeof(SankoModSwap))
                                                       .ToArray();

        [SettingSource("Chance", "The chance for each note to become a tsu, as a percentage.")]
        public BindableInt Chance { get; } = new BindableInt(50)
        {
            MinValue = 0,
            MaxValue = 100,
        };

        public void ApplyToBeatmap(IBeatmap beatmap)
        {
            Seed.Value ??= RNG.Next();
            var random = new Random((int)Seed.Value);

            SankoTsuConversion.Reset(beatmap);

            double chance = Chance.Value / 100.0;

            foreach (var hit in beatmap.HitObjects.OfType<Hit>())
            {
                if (random.NextDouble() < chance)
                    hit.Type = HitType.Tsu;
            }
        }
    }
}
