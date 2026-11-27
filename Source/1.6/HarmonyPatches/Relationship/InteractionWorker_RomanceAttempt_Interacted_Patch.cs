using HarmonyLib;
using RimWorld;
using Verse;

namespace Maux36.RimPsyche
{
    [HarmonyPatch(typeof(InteractionWorker_RomanceAttempt), nameof(InteractionWorker_RomanceAttempt.Interacted))]
    public static class InteractionWorker_RomanceAttempt_Interacted_Patch
    {
        public static bool Prepare() => Rimpsyche.SexualityModuleLoaded || Rimpsyche.RelationshipModuleLoaded;
        public static void Postfix(Pawn initiator)
        {
            var initPsyche = initiator.compPsyche();
            if (initPsyche?.Enabled != true) return;
            initPsyche.Sexuality.rebuffCacheDirty = true;
        }
    }
}
