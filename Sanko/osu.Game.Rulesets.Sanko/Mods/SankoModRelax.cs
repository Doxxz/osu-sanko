// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Localisation;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Sanko.Objects.Drawables;

namespace osu.Game.Rulesets.Sanko.Mods
{
    public class SankoModRelax : ModRelax, IApplicableToDrawableHitObject
    {
        public override LocalisableString Description => @"No need to remember which key is correct anymore!";

        public override Type[] IncompatibleMods => base.IncompatibleMods.Concat(new[] { typeof(SankoModSingleTap) }).ToArray();

        public void ApplyToDrawableHitObject(DrawableHitObject drawable)
        {
            var allActions = Enum.GetValues<SankoAction>();

            drawable.HitObjectApplied += dho =>
            {
                switch (dho)
                {
                    case DrawableHit hit:
                        hit.HitActions = allActions;
                        break;

                    case DrawableSwell swell:
                        swell.MustAlternate = false;
                        break;
                }
            };
        }
    }
}
