using RimWorld;
using Verse;

namespace Maux36.RimPsyche.TIE
{
    public class RPC_TIE_Utility
    {
        public static InteractionDef CYB_Mute_Chitchat_Initiator_OverrideDef;
        public static InteractionDef CYB_Mute_Chitchat_Recipient_OverrideDef;
        public static string conversationMemoryStringMuted = "Nonverbal communication about {0}";

        public static void Init()
        {
            CYB_Mute_Chitchat_Initiator_OverrideDef = DefDatabase<InteractionDef>.GetNamed("CYB_Mute_Chitchat_Initiator");
            CYB_Mute_Chitchat_Initiator_OverrideDef.label = DefOfRimpsyche.Rimpsyche_Smalltalk.label;
            CYB_Mute_Chitchat_Recipient_OverrideDef = DefDatabase<InteractionDef>.GetNamed("CYB_Mute_Chitchat_Recipient");
            CYB_Mute_Chitchat_Recipient_OverrideDef.label = DefOfRimpsyche.Rimpsyche_Smalltalk.label;
            if (LanguageDatabase.activeLanguage.HaveTextForKey("MemoryReportStringMuted"))
            {
                conversationMemoryStringMuted = "MemoryReportStringMuted".Translate();
            }
        }
    }
}
