using HarmonyLib;
using System.Collections.Generic;
using Verse;
using static TalkingIsntEverything.Util;

namespace Maux36.RimPsyche.Intimacy.TIE
{
    [HarmonyPatch(typeof(InteractionWorker_StartConversation), nameof(InteractionWorker_StartConversation.Interacted))]
    public class InteractionWorker_StartConversation_Interacted_Patch
    {

        public static bool PrefixInteracted(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks, out string letterText, out string letterLabel, out LetterDef letterDef, out LookTargets lookTargets)
        {
            letterText = null;
            letterLabel = null;
            letterDef = null;
            lookTargets = null;
            bool initMute = false;
            bool reciMute = false;
            if (IsMute(initiator))
                initMute = true;
            if (IsMute(recipient))
                reciMute = true;
            if (!initMute && !reciMute)
                return true;

            var initiatorPsyche = initiator.compPsyche();
            var recipientPsyche = recipient.compPsyche();

            return false;
        }
    }
}
