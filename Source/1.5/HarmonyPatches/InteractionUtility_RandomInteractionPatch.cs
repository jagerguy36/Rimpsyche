using HarmonyLib;
using RimWorld;
using Verse;

namespace Maux36.RimPsyche
{
    [HarmonyPatch(typeof(InteractionUtility), nameof(InteractionUtility.CanInitiateRandomInteraction))]
    public static class InteractionUtility_CanInitiateRandomInteractionPatch
    {
        [HarmonyPrefix]
        public static bool RimPsycheOverrideInitiateRandomInteraction(ref bool __result, Pawn p)
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

    [HarmonyPatch(typeof(InteractionUtility), nameof(InteractionUtility.CanReceiveRandomInteraction))]
    public static class InteractionUtility_CanReceiveRandomInteractionPatch
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
