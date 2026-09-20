// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Sanko.Objects;

namespace osu.Game.Rulesets.Sanko.Beatmaps
{
    /// <summary>
    /// Converts don/kat notes into tsu notes.
    /// </summary>
    /// <remarks>
    /// Dons are never converted into kats (or vice versa); only whole notes are turned into tsus.
    /// Isolated notes, same-colour sequences (including alternating sequences) and big note sequences are each rolled separately.
    /// A unit which converts to tsus blocks the following unit from converting, so two converted units are never adjacent.
    /// </remarks>
    public static class SankoTsuConversion
    {
        /// <summary>
        /// The default chance for an isolated note to be converted to a tsu.
        /// </summary>
        public const double DEFAULT_ISOLATED_CHANCE = 0.6;

        /// <summary>
        /// The default chance for a non-isolated sequence to be converted to tsus.
        /// </summary>
        public const double DEFAULT_NON_ISOLATED_CHANCE = 0.4;

        /// <summary>
        /// The default chance for a big note sequence to be converted to tsus.
        /// </summary>
        /// <remarks>
        /// Kept exactly 5% below <see cref="DEFAULT_ISOLATED_CHANCE"/>, but spelled out literally so that the
        /// value stays exactly representable (and identical to the mod's percentage-based default).
        /// </remarks>
        public const double DEFAULT_BIG_CHANCE = 0.55;

        /// <summary>
        /// Converts don/kat notes into tsu notes.
        /// </summary>
        /// <param name="beatmap">The beatmap to convert.</param>
        /// <param name="isolatedChance">The chance (0-1) that an isolated note becomes a tsu.</param>
        /// <param name="nonIsolatedChance">The chance (0-1) that a same-colour sequence or an alternating sequence becomes tsus.</param>
        /// <param name="bigChance">The chance (0-1) that a big note sequence becomes tsus.</param>
        /// <param name="random">The random source used to roll conversions.</param>
        public static void Apply(IBeatmap beatmap, double isolatedChance, double nonIsolatedChance, double bigChance, Random random)
        {
            var segment = new List<Hit>();

            foreach (var hitObject in beatmap.HitObjects)
            {
                // Drum rolls and swells break every sequence.
                if (hitObject is Hit hit)
                {
                    segment.Add(hit);
                }
                else
                {
                    processSegment(segment, isolatedChance, nonIsolatedChance, bigChance, random);
                    segment.Clear();
                }
            }

            processSegment(segment, isolatedChance, nonIsolatedChance, bigChance, random);
        }

        /// <summary>
        /// Restores any converted tsu notes back to their underlying don/kat type.
        /// </summary>
        public static void Reset(IBeatmap beatmap)
        {
            foreach (var hit in beatmap.HitObjects.OfType<Hit>())
            {
                if (hit.Type == HitType.Tsu)
                    hit.Type = hit.OriginalType;
            }
        }

        /// <summary>
        /// Creates a random source whose seed is derived from the contents of a beatmap.
        /// </summary>
        /// <remarks>
        /// This is used whenever a random source is needed but the conversion must be reproducible, such as
        /// when no custom seed has been supplied. The seed is derived from the hit objects rather than the
        /// beatmap's identity, since the beatmap instance (and its database identity) is not stable across
        /// conversion passes.
        /// </remarks>
        public static Random CreateContentRandom(IBeatmap beatmap)
        {
            unchecked
            {
                int seed = 23;

                foreach (var hitObject in beatmap.HitObjects)
                {
                    seed = seed * 31 + hitObject.StartTime.GetHashCode();
                    seed = seed * 31 + ((hitObject as Hit)?.Type.GetHashCode() ?? 0);
                }

                return new Random(seed);
            }
        }

