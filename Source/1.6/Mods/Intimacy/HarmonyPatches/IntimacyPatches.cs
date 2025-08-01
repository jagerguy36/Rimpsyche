using HarmonyLib;
using LoveyDoveySexWithEuterpe;
using Verse;

namespace Maux36.RimPsyche.Intimacy
{
    [HarmonyPatch(typeof(InteractionWorker_StartConversation), nameof(InteractionWorker_StartConversation.InteractionHook))]
    public class IntimacyPatches
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
}
