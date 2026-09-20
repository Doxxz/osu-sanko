// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Game.Audio;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sanko.Objects
{
    public class Hit : SankoStrongableHitObject, IHasDisplayColour
    {
        private HitObjectProperty<HitType> type;

        public Bindable<HitType> TypeBindable => type.Bindable;

        /// <summary>
        /// The <see cref="HitType"/> that actuates this <see cref="Hit"/>.
        /// </summary>
        public HitType Type
        {
            get => type.Value;
            set => type.Value = value;
        }

        /// <summary>
        /// The don/kat type this hit would have if it were not a tsu, derived from its underlying samples.
        /// </summary>
        /// <remarks>
        /// Tsu notes keep their underlying kat marker (a clap) so the original colour can be recovered
        /// (for example by tsu conversion mods). A tsu which came from a don has no clap marker.
        /// </remarks>
        public HitType OriginalType => getRimSamples().Any() ? HitType.Rim : HitType.Centre;

        public Bindable<Color4> DisplayColour { get; } = new Bindable<Color4>(COLOUR_CENTRE);

        public static readonly Color4 COLOUR_CENTRE = Color4Extensions.FromHex(@"bb1177");
        public static readonly Color4 COLOUR_RIM = Color4Extensions.FromHex(@"2299bb");
        public static readonly Color4 COLOUR_TSU = Color4Extensions.FromHex(@"33bb44");

        public Hit()
        {
            TypeBindable.BindValueChanged(_ =>
            {
                updateSamplesFromType();
                DisplayColour.Value = Type switch
                {
                    HitType.Rim => COLOUR_RIM,
                    HitType.Tsu => COLOUR_TSU,
                    _ => COLOUR_CENTRE,
                };
            });

            SamplesBindable.BindCollectionChanged((_, _) => updateTypeFromSamples());
        }

        /// <summary>
        /// Whether <see cref="updateSamplesFromType"/> is currently mutating <see cref="HitObject.Samples"/>.
        /// Sample mutations are applied one marker at a time, so inference is suppressed to avoid
        /// transiently reclassifying the hit type while a type change is in progress.
        /// </summary>
        private bool updatingSamples;

        private void updateTypeFromSamples()
        {
            if (updatingSamples)
                return;

            if (getTsuSamples().Any())
                Type = HitType.Tsu;
            else
                Type = getRimSamples().Any() ? HitType.Rim : HitType.Centre;
        }

        /// <summary>
        /// Returns an array of any samples which would cause this object to be a "rim" type hit.
        /// </summary>
        /// <remarks>
        /// Only a clap marks a rim note. A whistle is reserved for tsu notes (see <see cref="getTsuSamples"/>),
        /// so unlike osu!taiko it can not also mean "rim".
        /// </remarks>
        private HitSampleInfo[] getRimSamples() => Samples.Where(s => s.Name == HitSampleInfo.HIT_CLAP).ToArray();

        /// <summary>
        /// Returns an array of any samples which would cause this object to be a "tsu" type hit.
        /// </summary>
        private HitSampleInfo[] getTsuSamples() => Samples.Where(s => s.Name == HitSampleInfo.HIT_WHISTLE).ToArray();

        private void updateSamplesFromType()
        {
            updatingSamples = true;

            try
            {
                var rimSamples = getRimSamples();
                var tsuSamples = getTsuSamples();

                bool isRimType = Type == HitType.Rim;
                bool isTsuType = Type == HitType.Tsu;

                // A tsu keeps its underlying kat (clap) marker, so the original colour can be recovered later.
                if (!isTsuType && isRimType != rimSamples.Any())
                {
                    if (isRimType)
                        Samples.Add(CreateHitSampleInfo(HitSampleInfo.HIT_CLAP));
                    else
                    {
                        foreach (var sample in rimSamples)
                            Samples.Remove(sample);
                    }
                }

                if (isTsuType != tsuSamples.Any())
                {
                    if (isTsuType)
                        Samples.Add(CreateHitSampleInfo(HitSampleInfo.HIT_WHISTLE));
                    else
                    {
                        foreach (var sample in tsuSamples)
                            Samples.Remove(sample);
                    }
                }
            }
            finally
            {
                updatingSamples = false;
            }
        }

        protected override StrongNestedHitObject CreateStrongNestedHit(double startTime) => new StrongNestedHit(this)
        {
            StartTime = startTime,
            Samples = Samples
        };

        public class StrongNestedHit : StrongNestedHitObject
        {
            public StrongNestedHit(SankoHitObject parent)
                : base(parent)
            {
            }
        }
    }
}
