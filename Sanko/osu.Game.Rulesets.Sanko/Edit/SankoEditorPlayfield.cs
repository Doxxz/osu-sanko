// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Sanko.UI;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Sanko.Edit
{
    public partial class SankoEditorPlayfield : SankoPlayfield
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            // This is the simplest way to extend the sanko playfield beyond the left of the drum area.
            // Required in the editor to not look weird underneath left toolbox area.
            AddInternal(new SkinnableDrawable(new SankoSkinComponentLookup(SankoSkinComponents.PlayfieldBackgroundRight), _ => new PlayfieldBackgroundRight())
            {
                Anchor = Anchor.TopLeft,
                Origin = Anchor.TopRight,
            });
        }
    }
}
