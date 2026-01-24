using RimWorld;

namespace Maux36.RimPsyche.TIE
{
    [DefOf]
    public static class RPC_TIE_DefOf
    {
        public static InteractionDef Rimpsyche_ConversationMuted;
        public static InteractionDef Rimpsyche_ConversationAttemptMuted;

        static RPC_TIE_DefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(RPC_TIE_DefOf));
        }
    }
}
