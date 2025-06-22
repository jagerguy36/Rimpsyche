using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                    if (allDef.inspectorTabsResolved == null)
                    {
                        allDef.inspectorTabsResolved = new List<InspectTabBase>(1);
                    }
                    allDef.inspectorTabsResolved.AddDistinct(InspectTabManager.GetSharedInstance(typeof(ITab_Pawn_Psyche)));
                    if (allDef.race?.corpseDef == null)
                    {
                        Log.Warning("thingDef.race?.corpseDef == null for thingDef = " + allDef.defName);
                        return;
                    }
                    if (allDef.race.corpseDef.inspectorTabsResolved == null)
                    {
                        allDef.race.corpseDef.inspectorTabsResolved = new List<InspectTabBase>(1);
                    }
                    allDef.race.corpseDef.inspectorTabsResolved.AddDistinct(InspectTabManager.GetSharedInstance(typeof(ITab_Pawn_Psyche)));
                }
            }
        }
    }
}
