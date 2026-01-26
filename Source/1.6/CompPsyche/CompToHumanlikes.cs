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
            var allThingDefs = DefDatabase<ThingDef>.AllDefsListForReading;
            for (int i = 0; i < allThingDefs.Count; i++)
            {
                var allDef = allThingDefs[i];
                if (allDef.race is { intelligence: Intelligence.Humanlike } && !allDef.IsCorpse)
                {
                    if (RimpsycheDatabase.MindlessDefShorthashSet.Contains(allDef.shortHash))
                        continue;
                    allDef.comps.Add(new CompProperties_Psyche());
                    PsycheCacheManager.TrackingDefHash.Add(allDef.shortHash);

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
