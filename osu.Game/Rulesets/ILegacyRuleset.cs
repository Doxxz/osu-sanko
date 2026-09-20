// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Scoring.Legacy;

namespace osu.Game.Rulesets
{
    public interface ILegacyRuleset
    {
        /// <summary>
        /// The ruleset ID reserved for the bundled osu!sanko ruleset.
        /// </summary>
        /// <remarks>
        /// Sanko is an osu!taiko clone, so it reuses taiko's legacy (stable) beatmap and score encoding.
        /// It cannot reuse taiko's ID (1) as that would collide with the real osu!taiko ruleset,
        /// so it is assigned the first ID past the official range.
        /// </remarks>
        const int SANKO_RULESET_ID = 4;

        /// <summary>
        /// The highest ruleset ID considered to use the legacy (stable) format.
        /// </summary>
        /// <remarks>
        /// The four official rulesets occupy IDs 0-3. The range is extended so that a bundled
        /// custom ruleset clone (osu!sanko) can reuse legacy score/beatmap encoding without claiming
        /// an official ruleset's ID.
        /// </remarks>
        const int MAX_LEGACY_RULESET_ID = SANKO_RULESET_ID;

        /// <summary>
        /// Identifies the server-side ID of a legacy ruleset.
        /// </summary>
        int LegacyID { get; }

        ILegacyScoreSimulator CreateLegacyScoreSimulator();
    }
}
