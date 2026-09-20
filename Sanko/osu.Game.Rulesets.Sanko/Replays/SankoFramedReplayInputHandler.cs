// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Replays;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Input.StateChanges;
using osu.Game.Replays;

namespace osu.Game.Rulesets.Sanko.Replays
{
    internal class SankoFramedReplayInputHandler : FramedReplayInputHandler<SankoReplayFrame>
    {
        public SankoFramedReplayInputHandler(Replay replay)
            : base(replay)
        {
        }

        protected override bool IsImportant(SankoReplayFrame frame) => frame.Actions.Any();

        protected override void CollectReplayInputs(List<IInput> inputs)
        {
            inputs.Add(new ReplayState<SankoAction> { PressedActions = CurrentFrame?.Actions ?? new List<SankoAction>() });
        }
    }
}
