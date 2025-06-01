using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Maux36.RimPsyche
{
    public class Pawn_PersonalityTracker : IExposable
    {
        private static readonly Dictionary<string, Facet[]> Groups = new()
        {
            ["Openness"] = new[] { Facet.Imagination, Facet.Intellect, Facet.Curiosity },
            ["Conscientiousness"] = new[] { Facet.Industriousness, Facet.Orderliness, Facet.Integrity },
            ["Extraversion"] = new[] { Facet.Sociability, Facet.Assertiveness, Facet.Enthusiasm },
            ["Agreeableness"] = new[] { Facet.Compassion, Facet.Cooperation, Facet.Politeness },
            ["Neuroticism"] = new[] { Facet.Volatility, Facet.Pessimism, Facet.Insecurity },
        };

        private Pawn pawn;
        // Facets
        private float imagination = 0f;
        private float intellect = 0f;
        private float curiosity = 0f;

        private float industriousness = 0f;
        private float orderliness = 0f;
        private float integrity = 0f;

        private float sociability = 0f;
        private float assertiveness = 0f;
        private float enthusiasm = 0f;

        private float compassion = 0f;
        private float cooperation = 0f;
        private float politeness = 0f;

        private float volatility = 0f;
        private float pessimism = 0f;
        private float insecurity = 0f;

        private Dictionary<string, float> personalityCache = new Dictionary<string, float>();

        public float GetPersonality(PersonalityDef personality)
        {
            if (personality == null || string.IsNullOrEmpty(personality.label))
                return 0f;
            if (personalityCache.TryGetValue(personality.label, out float cachedValue))
            {
                return cachedValue;
            }

            float sum = 0f;
            foreach(var w in personality.scoreWeight)
            {
                sum += GetFacetValueNorm(w.facet) * w.weight;
            }
            float result = Mathf.Clamp(sum * 0.02f, -1f, 1f);
            personalityCache[personality.label] = result;
            return result;
        }
        public float GetPersonalityDirect(PersonalityDef personality)
        {
            if (personality == null || string.IsNullOrEmpty(personality.label))
                return 0f;

            float sum = 0f;
            foreach (var w in personality.scoreWeight)
            {
                sum += GetFacetValue(w.facet) * w.weight;
            }
            float result = Mathf.Clamp(sum * 0.02f, -1f, 1f);
            return result;
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
            float minRange = -40f;
            float maxRange = 40f;
            float baseOCEANvalue = Rand.Range(minRange, maxRange);
            imagination = GenerateFacetValueWithBase(baseOCEANvalue);
            intellect = GenerateFacetValueWithBase(baseOCEANvalue);
            curiosity = GenerateFacetValueWithBase(baseOCEANvalue);

            baseOCEANvalue = Rand.Range(minRange, maxRange);
            industriousness = GenerateFacetValueWithBase(baseOCEANvalue);
            orderliness = GenerateFacetValueWithBase(baseOCEANvalue);
            integrity = GenerateFacetValueWithBase(baseOCEANvalue);

            baseOCEANvalue = Rand.Range(minRange, maxRange);
            sociability = GenerateFacetValueWithBase(baseOCEANvalue);
            assertiveness = GenerateFacetValueWithBase(baseOCEANvalue);
            enthusiasm = GenerateFacetValueWithBase(baseOCEANvalue);

            baseOCEANvalue = Rand.Range(minRange, maxRange);
            compassion = GenerateFacetValueWithBase(baseOCEANvalue);
            cooperation = GenerateFacetValueWithBase(baseOCEANvalue);
            politeness = GenerateFacetValueWithBase(baseOCEANvalue);

            baseOCEANvalue = Rand.Range(minRange, maxRange);
            volatility = GenerateFacetValueWithBase(baseOCEANvalue);
            pessimism = GenerateFacetValueWithBase(baseOCEANvalue);
            insecurity = GenerateFacetValueWithBase(baseOCEANvalue);
            DirtyCache();
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
                Facet.Industriousness => industriousness,
                Facet.Orderliness => orderliness,
                Facet.Integrity => integrity,

                // Extraversion
                Facet.Sociability => sociability,
                Facet.Assertiveness => assertiveness,
                Facet.Enthusiasm => enthusiasm,

                // Agreeableness
                Facet.Compassion => compassion,
                Facet.Cooperation => cooperation,
                Facet.Politeness => politeness,

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
                Facet.Industriousness => (int)(industriousness),
                Facet.Orderliness => (int)(orderliness),
                Facet.Integrity => (int)(integrity),

                // Extraversion
                Facet.Sociability => (int)(sociability),
                Facet.Assertiveness => (int)(assertiveness),
                Facet.Enthusiasm => (int)(enthusiasm),

                // Agreeableness
                Facet.Compassion => (int)(compassion),
                Facet.Cooperation => (int)(cooperation),
                Facet.Politeness => (int)(politeness),

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
                case Facet.Industriousness:
                    shouldDirtyCache = (int)industriousness != (int)value;
                    industriousness = value;
                    break;
                case Facet.Orderliness:
                    shouldDirtyCache = (int)orderliness != (int)value;
                    orderliness = value;
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
                case Facet.Politeness:
                    shouldDirtyCache = (int)politeness != (int)value;
                    politeness = value;
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

        public void AffectFacetValue(Dictionary<Facet, float> changes)
        {
            bool shouldDirtyCache = false;
            var netChanges = new Dictionary<Facet, float>();

            foreach (var group in Groups.Values)
            {
                for (int i = 0; i < group.Length; i++)
                {
                    var facetI = group[i];

                    if (!changes.TryGetValue(facetI, out float delta) || delta == 0f)
                        continue;

                    // Apply direct change
                    netChanges[facetI] = delta;

                    // Apply side-effects to other facets in the group
                    for (int j = 0; j < group.Length; j++)
                    {
                        if (i == j) continue;

                        var facetJ = group[j];
                        if (changes.ContainsKey(facetJ)) continue;
                        netChanges.TryGetValue(facetJ, out float existing);
                        netChanges[facetJ] = existing + delta * 0.1f;
                    }
                    // Apply side-effects to other facets in the group
                    foreach (var facetJ in group)
                    {
                        if (facetI == facetJ || changes.ContainsKey(facetJ))
                            continue;

                        netChanges.TryGetValue(facetJ, out float existing);
                        netChanges[facetJ] = existing + delta * 0.1f;
                    }
                }
            }

            // Apply all accumulated changes
            foreach (var kvp in netChanges)
            {
                float currentValue = GetFacetValue(kvp.Key);
                if (SetFacetValue(kvp.Key, currentValue + kvp.Value))
                {
                    shouldDirtyCache = true;
                }
            }

            if (shouldDirtyCache) DirtyCache();
        }

        public void SetPersonalityRating(PersonalityDef def, float newValue)
        {
            float current = GetPersonalityDirect(def);
            float delta = newValue - current;

            if (Mathf.Approximately(delta, 0f))
                return;

            // Invert 0.02f scaling factor
            float adjustedDelta = delta * 50f;

            // Total absolute weight (for normalization)
            float totalWeight = def.scoreWeight.Sum(w => Mathf.Abs(w.weight));
            if (totalWeight == 0f)
                return;

            var facetChanges = new Dictionary<Facet, float>();

            foreach (var weight in def.scoreWeight)
            {
                if (weight.weight == 0f) continue;

                float contribution = adjustedDelta * (weight.weight / totalWeight);
                if (contribution != 0f)
                    facetChanges[weight.facet] = contribution;
            }
            
            AffectFacetValue(facetChanges);
        }


        // Save
        public void ExposeData()
        {
            Scribe_Values.Look(ref imagination, "imagination", 0, false);
            Scribe_Values.Look(ref intellect, "intellect", 0, false);
            Scribe_Values.Look(ref curiosity, "curiosity", 0, false);

            Scribe_Values.Look(ref industriousness, "industriousness", 0, false);
            Scribe_Values.Look(ref orderliness, "orderliness", 0, false);
            Scribe_Values.Look(ref integrity, "integrity", 0, false);

            Scribe_Values.Look(ref sociability, "sociability", 0, false);
            Scribe_Values.Look(ref assertiveness, "assertiveness", 0, false);
            Scribe_Values.Look(ref enthusiasm, "enthusiasm", 0, false);

            Scribe_Values.Look(ref compassion, "compassion", 0, false);
            Scribe_Values.Look(ref cooperation, "cooperation", 0, false);
            Scribe_Values.Look(ref politeness, "politeness", 0, false);

            Scribe_Values.Look(ref volatility, "volatility", 0, false);
            Scribe_Values.Look(ref pessimism, "pessimism", 0, false);
            Scribe_Values.Look(ref insecurity, "insecurity", 0, false);
        }

        public void DirtyCache()
        {
            personalityCache.Clear();
        }

        public void LogAllFactors()
        {
            Log.Message($"{pawn.Name}'s OCEAN factors");
            Log.Message($"Imagination: {imagination}, Intellect: {intellect}, Curiosity: {curiosity}");
            Log.Message($"Industriousness: {industriousness}, Orderliness: {orderliness}, Integrity: {integrity}");
            Log.Message($"Sociability: {sociability}, Assertiveness: {assertiveness}, Enthusiasm: {enthusiasm}");
            Log.Message($"Compassion: {compassion}, Cooperation: {cooperation}, Politeness: {politeness}");
            Log.Message($"Volatility: {volatility}, Pessimism: {pessimism}, Insecurity: {insecurity}");
        }

    }
}
