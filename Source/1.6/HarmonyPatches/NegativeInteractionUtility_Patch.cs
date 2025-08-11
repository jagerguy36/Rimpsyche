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
            //Vanilla curve range 4 ~ 0.4
            if (initiator.story.traits.HasTrait(TraitDefOf.Abrasive))
            {
                num *= 2.3f;
            }
            var initPsyche = initiator.compPsyche(); 
            if (initPsyche != null)
            {
                //passion
                //compassion
                //aggressiveness
                //stability + tension
                //tact + passion
                num *= initPsyche.Personality.Evaluate(TactNegativeChanceMultiplier);
            }
            var reciPsyche = recipient.compPsyche();
            if (reciPsyche != null)
            {
                //passion
                //compassion
                //aggressiveness
                //stability + tension
                //tact + passion --> Treat the init/recip the same
                num *= reciPsyche.Personality.Evaluate(TensionNegativeChanceMultiplier);
            }
            __result = num;
            return false;
        }
        public static RimpsycheFormula TactNegativeChanceMultiplier = new(
            "TactNegativeChanceMultiplier",
            (tracker) =>
            {
                float tactFactor = tracker.GetPersonalityAsMult(PersonalityDefOf.Rimpsyche_Tact, 0.75f);
                return tactFactor;
            }
        );

        public static RimpsycheFormula TensionNegativeChanceMultiplier = new(
            "TensionNegativeChanceMultiplier",
            (tracker) =>
            {
                float tensionFactor = tracker.GetPersonalityAsMult(PersonalityDefOf.Rimpsyche_Tension, 1.2f);
                return tensionFactor;
            }
        );
    }
}
