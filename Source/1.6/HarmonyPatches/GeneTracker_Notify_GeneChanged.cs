using HarmonyLib;
using RimWorld;
using Verse;

namespace Maux36.RimPsyche
{
    [HarmonyPatch(typeof(Pawn_GeneTracker), "Notify_GenesChanged")]
    public static class GeneTracker_Notify_GeneChanged
    {
        public static void Postfix(Pawn_GeneTracker __instance, GeneDef addedOrRemovedGene, Pawn ___pawn)
        {
            if (ModsConfig.BiotechActive && RimpsycheDatabase.GeneGateDatabase.ContainsKey(addedOrRemovedGene.shortHash))
            {
                var compPsyche = ___pawn.compPsyche();
                if (compPsyche != null)
                {
                    compPsyche.Personality.DirtyGeneCache();
                }
            }
        }
    }
}
