// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Sanko.Edit.Blueprints;
using osu.Game.Screens.Edit.Compose.Components;
using osuTK;

namespace osu.Game.Rulesets.Sanko.Edit
{
    public partial class SankoBlueprintContainer : ComposeBlueprintContainer
    {
        public new SankoHitObjectComposer Composer => (SankoHitObjectComposer)base.Composer;

        public SankoBlueprintContainer(SankoHitObjectComposer composer)
            : base(composer)
        {
        }

        protected override SelectionHandler<HitObject> CreateSelectionHandler() => new SankoSelectionHandler();

        public override HitObjectSelectionBlueprint CreateHitObjectBlueprintFor(HitObject hitObject) =>
            new SankoSelectionBlueprint(hitObject);

        protected override bool TryMoveBlueprints(DragEvent e, IList<(SelectionBlueprint<HitObject> blueprint, Vector2[] originalSnapPositions)> blueprints)
        {
            Vector2 distanceTravelled = e.ScreenSpaceMousePosition - e.ScreenSpaceMouseDownPosition;

            // The final movement position, relative to movementBlueprintOriginalPosition.
            Vector2 movePosition = blueprints.First().originalSnapPositions.First() + distanceTravelled;

            // Retrieve a snapped position.
            var result = Composer.FindSnappedPositionAndTime(movePosition);

            var referenceBlueprint = blueprints.First().blueprint;
            bool moved = SelectionHandler.HandleMovement(new MoveSelectionEvent<HitObject>(referenceBlueprint, result.ScreenSpacePosition - referenceBlueprint.ScreenSpaceSelectionPoint));
            if (moved)
                ApplySnapResultTime(result, referenceBlueprint.Item.StartTime);
            return moved;
        }
    }
}
