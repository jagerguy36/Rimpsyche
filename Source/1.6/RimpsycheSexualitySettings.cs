using System.Collections.Generic;
using Verse;

namespace Maux36.RimPsyche
{
    public class RimpsycheSexualitySettings : ModSettings
    {
        public static readonly List<int> DefaultDistribution = [62, 11, 10, 6, 5, 3, 3];
        public static List<int> KinseyDistributionSetting = [.. DefaultDistribution];
        public static bool usePreferenceSystem = false;
        public static bool romanceAttemptGenderDiff = true;
        public static float minRelAttraction = 0.7f;

        static RimpsycheSexualitySettings()
        {
            KinseyDistributionSetting = [.. DefaultDistribution];
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Collections.Look(ref KinseyDistributionSetting, "KinseyDistributionSetting", LookMode.Value);
            Scribe_Values.Look(ref usePreferenceSystem, "usePreferenceSystem", true);
            Scribe_Values.Look(ref romanceAttemptGenderDiff, "romanceAttemptGenderDiff", false);
            Scribe_Values.Look(ref minRelAttraction, "minRelAttraction", 0.7f);

        }
    }
}
