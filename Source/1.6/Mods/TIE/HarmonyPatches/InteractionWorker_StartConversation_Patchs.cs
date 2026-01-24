using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection;
using Verse;
using static TalkingIsntEverything.Settings;
using static TalkingIsntEverything.Util;

namespace Maux36.RimPsyche.TIE
{
    [HarmonyPatch(typeof(InteractionWorker_StartConversation), nameof(InteractionWorker_StartConversation.RandomSelectionWeight))]
    public class InteractionWorker_StartConversation_RandomSelectionWeight_Patch
    {
        public static void Postfix(ref float __result, Pawn initiator)
        {
            if (!allowCasual)
                return;
            if (IsMute(initiator))
                __result *= 0.5f;
        }
    }
    [HarmonyPatch]
    public static class PlayLogEntry_Interaction_Patch
    {
        public static MethodBase TargetMethod()
        {
            return AccessTools.Constructor(typeof(PlayLogEntry_InteractionConversation), new Type[] { typeof(InteractionDef), typeof(Pawn), typeof(Pawn), typeof(string), typeof(string), typeof(List<RulePackDef>) });
        }

        public static void Prefix(ref InteractionDef intDef, Pawn initiator)
        {
            if (!IsMute(initiator))
                return;
            if (intDef == DefOfRimpsyche.Rimpsyche_Conversation)
                intDef = RPC_TIE_DefOf.Rimpsyche_ConversationMuted;
            else if (intDef == DefOfRimpsyche.Rimpsyche_ConversationAttempt)
                intDef = RPC_TIE_DefOf.Rimpsyche_ConversationAttemptMuted;
            return;
        }
    }

    [HarmonyPatch(typeof(Thought_MemoryPostDefined), "get_LabelCap")]
    public static class Patch_Thought_MemoryPostDefined_LabelCap
    {
        public static bool Prefix(Pawn ___pawn, Pawn ___otherPawn, string ___topicLabel, ref string ___cachedLabelCap)
        {
            if (___cachedLabelCap == null)
            {
                if (IsMute(___pawn) || IsMute(___otherPawn))
                {
                    ___cachedLabelCap = string.Format(RPC_TIE_Utility.conversationMemoryStringMuted, ___topicLabel).CapitalizeFirst();
                }
            }
            return true;
        }
    }
}