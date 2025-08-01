using HarmonyLib;
using LoveyDoveySexWithEuterpe;
using RimWorld;
using Verse;

namespace Maux36.RimPsyche.Intimacy
{
    [HarmonyPatch(typeof(InteractionWorker_StartConversation), nameof(InteractionWorker_StartConversation.InteractionHook))]
    public class Intimacy_StartConversation_InteractionHookPatch
    {
        private static void TryGainIntimacy(Pawn pawn, float amount)
        {
            (pawn.needs?.TryGetNeed<Need_Intimacy>())?.GainIntimacySocial(amount);
        }

        public static bool Prefix(Pawn initiator, Pawn recipient, float initOpinionOffset, float reciOpinionOffset)
        {
            if (initOpinionOffset > 0)
            {
                TryGainIntimacy(initiator, 0.01f);
            }
            if (reciOpinionOffset > 0)
            {
                TryGainIntimacy(recipient, 0.01f);
            }
            return true;

        }
    }

    [HarmonyPatch(typeof(InteractionWorker), nameof(InteractionWorker.Interacted))]
    public static class InteractionWorker_Interacted_Patch
    {
        private static void TryGainIntimacy(Pawn pawn, float amount)
        {
            (pawn.needs?.TryGetNeed<Need_Intimacy>())?.GainIntimacySocial(amount);
        }

        [HarmonyPostfix]
        public static void Postfix(InteractionWorker __instance, Pawn initiator, Pawn recipient)
        {
            if (__instance != null && __instance.interaction?.defName != null && __instance.interaction.defName == "Rimpsyche_Smalltalk")
            {
                TryGainIntimacy(initiator, 0.01f);
                TryGainIntimacy(recipient, 0.01f);
            }
        }
    }

}
