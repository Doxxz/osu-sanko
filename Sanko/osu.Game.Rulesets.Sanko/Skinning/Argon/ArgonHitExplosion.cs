// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Sanko.Objects;
using osu.Game.Rulesets.Sanko.UI;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sanko.Skinning.Argon
{
    public partial class ArgonHitExplosion : CompositeDrawable, IAnimatableHitExplosion
    {
        private readonly SankoSkinComponents component;

        private readonly Circle outer;
        private readonly Circle inner;

        public ArgonHitExplosion(SankoSkinComponents component)
        {
            this.component = component;

            RelativeSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                outer = new Circle
                {
                    Name = "Outer circle",
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                },
                inner = new Circle
                {
                    Name = "Inner circle",
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.White,
                    Size = new Vector2(0.85f),
                    Masking = true,
                },
            };
        }

        public void Animate(DrawableHitObject drawableHitObject)
        {
            this.FadeOut();

            bool isRim = (drawableHitObject.HitObject as Hit)?.Type == HitType.Rim;
            bool isTsu = (drawableHitObject.HitObject as Hit)?.Type == HitType.Tsu;

            outer.Colour = isTsu ? ArgonInputDrum.TSU_HIT_GRADIENT : isRim ? ArgonInputDrum.RIM_HIT_GRADIENT : ArgonInputDrum.CENTRE_HIT_GRADIENT;
            inner.EdgeEffect = new EdgeEffectParameters
            {
                Type = EdgeEffectType.Glow,
                Colour = (isTsu ? ArgonInputDrum.TSU_HIT_GLOW : isRim ? ArgonInputDrum.RIM_HIT_GLOW : ArgonInputDrum.CENTRE_HIT_GLOW).Opacity(0.5f),
                Radius = 45,
            };

            switch (component)
            {
                case SankoSkinComponents.SankoExplosionGreat:
                    this.FadeIn(30, Easing.In)
                        .Then()
                        .FadeOut(450, Easing.OutQuint);
                    break;

                case SankoSkinComponents.SankoExplosionOk:
                    this.FadeTo(0.2f, 30, Easing.In)
                        .Then()
                        .FadeOut(200, Easing.OutQuint);
                    break;
            }
        }

        public void AnimateSecondHit()
        {
            outer.ResizeTo(new Vector2(SankoStrongableHitObject.STRONG_SCALE), 500, Easing.OutQuint);
        }
    }
}
