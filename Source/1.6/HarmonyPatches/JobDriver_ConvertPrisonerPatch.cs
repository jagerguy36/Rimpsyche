using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using System;

namespace Maux36.RimPsyche
{
    public class JobDriver_ConvertPrisonerPatch
    {
        [HarmonyPatch]
        public static class JobDriver_ConvertPrisoner_MakeNewToils_Patch
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
                    foreach (var instruction in instructions)
                    {
                        if (instruction.opcode == OpCodes.Ldsfld &&
                            instruction.operand is FieldInfo fieldInfo &&
                            fieldInfo.DeclaringType == typeof(InteractionDefOf) &&
                            fieldInfo.Name == nameof(InteractionDefOf.Chitchat))
                        {
                            //Log.Message($"Convert Prisoner Found matching pattern in {method.Name}");
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

            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                foreach (var instruction in instructions)
                {
                    if (instruction.opcode == OpCodes.Ldsfld &&
                        instruction.operand is FieldInfo fieldInfo &&
                        fieldInfo.DeclaringType == typeof(InteractionDefOf) &&
                        fieldInfo.Name == nameof(InteractionDefOf.Chitchat))
                    {
                        yield return new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(DefOfRimpsyche), nameof(DefOfRimpsyche.Rimpsyche_Smalltalk)));
                    }
                    else
                    {
                        yield return instruction;
                    }
                }
            }
        }
    }
}