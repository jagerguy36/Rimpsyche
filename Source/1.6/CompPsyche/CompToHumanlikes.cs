using System.Collections.Generic;
using System;
using Verse;

namespace Maux36.RimPsyche
{
    [StaticConstructorOnStartup]
    public static class CompToHumanlikes
    {
        static CompToHumanlikes()
        {
            AddCompToHumanlikes();
        }

        public static void AddCompToHumanlikes()
        {
            foreach (var allDef in DefDatabase<ThingDef>.AllDefs)
            {
                if (allDef.race is { intelligence: Intelligence.Humanlike } && !allDef.IsCorpse)
                {
                    allDef.comps.Add(new CompProperties_Psyche());
                    PsycheCacheManager.TrackingDef.Add(allDef.defName);

                    var tabType = typeof(ITab_Pawn_Psyche);
                    var tabBase = InspectTabManager.GetSharedInstance(typeof(ITab_Pawn_Psyche));

                    //if (allDef.inspectorTabs.NullOrEmpty())
                    //{
                    //    allDef.inspectorTabs = new List<Type>(1);
                    //}
                    //if (allDef.inspectorTabsResolved.NullOrEmpty())
                    //{
                    //    allDef.inspectorTabsResolved = new List<InspectTabBase>(1);
                    //}
                    allDef.inspectorTabs?.AddDistinct(tabType);
                    allDef.inspectorTabsResolved?.AddDistinct(tabBase);

                    if (allDef.race?.corpseDef == null)
                    {
                        Log.Warning("thingDef.race?.corpseDef == null for thingDef = " + allDef.defName);
                        return;
                    }
                    //if (allDef.race.corpseDef.inspectorTabs.NullOrEmpty())
                    //{
                    //    allDef.race.corpseDef.inspectorTabs = new List<Type>(1);
                    //}
                    //if (allDef.race.corpseDef.inspectorTabsResolved.NullOrEmpty())
                    //{
                    //    allDef.race.corpseDef.inspectorTabsResolved = new List<InspectTabBase>(1);
                    //}
                    allDef.race.corpseDef.inspectorTabs?.AddDistinct(tabType);
                    allDef.race.corpseDef.inspectorTabsResolved?.AddDistinct(tabBase);
                }
            }
        }
    }
}
