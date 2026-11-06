using HarmonyLib;
using RimWorld;
using Verse;

namespace Maux36.RimPsyche
{
    [HarmonyPatch(typeof(InteractionWorker_Breakup), "Interacted")]
    public static class InteractionWorker_Breakup_Interacted_Patch
    {
        //Only the Sexuality and Relationship modules raise the relationship value.
        public static bool Prepare() => Rimpsyche.SexualityModuleLoaded || Rimpsyche.RelationshipModuleLoaded;

        //A breakup lowers the relationship value to 0.05 (both sides).
        public static void Postfix(Pawn initiator, Pawn recipient)
        {
            var iniPsyche = initiator.compPsyche();
            var reciPsyche = recipient.compPsyche();
            if (iniPsyche?.Enabled != true || reciPsyche?.Enabled != true) return;
            iniPsyche.Sexuality.ClampRelationshipWith(recipient, 0.05f);
            reciPsyche.Sexuality.ClampRelationshipWith(initiator, 0.05f);
        }
    }
}
