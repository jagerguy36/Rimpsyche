using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Verse;
using Verse.Sound;

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
}
