using System;
using System.Collections.Generic;
using Verse;

namespace Maux36.RimPsyche
{
    public class RimpsycheSettings : ModSettings
    {
        public const int facetCount = 15;
        public static bool ShowSummaryInBio => !hideSummaryInBio || !usePsycheTab;
        public static bool ShowDispositionSummary => showDispositionInSummary && Rimpsyche.DispositionModuleLoaded;
        public static bool ShowDispositionTab => showDispositionInTab && Rimpsyche.DispositionModuleLoaded;
        public static bool ShowThoughtTagEffect => showThoughtTagEffects && Rimpsyche.DispositionModuleLoaded;

        public static bool usePsycheTab = false;
        public static bool hideSummaryInBio = false;
        public static bool showDispositionInSummary = true;
        public static bool showEffectInDescription = true;
        public static bool showThoughtTagEffects = false;
        public static bool showDispositionInTab = false;
        public static bool personalityAsBar = true;
        public static bool showDeatiledPreference = false;
        public static bool allowFacetEdit = false;
        public static bool showFacetInMenu = false;
        public static bool showFacetGraph = false;
        public static bool confirmLoadSave = true;
        public static HashSet<DescriptorType> DescriptorTypesToShow = new();
        private List<string> bannedDescriptors;
        public static int ExtraBioHeight => ShowDispositionSummary ? 85 : 66;

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref usePsycheTab, "usePsycheTab", false);
            Scribe_Values.Look(ref hideSummaryInBio, "hideSummaryInBio", false);
            Scribe_Values.Look(ref showDispositionInSummary, "showDispositionInSummary", true);
            Scribe_Values.Look(ref showEffectInDescription, "showEffectInDescription", true);
            Scribe_Values.Look(ref showThoughtTagEffects, "showThoughtTagEffects", false);
            Scribe_Values.Look(ref showDispositionInTab, "showDispositionInTab", false);
            Scribe_Values.Look(ref personalityAsBar, "personalityAsBar", true);
            Scribe_Values.Look(ref showDeatiledPreference, "showDeatiledPreference", false);
            Scribe_Values.Look(ref allowFacetEdit, "allowFacetEdit", false);
            Scribe_Values.Look(ref showFacetInMenu, "showFacetInMenu", false);
            Scribe_Values.Look(ref showFacetGraph, "showFacetGraph", false);
            Scribe_Values.Look(ref confirmLoadSave, "confirmLoadSave", true);
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                bannedDescriptors = new List<string>();
                foreach (DescriptorType descriptor in Enum.GetValues(typeof(DescriptorType)))
                {
                    if (!DescriptorTypesToShow.Contains(descriptor))
                    {
                        bannedDescriptors.Add(descriptor.ToString());
                    }
                }
            }
            Scribe_Collections.Look(ref bannedDescriptors, "descriptors",LookMode.Value);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                DescriptorTypesToShow = [.. (DescriptorType[])Enum.GetValues(typeof(DescriptorType))];
                if (bannedDescriptors != null)
                {
                    foreach (string s in bannedDescriptors)
                    {
                        if (Enum.TryParse<DescriptorType>(s, out var value))
                        {
                            DescriptorTypesToShow.Remove(value);
                        }
                    }
                }
                bannedDescriptors = null;
            }
        }
    }
}
