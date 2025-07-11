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
                foreach (Type nestedType in typeof(JobDriver_ConvertPrisoner).GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    if (nestedType.Name.StartsWith("<MakeNewToils>d__"))
                    {
                        MethodInfo moveNextMethod = nestedType.GetMethod("MoveNext", BindingFlags.Instance | BindingFlags.NonPublic);
                        if (moveNextMethod != null)
                        {
                            return moveNextMethod;
                        }
                    }
                }
                return null;
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