using RimWorld;
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
            ["Agreeableness"] = new[] { Facet.Compassion, Facet.Cooperation, Facet.Humbleness },
            ["Neuroticism"] = new[] { Facet.Volatility, Facet.Pessimism, Facet.Insecurity },
        };

        private Pawn pawn;
        // Facets -50~50
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
        private float humbleness = 0f;

        private float volatility = 0f;
        private float pessimism = 0f;
        private float insecurity = 0f;

        private Dictionary<Facet, (float, float)> gateCacheInternal = null; //new Dictionary<Facet, Tuple<float, float>>();
        public Dictionary<Facet, (float, float)> gateCache
        {
            get
            {
                gateCacheInternal = gateCacheInternal ?? GenerateGate();
                return gateCacheInternal;
            }
        }

        //Personality Value is cached whenever FacetValueNorm is updated.
        private Dictionary<string, float> personalityCache = new Dictionary<string, float>();
        private Dictionary<string, (float, float)> scopeCacheInternal = null; //new Dictionary<string, Tuple<float, float>>();
        public Dictionary<string, (float, float)> scopeCache
        {
            get
            {
                scopeCacheInternal = scopeCacheInternal ?? GenerateScope();
                return scopeCacheInternal;
            }
        }

        private Dictionary<string, float> MultiplierCache = new Dictionary<string, float>();
        public float GetMultiplier(RimpsycheMultiplier rimpsycheMultiplier)
        {
            if (MultiplierCache.TryGetValue(rimpsycheMultiplier.multiplierName, out float cachedValue))
            {
                return cachedValue;
            }
            else
            {
                float calculatedValue = rimpsycheMultiplier.calculationFunction.Invoke(this);
                MultiplierCache[rimpsycheMultiplier.multiplierName] = calculatedValue;
                Log.Message($"calculating {pawn.Name}'s {rimpsycheMultiplier.multiplierName} : {calculatedValue} || {nameof(rimpsycheMultiplier)}");
                return calculatedValue;
            }
        }

        public float GetPersonality(PersonalityDef personality) //-1~1
        {
            if (personality == null || string.IsNullOrEmpty(personality.defName))
                return 0f;
            if (personalityCache.TryGetValue(personality.defName, out float cachedValue))
            {
                return cachedValue;
            }

            float sum = 0f;
            foreach(var w in personality.scoreWeight)
            {
                sum += GetFacetValueNorm(w.facet) * w.weight;
            }
            float result = Mathf.Clamp(sum * 0.02f, -1f, 1f);
            
            // Apply scope
            if (!scopeCache.NullOrEmpty())
            {
                if (scopeCache.TryGetValue(personality.defName, out var range))
                {
                    var (low, high) = range;
                    result = Rimpsyche_Utility.ApplyScope(result, low, high);
                }
            }
            personalityCache[personality.defName] = result;
            return result;
        }
        public float GetPersonalityDirect(PersonalityDef personality) //Non-Normalized Version
        {
            if (personality == null || string.IsNullOrEmpty(personality.defName))
                return 0f;

            float sum = 0f;
            foreach (var w in personality.scoreWeight)
            {
                sum += GetFacetValue(w.facet) * w.weight;
            }
            float result = Mathf.Clamp(sum * 0.02f, -1f, 1f);

            // Apply scope
            if (!scopeCache.NullOrEmpty())
            {
                if (scopeCache.TryGetValue(personality.defName, out var range))
                {
                    var (low, high) = range;
                    result = Rimpsyche_Utility.ApplyScope(result, low, high);
                }
            }
            return result;
        }

        public float GetPersonalityAsMult(PersonalityDef personality, float mult)
        {
            var p = GetPersonality(personality);
            if (p >= 0f)
            {
                return (mult - 1f) * p + 1f;
            }
            else
            {
                return (1f - (1/mult)) * p + 1f;
            }
        }

        // Initialization
        public Pawn_PersonalityTracker(Pawn p)
        {
            pawn = p;
        }
        public void Initialize(int inputSeed = 0)
        {
            float minRange = -35f;
            float maxRange = 35f;
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
            humbleness = GenerateFacetValueWithBase(baseOCEANvalue);

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
                result = Rand.Gaussian(baseValue, 10f); // center at basevalue, 3widthfactor == 30
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
        public Dictionary<Facet, (float, float)> GenerateGate()
        {
            var newGate = new Dictionary<Facet, (float, float)>();
            List<Trait> traits = pawn.story?.traits?.allTraits;
            foreach (Trait trait in traits)
            {
                Pair<string, int> pair = new Pair<string, int>(trait.def.defName, trait.Degree);
                if (RimpsycheDatabase.TraitGateDatabase.TryGetValue(pair, out var values))
                {
                    foreach(var value in values)
                    {
                        Log.Message($"{pawn.Name}'s gate is being added by {trait.def.defName} to {value.Item1}");
                        newGate[value.Item1] = (value.Item2, value.Item3);
                    }
                }
            }
            return newGate;
        }
        public Dictionary<string, (float, float)> GenerateScope()
        {
            var newScope = new Dictionary<string, (float, float)>();
            List<Trait> traits = pawn.story?.traits?.allTraits;
            foreach (Trait trait in traits)
            {
                Pair<string, int> pair = new Pair<string, int>(trait.def.defName, trait.Degree);
                if (RimpsycheDatabase.TraitScopeDatabase.TryGetValue(pair, out var values))
                {
                    foreach(var value in values)
                    {
                        Log.Message($"{pawn.Name}'s scope is being added by {trait.def.defName} to {value.Item1}");
                        newScope[value.Item1] = (value.Item2, value.Item3);
                    }
                }
            }
            return newScope;
        }
        // IO
        public float GetFacetValueRaw(Facet facet)
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
                Facet.Humbleness => humbleness,

                // Neuroticism
                Facet.Volatility => volatility,
                Facet.Pessimism => pessimism,
                Facet.Insecurity => insecurity,
                _ => throw new ArgumentOutOfRangeException(nameof(facet), facet, null),
            };
        }
        public float GetFacetValue(Facet facet)
        {
            float value = facet switch
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
                Facet.Humbleness => humbleness,

                // Neuroticism
                Facet.Volatility => volatility,
                Facet.Pessimism => pessimism,
                Facet.Insecurity => insecurity,

                _ => throw new ArgumentOutOfRangeException(nameof(facet), facet, null),
            };

            if (gateCache.NullOrEmpty())
            {
                return value;
            }
            else
            {
                if (gateCache.TryGetValue(facet, out var range))
                {
                    var (low, high) = range;
                    return Rimpsyche_Utility.ApplyGate(value, low, high);
                }
            }
            return value;
        }
        public int GetFacetValueNorm(Facet facet)
        {
            return (int)(GetFacetValue(facet));
        }
        public bool SetFacetValue(Facet facet, float value)
        {
            value = Mathf.Clamp(value, -50f, 50f);
            int originalNormValue = GetFacetValueNorm(facet);
            switch (facet)
            {
                // Openness
                case Facet.Imagination:
                    imagination = value;
                    break;
                case Facet.Intellect:
                    intellect = value;
                    break;
                case Facet.Curiosity:
                    curiosity = value;
                    break;

                // Conscientiousness
                case Facet.Industriousness:
                    industriousness = value;
                    break;
                case Facet.Orderliness:
                    orderliness = value;
                    break;
                case Facet.Integrity:
                    integrity = value;
                    break;

                // Extraversion
                case Facet.Sociability:
                    sociability = value;
                    break;
                case Facet.Assertiveness:
                    assertiveness = value;
                    break;
                case Facet.Enthusiasm:
                    enthusiasm = value;
                    break;

                // Agreeableness
                case Facet.Compassion:
                    compassion = value;
                    break;
                case Facet.Cooperation:
                    cooperation = value;
                    break;
                case Facet.Humbleness:
                    humbleness = value;
                    break;

                // Neuroticism
                case Facet.Volatility:
                    volatility = value;
                    break;
                case Facet.Pessimism:
                    pessimism = value;
                    break;
                case Facet.Insecurity:
                    insecurity = value;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(facet), facet, null);
            }
            var changedValue = GetFacetValueNorm(facet);
            return changedValue != originalNormValue;
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
                float adjustedFuture = currentValue+kvp.Value;

                if (!gateCache.NullOrEmpty())
                {
                    if (gateCache.TryGetValue(kvp.Key, out var range))
                    {
                        var (low, high) = range;
                        adjustedFuture = Rimpsyche_Utility.RestoreGatedValue(adjustedFuture, low, high);
                    }
                }
                Log.Message($"adjusting facet {kvp.Key} from {GetFacetValueRaw(kvp.Key)} to {adjustedFuture}");
                if (SetFacetValue(kvp.Key, adjustedFuture))
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

        public void DirtyTraitCache()
        {
            gateCacheInternal = null;
            scopeCacheInternal = null;
            DirtyCache();
        }
        public void DirtyCache()
        {
            personalityCache.Clear();
            MultiplierCache.Clear();
            var compPsyche = pawn.compPsyche();
            if (compPsyche != null)
            {
                compPsyche.Interests?.interestOffset.Clear();
            }
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
            Scribe_Values.Look(ref humbleness, "humbleness", 0, false);

            Scribe_Values.Look(ref volatility, "volatility", 0, false);
            Scribe_Values.Look(ref pessimism, "pessimism", 0, false);
            Scribe_Values.Look(ref insecurity, "insecurity", 0, false);
        }
        public void LogAllFactors()
        {
            Log.Message($"{pawn.Name}'s OCEAN factors");
            Log.Message($"Imagination: {imagination}, Intellect: {intellect}, Curiosity: {curiosity}");
            Log.Message($"Industriousness: {industriousness}, Orderliness: {orderliness}, Integrity: {integrity}");
            Log.Message($"Sociability: {sociability}, Assertiveness: {assertiveness}, Enthusiasm: {enthusiasm}");
            Log.Message($"Compassion: {compassion}, Cooperation: {cooperation}, Humbleness: {humbleness}");
            Log.Message($"Volatility: {volatility}, Pessimism: {pessimism}, Insecurity: {insecurity}");
        }

    }
}
