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
                Color oldColor = GUI.color;
                GUI.color = rect.Contains(Event.current.mousePosition) ? Rimpsyche_UI_Utility.ButtonLightColor : Rimpsyche_UI_Utility.ButtonDarkColor;
                GUI.DrawTexture(rect, Rimpsyche_UI_Utility.PsycheButton);
                if (Widgets.ButtonInvisible(rect, false))
                {
                    SoundDefOf.Tick_Low.PlayOneShotOnCamera(null);
                    Find.WindowStack.Add(new PsycheEditPopup(pawn));
                }
                GUI.color = oldColor;
            }
            return Widgets.InfoCardButton(x, y, pawn);
        }
    }
}
