// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Replays.Legacy;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.Replays.Types;

namespace osu.Game.Rulesets.Sanko.Replays
{
    public class SankoReplayFrame : ReplayFrame, IConvertibleReplayFrame
    {
        public List<SankoAction> Actions = new List<SankoAction>();

        public SankoReplayFrame()
        {
        }

        public SankoReplayFrame(double time, params SankoAction[] actions)
            : base(time)
        {
            Actions.AddRange(actions);
        }

        public void FromLegacy(LegacyReplayFrame currentFrame, IBeatmap beatmap, ReplayFrame? lastFrame = null)
        {
            if (currentFrame.MouseRight1) Actions.Add(SankoAction.LeftRim);
            if (currentFrame.MouseRight2) Actions.Add(SankoAction.RightRim);
            if (currentFrame.MouseLeft1) Actions.Add(SankoAction.LeftCentre);
            if (currentFrame.MouseLeft2) Actions.Add(SankoAction.RightCentre);

            // tsu has no stable mouse equivalent, so it is stored in the extra button bits.
            if (currentFrame.ButtonState.HasFlag(ReplayButtonState.Left3)) Actions.Add(SankoAction.LeftTsu);
            if (currentFrame.ButtonState.HasFlag(ReplayButtonState.Right3)) Actions.Add(SankoAction.RightTsu);
        }

        public LegacyReplayFrame ToLegacy(IBeatmap beatmap)
        {
            ReplayButtonState state = ReplayButtonState.None;

            if (Actions.Contains(SankoAction.LeftRim)) state |= ReplayButtonState.Right1;
            if (Actions.Contains(SankoAction.RightRim)) state |= ReplayButtonState.Right2;
            if (Actions.Contains(SankoAction.LeftCentre)) state |= ReplayButtonState.Left1;
            if (Actions.Contains(SankoAction.RightCentre)) state |= ReplayButtonState.Left2;

            // tsu has no stable mouse equivalent, so it is stored in the extra button bits.
            if (Actions.Contains(SankoAction.LeftTsu)) state |= ReplayButtonState.Left3;
            if (Actions.Contains(SankoAction.RightTsu)) state |= ReplayButtonState.Right3;

            return new LegacyReplayFrame(Time, null, null, state);
        }

        public override bool IsEquivalentTo(ReplayFrame other)
            => other is SankoReplayFrame sankoFrame && Time == sankoFrame.Time && Actions.SequenceEqual(sankoFrame.Actions);
    }
}
