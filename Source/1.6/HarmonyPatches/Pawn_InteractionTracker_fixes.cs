using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Maux36.RimPsyche
{
    //[HarmonyPatch(typeof(Pawn_InteractionsTracker), "CanInteractNowWith")]
    //public static class Pawn_InteractionTracker_CanInteractNowWith
    //{
    //    [HarmonyPostfix]
    //    public static void RimPsycheOverrideCanInteractNowWith(Pawn_InteractionsTracker __instance, ref bool __result, Pawn ___pawn, Pawn recipient)
    //    {
    //        if (__result == true)
    //        {
    //            var initiatorPsyche = ___pawn.compPsyche();
    //            var recipientPsyche = recipient.compPsyche();
    //            if (initiatorPsyche != null && recipientPsyche != null && (initiatorPsyche?.convoStartedTick > 0 || recipientPsyche.convoStartedTick > 0))
    //            {
    //                if (initiatorPsyche.convoPartner != recipient)
    //                {
    //                    __result = false;
    //                }
    //            }
    //        }
    //    }
    //}

    //InteractedTooRecentlyToInteractCheck will take care of it anyway
    //[HarmonyPatch(typeof(Pawn_InteractionsTracker), "InteractionsTrackerTick")]
    //public static class Pawn_InteractionTracker_InteractionsTrackerTick_Prefix
    //{
    //    [HarmonyPrefix]
    //    public static bool RimPsycheOverrideInteractionsTrackerTick(Pawn_InteractionsTracker __instance, Pawn ___pawn)
    //    {
    //        var compPsyche = ___pawn.compPsyche();
    //        if (compPsyche?.convoStartedTick > 0)
    //        {
    //            return false;
    //        }
    //        return true;
    //    }
    //}

    [HarmonyPatch(typeof(Pawn_InteractionsTracker), "InteractedTooRecentlyToInteract")]
    public static class Pawn_InteractionTracker_InteractedTooRecentlyToInteract
    {
        [HarmonyPrefix]
        public static bool RimPsycheOverrideInteractedTooRecentlyToInteract(Pawn_InteractionsTracker __instance, ref bool __result, Pawn ___pawn)
        {
            var compPsyche = ___pawn.compPsyche();
            if (compPsyche?.convoStartedTick > 0)
            {
                __result = true;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Pawn_InteractionsTracker), "SocialFightChance")]
    public static class Pawn_InteractionTracker_SocialFightChance
    {
        public static void Postfix(Pawn_InteractionsTracker __instance, ref float __result, Pawn ___pawn)
        {
            if (__result > 0f)
            {
                var compPsyche = ___pawn.compPsyche();
                if (compPsyche != null)
                {
                    __result = Mathf.Clamp01(__result * compPsyche.Personality.GetMultiplier(RimpsycheDatabase.SocialFightChanceMultiplier));
                }
            }
        }
    }
}
