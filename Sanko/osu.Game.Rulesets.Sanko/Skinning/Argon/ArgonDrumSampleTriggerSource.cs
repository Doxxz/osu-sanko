// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Game.Audio;
using osu.Game.Rulesets.Sanko.Objects;
using osu.Game.Rulesets.Sanko.UI;
using osu.Game.Rulesets.UI;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Sanko.Skinning.Argon
{
    public partial class ArgonDrumSampleTriggerSource : DrumSampleTriggerSource
    {
        [Resolved]
        private ISkinSource skinSource { get; set; } = null!;

        public ArgonDrumSampleTriggerSource(HitObjectContainer hitObjectContainer, SampleBalance balance)
            : base(hitObjectContainer, balance)
        {
        }

        public override void Play(HitType hitType, bool strong)
        {
            SankoHitObject? hitObject = GetMostValidObject() as SankoHitObject;

            if (hitObject == null)
                return;

            // Tsu has no dedicated argon samples, so it uses the base behaviour:
            // a non-strong tsu plays the whistle sample, and a strong tsu additionally plays the finish sample.
            if (hitType == HitType.Tsu)
            {
                base.Play(hitType, strong);
                return;
            }

            var originalSample = hitObject.CreateHitSampleInfo(SampleNameFor(hitType));

            // If the sample is provided by a legacy skin, we should not try and do anything special.
            if (skinSource.FindProvider(s => s.GetSample(originalSample) != null) is LegacySkinTransformer)
            {
                base.Play(hitType, strong);
                return;
            }

            // let the magic begin...
            var samplesToPlay = new List<ISampleInfo> { new VolumeAwareHitSampleInfo(originalSample, strong) };

            PlaySamples(samplesToPlay.ToArray());
        }
    }
}
