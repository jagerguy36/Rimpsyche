using System;
using UnityEngine;
using Verse;

namespace Maux36.RimPsyche
{
    public class Pawn_PersonalityTracker : IExposable
    {
        private Pawn pawn;
        // Facets
        private float imagination = 0f;
        private float intellect = 0f;
        private float curiosity = 0f;

        private float ambition = 0f;
        private float order = 0f;
        private float integrity = 0f;

        private float sociability = 0f;
        private float assertiveness = 0f;
        private float enthusiasm = 0f;

        private float compassion = 0f;
        private float cooperation = 0f;
        private float humility = 0f;

        private float volatility = 0f;
        private float pessimism = 0f;
        private float insecurity = 0f;

        private float InteractivityInternal = -1f;
        private float EngagementInteranal = -1f;
        private float SocialIntelligenceInternal = -1f;
        private float ToleranceInternal = -1f;
        private float DiligenceInternal = -1f;


        // Axis
        public float Interactivity
        {
            get
            {
                if (InteractivityInternal < 0f) InteractivityInternal = GetInteractivity();
                return InteractivityInternal;
            }
        }
        public float Engagement
        {
            get
            {
                if (EngagementInteranal < 0f) EngagementInteranal = GetEngagement();
                return EngagementInteranal;
            }
        }
        public float SocialIntelligence
        {
            get
            {
                if (SocialIntelligenceInternal < 0f) SocialIntelligenceInternal = GetSocialIntelligence();
                return SocialIntelligenceInternal;
            }
        }
        public float Tolerance
        {
            get
            {
                if (ToleranceInternal < 0f) ToleranceInternal = GetTolerance();
                return ToleranceInternal;
            }
        }
        public float Diligence
        {
            get
            {
                if (DiligenceInternal < 0f) DiligenceInternal = GetDiligence();
                return DiligenceInternal;
            }
        }

        // Definitions
        private float GetInteractivity() // -1 ~ 1
        {
            var score =
                GetFacetValueNorm(Facet.Sociability) * 0.4f
                + GetFacetValueNorm(Facet.Insecurity) * -0.3f
                + GetFacetValueNorm(Facet.Assertiveness) * 0.2f
                + GetFacetValueNorm(Facet.Enthusiasm) * 0.05f
                + GetFacetValueNorm(Facet.Curiosity) * 0.05f;
            return score * 0.02f;
        }
        private float GetEngagement() // -1 ~ 1
        {
            var score =
                GetFacetValueNorm(Facet.Curiosity) * 0.1f
                + GetFacetValueNorm(Facet.Sociability) * 0.2f
                + GetFacetValueNorm(Facet.Cooperation) * 0.5f
                + GetFacetValueNorm(Facet.Insecurity) * -0.2f;
            return score * 0.02f;
        }
        private float GetSocialIntelligence() // -1 ~ 1
        {
            var score =
                GetFacetValueNorm(Facet.Sociability) * 0.3f
                + GetFacetValueNorm(Facet.Compassion) * 0.1f
                + GetFacetValueNorm(Facet.Cooperation) * 0.3f
                + GetFacetValueNorm(Facet.Volatility) * -0.2f
                + GetFacetValueNorm(Facet.Pessimism) * -0.1f;
            return score * 0.02f;
        }
        private float GetTolerance() // -1 ~ 1
        {
            var score =
                GetFacetValueNorm(Facet.Curiosity) * 0.2f
                + GetFacetValueNorm(Facet.Assertiveness) * -0.15f
                + GetFacetValueNorm(Facet.Compassion) * 0.2f
                + GetFacetValueNorm(Facet.Cooperation) * 0.35f
                + GetFacetValueNorm(Facet.Humility) * 0.1f;
            return score * 0.02f;
        }
        private float GetDiligence()
        {
            var score =
                GetFacetValueNorm(Facet.Imagination) * -0.1f
                + GetFacetValueNorm(Facet.Ambition) * 0.2f
                + GetFacetValueNorm(Facet.Order) * 0.5f
                + GetFacetValueNorm(Facet.Volatility) * -0.2f;
            return score * 0.02f;
        }

        // Interaction
        public float GetTopicAttitude(Topic topic)
        {
            return 0;
        }

        // Initialization
        public Pawn_PersonalityTracker(Pawn p)
        {
            pawn = p;
        }
        public void Initialize(int inputSeed = 0)
        {
            float minRange = -25f;
            float maxRange = 25f;
            float baseOCEANvalue = Rand.Range(minRange, maxRange);
            imagination = GenerateFacetValueWithBase(baseOCEANvalue);
            intellect = GenerateFacetValueWithBase(baseOCEANvalue);
            curiosity = GenerateFacetValueWithBase(baseOCEANvalue);

            baseOCEANvalue = Rand.Range(minRange, maxRange);
            ambition = GenerateFacetValueWithBase(baseOCEANvalue);
            order = GenerateFacetValueWithBase(baseOCEANvalue);
            integrity = GenerateFacetValueWithBase(baseOCEANvalue);

            baseOCEANvalue = Rand.Range(minRange, maxRange);
            sociability = GenerateFacetValueWithBase(baseOCEANvalue);
            assertiveness = GenerateFacetValueWithBase(baseOCEANvalue);
            enthusiasm = GenerateFacetValueWithBase(baseOCEANvalue);

            baseOCEANvalue = Rand.Range(minRange, maxRange);
            compassion = GenerateFacetValueWithBase(baseOCEANvalue);
            cooperation = GenerateFacetValueWithBase(baseOCEANvalue);
            humility = GenerateFacetValueWithBase(baseOCEANvalue);

            baseOCEANvalue = Rand.Range(minRange, maxRange);
            volatility = GenerateFacetValueWithBase(baseOCEANvalue);
            pessimism = GenerateFacetValueWithBase(baseOCEANvalue);
            insecurity = GenerateFacetValueWithBase(baseOCEANvalue);
        }
        public float GenerateFacetValueWithBase(float baseValue, int maxAttempts = 4)
        {
            float result;
            int attempts = 0;

            do
            {
                result = Rand.Gaussian(baseValue, 7f); // center at basevalue, 3widthfactor == 21
                attempts++;
            }
            while ((result < -50f || result > 50f) && attempts < maxAttempts); 

            if (result < -50f || result > 50f)
            {
                result = Mathf.Clamp(result, -50f, 50f);
                //Log.Warning($"GenerateFacetValueWithBase failed to get valid value in {maxAttempts} attempts. Clamped to {result}.");
            }

            return result;
        }

