using System;
using UnityEngine;
using Verse;

namespace Maux36.RimPsyche
{
    public class Pawn_PersonalityTracker : IExposable
    {
        private Pawn pawn;
        public float imagination = 50f;
        public float intellect = 50f;
        public float curiosity = 50f;

        public float ambition = 50f;
        public float order = 50f;
        public float integrity = 50f;

        public float sociability = 50f;
        public float assertiveness = 50f;
        public float enthusiasm = 50f;

        public float compassion = 50f;
        public float cooperation = 50f;
        public float humility = 50f;

        public float volatility = 50f;
        public float impulsivity = 50f;
        public float insecurity = 50f;



        public void ExposeData()
        {
            Scribe_Values.Look(ref imagination, "imagination", 0, false);
            Scribe_Values.Look(ref intellect, "intellect", 0, false);
            Scribe_Values.Look(ref curiosity, "curiosity", 0, false);

            Scribe_Values.Look(ref ambition, "ambition", 0, false);
            Scribe_Values.Look(ref order, "order", 0, false);
            Scribe_Values.Look(ref integrity, "integrity", 0, false);

            Scribe_Values.Look(ref sociability, "sociability", 0, false);
            Scribe_Values.Look(ref assertiveness, "assertiveness", 0, false);
            Scribe_Values.Look(ref enthusiasm, "enthusiasm", 0, false);

            Scribe_Values.Look(ref compassion, "compassion", 0, false);
            Scribe_Values.Look(ref cooperation, "cooperation", 0, false);
            Scribe_Values.Look(ref humility, "humility", 0, false);

            Scribe_Values.Look(ref volatility, "volatility", 0, false);
            Scribe_Values.Look(ref impulsivity, "impulsivity", 0, false);
            Scribe_Values.Look(ref insecurity, "insecurity", 0, false);
        }
        public Pawn_PersonalityTracker(Pawn p)
        {
            pawn = p;
        }
        public void Initialize(int inputSeed = 0)
        {
            float baseOCEANvalue = Rand.Range(20f, 80f);
            imagination = GenerateFacetValueWithBase(baseOCEANvalue);
            intellect = GenerateFacetValueWithBase(baseOCEANvalue);
            curiosity = GenerateFacetValueWithBase(baseOCEANvalue);

            baseOCEANvalue = Rand.Range(20f, 80f);
            ambition = GenerateFacetValueWithBase(baseOCEANvalue);
            order = GenerateFacetValueWithBase(baseOCEANvalue);
            integrity = GenerateFacetValueWithBase(baseOCEANvalue);

            baseOCEANvalue = Rand.Range(20f, 80f);
            sociability = GenerateFacetValueWithBase(baseOCEANvalue);
            assertiveness = GenerateFacetValueWithBase(baseOCEANvalue);
            enthusiasm = GenerateFacetValueWithBase(baseOCEANvalue);

            baseOCEANvalue = Rand.Range(20f, 80f);
            compassion = GenerateFacetValueWithBase(baseOCEANvalue);
            cooperation = GenerateFacetValueWithBase(baseOCEANvalue);
            humility = GenerateFacetValueWithBase(baseOCEANvalue);

            baseOCEANvalue = Rand.Range(20f, 80f);
            volatility = GenerateFacetValueWithBase(baseOCEANvalue);
            impulsivity = GenerateFacetValueWithBase(baseOCEANvalue);
            insecurity = GenerateFacetValueWithBase(baseOCEANvalue);
        }
        public float GenerateFacetValueWithBase(float baseValue, int maxAttempts = 4)
        {
            float result;
            int attempts = 0;

            do
            {
                result = Rand.Gaussian(baseValue, 5f); // center at basevalue, 3widthfactor == 15
                attempts++;
            }
            while ((result < 0f || result > 100f) && attempts < maxAttempts); 

            // Optional: Clamp to valid range if all attempts fail
            if (result < 0f || result > 100f)
            {
                result = Mathf.Clamp(result, 0f, 100f);
                Log.Warning($"GenerateFacetValueWithBase failed to get valid value in {maxAttempts} attempts. Clamped to {result}.");
            }

            return result;
        }
        public float GetFacetValue(Facet facet)
        {
            return facet switch
            {
                // Openness
                Facet.Imagination => imagination,
                Facet.Intellect => intellect,
                Facet.Curiosity => curiosity,

                // Conscientiousness
                Facet.Ambition => ambition,
                Facet.Order => order,
                Facet.Integrity => integrity,

                // Extraversion
                Facet.Sociability => sociability,
                Facet.Assertiveness => assertiveness,
                Facet.Enthusiasm => enthusiasm,

                // Agreeableness
                Facet.Compassion => compassion,
                Facet.Cooperation => cooperation,
                Facet.Humility => humility,

                // Neuroticism
                Facet.Volatility => volatility,
                Facet.Impulsivity => impulsivity,
                Facet.Insecurity => insecurity,
                _ => throw new ArgumentOutOfRangeException(nameof(facet), facet, null),
            };
        }
        public void SetFacetValue(Facet facet, float value)
        {
            switch (facet)
            {
                // Openness
                case Facet.Imagination: imagination = value; break;
                case Facet.Intellect: intellect = value; break;
                case Facet.Curiosity: curiosity = value; break;

                // Conscientiousness
                case Facet.Ambition: ambition = value; break;
                case Facet.Order: order = value; break;
                case Facet.Integrity: integrity = value; break;

                // Extraversion
                case Facet.Sociability: sociability = value; break;
                case Facet.Assertiveness: assertiveness = value; break;
                case Facet.Enthusiasm: enthusiasm = value; break;

                // Agreeableness
                case Facet.Compassion: compassion = value; break;
                case Facet.Cooperation: cooperation = value; break;
                case Facet.Humility: humility = value; break;

                // Neuroticism
                case Facet.Volatility: volatility = value; break;
                case Facet.Impulsivity: impulsivity = value; break;
                case Facet.Insecurity: insecurity = value; break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(facet), facet, null);
            }
        }

        public void LogAllFactors()
        {
            Log.Message($"{pawn.Name}'s OCEAN factors");
            Log.Message($"Imagination: {imagination}, Intellect: {intellect}, Curiosity: {curiosity}");
            Log.Message($"Ambition: {ambition}, Order: {order}, Integrity: {integrity}");
            Log.Message($"Sociability: {sociability}, Assertiveness: {assertiveness}, Enthusiasm: {enthusiasm}");
            Log.Message($"Compassion: {compassion}, Cooperation: {cooperation}, Humility: {humility}");
            Log.Message($"Volatility: {volatility}, Impulsivity: {impulsivity}, Insecurity: {insecurity}");
        }

    }
}
