// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Sanko.Objects;
using osu.Game.Rulesets.Sanko.Objects.Drawables;
using osu.Game.Rulesets.Sanko.UI;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Sanko.Mods
{
    public class SankoModClassic : ModClassic, IApplicableToDrawableRuleset<SankoHitObject>, IApplicableToDrawableHitObject
    {
        public void ApplyToDrawableRuleset(DrawableRuleset<SankoHitObject> drawableRuleset)
        {
            var drawableSankoRuleset = (DrawableSankoRuleset)drawableRuleset;
            drawableSankoRuleset.LockPlayfieldAspectRange.Value = false;
        }

        public void ApplyToDrawableHitObject(DrawableHitObject drawable)
        {
            if (drawable is DrawableSankoHitObject hit)
                hit.SnapJudgementLocation = true;
        }
    }
}
