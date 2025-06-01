using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection.Emit;
using Verse;


namespace Maux36.RimPsyche
{

    [HarmonyPatch(typeof(Pawn_InteractionsTracker), "InteractionsTrackerTick")]
    public static class Pawn_InteractionTracker_InteractionsTrackerTick
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            var multiplierMethod = AccessTools.Method(typeof(Pawn_InteractionTracker_InteractionsTrackerTick), nameof(MyMultiplyValue));
            var pawnField = AccessTools.Field(typeof(Pawn_InteractionsTracker), "pawn");

            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldc_I4 &&
                    codes[i].operand is int intVal &&
                    (intVal == 22000 || intVal == 6600 || intVal == 550))
                {
                    codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
                    codes.Insert(i + 2, new CodeInstruction(OpCodes.Ldfld, pawnField));
                    codes.Insert(i + 3, new CodeInstruction(OpCodes.Call, multiplierMethod));
                    i += 3;
                }
            }

            return codes;
        }
        public static int MyMultiplyValue(int original, Pawn pawn)
        {
            float interactionScore = pawn?.compPsyche()?.Personality?.GetPersonality(PersonalityDefOf.Rimpsyche_Sociability) ?? 0f;
            int result = (int)((1f - 0.7f * interactionScore) * original);
            //Log.Message($"[Harmony] Adjusted interaction interval of {pawn.Name}: original={original}, interactionScore={interactionScore}, result={result}");
            return result;
        }
    }
}