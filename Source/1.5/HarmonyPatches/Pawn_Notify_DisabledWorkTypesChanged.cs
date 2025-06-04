using HarmonyLib;
using Verse;

namespace Maux36.RimPsyche
{
    [HarmonyPatch(typeof(Pawn), "Notify_DisabledWorkTypesChanged")]
    public static class Pawn_Notify_DisabledWorkTypesChanged
    {
        static void Postfix(Pawn __instance)
        {
            var compPsyche = __instance.compPsyche();
            if (compPsyche != null)
            {
                compPsyche.DirtyTraitCache();
            }
        }
    }
}
