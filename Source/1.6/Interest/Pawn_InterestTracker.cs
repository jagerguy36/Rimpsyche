using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Maux36.RimPsyche
{
    public class Pawn_InterestTracker : IExposable
    {
        private Pawn pawn;
        public Dictionary<string, float> interestOffset = new Dictionary<string, float>(); // 30~70
        public Dictionary<string, float> interestScore = new Dictionary<string, float>(); // -30~30

        public Pawn_InterestTracker(Pawn p)
        {
            pawn = p;
        }
        public void Initialize(int inputSeed = 0)
        {
            foreach (InterestDomainDef interestdomainDef in DefDatabase<InterestDomainDef>.AllDefs)
            {
                GenerateInterestOffsetsForDomain(interestdomainDef, generateScore = true);
            }
        }

        public void GenerateInterestOffsetsForDomain(InterestDomainDef interestdomainDef, bool generateScore = false) // 0~100
        {
            var compPsyche = pawn.compPsyche();
            float domainOffsetValue = 50;
            if (interestdomainDef.scoreWeight != null) //Add domain offset
            {
                foreach (var sw in interestdomainDef.scoreWeight)
                {
                    domainOffsetValue += compPsyche.Personality.GetFacetValueNorm(sw.facet) * sw.weight;
                }
            }
            foreach (var interest in interestdomainDef.interests)
            {
                float interestOffsetValue = domainOffsetValue;
                if (interest.scoreWeight != null)
                {
                    foreach (var sw in interest.scoreWeight)
                    {

                        interestOffsetValue += compPsyche.Personality.GetFacetValueNorm(sw.facet) * sw.weight;
                    }
                }
                interestOffset[interest.name] = Mathf.Clamp(interestOffsetValue, 30f, 70f);
                if (generateScore)
                {
                    GenerateInterestScore(interest.name);
                }
            }
        }
        public void GenerateInterestScore(string interestname, int maxAttempts = 4)
        {
            float result;
            int attempts = 0;

            do
            {
                result = Rand.Gaussian(0, 10f); // center at basevalue, 3widthfactor == 30
                attempts++;
            }
            while ((result < -30f || result > 30f) && attempts < maxAttempts);
            result = Mathf.Clamp(result, -30f, 30f);
            interestScore[interestname] = result;
        }

        public float GetOrCreateInterestScore(Interest key)
        {
            if (!interestOffset.TryGetValue(key.name, out float offsetValue))
            {
                GenerateInterestOffsetsForDomain(RimpsycheDatabase.InterestDomainDict[key]);
                if (!interestOffset.TryGetValue(key.name, out offsetValue))
                {
                    offsetValue = 50;
                }
            }
            if (!interestScore.TryGetValue(key.name, out float score))
            {
                GenerateInterestScore(key.name);
                if (!interestScore.TryGetValue(key.name, out score))
                {
                    score = 0;
                }
            }
            return Mathf.Clamp(offsetValue + score, 0f, 100f);
        }

        public void SetInterestScore(Interest key, float score)
        {
            float delta = score - GetOrCreateInterestScore(key);
            if (interestScore.TryGetValue(key.name, out float s))
            {
                if ((delta<0f && s == -30f) || (delta>0f && s == 30f)) return;
                interestScore[key.name] = Mathf.Clamp(interestScore[key.name] + delta, -30f, 30f);
            }
        }

        public Interest ChoseInterest()
        {
            return GenCollection.RandomElementByWeight(RimpsycheDatabase.InterestList, GetOrCreateInterestScore);
        }

        // Save
        public void ExposeData()
        {
            Scribe_Collections.Look(ref interestScore, "interestScore", LookMode.Value, LookMode.Value);
        }
    }
}
