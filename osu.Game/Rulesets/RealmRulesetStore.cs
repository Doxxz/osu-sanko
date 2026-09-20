// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.Beatmaps;
using osu.Game.Database;

namespace osu.Game.Rulesets
{
    public class RealmRulesetStore : RulesetStore
    {
        private readonly RealmAccess realmAccess;
        public override IEnumerable<RulesetInfo> AvailableRulesets => availableRulesets;

        private readonly List<RulesetInfo> availableRulesets = new List<RulesetInfo>();

        public RealmRulesetStore(RealmAccess realmAccess, Storage? storage = null)
            : base(storage)
        {
            this.realmAccess = realmAccess;
            prepareDetachedRulesets();
            informUserAboutBrokenRulesets();
        }

        private void prepareDetachedRulesets()
        {
            realmAccess.Write(realm =>
            {
                List<Ruleset> instances = LoadedAssemblies.Values
                                                          .Select(r => Activator.CreateInstance(r) as Ruleset)
                                                          .Where(r => r != null)
                                                          .Select(r => r.AsNonNull())
                                                          .ToList();

                // Repair ruleset entries whose stored online ID no longer matches the ruleset they point to.
                // This can happen when a bundled custom ruleset stops claiming an official legacy ruleset ID.
                // The entry is updated in place (rather than removed) to keep any beatmap links pointing at it valid.
                foreach (var existing in realm.All<RulesetInfo>().ToList())
                {
                    Ruleset? instance = instances.FirstOrDefault(i => i.RulesetInfo.InstantiationInfo.Equals(existing.InstantiationInfo, StringComparison.Ordinal));

                    if (instance == null || instance.RulesetInfo.OnlineID == existing.OnlineID)
                        continue;

                    Logger.Log($@"Updating ruleset entry for {existing.ShortName} (stored online ID {existing.OnlineID}, current online ID {instance.RulesetInfo.OnlineID}).");
                    existing.OnlineID = instance.RulesetInfo.OnlineID;
                }

                var rulesets = realm.All<RulesetInfo>();

                // add all legacy rulesets first to ensure they have exclusive choice of primary key.
                foreach (var r in instances.Where(r => r is ILegacyRuleset))
                {
                    if (realm.All<RulesetInfo>().FirstOrDefault(rr => rr.OnlineID == r.RulesetInfo.OnlineID) == null)
                        realm.Add(new RulesetInfo(r.RulesetInfo.ShortName, r.RulesetInfo.Name, r.RulesetInfo.InstantiationInfo, r.RulesetInfo.OnlineID));
                }

                // add any other rulesets which have assemblies present but are not yet in the database.
                foreach (var r in instances.Where(r => !(r is ILegacyRuleset)))
                {
                    if (rulesets.FirstOrDefault(ri => ri.InstantiationInfo.Equals(r.RulesetInfo.InstantiationInfo, StringComparison.Ordinal)) == null)
                    {
                        var existingSameShortName = rulesets.FirstOrDefault(ri => ri.ShortName == r.RulesetInfo.ShortName);

                        if (existingSameShortName != null)
                        {
                            // even if a matching InstantiationInfo was not found, there may be an existing ruleset with the same ShortName.
                            // this generally means the user or ruleset provider has renamed their dll but the underlying ruleset is *likely* the same one.
                            // in such cases, update the instantiation info of the existing entry to point to the new one.
                            existingSameShortName.InstantiationInfo = r.RulesetInfo.InstantiationInfo;
                        }
                        else
                            realm.Add(new RulesetInfo(r.RulesetInfo.ShortName, r.RulesetInfo.Name, r.RulesetInfo.InstantiationInfo, r.RulesetInfo.OnlineID));
                    }
                }

                List<RulesetInfo> detachedRulesets = new List<RulesetInfo>();

                // perform a consistency check and detach final rulesets from realm for cross-thread runtime usage.
                foreach (var r in rulesets.OrderBy(r => r.OnlineID))
                {
                    try
                    {
                        var resolvedType = Type.GetType(r.InstantiationInfo);

                        if (resolvedType == null)
                        {
                            // ruleset DLL was probably deleted.
                            r.Available = false;
                            continue;
                        }

                        var instance = (Activator.CreateInstance(resolvedType) as Ruleset);
                        var instanceInfo = instance?.RulesetInfo
                                           ?? throw new RulesetLoadException(@"Instantiation failure");

                        if (!checkRulesetUpToDate(instance))
                        {
                            throw new ArgumentOutOfRangeException(nameof(instance.RulesetAPIVersionSupported),
                                $"Ruleset API version is too old (was {instance.RulesetAPIVersionSupported}, expected {Ruleset.CURRENT_RULESET_API_VERSION})");
                        }

                        if (r.OnlineID != instanceInfo.OnlineID)
                            throw new InvalidOperationException($@"Online ID mismatch for ruleset {r.ShortName}: database has {r.OnlineID}, constructed instance has {instanceInfo.OnlineID}");

                        if (r.OnlineID > 0 && rulesets.Any(otherRuleset => otherRuleset.ShortName != r.ShortName && otherRuleset.OnlineID == r.OnlineID))
                            throw new InvalidOperationException($@"Ruleset {r.ShortName} shares online ID {r.OnlineID} with another ruleset");

                        // If a ruleset isn't up-to-date with the API, it could cause a crash at an arbitrary point of execution.
                        // To eagerly handle cases of missing implementations, enumerate all types here and mark as non-available on throw.
                        resolvedType.Assembly.GetTypes();

                        r.Name = instanceInfo.Name;
                        r.ShortName = instanceInfo.ShortName;
                        r.InstantiationInfo = instanceInfo.InstantiationInfo;
                        r.Available = true;

                        testRulesetCompatibility(r);

                        detachedRulesets.Add(r.Clone());
                    }
                    catch (Exception ex)
                    {
                        r.Available = false;
                        LogRulesetFailure(r, ex);
                    }
                }

                // Repair beatmaps whose linked ruleset is no longer available. This can happen if a bundled
                // ruleset previously claimed a different online ID, leaving the beatmap pointing at a ruleset
                // which can no longer be instantiated. The link is re-pointed rather than requiring a database reset.
                foreach (var beatmap in realm.All<BeatmapInfo>().ToList())
                {
                    RulesetInfo? currentRuleset;

                    try
                    {
                        currentRuleset = beatmap.Ruleset;
                    }
                    catch
                    {
                        currentRuleset = null;
                    }

                    if (currentRuleset != null && currentRuleset.Available)
                        continue;

                    int previousOnlineId;

                    try
                    {
                        previousOnlineId = currentRuleset?.OnlineID ?? -1;
                    }
                    catch
                    {
                        previousOnlineId = -1;
                    }

                    RulesetInfo? replacement = rulesets.FirstOrDefault(rs => rs.Available && rs.OnlineID == previousOnlineId)
                                               ?? rulesets.FirstOrDefault(rs => rs.Available && rs.OnlineID == 0);

                    if (replacement == null)
                        continue;

                    try
                    {
                        Logger.Log($@"Repairing ruleset link for beatmap ""{beatmap.DifficultyName}"" (was online ID {previousOnlineId}, now {replacement.OnlineID}).");
                        beatmap.Ruleset = replacement;
                    }
                    catch (Exception ex)
                    {
                        Logger.Log($@"Failed to repair ruleset link for beatmap ""{beatmap.DifficultyName}"": {ex}");
                    }
                }

                availableRulesets.AddRange(detachedRulesets.Order());
            });
        }

