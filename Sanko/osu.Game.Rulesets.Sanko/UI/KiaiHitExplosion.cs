// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Sanko.Objects;
using osu.Game.Rulesets.Sanko.Skinning.Default;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Sanko.UI
{
    public partial class KiaiHitExplosion : Container
    {
        public override bool RemoveWhenNotAlive => true;

        [Cached(typeof(DrawableHitObject))]
        public readonly DrawableHitObject JudgedObject;

        private readonly HitType hitType;

        private SkinnableDrawable skinnable = null!;

        public override double LifetimeStart => skinnable.Drawable.LifetimeStart;

        public override double LifetimeEnd => skinnable.Drawable.LifetimeEnd;

        public KiaiHitExplosion(DrawableHitObject judgedObject, HitType hitType)
        {
            JudgedObject = judgedObject;
            this.hitType = hitType;

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            RelativeSizeAxes = Axes.Both;
            Size = new Vector2(SankoHitObject.DEFAULT_SIZE, 1);
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Child = skinnable = new SkinnableDrawable(new SankoSkinComponentLookup(SankoSkinComponents.SankoExplosionKiai), _ => new DefaultKiaiHitExplosion(hitType));
        }
    }
}