        // IO
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
                Facet.Pessimism => pessimism,
                Facet.Insecurity => insecurity,
                _ => throw new ArgumentOutOfRangeException(nameof(facet), facet, null),
            };
        }
        public int GetFacetValueNorm(Facet facet)
        {
            return facet switch
            {
                // Openness
                Facet.Imagination => (int)(imagination),
                Facet.Intellect => (int)(intellect),
                Facet.Curiosity => (int)(curiosity),

                // Conscientiousness
                Facet.Ambition => (int)(ambition),
                Facet.Order => (int)(order),
                Facet.Integrity => (int)(integrity),

                // Extraversion
                Facet.Sociability => (int)(sociability),
                Facet.Assertiveness => (int)(assertiveness),
                Facet.Enthusiasm => (int)(enthusiasm),

                // Agreeableness
                Facet.Compassion => (int)(compassion),
                Facet.Cooperation => (int)(cooperation),
                Facet.Humility => (int)(humility),

                // Neuroticism
                Facet.Volatility => (int)(volatility),
                Facet.Pessimism => (int)(pessimism),
                Facet.Insecurity => (int)(insecurity),
                _ => throw new ArgumentOutOfRangeException(nameof(facet), facet, null),
            };
        }
        public bool SetFacetValue(Facet facet, float value)
        {
            bool shouldDirtyCache = false;
            switch (facet)
            {
                // Openness
                case Facet.Imagination:
                    shouldDirtyCache = (int)imagination != (int)value;
                    imagination = value;
                    break;
                case Facet.Intellect:
                    shouldDirtyCache = (int)intellect != (int)value;
                    intellect = value;
                    break;
                case Facet.Curiosity:
                    shouldDirtyCache = (int)curiosity != (int)value;
                    curiosity = value;
                    break;

                // Conscientiousness
                case Facet.Ambition:
                    shouldDirtyCache = (int)ambition != (int)value;
                    ambition = value;
                    break;
                case Facet.Order:
                    shouldDirtyCache = (int)order != (int)value;
                    order = value;
                    break;
                case Facet.Integrity:
                    shouldDirtyCache = (int)integrity != (int)value;
                    integrity = value;
                    break;

                // Extraversion
                case Facet.Sociability:
                    shouldDirtyCache = (int)sociability != (int)value;
                    sociability = value;
                    break;
                case Facet.Assertiveness:
                    shouldDirtyCache = (int)assertiveness != (int)value;
                    assertiveness = value;
                    break;
                case Facet.Enthusiasm:
                    shouldDirtyCache = (int)enthusiasm != (int)value;
                    enthusiasm = value;
                    break;

                // Agreeableness
                case Facet.Compassion:
                    shouldDirtyCache = (int)compassion != (int)value;
                    compassion = value;
                    break;
                case Facet.Cooperation:
                    shouldDirtyCache = (int)cooperation != (int)value;
                    cooperation = value;
                    break;
                case Facet.Humility:
                    shouldDirtyCache = (int)humility != (int)value;
                    humility = value;
                    break;

                // Neuroticism
                case Facet.Volatility:
                    shouldDirtyCache = (int)volatility != (int)value;
                    volatility = value;
                    break;
                case Facet.Pessimism:
                    shouldDirtyCache = (int)pessimism != (int)value;
                    pessimism = value;
                    break;
                case Facet.Insecurity:
                    shouldDirtyCache = (int)insecurity != (int)value;
                    insecurity = value;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(facet), facet, null);
            }
            return shouldDirtyCache;
        }

        // Save
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
            Scribe_Values.Look(ref pessimism, "pessimism", 0, false);
            Scribe_Values.Look(ref insecurity, "insecurity", 0, false);
        }

        public void DirtyCache()
        {
            return;
        }

        public void LogAllFactors()
        {
            Log.Message($"{pawn.Name}'s OCEAN factors");
            Log.Message($"Imagination: {imagination}, Intellect: {intellect}, Curiosity: {curiosity}");
            Log.Message($"Ambition: {ambition}, Order: {order}, Integrity: {integrity}");
            Log.Message($"Sociability: {sociability}, Assertiveness: {assertiveness}, Enthusiasm: {enthusiasm}");
            Log.Message($"Compassion: {compassion}, Cooperation: {cooperation}, Humility: {humility}");
            Log.Message($"Volatility: {volatility}, Pessimism: {pessimism}, Insecurity: {insecurity}");
        }

    }
}
