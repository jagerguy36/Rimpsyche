using RimWorld;
using Verse;

namespace Maux36.RimPsyche
{
    [DefOf]
    public static class DefOfRimpsyche
    {
        public static InteractionDef Rimpsyche_StartConversation;
        public static InteractionDef Rimpsyche_Conversation;
        public static InteractionDef Rimpsyche_EndConversation;

        public static RulePackDef Sentence_RimpsycheConversationFail;
        public static RulePackDef Sentence_RimpsycheSocialFightConvoInitiatorStarted;
        public static RulePackDef Sentence_RimpsycheSocialFightConvoRecipientStarted;

        public static ThoughtDef Rimpsyche_ConvoIgnored;
        public static ThoughtDef Rimpsyche_ConversationOpinion;

        public static InterestDomainDef Rimpsyche_InterestDomainGeneral;

        static DefOfRimpsyche()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(DefOfRimpsyche));
        }
    }
}
