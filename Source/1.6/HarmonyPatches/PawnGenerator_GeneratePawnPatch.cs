using HarmonyLib;
using RimWorld;
using Verse;

namespace Maux36.RimPsyche
{
    [HarmonyPatch(typeof(PawnGenerator), "GenerateNewPawnInternal")]
    public static class PawnGenerator_GeneratePawnPatch
    {
        [HarmonyPostfix]
        public static void InitializePersonality(ref Pawn __result)
        {
            var compPsyche = __result?.compPsyche();
            if (compPsyche != null)
            {
                //Log.Message($"Generated pawn: {__result.Name}'s personality");
                compPsyche.PsycheValueSetup();
                compPsyche.InterestScoreSetup();
                compPsyche.SexualitySetup();
            }
        }
    }
}
