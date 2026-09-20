// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Sanko.Objects;
using osu.Game.Rulesets.Sanko.UI;
using osuTK;
using osuTK.Input;

namespace osu.Game.Rulesets.Sanko.Edit.Blueprints
{
    public partial class HitPlacementBlueprint : HitObjectPlacementBlueprint
    {
        private readonly HitPiece piece;

        public new Hit HitObject => (Hit)base.HitObject;

        [Resolved]
        private SankoHitObjectComposer? composer { get; set; }

        public HitPlacementBlueprint()
            : base(new Hit())
        {
            Child = piece = new HitPiece
            {
                Size = new Vector2(SankoHitObject.DEFAULT_SIZE * SankoPlayfield.BASE_HEIGHT)
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            BeginPlacement();
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            if (e.Button != MouseButton.Left)
                return false;

            EndPlacement(true);
            return true;
        }

        public override SnapResult UpdateTimeAndPosition(Vector2 screenSpacePosition, double fallbackTime)
        {
            var result = composer?.FindSnappedPositionAndTime(screenSpacePosition) ?? new SnapResult(screenSpacePosition, fallbackTime);
            piece.Position = ToLocalSpace(result.ScreenSpacePosition);
            base.UpdateTimeAndPosition(result.ScreenSpacePosition, result.Time ?? fallbackTime);
            return result;
        }
    }
}
