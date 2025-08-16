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

                    allDef.inspectorTabs?.AddDistinct(tabType);
                    allDef.inspectorTabsResolved?.AddDistinct(tabBase);

                    if (allDef.race?.corpseDef == null)
                    {
                        Log.Warning("[Rimpsyche] thingDef.race?.corpseDef == null for thingDef = " + allDef.defName);
                        continue;
                    }

                    allDef.race.corpseDef.inspectorTabs?.AddDistinct(tabType);
                    allDef.race.corpseDef.inspectorTabsResolved?.AddDistinct(tabBase);
                }
            }
        }
    }
}
