// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Tools;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Sanko.Objects;
using osu.Game.Rulesets.Sanko.UI;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Edit.Compose.Components;
using osu.Game.Utils;
using osuTK;

namespace osu.Game.Rulesets.Sanko.Edit
{
    [Cached]
    public partial class SankoHitObjectComposer : ScrollingHitObjectComposer<SankoHitObject, SankoAction>
    {
        public override Bindable<TernaryState>? SelectionNewComboState => null;

        public IBindable<TernaryState> SelectionRimState => selectionRimState;
        private readonly Bindable<TernaryState> selectionRimState = new Bindable<TernaryState>();

        public IBindable<TernaryState> SelectionTsuState => selectionTsuState;
        private readonly Bindable<TernaryState> selectionTsuState = new Bindable<TernaryState>();

        public IBindable<TernaryState> SelectionStrongState => selectionStrongState;
        private readonly Bindable<TernaryState> selectionStrongState = new Bindable<TernaryState>();

        protected override bool ApplyHorizontalCentering => false;

        private Bindable<bool> limitPlacementToCurrentTime = null!;

        public SankoHitObjectComposer(SankoRuleset ruleset)
            : base(ruleset)
        {
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            limitPlacementToCurrentTime = config.GetBindable<bool>(OsuSetting.EditorLimitedDistanceSnap);
            setUpStateBindables();
        }

        public override SnapResult FindSnappedPositionAndTime(Vector2 screenSpacePosition)
        {
            if (limitPlacementToCurrentTime.Value
                && BlueprintContainer.CurrentHitObjectPlacement?.PlacementActive == PlacementBlueprint.PlacementState.Waiting)
            {
                var playfield = (SankoPlayfield)Playfield;
                double time = BeatSnapProvider.SnapTime(EditorClock.CurrentTime);
                return new SnapResult(playfield.ScreenSpacePositionAtTime(time), time, playfield);
            }

            return base.FindSnappedPositionAndTime(screenSpacePosition);
        }

        protected override IReadOnlyList<CompositionTool<SankoAction>> CompositionTools => new CompositionTool<SankoAction>[]
        {
            new HitCompositionTool(),
            new DrumRollCompositionTool(),
            new SwellCompositionTool()
        };

        protected override DrawableRuleset<SankoHitObject> CreateDrawableRuleset(Ruleset ruleset, IBeatmap beatmap, IReadOnlyList<Mod> mods) =>
            new DrawableSankoEditorRuleset(ruleset, beatmap, mods);

        protected override ComposeBlueprintContainer CreateBlueprintContainer()
            => new SankoBlueprintContainer(this);

        protected override BeatSnapGrid CreateBeatSnapGrid() => new SankoBeatSnapGrid();

        #region Selection handling

        protected override void UpdateTernaryStates()
        {
            base.UpdateTernaryStates();

            selectionRimState.Value = EditorBeatmap.SelectedHitObjects.OfType<Hit>().GetTernaryState(h => h.Type == HitType.Rim);
            selectionTsuState.Value = EditorBeatmap.SelectedHitObjects.OfType<Hit>().GetTernaryState(h => h.Type == HitType.Tsu);
            selectionStrongState.Value = EditorBeatmap.SelectedHitObjects.OfType<SankoStrongableHitObject>().GetTernaryState(h => h.IsStrong);
        }

        private void setUpStateBindables()
        {
            selectionStrongState.ValueChanged += state =>
            {
                switch (state.NewValue)
                {
                    case TernaryState.False:
                        SetStrongState(false);
                        break;

                    case TernaryState.True:
                        SetStrongState(true);
                        break;
                }
            };

            selectionRimState.ValueChanged += state =>
            {
                switch (state.NewValue)
                {
                    case TernaryState.False:
                        SetRimState(false);
                        break;

                    case TernaryState.True:
                        SetRimState(true);
                        break;
                }
            };

            selectionTsuState.ValueChanged += state =>
            {
                switch (state.NewValue)
                {
                    case TernaryState.False:
                        SetTsuState(false);
                        break;

                    case TernaryState.True:
                        SetTsuState(true);
                        break;
                }
            };
        }

        public void SetStrongState(bool state)
        {
            if (EditorBeatmap.SelectedHitObjects.OfType<SankoStrongableHitObject>().All(h => h.IsStrong == state))
                return;

            EditorBeatmap.PerformOnSelection(h =>
            {
                if (h is not SankoStrongableHitObject strongable) return;

                if (strongable.IsStrong != state)
                    strongable.IsStrong = state;
            });
        }

        public void SetRimState(bool state)
        {
            if (state)
            {
                if (EditorBeatmap.SelectedHitObjects.OfType<Hit>().All(h => h.Type == HitType.Rim))
                    return;

                EditorBeatmap.PerformOnSelection(h =>
                {
                    if (h is Hit sankoHit)
                        sankoHit.Type = HitType.Rim;
                });
            }
            else
            {
                if (EditorBeatmap.SelectedHitObjects.OfType<Hit>().All(h => h.Type != HitType.Rim))
                    return;

                EditorBeatmap.PerformOnSelection(h =>
                {
                    if (h is Hit sankoHit && sankoHit.Type == HitType.Rim)
                        sankoHit.Type = HitType.Centre;
                });
            }
        }

        public void SetTsuState(bool state)
        {
            if (state)
            {
                if (EditorBeatmap.SelectedHitObjects.OfType<Hit>().All(h => h.Type == HitType.Tsu))
                    return;

                EditorBeatmap.PerformOnSelection(h =>
                {
                    if (h is Hit sankoHit)
                        sankoHit.Type = HitType.Tsu;
                });
            }
            else
            {
                if (EditorBeatmap.SelectedHitObjects.OfType<Hit>().All(h => h.Type != HitType.Tsu))
                    return;

                EditorBeatmap.PerformOnSelection(h =>
                {
                    if (h is Hit sankoHit && sankoHit.Type == HitType.Tsu)
                        sankoHit.Type = HitType.Centre;
                });
            }
        }

        #endregion
    }
}
