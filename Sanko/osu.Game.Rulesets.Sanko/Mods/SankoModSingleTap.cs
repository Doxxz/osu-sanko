// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;
using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps.Timing;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Sanko.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Play;
using osu.Game.Utils;
using osu.Game.Rulesets.Sanko.UI;

namespace osu.Game.Rulesets.Sanko.Mods
{
    public partial class SankoModSingleTap : Mod, IApplicableToDrawableRuleset<SankoHitObject>, IUpdatableByPlayfield
    {
        public override string Name => @"Single Tap";
        public override string Acronym => @"SG";
        public override IconUsage? Icon => OsuIcon.ModSingleTap;
        public override LocalisableString Description => @"One key for dons, one key for kats, one key for tsu.";

        public override bool Ranked => true;
        public override Type[] IncompatibleMods => new[] { typeof(ModAutoplay), typeof(ModRelax), typeof(SankoModCinema) };
        public override ModType Type => ModType.Conversion;

        private DrawableSankoRuleset ruleset = null!;

        private SankoPlayfield playfield { get; set; } = null!;

        private SankoAction? lastAcceptedCentreAction { get; set; }
        private SankoAction? lastAcceptedRimAction { get; set; }
        private SankoAction? lastAcceptedTsuAction { get; set; }

        /// <summary>
        /// A tracker for periods where single tap should not be enforced (i.e. non-gameplay periods).
        /// </summary>
        /// <remarks>
        /// This is different from <see cref="Player.IsBreakTime"/> in that the periods here end strictly at the first object after the break, rather than the break's end time.
        /// </remarks>
        private PeriodTracker nonGameplayPeriods = null!;

        private IFrameStableClock gameplayClock = null!;

        public void ApplyToDrawableRuleset(DrawableRuleset<SankoHitObject> drawableRuleset)
        {
            ruleset = (DrawableSankoRuleset)drawableRuleset;
            ruleset.KeyBindingInputManager.Add(new InputInterceptor(this));
            playfield = (SankoPlayfield)ruleset.Playfield;

            var periods = new List<Period>();

            if (drawableRuleset.Objects.Any())
            {
                periods.Add(new Period(int.MinValue, getValidJudgementTime(ruleset.Objects.First()) - 1));

                foreach (BreakPeriod b in drawableRuleset.Beatmap.Breaks)
                    periods.Add(new Period(b.StartTime, getValidJudgementTime(ruleset.Objects.First(h => h.StartTime >= b.EndTime)) - 1));

                static double getValidJudgementTime(HitObject hitObject) => hitObject.StartTime - hitObject.HitWindows.WindowFor(HitResult.Ok);
            }

            nonGameplayPeriods = new PeriodTracker(periods);

            gameplayClock = drawableRuleset.FrameStableClock;
        }

        public void Update(Playfield playfield)
        {
            if (!nonGameplayPeriods.IsInAny(gameplayClock.CurrentTime)) return;

            lastAcceptedCentreAction = null;
            lastAcceptedRimAction = null;
            lastAcceptedTsuAction = null;
        }

        private bool checkCorrectAction(SankoAction action)
        {
            if (nonGameplayPeriods.IsInAny(gameplayClock.CurrentTime))
                return true;

            // If next hit object is strong, allow usage of all actions. Strong drumrolls are ignored in this check.
            if (playfield.HitObjectContainer.AliveObjects.FirstOrDefault(h => h.Result?.HasResult != true)?.HitObject is SankoStrongableHitObject hitObject
                && hitObject.IsStrong
                && hitObject is not DrumRoll)
                return true;

            if ((action == SankoAction.LeftCentre || action == SankoAction.RightCentre)
                && (lastAcceptedCentreAction == null || lastAcceptedCentreAction == action))
            {
                lastAcceptedCentreAction = action;
                return true;
            }

            if ((action == SankoAction.LeftRim || action == SankoAction.RightRim)
                && (lastAcceptedRimAction == null || lastAcceptedRimAction == action))
            {
                lastAcceptedRimAction = action;
                return true;
            }

            if ((action == SankoAction.LeftTsu || action == SankoAction.RightTsu)
                && (lastAcceptedTsuAction == null || lastAcceptedTsuAction == action))
            {
                lastAcceptedTsuAction = action;
                return true;
            }

            return false;
        }

        private partial class InputInterceptor : Component, IKeyBindingHandler<SankoAction>
        {
            private readonly SankoModSingleTap mod;

            public InputInterceptor(SankoModSingleTap mod)
            {
                this.mod = mod;
            }

            public bool OnPressed(KeyBindingPressEvent<SankoAction> e)
                // if the pressed action is incorrect, block it from reaching gameplay.
                => !mod.checkCorrectAction(e.Action);

            public void OnReleased(KeyBindingReleaseEvent<SankoAction> e)
            {
            }
        }
    }
}
