using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
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
                var logEntryCtor = AccessTools.Constructor(typeof(PlayLogEntry_Interaction), new[] { typeof(InteractionDef), typeof(Pawn), typeof(Pawn), typeof(List<RulePackDef>) });
                var skiplabel = generator.DefineLabel();
                var normallabel = generator.DefineLabel();
                bool foundInjection = false;

                for (int i = 0; i < codes.Count; i++)
                {
                    var instr = codes[i];
                    if (!foundInjection && i + 4 < codes.Count)
                    {
                        var instr2 = codes[i + 4];
                        if (instr.opcode == OpCodes.Ldarg_0 && instr2.opcode == OpCodes.Newobj && Equals(instr2.operand, logEntryCtor))
                        {
                            foundInjection = true;
                            yield return new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(DefOfRimpsyche), nameof(DefOfRimpsyche.Rimpsyche_StartConversation)));
                            yield return new CodeInstruction(OpCodes.Beq, skiplabel);
                            yield return new CodeInstruction(OpCodes.Br, normallabel);

                            yield return new CodeInstruction(OpCodes.Ldc_I4_1).WithLabels(skiplabel); ;
                            yield return new CodeInstruction(OpCodes.Ret);
                            yield return new CodeInstruction(OpCodes.Nop).WithLabels(normallabel);
                            yield return new CodeInstruction(OpCodes.Ldarg_2);
                        }
                        yield return instr;
                    }
                    else yield return instr;
                }
            }
        }

        //[HarmonyPatch(typeof(Pawn_InteractionsTracker), nameof(Pawn_InteractionsTracker.TryInteractWith))]
        //public static class Patch_TryInteractWith
        //{
        //    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator) //DEV VERSION
        //    {
        //        List<CodeInstruction> codes = instructions.ToList();
        //        var defNameField = AccessTools.Field(typeof(Def), nameof(Def.defName));
        //        var stringEquals = AccessTools.Method(typeof(Patch_TryInteractWith), nameof(Tester));
        //        var logEntryCtor = AccessTools.Constructor(typeof(PlayLogEntry_Interaction), new[] { typeof(InteractionDef), typeof(Pawn), typeof(Pawn), typeof(List<RulePackDef>) });
        //        var label1 = generator.DefineLabel();
        //        var label2 = generator.DefineLabel();
        //        bool foundInjection = false;

        //        for (int i = 0; i < codes.Count; i++)
        //        {
        //            var instr = codes[i];
        //            if (!foundInjection && i + 4 < codes.Count)
        //            {
        //                var instr2 = codes[i + 4];
        //                if (instr.opcode == OpCodes.Ldarg_0 && instr2.opcode == OpCodes.Newobj && Equals(instr2.operand, logEntryCtor))
        //                {
        //                    foundInjection = true;
        //                    yield return new CodeInstruction(OpCodes.Ldfld, defNameField);
        //                    yield return new CodeInstruction(OpCodes.Ldstr, "Rimpsyche_Conversation");
        //                    yield return new CodeInstruction(OpCodes.Call, stringEquals);
        //                    yield return new CodeInstruction(OpCodes.Brtrue_S, label1);
        //                    yield return new CodeInstruction(OpCodes.Br, label2);

        //                    yield return new CodeInstruction(OpCodes.Ldc_I4_1).WithLabels(label1); ;
        //                    yield return new CodeInstruction(OpCodes.Ret);
        //                    yield return new CodeInstruction(OpCodes.Nop).WithLabels(label2);
        //                    yield return new CodeInstruction(OpCodes.Ldarg_2);
        //                }
        //                yield return instr;
        //            }
        //            else yield return instr;
        //        }
        //    }
        //    public static bool Tester(string a, string b) //FOR DEV
        //    {
        //        Log.Message("Testing condition");
        //        if (a == b)
        //        {
        //            return true;
        //        }
        //        return false;
        //    }
        //}
    }
}