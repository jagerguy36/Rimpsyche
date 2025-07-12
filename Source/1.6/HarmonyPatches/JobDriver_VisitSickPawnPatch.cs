using HarmonyLib;
using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using System;

namespace Maux36.RimPsyche
{
    public class JobDriver_VisitSickPawnPatch
    {
        [HarmonyPatch]
        public static class JobDriver_VisitSickPawn_MakeNewToils_Patch
        {
            public static MethodBase TargetMethod()
            {
                Type jobDriverType = typeof(JobDriver_VisitSickPawn);
                foreach (var method in jobDriverType.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    if (MethodMatches(method))
                    {
                        return method;
                    }
                }
                return null;
            }

            private static bool MethodMatches(MethodInfo method)
            {
                if (!method.Name.Contains("MakeNewToils"))
                    return false;
                try
                {
                    var instructions = PatchProcessor.GetOriginalInstructions(method);
                    for (int i = 0; i < instructions.Count - 1; i++)
                    {
                        if (instructions[i].opcode == OpCodes.Call &&
                            instructions[i].operand is MethodInfo mi &&
                            mi == AccessTools.PropertyGetter(typeof(Rand), nameof(Rand.Value)) &&
                            instructions[i + 1].opcode == OpCodes.Ldc_R4 &&
                            (float)instructions[i + 1].operand == 0.8f)
                        {
                            //Log.Message($"Visit Sick Pawn Found matching pattern in {method.Name}");
                            return true;
                        }
                    }
                }
                catch
                {
                    return false;
                }
                return false;
            }



            [HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> ReplaceInteractionTranspiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count - 7; i++)
                {
                    if (
                        codes[i].opcode == OpCodes.Call && codes[i].operand is MethodInfo mi1 && mi1 == AccessTools.PropertyGetter(typeof(Rand), nameof(Rand.Value)) &&
                        codes[i + 1].opcode == OpCodes.Ldc_R4 && (float)codes[i + 1].operand == 0.8f
                    )
                    {
                        codes.RemoveRange(i, 7);
                        codes.InsertRange(i, new[]
                        {
                            new CodeInstruction(OpCodes.Ldarg_0),
                            new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(JobDriver_VisitSickPawn), "pawn")),
                            new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(JobDriver_VisitSickPawnPatch), nameof(ChoseInteraction))),
                            new CodeInstruction(OpCodes.Stloc_0)
                        });
                        break;
                    }
                }
                return codes;
            }
        }

        public static InteractionDef ChoseInteraction(Pawn pawn)
        {
            //float smalltalkChance = 1f / (1f + (1 + pawn.compPsyche().Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Talkativeness)) * 1.6f);
            float smalltalkChance = 0.8f;
            InteractionDef intDef = ((Rand.Value < smalltalkChance) ? DefOfRimpsyche.Rimpsyche_Smalltalk : DefOfRimpsyche.Rimpsyche_Conversation);
            return intDef;
        }
    }
}