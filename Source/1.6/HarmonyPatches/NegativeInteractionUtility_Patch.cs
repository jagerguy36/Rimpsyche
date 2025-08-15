using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Maux36.RimPsyche
{
    [HarmonyPatch(typeof(NegativeInteractionUtility), nameof(NegativeInteractionUtility.NegativeInteractionChanceFactor))]
    public static class NegativeInteractionUtility_Patch
    {
        private static readonly SimpleCurve OpinionFactorCurve = new SimpleCurve
        {
            new CurvePoint(-100f, 6f),
            new CurvePoint(-50f, 4f),
            new CurvePoint(-25f, 2f),
            new CurvePoint(0f, 1f),
            new CurvePoint(50f, 0.1f),
            new CurvePoint(100f, 0f)
        };
        private static bool Prefix(ref float __result, Pawn initiator, Pawn recipient)
        {
            var initPsyche = initiator.compPsyche(); 
            var reciPsyche = recipient.compPsyche();
            if (initPsyche == null || reciPsyche == null)
            {
                return true;
            }
            if (initiator.story.traits.HasTrait(TraitDefOf.Kind))
            {
                __result = 0f;
                return false;
            }
            float num = 1f;
            num *= OpinionFactorCurve.Evaluate(initiator.relations.OpinionOf(recipient));
            if (initiator.story.traits.HasTrait(TraitDefOf.Abrasive))
            {
                num *= 2f; //Reduce influence because tact is already influencing the outcome
            }
            //Vanilla curve range 4 ~ 0.4
            num *= initPsyche.Personality.Evaluate(InitNegativeChanceMultiplier); //3.85~0.45
            num *= reciPsyche.Personality.Evaluate(reciNegativeChanceMultiplier); //1.1~0.9
            var initPlayfulness = initPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Playfulness);
            var reciPlayfulness = reciPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Playfulness);
            if(initPlayfulness > 0f && reciPlayfulness < 0f)
            {
                num *= (1f + 0.2f * (initPlayfulness * initPlayfulness * reciPlayfulness * reciPlayfulness));
            }
            //Age from CompatFactor
            float x = Mathf.Abs(initiator.ageTracker.AgeBiologicalYearsFloat - recipient.ageTracker.AgeBiologicalYearsFloat);
            float ageInfluence = 1f + Mathf.Clamp(GenMath.LerpDouble(0f, 20f, 0.25f, -0.25f, x), -0.25f, 0.25f);
            num *= ageInfluence;
            __result = num;
            return false;
        }
        public static RimpsycheFormula InitNegativeChanceMultiplier = new(
            "InitNegativeChanceMultiplier",
            (tracker) =>
            {
                float intentFactor = (2f * tracker.GetPersonality(PersonalityDefOf.Rimpsyche_Aggressiveness) - 1.5f* tracker.GetPersonality(PersonalityDefOf.Rimpsyche_Compassion) - 0.6f * tracker.GetPersonality(PersonalityDefOf.Rimpsyche_Sociability) + 0.4f * tracker.GetPersonality(PersonalityDefOf.Rimpsyche_Tension) + 0.5f * tracker.GetPersonality(PersonalityDefOf.Rimpsyche_Competitiveness)); // -5~5
                intentFactor = intentFactor > 0f ? 1f + (intentFactor / 2f) : 1f + (intentFactor / 10f); // 0.5~3.5
                float deliveryFactor = 1f - tracker.GetPersonality(PersonalityDefOf.Rimpsyche_Tact)*0.1f; // 0.9~1.1
                return intentFactor * deliveryFactor;
            }
        );

        public static RimpsycheFormula reciNegativeChanceMultiplier = new(
            "reciNegativeChanceMultiplier",
            (tracker) =>
            {
                float securityFactor = ( tracker.GetPersonality(PersonalityDefOf.Rimpsyche_Tension) - tracker.GetPersonality(PersonalityDefOf.Rimpsyche_Stability) - tracker.GetPersonality(PersonalityDefOf.Rimpsyche_Confidence)); // -3~3
                //securityFactor = securityFactor > 0f ? 1f + (securityFactor / 30f) : 1f + (securityFactor / 10f); // 0.9~1.1
                securityFactor = 1f + securityFactor / 30f;
                return securityFactor;
            }
        );
    }
}
