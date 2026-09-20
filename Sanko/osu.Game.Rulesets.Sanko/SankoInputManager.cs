// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Input.Bindings;
using osu.Framework.Localisation;
using osu.Game.Localisation.Taiko;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Sanko
{
    [Cached] // Used for touch input, see DrumTouchInputArea.
    public partial class SankoInputManager : RulesetInputManager<SankoAction>
    {
        public SankoInputManager(RulesetInfo ruleset)
            : base(ruleset, 0, SimultaneousBindingMode.Unique)
        {
        }
    }

    public enum SankoAction
    {
        [LocalisableDescription(typeof(SankoActionStrings), nameof(SankoActionStrings.LeftGreen))]
        LeftTsu,

        [LocalisableDescription(typeof(SankoActionStrings), nameof(SankoActionStrings.LeftBlue))]
        LeftRim,

        [LocalisableDescription(typeof(SankoActionStrings), nameof(SankoActionStrings.LeftRed))]
        LeftCentre,

        [LocalisableDescription(typeof(SankoActionStrings), nameof(SankoActionStrings.RightRed))]
        RightCentre,

        [LocalisableDescription(typeof(SankoActionStrings), nameof(SankoActionStrings.RightBlue))]
        RightRim,

        [LocalisableDescription(typeof(SankoActionStrings), nameof(SankoActionStrings.RightGreen))]
        RightTsu,

        [LocalisableDescription(typeof(TaikoEditorStrings), nameof(TaikoEditorStrings.HitTool))]
        EditorHitTool = 10000,

        [LocalisableDescription(typeof(TaikoEditorStrings), nameof(TaikoEditorStrings.DrumRollTool))]
        EditorDrumRollTool,

        [LocalisableDescription(typeof(TaikoEditorStrings), nameof(TaikoEditorStrings.SwellTool))]
        EditorSwellTool,
    }

    /// <summary>
    /// Localisation for the sanko-specific actions which have no equivalent in the base game.
    /// </summary>
    /// <remarks>
    /// Sanko actions are named after the note colours they play: don is red, kat is blue and tsu is green.
    /// </remarks>
    public static class SankoActionStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.Sanko.Action";

        /// <summary>
        /// "Left (green)"
        /// </summary>
        public static LocalisableString LeftGreen => new TranslatableString(getKey(@"left_green"), @"Left (green)");

        /// <summary>
        /// "Left (blue)"
        /// </summary>
        public static LocalisableString LeftBlue => new TranslatableString(getKey(@"left_blue"), @"Left (blue)");

        /// <summary>
        /// "Left (red)"
        /// </summary>
        public static LocalisableString LeftRed => new TranslatableString(getKey(@"left_red"), @"Left (red)");

        /// <summary>
        /// "Right (red)"
        /// </summary>
        public static LocalisableString RightRed => new TranslatableString(getKey(@"right_red"), @"Right (red)");

        /// <summary>
        /// "Right (blue)"
        /// </summary>
        public static LocalisableString RightBlue => new TranslatableString(getKey(@"right_blue"), @"Right (blue)");

        /// <summary>
        /// "Right (green)"
        /// </summary>
        public static LocalisableString RightGreen => new TranslatableString(getKey(@"right_green"), @"Right (green)");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