        private static void processSegment(List<Hit> notes, double isolatedChance, double nonIsolatedChance, double bigChance, Random random)
        {
            // Split the segment into maximal runs of consecutive, same-colour notes (big notes included).
            var runs = new List<(int start, int end)>();

            int position = 0;

            while (position < notes.Count)
            {
                int runEnd = position;

                while (runEnd + 1 < notes.Count && notes[runEnd + 1].OriginalType == notes[position].OriginalType)
                    runEnd++;

                runs.Add((position, runEnd));
                position = runEnd + 1;
            }

            // Whether the previous unit (isolated note, sequence, alternating sequence or big note sequence) converted.
            // A converted unit blocks the next unit from converting, so two converted units are never adjacent.
            bool previousConverted = false;
            int index = 0;

            while (index < runs.Count)
            {
                (int runStart, int runEnd) = runs[index];

                if (containsBigNote(notes, runStart, runEnd))
                {
                    previousConverted = tryConvertBigSequence(notes, runStart, runEnd, bigChance, previousConverted, random);
                    index++;
                }
                else if (runEnd > runStart)
                {
                    // A sequence of two or more same-colour small notes converts as a whole.
                    previousConverted = tryConvertSequence(notes, runStart, runEnd, nonIsolatedChance, previousConverted, random);
                    index++;
                }
                else
                {
                    // Gather consecutive single-note runs; together they form a strictly alternating block.
                    int blockStart = index;

                    while (index < runs.Count && runs[index].end == runs[index].start && !notes[runs[index].start].IsStrong)
                        index++;

                    int blockLength = index - blockStart;

                    if (blockLength >= 4)
                        previousConverted = tryConvertAlternatingSequence(notes, runs, blockStart, index, nonIsolatedChance, previousConverted, random);
                    else
                    {
                        for (int i = blockStart; i < index; i++)
                            previousConverted = tryConvertIsolatedNote(notes, runs[i].start, isolatedChance, previousConverted, random);
                    }
                }
            }
        }

        private static bool containsBigNote(List<Hit> notes, int start, int end)
        {
            for (int i = start; i <= end; i++)
            {
                if (notes[i].IsStrong)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Attempts to convert a big note sequence, returning whether it converted.
        /// </summary>
        private static bool tryConvertBigSequence(List<Hit> notes, int start, int end, double chance, bool previousConverted, Random random)
        {
            // A big note sequence only converts if the note immediately before it is a different colour.
            if (previousConverted || start <= 0 || notes[start - 1].OriginalType == notes[start].OriginalType || random.NextDouble() >= chance)
                return false;

            convert(notes, start, end);
            return true;
        }

        /// <summary>
        /// Attempts to convert a same-colour small note sequence, returning whether it converted.
        /// </summary>
        private static bool tryConvertSequence(List<Hit> notes, int start, int end, double chance, bool previousConverted, Random random)
        {
            if (previousConverted || random.NextDouble() >= chance)
                return false;

            convert(notes, start, end);
            return true;
        }

        /// <summary>
        /// Attempts to convert an alternating sequence, returning whether it converted.
        /// </summary>
        private static bool tryConvertAlternatingSequence(List<Hit> notes, List<(int start, int end)> runs, int blockStart, int blockEnd, double chance, bool previousConverted, Random random)
        {
            if (previousConverted || random.NextDouble() >= chance)
                return false;

            // A single colour is chosen to be converted.
            HitType convertedColour = random.Next(2) == 0 ? HitType.Centre : HitType.Rim;

            for (int i = blockStart; i < blockEnd; i++)
            {
                int noteIndex = runs[i].start;

                if (notes[noteIndex].OriginalType == convertedColour)
                    notes[noteIndex].Type = HitType.Tsu;
            }

            return true;
        }

        /// <summary>
        /// Attempts to convert a single isolated note, returning whether it converted.
        /// </summary>
        private static bool tryConvertIsolatedNote(List<Hit> notes, int noteIndex, double chance, bool previousConverted, Random random)
        {
            // A note is only isolated when it is surrounded on both sides by the opposite colour.
            if (previousConverted || noteIndex <= 0 || noteIndex >= notes.Count - 1 || random.NextDouble() >= chance)
                return false;

            notes[noteIndex].Type = HitType.Tsu;
            return true;
        }

        private static void convert(List<Hit> notes, int start, int end)
        {
            for (int i = start; i <= end; i++)
                notes[i].Type = HitType.Tsu;
        }
    }
}
