using HarmonyLib;
using RimWorld;
using Verse;

namespace Maux36.RimPsyche
{
    //Only meaningful in currentSocialMode and TryInteractRandomly. For TryInteractRandomly -> InteractedTooRecentlyToInteract already captures it.
    //For currentSocialMode, it's only used for InteractionsTrackerTick, which can be later with InteractedTooRecentlyToInteract check inside TryInteractRandomly.
    //[HarmonyPatch(typeof(InteractionUtility), nameof(InteractionUtility.CanInitiateRandomInteraction))]
    //public static class InteractionUtility_CanInitiateRandomInteractionPatch
    //{
    //    [HarmonyPrefix]
    //    public static bool RimPsycheOverrideInitiateRandomInteraction(ref bool __result, Pawn p)
    //    {
    //        var compPsyche = p.compPsyche();
    //        if (compPsyche?.convoStartedTick > 0)
    //        {
    //            __result = false;
    //            return false;
    //        }
    //        return true;
    //    }
    //}

    [HarmonyPatch(typeof(SocialInteractionUtility), nameof(SocialInteractionUtility.CanReceiveRandomInteraction))]
    public static class SocialInteractionUtility_CanReceiveRandomInteractionPatch
    {
        [HarmonyPrefix]
        public static bool RimPsycheOverrideReceiveRandomInteraction(ref bool __result, Pawn p)
        {
            var compPsyche = p.compPsyche();
            if (compPsyche?.convoStartedTick > 0)
            {
                __result = false;
                return false;
            }
            return true;
        }
    }
}
