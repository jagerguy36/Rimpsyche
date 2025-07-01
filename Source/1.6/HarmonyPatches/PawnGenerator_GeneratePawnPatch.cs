//using HarmonyLib;
//using Verse;

//namespace Maux36.RimPsyche
//{
//    [HarmonyPatch(typeof(PawnGenerator), "GenerateNewPawnInternal")]
//    public static class PawnGenerator_GeneratePawnPatch
//    {
//        [HarmonyPostfix]
//        public static void InitializePersonality(ref Pawn __result)
//        {
//            var compPsyche = __result?.compPsyche();
//            if (compPsyche != null)
//            {
//                Log.Message($"{__result.Name} post genrerating");
//                compPsyche.PostGen = true;
//                compPsyche.PsycheValueSetup();
//                compPsyche.InterestScoreSetup();
//                compPsyche.SexualitySetup();
//            }
//        }
//    }
//}
