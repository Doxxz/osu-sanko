// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Rulesets.Sanko.Objects;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Sanko.Skinning.Legacy
{
    /// <summary>
    /// A tsu hit circle which reuses the skin's taiko hit circle textures (and overlays), tinted green.
    /// </summary>
    /// <remarks>
    /// Legacy skins have no dedicated tsu texture, so the don/kat textures are recoloured instead.
    /// </remarks>
    public partial class LegacyTsuHit : LegacyCirclePiece
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            AccentColour = LegacyColourCompatibility.DisallowZeroAlpha(Hit.COLOUR_TSU);
        }
    }
}
