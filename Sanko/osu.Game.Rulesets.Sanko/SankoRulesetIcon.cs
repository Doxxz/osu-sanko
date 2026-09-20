// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;

namespace osu.Game.Rulesets.Sanko
{
    /// <summary>
    /// The osu!sanko ruleset icon, loaded from the ruleset's own resources.
    /// </summary>
    /// <remarks>
    /// Ruleset icons are usually looked up from the shared osu! icon font, but that font only contains the four
    /// official rulesets. Sanko ships its own texture instead and draws it like any other sprite, so it is tinted
    /// and scaled by whatever UI it is placed in.
    /// </remarks>
    public partial class SankoRulesetIcon : Sprite
    {
        private readonly Ruleset ruleset;

        public SankoRulesetIcon(Ruleset ruleset)
        {
            this.ruleset = ruleset;
        }

        [BackgroundDependencyLoader]
        private void load(IRenderer renderer)
        {
            Texture = new TextureStore(renderer, new TextureLoaderStore(ruleset.CreateResourceStore()), false).Get("Textures/RulesetIcon");
        }
    }
}
