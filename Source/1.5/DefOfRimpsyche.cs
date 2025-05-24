using RimWorld;
using Verse;

namespace Maux36.RimPsyche
{
    [DefOf]
    public static class DefOfRimpsyche
    {
        public static InteractionDef Rimpsyche_StartConversation;
        public static InteractionDef Rimpsyche_Conversation;

        public static RulePackDef Sentence_ConversationFail;

        public static ThoughtDef Rimpsyche_ConvoIgnored;
        public static ThoughtDef Rimpsyche_ConversationOpinion;

        static DefOfRimpsyche()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(DefOfRimpsyche));
        }

        public static InterestDomainDef Rimpsyche_InterestDomainGeneral;
    }
}
