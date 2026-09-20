// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.Replays.Legacy
{
    [Flags]
    public enum ReplayButtonState
    {
        None = 0,
        Left1 = 1,
        Right1 = 2,
        Left2 = 4,
        Right2 = 8,
        Smoke = 16,

        /// <summary>
        /// Extra button bits used for ruleset-specific inputs which have no stable mouse equivalent.
        /// </summary>
        /// <remarks>
        /// osu!sanko uses these to persist tsu key presses, as the four stable bits are already taken by its
        /// don/kat inputs.
        /// </remarks>
        Left3 = 32,
        Right3 = 64
    }
}
