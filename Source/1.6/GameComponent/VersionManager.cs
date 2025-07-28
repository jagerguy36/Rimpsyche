using Verse;

namespace Maux36.RimPsyche
{
    public class VersionManager : GameComponent
    {
        public static string pre_updateVersion = "";
        public static string lastKnownModVersion = "1.0.0";
        public static bool modVersionUpdated = false;

        public VersionManager(Game game) : base()
        {
            //if (Rimpsyche.currentVersion != lastKnownModVersion)
            //{
            //    modVersionUpdated = true;
            //    pre_updateVersion = lastKnownModVersion;
            //    lastKnownModVersion = Rimpsyche.currentVersion;
            //    Log.Message($"[Rimpsyche] Version updated from {pre_updateVersion} -> {lastKnownModVersion}");
            //}
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref lastKnownModVersion, "lastKnownMyModVersion", "0.0.0");
        }
    }
}
