using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Verse;

namespace Maux36.RimPsyche
{
    [HarmonyPatch(typeof(CharacterCardUtility), nameof(CharacterCardUtility.DrawCharacterCard))]
    public static class CharacterCardUtility_DrawCharacterCard_Patch
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codes)
        {
            MethodInfo methodInfoInfoCardButton = AccessTools.Method(typeof(Widgets), nameof(Widgets.InfoCardButton), new Type[] { typeof(float), typeof(float), typeof(Thing) });
            foreach (CodeInstruction c in codes)
            {
                if (c.Calls(methodInfoInfoCardButton))
                {
                    yield return CodeInstruction.Call(typeof(CharacterCardUtility_DrawCharacterCard_Patch), nameof(PsycheCardButton));
                    continue;
                }
                yield return c;
            }
        }

        public static bool PsycheCardButton(float x, float y, Pawn pawn)
        {
            if (pawn.compPsyche() != null)
            {
                Rect rect = new Rect(x + 23f, y - 3f, 30f, 30f);
                Rimpsyche_UI_Utility.DrawEditButton(rect, pawn, true);
            }
            return Widgets.InfoCardButton(x, y, pawn);
        }
    }
    [HarmonyPatch(typeof(ITab_Pawn_Character), "UpdateSize")]
    public static class ITab_Pawn_Character_UpdateSize_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(ITab_Pawn_Character __instance)
        {
            if (!RimpsycheSettings.ShowSummaryInBio)
                return;
            Pawn pawn = __instance.PawnToShowInfoAbout;
            CompPsyche compPsyche = pawn?.compPsyche();
            if (compPsyche != null)
            {
                ref Vector2 size = ref __instance.size;
                size.y += RimpsycheSettings.ExtraBioHeight;
            }
        }
    }
    [HarmonyPatch(typeof(ITab_Pawn_Character), "FillTab")]
    public static class ITab_Pawn_Character_FillTab_Transpiler
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codes, ILGenerator generator)
        {
            var drawCardMethod = AccessTools.Method(typeof(CharacterCardUtility), nameof(CharacterCardUtility.DrawCharacterCard));
            var DrawBioPersonalitySummaryMethod = AccessTools.Method(typeof(PsycheInfoCard), nameof(PsycheInfoCard.DrawBioPersonalitySummary));
            var savedRectLocal = generator.DeclareLocal(typeof(Rect));
            var savedPawnLocal = generator.DeclareLocal(typeof(Pawn));
            bool RectMatched = false;
            bool PawnMatched = false;

            foreach (var code in codes)
            {
                yield return code;
                if (code.opcode == OpCodes.Newobj && code.operand is ConstructorInfo ctor && ctor.DeclaringType == typeof(Rect))
                {
                    yield return new CodeInstruction(OpCodes.Dup);
                    yield return new CodeInstruction(OpCodes.Stloc, savedRectLocal);
                    RectMatched = true;
                }

                else if (RectMatched && code.operand is MethodInfo m && m.Name == "get_PawnToShowInfoAbout")
                {
                    yield return new CodeInstruction(OpCodes.Dup);
                    yield return new CodeInstruction(OpCodes.Stloc, savedPawnLocal);
                    PawnMatched = true;
                }

                else if (PawnMatched && code.Calls(drawCardMethod))
                {
                    yield return new CodeInstruction(OpCodes.Ldloc, savedRectLocal);
                    yield return new CodeInstruction(OpCodes.Ldloc, savedPawnLocal);
                    yield return new CodeInstruction(OpCodes.Call, DrawBioPersonalitySummaryMethod);
                }
            }
        }
    }
}
