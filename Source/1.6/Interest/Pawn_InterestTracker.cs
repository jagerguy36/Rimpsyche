using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Maux36.RimPsyche
{
    public class Pawn_InterestTracker : IExposable
    {
        private readonly Pawn pawn;
        private readonly CompPsyche compPsyche;
        public Dictionary<int, float> interestOffset = new Dictionary<int, float>(); // 35~65
        public Dictionary<string, float> interestScore = new Dictionary<string, float>(); // -35~35
        private InterestSampler cachedSampler;

        public Pawn_InterestTracker(Pawn p)
        {
            pawn = p;
            compPsyche = p.compPsyche();
        }
        public void Initialize(PsycheData psycheData = null)
        {
            if (psycheData != null)
            {
                interestScore = new Dictionary<string, float>(psycheData.interestScore);
                cachedSampler = null;
                return;
            }
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
                    GenerateInterestScore(interest.name);
                }
            }
        }
        public void GenerateInterestScore(string interestname, int maxAttempts = 4)
        {
            float result = Rand.Range(-35f, 35f);
            interestScore[interestname] = result;
        }

        public float GetOrCreateInterestScore(Interest key)
        {
            if (!interestOffset.TryGetValue(key.id, out float offsetValue))
            {
                GenerateInterestOffsetsForDomain(RimpsycheDatabase.InterestDomainIdDict[key.id]);
                if (!interestOffset.TryGetValue(key.id, out offsetValue))
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
                if ((delta<0f && s == -35f) || (delta>0f && s == 35f)) return;
                interestScore[key.name] = Mathf.Clamp(interestScore[key.name] + delta, -35f, 35f);
                cachedSampler = null;
            }
        }

        public Interest ChooseInterest()
        {
            return GenCollection.RandomElementByWeight(RimpsycheDatabase.InterestList, GetOrCreateInterestScore);
        }

        public Interest SampleInterest()
        {
            cachedSampler ??= new InterestSampler(RimpsycheDatabase.InterestList);
            return cachedSampler?.SampleInterest();
        }

        public Interest ChooseInterest(int poolIndex)
        {
            return RimpsycheDatabase.InterestList.RandomElementByWeight((Interest interest) => (interest.topicPool[poolIndex].Count > 0) ? GetOrCreateInterestScore(interest) : 0f);
        }

        public void NotifyPersonalityDirtied()
        {
            interestOffset.Clear();
            cachedSampler = null;
        }

        // Save
        public void ExposeData()
        {
            Scribe_Collections.Look(ref interestScore, "interestScore", LookMode.Value, LookMode.Value);
        }
    }

    public class InterestSampler
    {
        public int interestCount;
        public float[] probArr;
        public int[] aliasArr;

        public InterestSampler(List<Interest> interestList)
        {
            interestCount = interestList.Count;
            probArr = new float[interestCount];
            aliasArr = new int[interestCount];

            float[] weights = new float[interestCount];
            float sum = 0f;
            for (int i = 0; i < interestList.Count; i++)
            {
                float w = GetOrCreateInterestScore(interestList[i]);
                weights[i] = w;
                sum += w;
            }

            if (sum <= 0)
            {
                interestCount = 0;
                return;
            }

            float scale = interestCount / sum;
            Stack<int> small = new Stack<int>();
            Stack<int> large = new Stack<int>();

            for (int i = 0; i < interestCount; i++)
            {
                weights[i] *= scale;
                if (weights[i] < 1f)
                    small.Push(i);
                else
                    large.Push(i);
            }

            while (small.Count > 0 && large.Count > 0)
            {
                int s = small.Pop();
                int l = large.Pop();

                probArr[s] = weights[s];
                aliasArr[s] = l;

                weights[l] = (weights[l] + weights[s]) - 1f;
                if (weights[l] < 1f)
                    small.Push(l);
                else
                    large.Push(l);
            }

            while (large.Count > 0)
                probArr[large.Pop()] = 1f;

            while (small.Count > 0)
                probArr[small.Pop()] = 1f;
        }
        public Interest SampleInterest()
        {
            var interestList = RimpsycheDatabase.InterestList;
            if (interestCount <= 0) return null;
            int i = Rand.Range(0, interestCount);
            return Rand.Value < probArr[i] ? interestList[i] : interestList[aliasArr[i]];
        }
    }
}
