using Verse;

namespace Maux36.RimPsyche
{
    public class RimpsycheSettings : ModSettings
    {
        public const int facetCount = 15;
        public static bool allowFacetEdit = false;
        public static bool showFacetInMenu = false;
        public static bool showFacetGraph = false;
        public static float[] KinseyDistributionSetting = [60f, 10f, 10f, 5f, 5f, 5f, 5f];

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref allowFacetEdit, "allowFacetEdit", false);
            Scribe_Values.Look(ref showFacetInMenu, "showFacetInMenu", false);
            Scribe_Values.Look(ref showFacetGraph, "showFacetGraph", false);
            Scribe_Collections.Look(ref KinseyDistributionSetting, "KinseyDistributionSetting", LookMode.Value);
        }
    }
}
