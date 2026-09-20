// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Rulesets.Sanko.UI;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Sanko.Skinning.Legacy
{
    /// <summary>
    /// A component of the playfield that captures input and displays input as a drum.
    /// </summary>
    internal partial class LegacyInputDrum : Container
    {
        private LegacyHalfDrum left = null!;
        private LegacyHalfDrum right = null!;

        public LegacyInputDrum()
        {
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(ISkinSource skin)
        {
            Child = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    new Sprite
                    {
                        Texture = skin.GetTexture("taiko-bar-left")
                    },
                    left = new LegacyHalfDrum(false)
                    {
                        Name = "Left Half",
                        RelativeSizeAxes = Axes.Both,
                        RimAction = SankoAction.LeftRim,
                        CentreAction = SankoAction.LeftCentre,
                        TsuAction = SankoAction.LeftTsu
                    },
                    right = new LegacyHalfDrum(true)
                    {
                        Name = "Right Half",
                        RelativeSizeAxes = Axes.Both,
                        Origin = Anchor.TopRight,
                        Scale = new Vector2(-1, 1),
                        RimAction = SankoAction.RightRim,
                        CentreAction = SankoAction.RightCentre,
                        TsuAction = SankoAction.RightTsu
                    }
                }
            };

            // this will be used in the future for stable skin alignment. keeping here for reference.
            const float sanko_bar_y = 0;

            // stable things
            const float ratio = LegacySkin.STABLE_MAGIC_SCALE_FACTOR;

            // because the right half is flipped, we need to position using width - position to get the true "topleft" origin position
            const float negative_scale_adjust = SankoPlayfield.INPUT_DRUM_WIDTH / ratio;

            if (skin.GetConfig<SkinConfiguration.LegacySetting, decimal>(SkinConfiguration.LegacySetting.Version)?.Value >= 2.1m)
            {
                left.Centre.Position = new Vector2(0, sanko_bar_y) * ratio;
                right.Centre.Position = new Vector2(negative_scale_adjust - 56, sanko_bar_y) * ratio;
                left.Rim.Position = new Vector2(0, sanko_bar_y) * ratio;
                right.Rim.Position = new Vector2(negative_scale_adjust - 56, sanko_bar_y) * ratio;
            }
            else
            {
                left.Centre.Position = new Vector2(18, sanko_bar_y + 31) * ratio;
                right.Centre.Position = new Vector2(negative_scale_adjust - 54, sanko_bar_y + 31) * ratio;
                left.Rim.Position = new Vector2(8, sanko_bar_y + 23) * ratio;
                right.Rim.Position = new Vector2(negative_scale_adjust - 53, sanko_bar_y + 23) * ratio;
            }

            // the tsu flash reuses the outer drum texture, so it must sit exactly where the rim flash does.
            left.Tsu.Position = left.Rim.Position;
            right.Tsu.Position = right.Rim.Position;
        }

        /// <summary>
        /// A half-drum. Contains one centre and one rim hit.
        /// </summary>
        private partial class LegacyHalfDrum : Container, IKeyBindingHandler<SankoAction>
        {
            /// <summary>
            /// The key to be used for the rim of the half-drum.
            /// </summary>
            public SankoAction RimAction;

            /// <summary>
            /// The key to be used for the centre of the half-drum.
            /// </summary>
            public SankoAction CentreAction;

            /// <summary>
            /// The key to be used for the tsu of the half-drum.
            /// </summary>
            public SankoAction TsuAction;

            public readonly Sprite Rim;
            public readonly Sprite Tsu;
            public readonly Sprite Centre;

            public LegacyHalfDrum(bool flipped)
            {
                Masking = true;

                Children = new Drawable[]
                {
                    Rim = new Sprite
                    {
                        Scale = new Vector2(-1, 1),
                        Origin = flipped ? Anchor.TopLeft : Anchor.TopRight,
                        Alpha = 0,
                    },
                    Tsu = new Sprite
                    {
                        Scale = new Vector2(-1, 1),
                        Origin = flipped ? Anchor.TopLeft : Anchor.TopRight,
                        Alpha = 0,
                    },
                    Centre = new Sprite
                    {
                        Alpha = 0,
                        Origin = flipped ? Anchor.TopRight : Anchor.TopLeft,
                    }
                };
            }

            [BackgroundDependencyLoader]
            private void load(ISkinSource skin, OsuColour colours)
            {
                Rim.Texture = skin.GetTexture(@"taiko-drum-outer");
                Tsu.Texture = skin.GetTexture(@"taiko-drum-outer");
                Centre.Texture = skin.GetTexture(@"taiko-drum-inner");

                Tsu.Colour = colours.Green3;
            }

            public bool OnPressed(KeyBindingPressEvent<SankoAction> e)
            {
                Drawable? target = null;

                if (e.Action == CentreAction)
                {
                    target = Centre;
                }
                else if (e.Action == RimAction)
                {
                    target = Rim;
                }
                else if (e.Action == TsuAction)
                {
                    target = Tsu;
                }

                if (target != null)
                {
                    const float down_time = 80;
                    const float up_time = 50;

                    target
                        .FadeTo(1, down_time * (1 - target.Alpha), Easing.Out)
                        .Delay(100).FadeOut(up_time);
                }

                return false;
            }

            public void OnReleased(KeyBindingReleaseEvent<SankoAction> e)
            {
            }
        }
    }
}
