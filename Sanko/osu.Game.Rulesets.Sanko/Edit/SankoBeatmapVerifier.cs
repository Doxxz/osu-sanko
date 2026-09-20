// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Checks;
using osu.Game.Rulesets.Edit.Checks.Components;
using osu.Game.Rulesets.Sanko.Edit.Checks;

namespace osu.Game.Rulesets.Sanko.Edit
{
    public class SankoBeatmapVerifier : IBeatmapVerifier
    {
        private readonly List<ICheck> checks = new List<ICheck>
        {
            // Compose
            new CheckConcurrentObjects(),

            // Spread
            new CheckSankoLowestDiffDrainTime(),

            // Settings
            new CheckSankoAbnormalDifficultySettings(),

            // Timing
            new CheckSankoInconsistentSkipBarLine(),
        };

        public IEnumerable<Issue> Run(BeatmapVerifierContext context)
        {
            return checks.SelectMany(check => check.Run(context));
        }
    }
}
