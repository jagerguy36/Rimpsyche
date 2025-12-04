using System.Collections.Generic;
using Verse;

namespace Maux36.RimPsyche
{
    public class RimpsycheSettings : ModSettings
    {
        public const int facetCount = 15;
        public static bool allowFacetEdit = false;
        public static bool showFacetInMenu = false;
        public static bool showFacetGraph = false;
        public static readonly List<int> DefaultDistribution = [62, 11, 10, 6, 5, 3, 3];
        public static List<int> KinseyDistributionSetting = DefaultDistribution;
        public static bool romanceAttemptGenderDiff = true;
        public static float minRelAttraction = 0.7f;

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref allowFacetEdit, "allowFacetEdit", false);
            Scribe_Values.Look(ref showFacetInMenu, "showFacetInMenu", false);
            Scribe_Values.Look(ref showFacetGraph, "showFacetGraph", false);
            Scribe_Collections.Look(ref KinseyDistributionSetting, "KinseyDistributionSetting", LookMode.Value, DefaultDistribution);
            if (KinseyDistributionSetting == null)
            {
                KinseyDistributionSetting = DefaultDistribution;
            }
            Scribe_Values.Look(ref romanceAttemptGenderDiff, "romanceAttemptGenderDiff", true);
            Scribe_Values.Look(ref minRelAttraction, "minRelAttraction", 0.7f);

        }
    }
}
