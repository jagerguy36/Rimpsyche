using System;
using System.Collections.Generic;
using Verse;

namespace Maux36.RimPsyche
{
    public class VersionManager : GameComponent
    {
        public static string lastKnownModVersion = "1.0.0";
        public static HashSet<int> DiscardedPawnThingIDnumber = [];

        //Backward compatibility 1.0.23
        public static Version sexuality_variables_implemented = new("1.0.23");
        public static bool shouldSetupSexualityVariable = false;

        public VersionManager(Game game) : base()
        {
        }

        public override void ExposeData()
        {
            base.ExposeData();
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                Log.Message($"[Rimpsyche] Last known version: {lastKnownModVersion} | current version: {Rimpsyche.currentVersion}");
                if (Rimpsyche.currentVersion != lastKnownModVersion)
                {
                    Log.Message($"[Rimpsyche] Version updated {lastKnownModVersion} -> {Rimpsyche.currentVersion}");
                    var pre_updateVersion = new Version(lastKnownModVersion);
                    lastKnownModVersion = Rimpsyche.currentVersion;
                    shouldSetupSexualityVariable = (pre_updateVersion < sexuality_variables_implemented);
                }
                DiscardedPawnThingIDnumber.Clear();
            }
            Scribe_Values.Look(ref lastKnownModVersion, "lastKnownMyModVersion", "0.0.0");
        }
    }
}
