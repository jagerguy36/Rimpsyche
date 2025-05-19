using RimWorld;

namespace Maux36.RimPsyche
{
    [DefOf]
    public static class DefOfRimpsyche
    {
        public static InteractionDef Rimpsyche_StartConversation;
        public static InteractionDef Rimpsyche_Conversation;
        static DefOfRimpsyche()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(DefOfRimpsyche));
        }

        public static InterestDomainDef Rimpsyche_InterestDomainGeneral;
    }
}
