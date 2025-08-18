using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Maux36.RimPsyche
{
    [HarmonyPatch(typeof(Hediff_Inhumanized), nameof(Hediff_Inhumanized.PostAdd))]
    public static class Hediff_Inhumanized_PostAdd
    {
        public static void Postfix(Hediff_Inhumanized __instance, Pawn ___pawn)
        {
            var compPsyche = ___pawn.compPsyche();
            if (compPsyche! == null)
            {
                compPsyche.NullifyCheck();
            }
        }
    }

    [HarmonyPatch(typeof(Hediff_Inhumanized), nameof(Hediff_Inhumanized.PostRemoved))]
    public static class Hediff_Inhumanized_PostRemoved
    {
        public static void Postfix(Hediff_Inhumanized __instance, Pawn ___pawn)
        {
            var compPsyche = ___pawn.compPsyche();
            if (compPsyche! == null)
            {
                compPsyche.NullifyCheck();
            }
        }
    }
}
