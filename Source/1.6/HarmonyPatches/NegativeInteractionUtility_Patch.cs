using HarmonyLib;
using RimWorld;
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
            if (initiator.story.traits.HasTrait(TraitDefOf.Kind))
            {
                __result = 0f;
                return false;
            }
            float num = 1f;
            num *= OpinionFactorCurve.Evaluate(initiator.relations.OpinionOf(recipient));
            if (initiator.story.traits.HasTrait(TraitDefOf.Abrasive))
            {
                num *= 1.8f; //Reduce influence because tact is already influencing the outcome
            }
            //Vanilla curve range 4 ~ 0.4
            var initPsyche = initiator.compPsyche(); 
            if (initPsyche != null)
            {
                num *= initPsyche.Personality.Evaluate(InitNegativeChanceMultiplier); //2~0.6
            }
            var reciPsyche = recipient.compPsyche();
            if (reciPsyche != null)
            {
                num *= reciPsyche.Personality.Evaluate(reciNegativeChanceMultiplier); //2~0.6
            }
            var initPlayfulness = initPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Playfulness);
            var reciPlayfulness = reciPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Playfulness);
            if(initPlayfulness > 0f && reciPlayfulness < 0f)
            {
                num *= (1f + 0.2f * (initPlayfulness * initPlayfulness * reciPlayfulness * reciPlayfulness));
            }
            __result = num;
            return false;
        }
        public static RimpsycheFormula InitNegativeChanceMultiplier = new(
            "InitNegativeChanceMultiplier",
            (tracker) =>
            {
                float intentFactor = tracker.GetPersonality(PersonalityDefOf.Rimpsyche_Aggressiveness) - tracker.GetPersonality(PersonalityDefOf.Rimpsyche_Compassion);
                intentFactor = intentFactor > 0f ? 1f + (intentFactor / 3f) : 1f + (intentFactor / 8f); // 0.75~1.666
                float deliveryFactor = 1f + tracker.GetPersonality(PersonalityDefOf.Rimpsyche_Tact)*0.2f; // 0.8~1.2
                return intentFactor * deliveryFactor;
            }
        );

        public static RimpsycheFormula reciNegativeChanceMultiplier = new(
            "reciNegativeChanceMultiplier",
            (tracker) =>
            {
                float securityFactor = tracker.GetPersonality(PersonalityDefOf.Rimpsyche_Tension) - tracker.GetPersonality(PersonalityDefOf.Rimpsyche_Stability) + tracker.GetPersonality(PersonalityDefOf.Rimpsyche_Confidence);
                securityFactor = securityFactor > 0f ? 1f + (2f * securityFactor / 9f) : 1f + (securityFactor / 12f); // 0.75~1.666
                float temperamentFactor = 1f + (tracker.GetPersonality(PersonalityDefOf.Rimpsyche_Aggressiveness) + tracker.GetPersonality(PersonalityDefOf.Rimpsyche_Competitiveness))*0.1f; // 0.8~1.2
                return securityFactor * temperamentFactor;
            }
        );
    }
}
