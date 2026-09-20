// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Sanko.UI;
using osu.Game.Screens.Edit.Compose.Components;

namespace osu.Game.Rulesets.Sanko.Edit
{
    public partial class SankoBeatSnapGrid : BeatSnapGrid
    {
        protected override IEnumerable<Container> GetTargetContainers(HitObjectComposer composer) => new[]
        {
            ((SankoPlayfield)composer.Playfield).UnderlayElements
        };
    }
}
