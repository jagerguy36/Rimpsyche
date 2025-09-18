// using HarmonyLib;
// using RimWorld;
// using Verse;

// namespace Maux36.RimPsyche
// {
//     [HarmonyPatch(typeof(InteractionWorker_DeepTalk), nameof(InteractionWorker_DeepTalk.RandomSelectionWeight))]
//     public static class InteractionWorker_DeepTalk_SelectionWeightPatch
//     {
//         [HarmonyPrefix]
//         public static bool RimPsycheOverrideDeepTalk(InteractionWorker_DeepTalk __instance, ref float __result, Pawn initiator, Pawn recipient)
//         {
//             __result = 0f;
//             return false;
//         }
//     }
// }
