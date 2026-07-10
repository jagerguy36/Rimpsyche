using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection;
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
                Rimpsyche_UI_Utility.DrawEditButton(rect, pawn);
            }
            return Widgets.InfoCardButton(x, y, pawn);
        }
    }

    [HarmonyPatch(typeof(CharacterCardUtility), "DoLeftSection")]
    internal static class Patch_CharacterCardUtility_DoleftSection
    {
        private static void Prefix(ref Rect leftRect, Pawn pawn)
        {
            if (!RimpsycheSettings.showSummaryInBio)
                return;
            CompPsyche compPsyche = pawn?.compPsyche();
            if (compPsyche != null)
            {
                leftRect.height -= RimpsycheSettings.ExtraBioHeight;
            }
        }
    }

    [HarmonyPatch(typeof(CharacterCardUtility), "DrawCharacterCard")]
    internal static class Patch_CharacterCardUtility_DrawCharacterCard
    {
        private static void Postfix(Rect rect, Pawn pawn)
        {
            if (!RimpsycheSettings.showSummaryInBio)
                return;
            CompPsyche compPsyche = pawn?.compPsyche();
            if (compPsyche != null)
            {
                PsycheInfoCard.DrawBioPersonalitySummary(pawn, compPsyche, new Rect(rect.x, rect.yMax - (float)RimpsycheSettings.ExtraBioHeight, rect.width, rect.height));
            }
        }


    }

    [HarmonyPatch(typeof(CharacterCardUtility), "PawnCardSize")]
    internal static class Patch_CharacterCardUtility_PawnCardSize
    {
        private static void Postfix(ref Vector2 __result, Pawn pawn)
        {
            if (!RimpsycheSettings.showSummaryInBio)
                return;
            CompPsyche compPsyche = pawn?.compPsyche();
            if (compPsyche != null)
            {
                __result += new Vector2(0f, (float)RimpsycheSettings.ExtraBioHeight);
            }
        }
    }
}
