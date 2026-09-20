// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Sanko.Objects;
using osuTK;

namespace osu.Game.Rulesets.Sanko.Skinning.Default
{
    public partial class TsuHitCirclePiece : CirclePiece
    {
        public TsuHitCirclePiece()
        {
            Add(new TsuHitSymbolPiece());
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AccentColour = Hit.COLOUR_TSU;
        }

        /// <summary>
        /// The symbol used for tsu hit pieces.
        /// </summary>
        public partial class TsuHitSymbolPiece : Container
        {
            public TsuHitSymbolPiece()
            {
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;

                RelativeSizeAxes = Axes.Both;
                Size = new Vector2(SYMBOL_SIZE);
                Padding = new MarginPadding(SYMBOL_BORDER);

                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Rotation = 45,
                    }
                };
            }
        }
    }
}
