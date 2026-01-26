using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Verse;

namespace Maux36.RimPsyche
{
    [HarmonyPatch(typeof(NegativeInteractionUtility), nameof(NegativeInteractionUtility.NegativeInteractionChanceFactor))]
    public static class NegativeInteractionUtility_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var codes = new List<CodeInstruction>(instructions);
            bool CompatCurveFound = false;
            bool skipping = false;
            int patchCount = 0;

            FieldInfo compatibilityCurveField = AccessTools.Field(typeof(NegativeInteractionUtility), "CompatibilityFactorCurve");
            for (int i = 0; i < codes.Count; i++)
            {
                var code = codes[i];
                if (!CompatCurveFound &&
                    codes[i + 1].opcode == OpCodes.Ldsfld &&
                    Equals(codes[i + 1].operand, compatibilityCurveField))
                {
                    CompatCurveFound = true;
                    skipping = true;
                    patchCount += 1;
                    continue;
                }
                if (skipping)
                {
                    if (codes[i].opcode == OpCodes.Mul)
                    {
                        i += 1;
                        skipping = false;
                        patchCount += 1;
                    }
                    continue;
                }

                //Reduce influence of abrasiveness because tact is already influencing the outcome
                if (CompatCurveFound &&
                    code.opcode == OpCodes.Ldc_R4 && (float)code.operand == 2.3f)
                {
                    yield return new CodeInstruction(OpCodes.Ldc_R4, 2f);
                    patchCount += 1;
                    continue;
                }
                yield return code;
            }
            if (patchCount != 3)
                Log.Error("[Rimpsyche] Failed to patch negative interaction chance factor");

        }
        private static void Postfix(ref float __result, Pawn initiator, Pawn recipient)
        {
            if (__result == 0f) return;
            var initPsyche = initiator.compPsyche();
            var reciPsyche = recipient.compPsyche();
            if (initPsyche?.Enabled != true || reciPsyche?.Enabled != true)
            {
                return;
            }
            //Replace compatibility factor
            //Curve dictates 0.4f ~ 4f for -2.5f ~ 3f
            var rawIntentFactor = initPsyche.Evaluate(InitIntentFactor); // -4~3.5
            //Age from CompatFactor
            float x = Mathf.Abs(Rimpsyche_Utility.GetPawnAge(initiator) - Rimpsyche_Utility.GetPawnAge(recipient));
            float ageInfluence = 1f + Mathf.Clamp(GenMath.LerpDouble(0f, 20f, -0.2f, 0.2f, x), -0.2f, 0.2f);
            float intentFactor = rawIntentFactor * ageInfluence; // -4.8~4.2
            intentFactor = intentFactor > 0f ? 1f + (intentFactor / 2f) : 1f + (intentFactor / 10f); //0.52~3.1
            __result *= intentFactor;
            float deliveryFactor = 1f - initPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Tact) * 0.1f; // 0.9~1.1
            __result *= deliveryFactor;
            __result *= reciPsyche.Evaluate(reciNegativeChanceMultiplier); //0.9~1.1
            var initPlayfulness = initPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Playfulness);
            var reciPlayfulness = reciPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Playfulness);
            if (initPlayfulness > 0f && reciPlayfulness < 0f)
            {
                __result *= (1f + 0.1f * (initPlayfulness * initPlayfulness * reciPlayfulness * reciPlayfulness)); // 0.9~1.1
            }
            //Range: 0.37908 ~ 4.1261
        }

        public static RimpsycheFormula InitIntentFactor = new(
            "InitIntentFactor",
            (tracker) =>
            {
                var aggressiveness = 1.7f * tracker.GetPersonality(PersonalityDefOf.Rimpsyche_Aggressiveness);
                var coldheartedness = -1.3f * tracker.GetPersonality(PersonalityDefOf.Rimpsyche_Compassion);
                var tension = 0.5f * tracker.GetPersonality(PersonalityDefOf.Rimpsyche_Tension);
                var cooperationM = -0.6f * Mathf.Min(0f, tracker.GetPersonality(PersonalityDefOf.Rimpsyche_Competitiveness));
                var sociabilityM = 0.4f * Mathf.Max(0f, tracker.GetPersonality(PersonalityDefOf.Rimpsyche_Sociability));
                float intentFactor = (aggressiveness + coldheartedness + tension - cooperationM - sociabilityM); // -4~3.5
                return intentFactor;
            },
            RimpsycheFormulaManager.FormulaIdDict
        );

        public static RimpsycheFormula reciNegativeChanceMultiplier = new(
            "reciNegativeChanceMultiplier",
            (tracker) =>
            {
                float securityFactor = (tracker.GetPersonality(PersonalityDefOf.Rimpsyche_Tension) - tracker.GetPersonality(PersonalityDefOf.Rimpsyche_Stability) - tracker.GetPersonality(PersonalityDefOf.Rimpsyche_Confidence)); // -3~3
                //securityFactor = securityFactor > 0f ? 1f + (securityFactor / 30f) : 1f + (securityFactor / 10f); // 0.9~1.1
                securityFactor = 1f + securityFactor / 30f;
                return securityFactor;
            },
            RimpsycheFormulaManager.FormulaIdDict
        );
    }
}
