using Verse;

namespace Maux36.RimPsyche
{
    public class RimpsycheSettings : ModSettings
    {
        public const int facetCount = 15;
        public static bool allowFacetEdit = false;
        public static int CalcEveryTick = 75;

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref allowFacetEdit, "allowFacetEdit", false, true);
        }
    }
}
