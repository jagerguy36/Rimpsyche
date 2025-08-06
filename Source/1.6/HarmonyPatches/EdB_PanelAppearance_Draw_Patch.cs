
// namespace Maux36.RimPsyche
// {
//     [HarmonyPatch]
//     public class EdB_PanelAppearance_Draw_Patch
//     {
//         public static bool Prepare()
//         {
//             return ModsConfig.IsActive("EdB.PrepareCarefully");
//         }

//         public static IEnumerable<MethodBase> TargetMethods()
//         {
//             yield return AccessTools.Method("EdB.PrepareCarefully.PanelAppearance.Draw");
//         }

//         public static void Postfix(Rect ___RectButtonRandomize, Pawn ___CachedPawn)
//         {
//             if (___CachedPawn?.compPsyche() != null)
//             {
//                 Rect rect = new Rect(___RectButtonRandomize.x - 10f, ___RectButtonRandomize.y - 3f, ___RectButtonRandomize.width, ___RectButtonRandomize.height);
//                 Rimpsyche_UI_Utility.DrawEditButton(rect, ___CachedPawn);
//             }
//         }
//     }
// }