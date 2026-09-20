// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Sanko.Beatmaps;

namespace osu.Game.Rulesets.Sanko.Mods
{
    /// <summary>
    /// Converts isolated notes and note sequences into tsu notes, with configurable chances.
    /// </summary>
    public class SankoModTsu : ModRandom, IApplicableToBeatmap
    {
        public override string Name => "Tsu Conversion";
        public override string Acronym => "TS";
        public override LocalisableString Description => @"Modify the chance of notes to become tsu.";
        public override Type[] IncompatibleMods => base.IncompatibleMods
                                                       .Append(typeof(SankoModTsuRandom))
                                                       .Append(typeof(SankoModRandom))
                                                       .Append(typeof(SankoModSwap))
                                                       .ToArray();

        [SettingSource("Isolated notes", "The chance for an isolated note to become a tsu, as a percentage.")]
        public BindableInt IsolatedChance { get; } = new BindableInt(60)
        {
            MinValue = 0,
            MaxValue = 100,
        };

        [SettingSource("Sequences", "The chance for a sequence of notes to become tsus, as a percentage.")]
        public BindableInt NonIsolatedChance { get; } = new BindableInt(40)
        {
            MinValue = 0,
            MaxValue = 100,
        };

        [SettingSource("Big notes", "The chance for an isolated big note or a sequence to become tsus, as a percentage.")]
        public BindableInt BigChance { get; } = new BindableInt(55)
        {
            MinValue = 0,
            MaxValue = 100,
        };

        public void ApplyToBeatmap(IBeatmap beatmap)
        {
            SankoTsuConversion.Reset(beatmap);

            // Without a custom seed, derive one from the beatmap's contents. Mods are cloned for every attempt,
            // so a randomly generated seed here would make the conversion differ on every play.
            var random = Seed.Value is int seed
                ? new Random(seed)
                : SankoTsuConversion.CreateContentRandom(beatmap);

            SankoTsuConversion.Apply(beatmap, IsolatedChance.Value / 100.0, NonIsolatedChance.Value / 100.0, BigChance.Value / 100.0, random);
        }
    }
}
