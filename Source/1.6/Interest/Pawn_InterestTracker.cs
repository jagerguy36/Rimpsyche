using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Maux36.RimPsyche
{
    public class Pawn_InterestTracker : IExposable
    {
        private Pawn pawn;
        public Dictionary<int, float> interestOffset = new Dictionary<int, float>(); // 35~65
        public Dictionary<int, float> interestScore = new Dictionary<int, float>(); // -35~35

        public Pawn_InterestTracker(Pawn p)
        {
            pawn = p;
        }
        public void Initialize(int inputSeed = 0)
        {
            foreach (InterestDomainDef interestdomainDef in DefDatabase<InterestDomainDef>.AllDefs)
            {
                GenerateInterestOffsetsForDomain(interestdomainDef, true);
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
                interestOffset[interest.id] = Mathf.Clamp(interestOffsetValue, 35f, 65f);
                if (generateScore)
                {
                    GenerateInterestScore(interest.id);
                }
            }
        }
        public void GenerateInterestScore(int interestId, int maxAttempts = 4)
        {
            float result = Rand.Range(-35f, 35f);
            interestScore[interestId] = result;
        }

        public float GetOrCreateInterestScore(Interest key)
        {
            if (!interestOffset.TryGetValue(key.id, out float offsetValue))
            {
                GenerateInterestOffsetsForDomain(RimpsycheDatabase.InterestDomainDict[key]);
                if (!interestOffset.TryGetValue(key.id, out offsetValue))
                {
                    offsetValue = 50;
                }
            }
            if (!interestScore.TryGetValue(key.id, out float score))
            {
                GenerateInterestScore(key.id);
                if (!interestScore.TryGetValue(key.id, out score))
                {
                    score = 0;
                }
            }
            return Mathf.Clamp(offsetValue + score, 0f, 100f);
        }

        public void SetInterestScore(Interest key, float score)
        {
            float delta = score - GetOrCreateInterestScore(key);
            if (interestScore.TryGetValue(key.id, out float s))
            {
                if ((delta<0f && s == -35f) || (delta>0f && s == 35f)) return;
                interestScore[key.id] = Mathf.Clamp(interestScore[key.id] + delta, -35f, 35f);
            }
        }

        public Interest ChooseInterest()
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
