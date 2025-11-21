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
        public static List<int> KinseyDistributionSetting = [60, 10, 10, 5, 5, 5, 5];
        public static bool romanceAttemptGenderDiff = true;

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref allowFacetEdit, "allowFacetEdit", false);
            Scribe_Values.Look(ref showFacetInMenu, "showFacetInMenu", false);
            Scribe_Values.Look(ref showFacetGraph, "showFacetGraph", false);
            Scribe_Collections.Look(ref KinseyDistributionSetting, "KinseyDistributionSetting", LookMode.Value, [60, 10, 10, 5, 5, 5, 5]);
            if (KinseyDistributionSetting == null)
            {
                KinseyDistributionSetting = [60, 10, 10, 5, 5, 5, 5];
            }
            Scribe_Values.Look(ref romanceAttemptGenderDiff, "romanceAttemptGenderDiff", true);

        }
    }
}
