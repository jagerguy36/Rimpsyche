using HarmonyLib;
using Verse;

namespace Maux36.RimPsyche
{
    [HarmonyPatch(typeof(Hediff_Shambler), nameof(Hediff_Shambler.PostAdd))]
    public static class Hediff_Shambler_PostAdd
    {
        public static void Postfix(Hediff_Shambler __instance, Pawn ___pawn)
        {
            var compPsyche = ___pawn.compPsyche();
            if (compPsyche != null)
            {
                compPsyche.NullifyCheck();
            }
        }
    }

    [HarmonyPatch(typeof(Hediff_Shambler), nameof(Hediff_Shambler.PostRemoved))]
    public static class Hediff_Shambler_PostRemoved
    {
        public static void Postfix(Hediff_Shambler __instance, Pawn ___pawn)
        {
            var compPsyche = ___pawn.compPsyche();
            if (compPsyche != null)
            {
                compPsyche.NullifyCheck();
            }
        }
    }
}
