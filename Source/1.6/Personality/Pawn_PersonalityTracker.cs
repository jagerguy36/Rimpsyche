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
        private CompPsyche compPsyche;
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

        private Dictionary<Facet, (float, float)> geneGateAccumulatorInternal = null;
        public Dictionary<Facet, string> geneGateInfoCache = [];
        public Dictionary<Facet, (float, float)> geneGateAccumulator
        {
            get
            {
                geneGateAccumulatorInternal = geneGateAccumulatorInternal ?? GenerateGeneGateAccumulator();
                return geneGateAccumulatorInternal;
            }
        }
        private Dictionary<Facet, (float, float)> gateCacheInternal = null; //new Dictionary<Facet, Tuple<float, float>>();
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
        private Dictionary<string, float> personalityCache = new Dictionary<string, float>();
        public Dictionary<string, string> scopeInfoCache = [];
        private Dictionary<string, (float, float)> scopeCacheInternal = null; //new Dictionary<string, Tuple<float, float>>();
        public Dictionary<string, (float, float)> scopeCache
        {
            get
            {
                scopeCacheInternal = scopeCacheInternal ?? GenerateScope();
                return scopeCacheInternal;
            }
        }

        private Dictionary<string, float> EvaluationCache = new Dictionary<string, float>();
        public float Evaluate(RimpsycheFormula rimpsycheMultiplier)
        {
            if (EvaluationCache.TryGetValue(rimpsycheMultiplier.formulaName, out float cachedValue))
            {
                return cachedValue;
            }
            else
            {
                float calculatedValue = rimpsycheMultiplier.calculationFunction.Invoke(this);
                EvaluationCache[rimpsycheMultiplier.formulaName] = calculatedValue;
                //Log.Message($"calculating {pawn.Name}'s {rimpsycheMultiplier.formulaName} : {calculatedValue} || {nameof(rimpsycheMultiplier)}");
                return calculatedValue;
            }
        }

        public float GetPersonality(string personalityDefName) //-1~1
        {
            if (personalityCache.TryGetValue(personalityDefName, out float cachedValue))
            {
                return cachedValue;
            }

            float sum = 0f;
            var personality = RimpsycheDatabase.PersonalityDict[personalityDefName];
            foreach (var w in personality.scoreWeight)
            {
                sum += GetFacetValueNorm(w.facet) * w.weight;
            }
            float result = Mathf.Clamp(sum * 0.02f, -1f, 1f);

            // Apply scope
            if (!scopeCache.NullOrEmpty())
            {
                if (scopeCache.TryGetValue(personalityDefName, out var range))
                {
                    var (low, high) = range;
                    result = Rimpsyche_Utility.ApplyScope(result, low, high);
                }
            }
            personalityCache[personalityDefName] = result;
            return result;
        }
        public float GetPersonality(PersonalityDef personality) //-1~1
        {
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
            compPsyche = p.compPsyche();
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
            gateInfoCache.Clear();
            var newGate = new Dictionary<Facet, (float, float)>();
            List<Trait> traits = pawn.story?.traits?.allTraits;
            var gateAccumulator = geneGateAccumulator;
            if (traits != null)
            {
                foreach (Trait trait in traits)
                {
                    if (trait.Suppressed) continue;
                    Pair<string, int> pair = new Pair<string, int>(trait.def.defName, trait.Degree);
                    if (RimpsycheDatabase.TraitGateDatabase.TryGetValue(pair, out var values))
                    {
                        foreach (var value in values)
                        {
                            var facet = value.Item1;
                            float centerOffset = value.Item2;
                            float range = value.Item3;
                            if (gateAccumulator.TryGetValue(facet, out var existingData))
                            {
                                float newCenter;
                                float existingCenter = existingData.center;
                                if ((existingCenter >= 0f && centerOffset >= 0f) || (existingCenter < 0f && centerOffset < 0f))
                                {
                                    newCenter = Math.Abs(existingCenter) > Math.Abs(centerOffset) ? existingCenter : centerOffset;
                                }
                                else
                                {
                                    newCenter = existingCenter + centerOffset;
                                }
                                float newMinRange = Math.Min(existingData.minRange, range);
                                
                                gateAccumulator[facet] = (newCenter, newMinRange);
                                //Log.Message($"{pawn.Name}'s gate for {facet} updated: center sum = {newCenter}, min range = {newMinRange}");
                            }
                            else
                            {
                                gateAccumulator.Add(facet, (centerOffset, range));
                                //Log.Message($"{pawn.Name}'s gate for {facet} added by {trait.Label}: center = {centerOffset}, range = {range}");
                            }
                            if (gateInfoCache.TryGetValue(facet, out string explanation))
                            {
                                gateInfoCache[facet] = explanation + $", {trait.Label}";
                            }
                            else
                            {
                                if (geneGateInfoCache.TryGetValue(facet, out string geneExplanation))
                                {
                                    gateInfoCache.Add(facet, $"{geneExplanation}\n{"TraitAffected".Translate()} {trait.Label}");
                                }
                                else
                                {
                                    gateInfoCache.Add(facet, $"{"TraitAffected".Translate()} {trait.Label}");
                                }
                            }
                        }
                    }
                }
            }
            // Convert the accumulated data into the final (min, max) gate format.
            foreach (var kvp in gateAccumulator)
            {
                var facet = kvp.Key;
                var totalCenter = kvp.Value.center;
                var minRange = kvp.Value.minRange;

                // Convert from the final (center, range) to (min, max)
                newGate.Add(facet, (totalCenter - minRange, totalCenter + minRange));
            }
            return newGate;
        }
        public Dictionary<Facet, (float, float)> GenerateGeneGateAccumulator()
        {
            geneGateInfoCache.clear();
            var gateAccumulator = new Dictionary<Facet, (float center, float minRange)>();
            var genes = pawn.genes?.GenesListForReading;
            if (genes == null)
            {
                return gateAccumulator;
            }
            foreach (Gene gene in genes)
            {
                if (!gene.Active) continue;
                geneDefName = gene.def.defName;
                if (RimpsycheDatabase.GeneGateDatabase.TryGetValue(geneDefName, out var values))
                {
                    foreach (var value in values)
                    {
                        var facet = value.Item1;
                        float centerOffset = value.Item2;
                        float range = value.Item3;
                        if (gateAccumulator.TryGetValue(facet, out var existingData))
                        {
                            float newCenter;
                            float existingCenter = existingData.center;
                            if ((existingCenter >= 0f && centerOffset >= 0f) || (existingCenter < 0f && centerOffset < 0f))
                            {
                                newCenter = Math.Abs(existingCenter) > Math.Abs(centerOffset) ? existingCenter : centerOffset;
                            }
                            else
                            {
                                newCenter = existingCenter + centerOffset;
                            }
                            float newMinRange = Math.Min(existingData.minRange, range);
                            
                            gateAccumulator[facet] = (newCenter, newMinRange);
                            //Log.Message($"{pawn.Name}'s gate for {facet} updated: center sum = {newCenter}, min range = {newMinRange}");
                        }
                        else
                        {
                            gateAccumulator.Add(facet, (centerOffset, range));
                            //Log.Message($"{pawn.Name}'s gate for {facet} added by {trait.Label}: center = {centerOffset}, range = {range}");
                        }
                        if (geneGateInfoCache.TryGetValue(facet, out string explanation))
                        {
                            geneGateInfoCache[facet] = explanation + $", {trait.Label}";
                        }
                        else
                        {
                            geneGateInfoCache.Add(facet, $"{"GeneAffected".Translate()} {trait.Label}");
                        }
                    }
                }
            }
            return gateAccumulator;
        }


        public Dictionary<string, (float, float)> GenerateScope()
        {
            scopeInfoCache.Clear();
            var scopeAccumulator = new Dictionary<string, (float center, float minRange)>();
            var newScope = new Dictionary<string, (float, float)>();
            List<Trait> traits = pawn.story?.traits?.allTraits;
            if (traits == null)
            {
                return newScope;
            }
            foreach (Trait trait in traits)
            {
                if (trait.Suppressed) continue;
                Pair<string, int> pair = new Pair<string, int>(trait.def.defName, trait.Degree);
                if (RimpsycheDatabase.TraitScopeDatabase.TryGetValue(pair, out var values))
                {
                    foreach(var value in values)
                    {
                        var personalityName = value.Item1;
                        float centerOffset = value.Item2;
                        float range = value.Item3;
                        if (scopeAccumulator.TryGetValue(personalityName, out var existingData))
                        {
                            float newCenter;
                            float existingCenter = existingData.center;
                            if ((existingCenter >= 0f && centerOffset >= 0f) || (existingCenter < 0f && centerOffset < 0f))
                            {
                                newCenter = Math.Abs(existingCenter) > Math.Abs(centerOffset) ? existingCenter : centerOffset;
                            }
                            else
                            {
                                newCenter = existingCenter + centerOffset;
                            }
                            float newMinRange = Math.Min(existingData.minRange, range);

                            scopeAccumulator[personalityName] = (newCenter, newMinRange);
                            //Log.Message($"{pawn.Name}'s scope for {personalityName} updated: center sum = {newCenter}, min range = {newMinRange}");
                        }
                        else
                        {
                            scopeAccumulator.Add(personalityName, (centerOffset, range));
                            //Log.Message($"{pawn.Name}'s scope is being added by {trait.def.defName} to {personalityName}");
                        }
                        if (scopeInfoCache.TryGetValue(personalityName, out string explanation))
                        {
                            scopeInfoCache[personalityName] = explanation + $", {trait.Label}";
                        }
                        else
                        {
                            scopeInfoCache.Add(personalityName, $"{"TraitAffected".Translate()} {trait.Label}");
                        }
                    }
                }
            }
            foreach (var kvp in scopeAccumulator)
            {
                var facet = kvp.Key;
                var totalCenter = kvp.Value.center;
                var minRange = kvp.Value.minRange;
                newScope.Add(facet, (totalCenter - minRange, totalCenter + minRange));
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

        public void DirtyTraitCache()
        {
            gateCacheInternal = null;
            scopeCacheInternal = null;
            DirtyCache();
        }
        public void DirtyCache()
        {
            personalityCache.Clear();
            EvaluationCache.Clear();
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
    }
}
