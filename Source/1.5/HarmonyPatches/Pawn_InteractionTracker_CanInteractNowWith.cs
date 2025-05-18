using HarmonyLib;
using RimWorld;
using Verse;

namespace Maux36.RimPsyche
{
    [HarmonyPatch(typeof(Pawn_InteractionsTracker), "CanInteractNowWith")]
    public static class Pawn_InteractionTracker_CanInteractNowWith
    {
        [HarmonyPostfix]
        public static void RimPsycheOverrideCanInteractNowWith(Pawn_InteractionsTracker __instance, ref bool __result, Pawn ___pawn, Pawn recipient)
        {
            if (__result == true)
            {
                var initiatorPsyche = ___pawn.compPsyche();
                var recipientPsyche = recipient.compPsyche();
                if (initiatorPsyche != null && recipientPsyche != null && (initiatorPsyche?.convoStartedTick > 0 || recipientPsyche.convoStartedTick > 0))
                {
                    if (initiatorPsyche.convoPartner != recipient)
                    {
                        __result = false;
                    }
                }
            }
        }
    }
}
