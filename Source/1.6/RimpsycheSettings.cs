using Verse;

namespace Maux36.RimPsyche
{
    public class RimpsycheSettings : ModSettings
    {
        public const int facetCount = 15;
        public static bool showSummaryInBio = true;
        public static bool showDispositionInSummary = true;
        public static bool showEffectInDescription = false;
        public static bool showDispositionInTab = false;
        public static bool personalityAsBar = true;
        public static bool allowFacetEdit = false;
        public static bool showFacetInMenu = false;
        public static bool showFacetGraph = false;
        public static bool confirmLoadSave = true;
        public static int ExtraBioHeight = 85;

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref showSummaryInBio, "showSummaryInBio", true);
            Scribe_Values.Look(ref showDispositionInSummary, "showDispositionInSummary", true);
            Scribe_Values.Look(ref showEffectInDescription, "showEffectInDescription", false);
            Scribe_Values.Look(ref showDispositionInTab, "showDispositionInTab", false);
            Scribe_Values.Look(ref personalityAsBar, "personalityAsBar", true);
            Scribe_Values.Look(ref allowFacetEdit, "allowFacetEdit", false);
            Scribe_Values.Look(ref showFacetInMenu, "showFacetInMenu", false);
            Scribe_Values.Look(ref showFacetGraph, "showFacetGraph", false);
            Scribe_Values.Look(ref confirmLoadSave, "confirmLoadSave", true);
            ExtraBioHeight = showDispositionInSummary ? 85 : 66;
        }
    }
}
