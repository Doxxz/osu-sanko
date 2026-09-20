// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Sanko.Objects;
using osu.Game.Rulesets.Sanko.Objects.Drawables;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Play;

namespace osu.Game.Rulesets.Sanko.UI
{
    internal partial class DrumSamplePlayer : CompositeDrawable, IKeyBindingHandler<SankoAction>
    {
        private DrumSampleTriggerSource leftCentreTrigger = null!;
        private DrumSampleTriggerSource rightCentreTrigger = null!;
        private DrumSampleTriggerSource leftRimTrigger = null!;
        private DrumSampleTriggerSource rightRimTrigger = null!;
        private DrumSampleTriggerSource strongCentreTrigger = null!;
        private DrumSampleTriggerSource strongRimTrigger = null!;
        private DrumSampleTriggerSource leftTsuTrigger = null!;
        private DrumSampleTriggerSource rightTsuTrigger = null!;
        private DrumSampleTriggerSource strongTsuTrigger = null!;

        private double lastHitTime;
        private SankoAction? lastAction;

        [BackgroundDependencyLoader]
        private void load(Playfield playfield)
        {
            var hitObjectContainer = playfield.HitObjectContainer;
            InternalChildren = new Drawable[]
            {
                leftCentreTrigger = CreateTriggerSource(hitObjectContainer, SampleBalance.Left),
                rightCentreTrigger = CreateTriggerSource(hitObjectContainer, SampleBalance.Right),
                leftRimTrigger = CreateTriggerSource(hitObjectContainer, SampleBalance.Left),
                rightRimTrigger = CreateTriggerSource(hitObjectContainer, SampleBalance.Right),
                strongCentreTrigger = CreateTriggerSource(hitObjectContainer, SampleBalance.Centre),
                strongRimTrigger = CreateTriggerSource(hitObjectContainer, SampleBalance.Centre),
                leftTsuTrigger = CreateTriggerSource(hitObjectContainer, SampleBalance.Left),
                rightTsuTrigger = CreateTriggerSource(hitObjectContainer, SampleBalance.Right),
                strongTsuTrigger = CreateTriggerSource(hitObjectContainer, SampleBalance.Centre)
            };
        }

        protected virtual DrumSampleTriggerSource CreateTriggerSource(HitObjectContainer hitObjectContainer, SampleBalance balance)
            => new DrumSampleTriggerSource(hitObjectContainer);

        public bool OnPressed(KeyBindingPressEvent<SankoAction> e)
        {
            if ((Clock as IGameplayClock)?.IsRewinding == true)
                return false;

            HitType hitType;

            DrumSampleTriggerSource triggerSource;

            bool strong = checkStrongValidity(e.Action, lastAction, Time.Current - lastHitTime);

            switch (e.Action)
            {
                case SankoAction.LeftCentre:
                    hitType = HitType.Centre;
                    triggerSource = strong ? strongCentreTrigger : leftCentreTrigger;
                    break;

                case SankoAction.RightCentre:
                    hitType = HitType.Centre;
                    triggerSource = strong ? strongCentreTrigger : rightCentreTrigger;
                    break;

                case SankoAction.LeftRim:
                    hitType = HitType.Rim;
                    triggerSource = strong ? strongRimTrigger : leftRimTrigger;
                    break;

                case SankoAction.RightRim:
                    hitType = HitType.Rim;
                    triggerSource = strong ? strongRimTrigger : rightRimTrigger;
                    break;

                case SankoAction.LeftTsu:
                    hitType = HitType.Tsu;
                    triggerSource = strong ? strongTsuTrigger : leftTsuTrigger;
                    break;

                case SankoAction.RightTsu:
                    hitType = HitType.Tsu;
                    triggerSource = strong ? strongTsuTrigger : rightTsuTrigger;
                    break;

                default:
                    return false;
            }

            if (strong)
            {
                switch (hitType)
                {
                    case HitType.Centre:
                        flushCenterTriggerSources();
                        break;

                    case HitType.Rim:
                        flushRimTriggerSources();
                        break;

                    case HitType.Tsu:
                        flushTsuTriggerSources();
                        break;
                }
            }

            Play(triggerSource, hitType, strong);

            lastHitTime = Time.Current;
            lastAction = e.Action;

            return false;
        }

        protected virtual void Play(DrumSampleTriggerSource triggerSource, HitType hitType, bool strong) =>
            triggerSource.Play(hitType, strong);

        private bool checkStrongValidity(SankoAction newAction, SankoAction? lastAction, double timeBetweenActions)
        {
            if (lastAction == null)
                return false;

            if (timeBetweenActions < 0 || timeBetweenActions > DrawableHit.StrongNestedHit.SECOND_HIT_WINDOW)
                return false;

            switch (newAction)
            {
                case SankoAction.LeftCentre:
                    return lastAction == SankoAction.RightCentre;

                case SankoAction.RightCentre:
                    return lastAction == SankoAction.LeftCentre;

                case SankoAction.LeftRim:
                    return lastAction == SankoAction.RightRim;

                case SankoAction.RightRim:
                    return lastAction == SankoAction.LeftRim;

                case SankoAction.LeftTsu:
                    return lastAction == SankoAction.RightTsu;

                case SankoAction.RightTsu:
                    return lastAction == SankoAction.LeftTsu;

                default:
                    return false;
            }
        }

        private void flushCenterTriggerSources()
        {
            leftCentreTrigger.StopAllPlayback();
            rightCentreTrigger.StopAllPlayback();
            strongCentreTrigger.StopAllPlayback();
        }

        private void flushRimTriggerSources()
        {
            leftRimTrigger.StopAllPlayback();
            rightRimTrigger.StopAllPlayback();
            strongRimTrigger.StopAllPlayback();
        }

        private void flushTsuTriggerSources()
        {
            leftTsuTrigger.StopAllPlayback();
            rightTsuTrigger.StopAllPlayback();
            strongTsuTrigger.StopAllPlayback();
        }

        public void OnReleased(KeyBindingReleaseEvent<SankoAction> e)
        {
        }
    }
}
