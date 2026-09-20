// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Play.HUD;
using osu.Game.Screens.Play.HUD.HitErrorMeters;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Sanko.Skinning.Argon
{
    public class SankoArgonSkinTransformer : SkinTransformer
    {
        public SankoArgonSkinTransformer(ISkin skin)
            : base(skin)
        {
        }

        public override Drawable? GetDrawableComponent(ISkinComponentLookup lookup)
        {
            switch (lookup)
            {
                case GlobalSkinnableContainerLookup containerLookup:
                    // Only handle per ruleset defaults here.
                    if (containerLookup.Ruleset == null)
                        return base.GetDrawableComponent(lookup);

                    switch (containerLookup.Lookup)
                    {
                        case GlobalSkinnableContainers.MainHUDComponents:
                            return new DefaultSkinComponentsContainer(container =>
                            {
                                var leaderboard = container.OfType<DrawableGameplayLeaderboard>().FirstOrDefault();
                                var comboCounter = container.OfType<ArgonComboCounter>().FirstOrDefault();
                                var spectatorList = container.OfType<SpectatorList>().FirstOrDefault();
                                var hitError = container.OfType<HitErrorMeter>().FirstOrDefault();
                                var hitError2 = container.OfType<HitErrorMeter>().LastOrDefault();

                                if (leaderboard != null)
                                {
                                    leaderboard.Anchor = leaderboard.Origin = Anchor.BottomLeft;
                                    leaderboard.Position = new Vector2(36, -140);
                                    leaderboard.Height = 140;
                                }

                                comboCounter?.Position = new Vector2(36, -66);

                                if (spectatorList != null)
                                {
                                    spectatorList.Position = new Vector2(320, -280);
                                    spectatorList.Anchor = Anchor.BottomLeft;
                                    spectatorList.Origin = Anchor.TopLeft;
                                }

                                if (hitError != null)
                                {
                                    hitError.Anchor = Anchor.CentreLeft;
                                    hitError.Origin = Anchor.CentreLeft;
                                }

                                if (hitError2 != null)
                                {
                                    hitError2.Anchor = Anchor.CentreRight;
                                    hitError2.Scale = new Vector2(-1, 1);
                                    // origin flipped to match scale above.
                                    hitError2.Origin = Anchor.CentreLeft;
                                }

                                foreach (var d in container.OfType<ISerialisableDrawable>())
                                    d.UsesFixedAnchor = true;
                            })
                            {
                                RelativeSizeAxes = Axes.Both,
                                Children = new Drawable[]
                                {
                                    new DrawableGameplayLeaderboard(),
                                    new ArgonComboCounter
                                    {
                                        Anchor = Anchor.BottomLeft,
                                        Origin = Anchor.BottomLeft,
                                        Scale = new Vector2(1.3f),
                                    },
                                    new SpectatorList
                                    {
                                        Anchor = Anchor.BottomLeft,
                                        Origin = Anchor.BottomLeft,
                                    },
                                    new BarHitErrorMeter(),
                                    new BarHitErrorMeter(),
                                },
                            };
                    }

                    return null;

                case SkinComponentLookup<HitResult> resultComponent:
                    // This should eventually be moved to a skin setting, when supported.
                    if (Skin is ArgonProSkin && resultComponent.Component >= HitResult.Great)
                        return Drawable.Empty();

                    return new ArgonJudgementPiece(resultComponent.Component);

                case SankoSkinComponentLookup sankoComponent:
                    // TODO: Once everything is finalised, consider throwing UnsupportedSkinComponentException on missing entries.
                    switch (sankoComponent.Component)
                    {
                        case SankoSkinComponents.CentreHit:
                            return new ArgonCentreCirclePiece();

                        case SankoSkinComponents.RimHit:
                            return new ArgonRimCirclePiece();

                        case SankoSkinComponents.TsuHit:
                            return new ArgonTsuCirclePiece();

                        case SankoSkinComponents.PlayfieldBackgroundLeft:
                            return new ArgonPlayfieldBackgroundLeft();

                        case SankoSkinComponents.PlayfieldBackgroundRight:
                            return new ArgonPlayfieldBackgroundRight();

                        case SankoSkinComponents.InputDrum:
                            return new ArgonInputDrum();

                        case SankoSkinComponents.HitTarget:
                            return new ArgonHitTarget();

                        case SankoSkinComponents.BarLine:
                            return new ArgonBarLine();

                        case SankoSkinComponents.DrumRollBody:
                            return new ArgonElongatedCirclePiece();

                        case SankoSkinComponents.DrumRollTick:
                            return new ArgonTickPiece();

                        case SankoSkinComponents.SankoExplosionKiai:
                            // the drawable needs to expire as soon as possible to avoid accumulating empty drawables on the playfield.
                            return Drawable.Empty().With(d => d.Expire());

                        case SankoSkinComponents.DrumSamplePlayer:
                            return new ArgonDrumSamplePlayer();

                        case SankoSkinComponents.SankoExplosionGreat:
                        case SankoSkinComponents.SankoExplosionMiss:
                        case SankoSkinComponents.SankoExplosionOk:
                            return new ArgonHitExplosion(sankoComponent.Component);

                        case SankoSkinComponents.Swell:
                            return new ArgonSwell();
                    }

                    break;
            }

            return base.GetDrawableComponent(lookup);
        }
    }
}
