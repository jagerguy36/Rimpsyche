using RimWorld;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
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

        private readonly Pawn pawn;
        private readonly CompPsyche compPsyche;
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

        private Dictionary<Facet, (float negCenter, float posCenter, float minRange, int rank)> geneGateAccumulatorInternal = null;
        public Dictionary<Facet, string> geneGateInfoCache = [];
        public Dictionary<Facet, (float negCenter, float posCenter, float minRange, int rank)> geneGateAccumulator
        {
            get
            {
                geneGateAccumulatorInternal = geneGateAccumulatorInternal ?? GenerateGeneGateAccumulator();
                return geneGateAccumulatorInternal;
            }
        }
        private Dictionary<Facet, (float, float)> gateCacheInternal = null;
        public Dictionary<Facet, string> gateInfoCache = [];
        public Dictionary<Facet, (float, float)> gateCache
        {
            get
            {
                gateCacheInternal = gateCacheInternal ?? GenerateGate();
                return gateCacheInternal;
            }
        }

        //Personality Value is cached whenever FacetValueNorm is updated.
        private Dictionary<int, float> personalityCache = new Dictionary<int, float>();
        public Dictionary<int, string> scopeInfoCache = [];
        private Dictionary<int, (float, float)> scopeCacheInternal = null;
        public Dictionary<int, (float, float)> scopeCache
        {
            get
            {
                scopeCacheInternal = scopeCacheInternal ?? GenerateScope();
                return scopeCacheInternal;
            }
        }

        public float GetPersonality(string personalityDefName) //-1~1
        {
            PersonalityDef personalityDef = RimpsycheDatabase.PersonalityDict[personalityDefName];
            if (personalityCache.TryGetValue(personalityDef.shortHash, out float cachedValue))
            {
                return cachedValue;
            }

            float sum = 0f;
            var weights = personalityDef.scoreWeight;
            for (int i = 0; i < weights.Count; i++)
            {
                var w = weights[i];
                sum += GetFacetValueNorm(w.facet) * w.weight;
            }
            float result = Mathf.Clamp(sum * 0.02f, -1f, 1f);

            // Apply scope
            if (!scopeCache.NullOrEmpty())
            {
                if (scopeCache.TryGetValue(personalityDef.shortHash, out var range))
                {
                    var (low, high) = range;
                    result = Rimpsyche_Utility.ApplyScope(result, low, high);
                }
            }
            personalityCache[personalityDef.shortHash] = result;
            return result;
        }
        public float GetPersonality(PersonalityDef personalityDef) //-1~1
        {
            if (personalityDef == null)
                return 0f;

            if (personalityCache.TryGetValue(personalityDef.shortHash, out float cachedValue))
            {
                return cachedValue;
            }

            float sum = 0f;
            foreach(var w in personalityDef.scoreWeight)
            {
                sum += GetFacetValueNorm(w.facet) * w.weight;
            }
            float result = Mathf.Clamp(sum * 0.02f, -1f, 1f);
            
            // Apply scope
            if (!scopeCache.NullOrEmpty())
            {
                if (scopeCache.TryGetValue(personalityDef.shortHash, out var range))
                {
                    var (low, high) = range;
                    result = Rimpsyche_Utility.ApplyScope(result, low, high);
                }
            }
            personalityCache[personalityDef.shortHash] = result;
            return result;
        }
        /// <summary>
        /// Get Non-normalized version of personality. Should only be used for editing Personality
        /// Otherwise, all reference to personality should use GetPersonality instead.
        /// </summary>
        /// <param name="personality"></param>
        /// <returns></returns>
        public float GetPersonalityDirect(PersonalityDef personalityDef) //Non-Normalized Version
        {
            if (personalityDef == null)
                return 0f;

            float sum = 0f;
            var weights = personalityDef.scoreWeight;
            for (int i = 0; i < weights.Count; i++)
            {
                var w = weights[i];
                sum += GetFacetValue(w.facet) * w.weight;
            }
            float result = Mathf.Clamp(sum * 0.02f, -1f, 1f);

            // Apply scope
            if (!scopeCache.NullOrEmpty())
            {
                if (scopeCache.TryGetValue(personalityDef.shortHash, out var range))
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
                if (mult < 1f)
                    return 1f / (1f + ((1f / mult) - 1f) * p);
                return 1f + (mult - 1f) * p;
            }
            else
            {
                if (mult < 1f)
                    return 1f - ((1f / mult) - 1f) * p;
                return 1f / (1f - (mult - 1f) * p);
            }
        }
        public float Evaluate(RimpsycheFormula rimpsycheMultiplier)
        {
            return compPsyche.Evaluate(rimpsycheMultiplier);
        }

        // Initialization
        public Pawn_PersonalityTracker(Pawn p)
        {
            pawn = p;
            compPsyche = p.compPsyche();
        }
        public void Initialize(PsycheData psycheData = null)
        {
            if (psycheData != null)
            {
                imagination = psycheData.imagination;
                intellect = psycheData.intellect;
                curiosity = psycheData.curiosity;

                industriousness = psycheData.industriousness;
                orderliness = psycheData.orderliness;
                integrity = psycheData.integrity;

                sociability = psycheData.sociability;
                assertiveness = psycheData.assertiveness;
                enthusiasm = psycheData.enthusiasm;

                compassion = psycheData.compassion;
                cooperation = psycheData.cooperation;
                humbleness = psycheData.humbleness;

                volatility = psycheData.volatility;
                pessimism = psycheData.pessimism;
                insecurity = psycheData.insecurity;
                DirtyCache();
                return;
            }
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
            gateInfoCache.Clear();
            var newGate = new Dictionary<Facet, (float, float)>();
            List<Trait> traits = pawn.story?.traits?.allTraits;
            var gateAccumulator = new Dictionary<Facet, (float negCenter, float posCenter, float minRange, int rank)>(geneGateAccumulator);
            if (traits != null)
            {
                foreach (Trait trait in traits)
                {
                    if (trait.Suppressed) continue;
                    int key = (trait.def.shortHash << 16) | (trait.Degree + 256);
                    if (RimpsycheDatabase.TraitGateDatabase.TryGetValue(key, out var values))
                    {
                        foreach (var value in values)
                        {
                            var facet = value.Facet;
                            float centerOffset = value.CenterOffset;
                            float range = value.Range;
                            int rank = value.Rank;

                            float existingPosCenter = 0f;
                            float existingNegCenter = 0f;
                            if (gateAccumulator.TryGetValue(facet, out var existingData))
                            {
                                float existingRank = existingData.rank;
                                if (existingRank > rank)
                                {
                                    if (gateInfoCache.TryGetValue(facet, out string explanationInt))
                                    {
                                        gateInfoCache[facet] = explanationInt + $", {trait.Label}";
                                    }
                                    else
                                    {
                                        gateInfoCache[facet] = $"{"TraitAffected".Translate()} {trait.Label}";
                                    }
                                    continue;
                                }
                                else if (existingRank == rank)
                                {
                                    existingPosCenter = existingData.posCenter;
                                    existingNegCenter = existingData.negCenter;
                                    range = Math.Min(existingData.minRange, range);
                                }
                            }
                            if (centerOffset > 0f)
                            {
                                existingPosCenter = Mathf.Max(centerOffset, existingPosCenter);
                            }
                            else
                            {
                                existingNegCenter = Mathf.Min(centerOffset, existingNegCenter);
                            }
                            gateAccumulator[facet] = (existingNegCenter, existingPosCenter, range, rank);
                            if (gateInfoCache.TryGetValue(facet, out string explanation))
                            {
                                gateInfoCache[facet] = explanation + $", {trait.Label}";
                            }
                            else
                            {
                                gateInfoCache[facet] = $"{"TraitAffected".Translate()} {trait.Label}";
                            }
                        }
                    }
                }
            }
            foreach (Facet facet in Enum.GetValues(typeof(Facet)))
            {
                if (geneGateInfoCache.TryGetValue(facet, out string geneExplanation))
                {
                    if (gateInfoCache.TryGetValue(facet, out string gateExplanation))
                    {
                        gateInfoCache[facet] = $"{geneExplanation}\n{gateExplanation}";
                    }
                    else
                    {
                        gateInfoCache[facet] = geneExplanation;
                    }
                }
                else
                {
                    if (gateInfoCache.TryGetValue(facet, out string gateExplanation))
                    {
                        gateInfoCache[facet] = $"{gateExplanation}";
                    }
                }
            }
            // Convert the accumulated data into the final (min, max) gate format.
            foreach (var kvp in gateAccumulator)
            {
                var facet = kvp.Key;
                var totalCenter = kvp.Value.negCenter + kvp.Value.posCenter;
                var minRange = kvp.Value.minRange;
                var lowest = Mathf.Clamp(totalCenter - minRange, -50f, 50f);
                var highest = Mathf.Clamp(totalCenter + minRange, -50f, 50f);
                newGate.Add(facet, (lowest, highest));
            }
            return newGate;
        }
        public Dictionary<Facet, (float, float, float, int)> GenerateGeneGateAccumulator()
        {
            geneGateInfoCache.Clear();
            var gateAccumulator = new Dictionary<Facet, (float negCenter, float posCenter, float minRange, int rank)>();
            var genes = pawn.genes?.GenesListForReading;
            if (genes == null)
            {
                return gateAccumulator;
            }
            foreach (Gene gene in genes)
            {
                if (!gene.Active) continue;
                var geneHash = gene.def.shortHash;
                if (RimpsycheDatabase.GeneGateDatabase.TryGetValue(geneHash, out var values))
                {
                    foreach (var value in values)
                    {
                        var facet = value.Facet;
                        float centerOffset = value.CenterOffset;
                        float range = value.Range;
                        int rank = value.Rank;

                        float existingPosCenter = 0f;
                        float existingNegCenter = 0f;
                        if (gateAccumulator.TryGetValue(facet, out var existingData))
                        {
                            float existingRank = existingData.rank;
                            if (existingRank > rank)
                            {
                                if (geneGateInfoCache.TryGetValue(facet, out string explanationInt))
                                {
                                    geneGateInfoCache[facet] = explanationInt + $", {gene.Label}";
                                }
                                else
                                {
                                    geneGateInfoCache[facet] = $"{"GeneAffected".Translate()} {gene.Label}";
                                }
                                continue;
                            }
                            else if (existingRank == rank)
                            {
                                existingPosCenter = existingData.posCenter;
                                existingNegCenter = existingData.negCenter;
                                range = Math.Min(existingData.minRange, range);
                            }
                        }
                        if (centerOffset > 0f)
                        {
                            existingPosCenter = Mathf.Max(centerOffset, existingPosCenter);
                        }
                        else
                        {
                            existingNegCenter = Mathf.Min(centerOffset, existingNegCenter);
                        }
                        gateAccumulator[facet] = (existingNegCenter, existingPosCenter, range, rank);
                        if (geneGateInfoCache.TryGetValue(facet, out string explanation))
                        {
                            geneGateInfoCache[facet] = explanation + $", {gene.Label}";
                        }
                        else
                        {
                            geneGateInfoCache[facet] = $"{"GeneAffected".Translate()} {gene.Label}";
                        }
                    }
                }
            }
            return gateAccumulator;
        }


        public Dictionary<int, (float, float)> GenerateScope()
        {
            scopeInfoCache.Clear();
            var scopeAccumulator = new Dictionary<int, (float negCenter, float posCenter, float minRange)>();
            var newScope = new Dictionary<int, (float, float)>();
            List<Trait> traits = pawn.story?.traits?.allTraits;
            if (traits == null)
            {
                return newScope;
            }
            foreach (Trait trait in traits)
            {
                if (trait.Suppressed) continue;
                int key = (trait.def.shortHash << 16) | (trait.Degree + 256);
                if (RimpsycheDatabase.TraitScopeDatabase.TryGetValue(key, out var values))
                {
                    foreach(var value in values)
                    {
                        int personalityHash = value.Item1;
                        float centerOffset = value.Item2;
                        float range = value.Item3;

                        float existingPosCenter = 0f;
                        float existingNegCenter = 0f;
                        if (scopeAccumulator.TryGetValue(personalityHash, out var existingData))
                        {
                            existingPosCenter = existingData.posCenter;
                            existingNegCenter = existingData.negCenter;
                            range = Math.Min(existingData.minRange, range);
                        }

                        if (centerOffset > 0f)
                        {
                            existingPosCenter = Mathf.Max(centerOffset, existingPosCenter);
                        }
                        else
                        {
                            existingNegCenter = Mathf.Min(centerOffset, existingNegCenter);
                        }
                        scopeAccumulator[personalityHash] = (existingNegCenter, existingPosCenter, range);
                        if (scopeInfoCache.TryGetValue(personalityHash, out string explanation))
                        {
                            scopeInfoCache[personalityHash] = explanation + $", {trait.Label}";
                        }
                        else
                        {
                            scopeInfoCache.Add(personalityHash, $"{"TraitAffected".Translate()} {trait.Label}");
                        }
                    }
                }
            }
            foreach (var kvp in scopeAccumulator)
            {
                var facet = kvp.Key;
                var totalCenter = kvp.Value.negCenter + kvp.Value.posCenter;
                var minRange = kvp.Value.minRange;
                var lowest = Mathf.Clamp(totalCenter - minRange, -1f, 1f); //safety
                var highest = Mathf.Clamp(totalCenter + minRange, -1f, 1f); //safety
                newScope.Add(facet, (lowest, highest));
            }
            return newScope;
        }
        // IO
        public float GetFacetValueRaw(Facet facet) //Actual raw facet values
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
        public float GetFacetValue(Facet facet) //Gated facet values
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
            return Mathf.Clamp(value, -50f, 50f);
        }
        public int GetFacetValueNorm(Facet facet) //Normalized facet value to use with personalities
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
                int groupLength = group.Length;
                for (int i = 0; i < groupLength; i++)
                {
                    var facetI = group[i];

                    if (!changes.TryGetValue(facetI, out float delta) || Mathf.Approximately(delta,0f))
                        continue;
                    
                    float currentFacetValue = GetFacetValueRaw(facetI);
                    if (delta > 0 && Mathf.Approximately(currentFacetValue, 50f))
                        continue;

                    if (delta < 0 && Mathf.Approximately(currentFacetValue, -50f))
                        continue;

                    // Apply direct change
                    netChanges[facetI] = delta;

                    // Apply side-effects to other facets in the group
                    for (int j = 0; j < groupLength; j++)
                    {
                        if (i == j) continue;

                        var facetJ = group[j];

                        if (!changes.ContainsKey(facetJ))
                        {
                            if (!netChanges.ContainsKey(facetJ))
                                netChanges[facetJ] = delta * 0.1f;
                            else
                                netChanges[facetJ] += delta * 0.1f;
                        }
                    }
                }
            }

            // Apply all accumulated changes
            bool hasGateCache = !gateCache.NullOrEmpty();
            foreach (var kvp in netChanges)
            {
                float currentValue = GetFacetValue(kvp.Key);
                float adjustedFuture = currentValue+kvp.Value;

                if (hasGateCache && gateCache.TryGetValue(kvp.Key, out var range))
                {
                    var (low, high) = range;
                    adjustedFuture = Rimpsyche_Utility.RestoreGatedValue(adjustedFuture, low, high);
                }
                //Log.Message($"adjusting facet {kvp.Key} from {GetFacetValueRaw(kvp.Key)} to {adjustedFuture}");
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
            float totalWeight = 0f;
            foreach (var fw in def.scoreWeight)
            {
                totalWeight += Mathf.Abs(fw.weight);
            }
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

        public void DirtyGeneCache()
        {
            geneGateAccumulatorInternal = null;
            gateCacheInternal = null;
            scopeCacheInternal = null;
            DirtyCache();
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
            if (compPsyche != null)
            {
                compPsyche.EvaluationCache.Clear();
                compPsyche.ThoughtEvaluationCache.Clear();
                compPsyche.OpinionEvaluationCache.Clear();
                compPsyche.JoyChanceEvaluationCache.Clear();
                compPsyche.TopicOpinionCache.Clear();
                compPsyche.Interests?.NotifyPersonalityDirtied();
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
    }
}
