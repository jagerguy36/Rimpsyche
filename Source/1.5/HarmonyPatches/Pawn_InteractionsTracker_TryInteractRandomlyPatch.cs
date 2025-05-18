using HarmonyLib;
using RimWorld;
using Verse;

namespace Maux36.RimPsyche
{
    [HarmonyPatch(typeof(Pawn_InteractionsTracker), "TryInteractRandomly")]
    public static class Pawn_InteractionTracker_TryInteractRandomly
    {
        [HarmonyPrefix]
        public static bool RimPsycheOverrideTryInteractRandomly(Pawn_InteractionsTracker __instance, ref bool __result, Pawn ___pawn)
        {
            var initiatorCompPsyche = ___pawn.compPsyche();
            if (initiatorCompPsyche?.convoStartedTick > 0)
            {
                __result = false;
                return false;
            }
            return true;
        }
    }
}
