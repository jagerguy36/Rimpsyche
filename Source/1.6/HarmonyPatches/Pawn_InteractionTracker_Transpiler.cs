using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection.Emit;
using Verse;


namespace Maux36.RimPsyche
{
    public class Pawn_InteractionTracker_Transpiler
    {
        [HarmonyPatch(typeof(Pawn_InteractionsTracker), "InteractionsTrackerTickInterval")]
        public static class Pawn_InteractionTracker_InteractionsTrackerTick
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = new List<CodeInstruction>(instructions);
                var multiplierMethod = AccessTools.Method(typeof(Pawn_InteractionTracker_InteractionsTrackerTick), nameof(InteractionMultipliedValue));
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
            //Small -> Frequent
            public static int InteractionMultipliedValue(int original, Pawn pawn)
            {
                var psyche = pawn?.compPsyche();
                if (psyche == null)
                {
                    return original;
                }
                float socialInteractionMult = psyche.Personality.Evaluate(SocialInteractionIntervalMultiplier);
                int result = (int)(original * socialInteractionMult);
                return result;
            }
        }

        public static RimpsycheFormula SocialInteractionIntervalMultiplier = new(
            "SocialInteractionIntervalMultiplier",
            (tracker) =>
            {
                float mult = 1f;
                float sociability = tracker.GetPersonality(PersonalityDefOf.Rimpsyche_Sociability);
                return mult * (1f - 0.7f * sociability);
            }
        );
    }
}