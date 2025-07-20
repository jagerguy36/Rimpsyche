using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        [HarmonyPatch(typeof(Pawn_InteractionsTracker), nameof(Pawn_InteractionsTracker.TryInteractWith))]
        public static class Patch_TryInteractWith
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
            {
                List<CodeInstruction> codes = instructions.ToList();

                var defNameField = AccessTools.Field(typeof(Def), nameof(Def.defName));
                var stringEquals = AccessTools.Method(typeof(Patch_TryInteractWith), nameof(Tester));
                var playLogAdd = AccessTools.Method(typeof(PlayLog), nameof(PlayLog.Add));
                var logEntryCtor = AccessTools.Constructor(typeof(PlayLogEntry_Interaction), new[] { typeof(InteractionDef), typeof(Pawn), typeof(Pawn), typeof(List<RulePackDef>) });
                Label returnTrueLabel = generator.DefineLabel();
                bool foundInjection = false;

                for (int i = 0; i < codes.Count; i++)
                {
                    CodeInstruction code = codes[i];

                    if (!foundInjection && i + 5 < codes.Count)
                    {
                        var instr = codes[i];
                        var instr2 = codes[i + 5];
                        if (instr.opcode == OpCodes.Ldarg_2 && instr2.opcode == OpCodes.Newobj && Equals(instr2.operand, logEntryCtor))
                        {
                            foundInjection = true;
                            // Inject the conditional before stloc.s playLogEntry_Interaction
                            yield return new CodeInstruction(OpCodes.Ldarg_2).MoveLabelsFrom(code);
                            yield return new CodeInstruction(OpCodes.Ldfld, defNameField);
                            yield return new CodeInstruction(OpCodes.Ldstr, "Rimpsyche_Conversation");
                            yield return new CodeInstruction(OpCodes.Call, stringEquals);
                            yield return new CodeInstruction(OpCodes.Brfalse_S, returnTrueLabel);

                            yield return new CodeInstruction(OpCodes.Ldc_I4_1);
                            yield return new CodeInstruction(OpCodes.Ret);
                            code.labels.Add(returnTrueLabel);
                        }
                    }
                    yield return code;
                }
            }
            public static bool Tester(string a, string b)
            {
                if (a == b)
                {
                    return true;
                }
                return false;
            }
        }
    }
}