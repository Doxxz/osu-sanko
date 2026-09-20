// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics;
using osu.Game.Audio;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sanko.UI;
using osu.Game.Screens.Play.HUD;
using osu.Game.Screens.Play.HUD.HitErrorMeters;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Sanko.Skinning.Legacy
{
    public class SankoLegacySkinTransformer : LegacySkinTransformer
    {
        public override bool IsProvidingLegacyResources => base.IsProvidingLegacyResources || hasHitCircle || hasBarLeft;

        private readonly Lazy<bool> hasExplosion;

        private bool hasHitCircle => GetTexture("taikohitcircle") != null;
        private bool hasBarLeft => GetTexture("taiko-bar-left") != null;

        public SankoLegacySkinTransformer(ISkin skin)
            : base(skin)
        {
            hasExplosion = new Lazy<bool>(() => GetTexture(getHitName(SankoSkinComponents.SankoExplosionGreat)) != null);
        }

        public override Drawable? GetDrawableComponent(ISkinComponentLookup lookup)
        {
            switch (lookup)
            {
                case GlobalSkinnableContainerLookup containerLookup:
                {
                    // Modifications for global components.
                    if (containerLookup.Ruleset == null)
                        return base.GetDrawableComponent(lookup);

                    // we don't have enough assets to display these components (this is especially the case on a "beatmap" skin).
                    if (!IsProvidingLegacyResources)
                        return null;

                    switch (containerLookup.Lookup)
                    {
                        case GlobalSkinnableContainers.MainHUDComponents:
                            return new DefaultSkinComponentsContainer(container =>
                            {
                                var combo = container.OfType<LegacyDefaultComboCounter>().FirstOrDefault();
                                var spectatorList = container.OfType<SpectatorList>().FirstOrDefault();
                                var leaderboard = container.OfType<DrawableGameplayLeaderboard>().FirstOrDefault();
                                var hitError = container.OfType<HitErrorMeter>().FirstOrDefault();

                                Vector2 pos = new Vector2();

                                if (combo != null)
                                {
                                    combo.Anchor = Anchor.BottomLeft;
                                    combo.Origin = Anchor.BottomLeft;
                                    combo.Scale = new Vector2(1.28f);

                                    pos += new Vector2(10, -(combo.DrawHeight * 1.56f + 20) * combo.Scale.X);
                                }

                                if (leaderboard != null)
                                {
                                    leaderboard.Anchor = Anchor.BottomLeft;
                                    leaderboard.Origin = Anchor.BottomLeft;
                                    leaderboard.Position = pos;
                                    leaderboard.Height = 170;
                                    pos += new Vector2(10 + leaderboard.Width, -leaderboard.Height);
                                }

                                if (spectatorList != null)
                                {
                                    spectatorList.Anchor = Anchor.BottomLeft;
                                    spectatorList.Origin = Anchor.TopLeft;
                                    spectatorList.Position = pos;
                                }

                                if (hitError != null)
                                {
                                    hitError.Anchor = Anchor.BottomCentre;
                                    hitError.Origin = Anchor.BottomCentre;
                                }

                                foreach (var d in container.OfType<ISerialisableDrawable>())
                                    d.UsesFixedAnchor = true;
                            })
                            {
                                new LegacyDefaultComboCounter(),
                                new SpectatorList(),
                                new DrawableGameplayLeaderboard(),
                                new LegacyBarHitErrorMeter(),
                            };
                    }

                    return null;
                }

                case SkinComponentLookup<HitResult>:
                {
                    // if a sanko skin is providing explosion sprites, hide the judgements completely
                    if (hasExplosion.Value)
                        return Drawable.Empty().With(d => d.Expire());

                    break;
                }

                case SankoSkinComponentLookup sankoComponent:
                {
                    switch (sankoComponent.Component)
                    {
                        case SankoSkinComponents.DrumRollHead:
                            if (GetTexture("taiko-roll-middle") != null)
                                return new LegacyCirclePiece();

                            return null;

                        case SankoSkinComponents.DrumRollBody:
                            if (GetTexture("taiko-roll-middle") != null)
                                return new LegacyDrumRoll();

                            return null;

                        case SankoSkinComponents.InputDrum:
                            if (hasBarLeft)
                                return new LegacyInputDrum();

                            return null;

                        case SankoSkinComponents.DrumSamplePlayer:
                            return null;

                        case SankoSkinComponents.CentreHit:
                        case SankoSkinComponents.RimHit:
                            if (hasHitCircle)
                                return new LegacyHit(sankoComponent.Component);

                            return null;

                        case SankoSkinComponents.TsuHit:
                            // legacy skins have no tsu texture, so reuse the taiko hit circle textures tinted green.
                            if (hasHitCircle)
                                return new LegacyTsuHit();

                            return null;

                        case SankoSkinComponents.DrumRollTick:
                            return this.GetAnimation("sliderscorepoint", false, false);

                        case SankoSkinComponents.Swell:
                            if (GetTexture("spinner-circle") != null)
                                return new LegacySwell();

                            return null;

                        case SankoSkinComponents.HitTarget:
                            if (GetTexture("taikobigcircle") != null)
                                return new SankoLegacyHitTarget();

                            return null;

                        case SankoSkinComponents.PlayfieldBackgroundRight:
                            if (GetTexture("taiko-bar-right") != null)
                                return new SankoLegacyPlayfieldBackgroundRight();

                            return null;

                        case SankoSkinComponents.PlayfieldBackgroundLeft:
                            // This is displayed inside LegacyInputDrum. It is required to be there for layout purposes (can be seen on legacy skins).
                            if (GetTexture("taiko-bar-right") != null)
                                return Drawable.Empty();

                            return null;

                        case SankoSkinComponents.BarLine:
                            if (GetTexture("taiko-barline") != null)
                                return new LegacyBarLine();

                            return null;

                        case SankoSkinComponents.SankoExplosionMiss:
                            var missSprite = this.GetAnimation(getHitName(sankoComponent.Component), true, false);
                            if (missSprite != null)
                                return new LegacyHitExplosion(missSprite);

                            return null;

                        case SankoSkinComponents.SankoExplosionOk:
                        case SankoSkinComponents.SankoExplosionGreat:
                            string hitName = getHitName(sankoComponent.Component);
                            var hitSprite = this.GetAnimation(hitName, true, false);

                            if (hitSprite != null)
                            {
                                var strongHitSprite = this.GetAnimation($"{hitName}k", true, false);

                                return new LegacyHitExplosion(hitSprite, strongHitSprite);
                            }

                            return null;

                        case SankoSkinComponents.SankoExplosionKiai:
                            // suppress the default kiai explosion if the skin brings its own sprites.
                            // the drawable needs to expire as soon as possible to avoid accumulating empty drawables on the playfield.
                            if (hasExplosion.Value)
                                return Drawable.Empty().With(d => d.Expire());

                            return null;

                        case SankoSkinComponents.Scroller:
                            if (GetTexture("taiko-slider") != null)
                                return new LegacySankoScroller();

                            return null;

                        case SankoSkinComponents.Mascot:
                            return new DrawableSankoMascot();

                        case SankoSkinComponents.KiaiGlow:
                            if (GetTexture("taiko-glow") != null)
                                return new LegacyKiaiGlow();

                            return null;

                        default:
                            throw new UnsupportedSkinComponentException(lookup);
                    }
                }
            }

            return base.GetDrawableComponent(lookup);
        }

        private string getHitName(SankoSkinComponents component)
        {
            switch (component)
            {
                case SankoSkinComponents.SankoExplosionMiss:
                    return "taiko-hit0";

                case SankoSkinComponents.SankoExplosionOk:
                    return "taiko-hit100";

                case SankoSkinComponents.SankoExplosionGreat:
                    return "taiko-hit300";
            }

            throw new ArgumentOutOfRangeException(nameof(component), $"Invalid component type: {component}");
        }

        public override ISample? GetSample(ISampleInfo sampleInfo)
        {
            if (sampleInfo is HitSampleInfo hitSampleInfo)
                return base.GetSample(new LegacySankoSampleInfo(hitSampleInfo));

            return base.GetSample(sampleInfo);
        }

        private class LegacySankoSampleInfo : HitSampleInfo
        {
            public LegacySankoSampleInfo(HitSampleInfo sampleInfo)
                : base(sampleInfo.Name, sampleInfo.Bank, sampleInfo.Suffix, sampleInfo.Volume, sampleInfo.EditorAutoBank, sampleInfo.UseBeatmapSamples)

            {
            }

            public override IEnumerable<string> LookupNames
            {
                get
                {
                    foreach (string name in base.LookupNames)
                        yield return name.Insert(name.LastIndexOf('/') + 1, "taiko-");
                }
            }
        }
    }
}
