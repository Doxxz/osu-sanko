// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Sanko.Objects;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.Sanko.Beatmaps;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.Sanko.Replays
{
    public class SankoAutoGenerator : AutoGenerator<SankoReplayFrame>
    {
        public new SankoBeatmap Beatmap => (SankoBeatmap)base.Beatmap;

        private const double swell_hit_speed = 50;

        public SankoAutoGenerator(IBeatmap beatmap)
            : base(beatmap)
        {
        }

        protected override void GenerateFrames()
        {
            if (Beatmap.HitObjects.Count == 0)
                return;

            bool hitButton = true;

            Frames.Add(new SankoReplayFrame(Beatmap.HitObjects[0].StartTime - 1000));

            for (int i = 0; i < Beatmap.HitObjects.Count; i++)
            {
                SankoHitObject h = Beatmap.HitObjects[i];
                double endTime = h.GetEndTime();

                switch (h)
                {
                    case Swell swell:
                    {
                        int d = 0;
                        int count = 0;
                        int req = swell.RequiredHits;
                        double hitRate = Math.Min(swell_hit_speed, swell.Duration / req);

                        for (double j = h.StartTime; j < endTime; j += hitRate)
                        {
                            SankoAction action;

                            switch (d)
                            {
                                default:
                                case 0:
                                    action = SankoAction.LeftCentre;
                                    break;

                                case 1:
                                    action = SankoAction.LeftRim;
                                    break;

                                case 2:
                                    action = SankoAction.RightCentre;
                                    break;

                                case 3:
                                    action = SankoAction.RightRim;
                                    break;
                            }

                            Frames.Add(new SankoReplayFrame(j, action));
                            d = (d + 1) % 4;
                            if (++count == req)
                                break;
                        }

                        break;
                    }

                    case DrumRoll drumRoll:
                    {
                        foreach (var tick in drumRoll.NestedHitObjects.OfType<DrumRollTick>())
                        {
                            Frames.Add(new SankoReplayFrame(tick.StartTime, hitButton ? SankoAction.LeftCentre : SankoAction.RightCentre));
                            hitButton = !hitButton;
                        }

                        break;
                    }

                    case Hit hit:
                    {
                        SankoAction[] actions;

                        switch (hit.Type)
                        {
                            case HitType.Rim:
                                actions = hit.IsStrong
                                    ? new[] { SankoAction.LeftRim, SankoAction.RightRim }
                                    : new[] { hitButton ? SankoAction.LeftRim : SankoAction.RightRim };
                                break;

                            case HitType.Tsu:
                                actions = hit.IsStrong
                                    ? new[] { SankoAction.LeftTsu, SankoAction.RightTsu }
                                    : new[] { hitButton ? SankoAction.LeftTsu : SankoAction.RightTsu };
                                break;

                            default:
                                actions = hit.IsStrong
                                    ? new[] { SankoAction.LeftCentre, SankoAction.RightCentre }
                                    : new[] { hitButton ? SankoAction.LeftCentre : SankoAction.RightCentre };
                                break;
                        }

                        Frames.Add(new SankoReplayFrame(h.StartTime, actions));
                        break;
                    }

                    default:
                        throw new InvalidOperationException("Unknown hit object type.");
                }

                var nextHitObject = GetNextObject(i); // Get the next object that requires pressing the same button

                bool canDelayKeyUp = nextHitObject == null || nextHitObject.StartTime > endTime + KEY_UP_DELAY;
                double calculatedDelay = canDelayKeyUp ? KEY_UP_DELAY : (nextHitObject.AsNonNull().StartTime - endTime) * 0.9;
                Frames.Add(new SankoReplayFrame(endTime + calculatedDelay));

                hitButton = !hitButton;
            }
        }
    }
}
