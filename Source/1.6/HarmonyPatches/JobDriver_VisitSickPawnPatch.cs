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
            private static Type _lambdaContainingType;

            public static MethodBase TargetMethod()
            {
                Type jobDriverType = typeof(JobDriver_VisitSickPawn);

                MethodInfo targetMethod = jobDriverType.GetMethod("<MakeNewToils>b__7_1",BindingFlags.NonPublic | BindingFlags.Instance,null,[typeof(int)],null);

                if (targetMethod != null)
                {
                    _lambdaContainingType = jobDriverType;
                    return targetMethod;
                }

                foreach (Type nestedType in jobDriverType.GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    if (nestedType.Name.StartsWith("<MakeNewToils>d__"))
                    {
                        targetMethod = nestedType.GetMethod("<MakeNewToils>b__7_1",BindingFlags.NonPublic | BindingFlags.Instance,null,[typeof(int)],null);
                        if (targetMethod != null)
                        {
                            _lambdaContainingType = nestedType;
                            return targetMethod;
                        }
                    }
                }
                return null;
            }


            [HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> ReplaceInteractionTranspiler(IEnumerable<CodeInstruction> instructions)
            {
                if (_lambdaContainingType == null)
                {
                    return instructions;
                }

                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count - 4; i++)
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
            Log.Message($"This is called by {pawn.Name}");
            return DefOfRimpsyche.Rimpsyche_Smalltalk;
        }
    }
}