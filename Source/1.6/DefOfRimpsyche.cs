using RimWorld;
using Verse;

namespace Maux36.RimPsyche
{
    [DefOf]
    public static class DefOfRimpsyche
    {
        public static InteractionDef Rimpsyche_Conversation;
        public static InteractionDef Rimpsyche_ReportConversation;

        public static RulePackDef Sentence_RimpsycheConversationPositiveGood;
        public static RulePackDef Sentence_RimpsycheConversationPositiveBad;
        public static RulePackDef Sentence_RimpsycheConversationNegativeGood;
        public static RulePackDef Sentence_RimpsycheConversationNegativeBad;
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
