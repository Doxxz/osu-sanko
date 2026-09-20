// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Rulesets.Sanko.Objects;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Components.Timelines.Summary;
using osu.Game.Screens.Edit.GameplayTest;
using osu.Game.Tests.Visual;
using osuTK.Input;

namespace osu.Game.Rulesets.Sanko.Tests.Editor
{
    public partial class TestSceneSankoEditorTestGameplay : EditorTestScene
    {
        protected override Ruleset CreateEditorRuleset() => new SankoRuleset();

        [Test]
        public void TestBasicGameplayTest()
        {
            AddStep("add objects", () =>
            {
                EditorBeatmap.Clear();
                EditorBeatmap.Add(new Swell { StartTime = 500, EndTime = 1500 });
                EditorBeatmap.Add(new Hit { StartTime = 3000 });
            });
            AddStep("seek to 250", () => EditorClock.Seek(250));
            AddUntilStep("wait for seek", () => EditorClock.CurrentTime, () => Is.EqualTo(250));

            AddStep("click test gameplay button", () =>
            {
                var button = Editor.ChildrenOfType<TestGameplayButton>().Single();

                InputManager.MoveMouseTo(button);
                InputManager.Click(MouseButton.Left);
            });
            AddUntilStep("save prompt shown", () => DialogOverlay.CurrentDialog is SaveRequiredPopupDialog);

            AddStep("save changes", () => DialogOverlay.CurrentDialog!.PerformOkAction());
            AddUntilStep("player pushed", () => Stack.CurrentScreen is EditorPlayer);
            AddUntilStep("wait for return to editor", () => Stack.CurrentScreen is Screens.Edit.Editor);
        }
    }
}