        private bool checkRulesetUpToDate(Ruleset instance)
        {
            switch (instance.RulesetAPIVersionSupported)
            {
                // The default `virtual` implementation leaves the version string empty.
                // Consider rulesets which haven't override the version as up-to-date for now.
                // At some point (once ruleset devs add versioning), we'll probably want to disallow this for deployed builds.
                case @"":
                // Ruleset is up-to-date, all good.
                case Ruleset.CURRENT_RULESET_API_VERSION:
                    return true;

                default:
                    return false;
            }
        }

        private void testRulesetCompatibility(RulesetInfo rulesetInfo)
        {
            // do various operations to ensure that we are in a good state.
            // if we can avoid loading the ruleset at this point (rather than erroring later in runtime) then that is preferred.
            var instance = rulesetInfo.CreateInstance();

            instance.CreateAllMods();
            instance.CreateIcon();
            instance.CreateResourceStore();

            var beatmap = new Beatmap();
            var converter = instance.CreateBeatmapConverter(beatmap);

            instance.CreateBeatmapProcessor(converter.Convert());
        }

        private void informUserAboutBrokenRulesets()
        {
            if (RulesetStorage == null)
                return;

            foreach (string brokenRulesetDll in RulesetStorage.GetFiles(@".", @"*.dll.broken"))
            {
                Logger.Log($"Ruleset '{Path.GetFileNameWithoutExtension(brokenRulesetDll)}' has been disabled due to causing a crash.\n\n"
                           + "Please update the ruleset or report the issue to the developers of the ruleset if no updates are available.", level: LogLevel.Important);
            }
        }

        internal void TryDisableCustomRulesetsCausing(Exception exception)
        {
            try
            {
                var stackTrace = new StackTrace(exception);

                foreach (var frame in stackTrace.GetFrames())
                {
                    var declaringAssembly = frame.GetMethod()?.DeclaringType?.Assembly;
                    if (declaringAssembly == null)
                        continue;

                    if (UserRulesetAssemblies.Contains(declaringAssembly))
                    {
                        string sourceLocation = declaringAssembly.Location;
                        string destinationLocation = Path.ChangeExtension(sourceLocation, @".dll.broken");

                        if (File.Exists(sourceLocation))
                        {
                            Logger.Log($"Unhandled exception traced back to custom ruleset {Path.GetFileNameWithoutExtension(sourceLocation)}. Marking as broken.");
                            File.Move(sourceLocation, destinationLocation);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Attempt to trace back crash to custom ruleset failed: {ex}");
            }
        }
    }
}
