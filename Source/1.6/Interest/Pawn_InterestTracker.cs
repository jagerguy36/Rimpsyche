using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Maux36.RimPsyche
{
    public class Pawn_InterestTracker : IExposable
    {
        private readonly Pawn pawn;
        private readonly CompPsyche compPsyche;
        //Domain offset range should be within -10~10
        //Interest offset range should be within -10~10
        //50 + DomainOffset + InterestOffset range is clamped to 35~65
        public Dictionary<int, float> adjustedInterestScore = new Dictionary<int, float>(); // (35~65) + (-35~35)
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
                GenerateAdjustedInterestScoreForDomain(interestdomainDef, true);
            }
        }

        public void GenerateAdjustedInterestScoreForDomain(InterestDomainDef interestdomainDef, bool generateScore = false) // 0~100
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
                interestOffsetValue = Mathf.Clamp(interestOffsetValue, 35f, 65f);
                var interestScore = GetOrGenerateInterestScore(interest);
                adjustedInterestScore[interest.id] = interestOffsetValue + interestScore;
            }
        }
        private void GenerateInterestScore(string interestname, int maxAttempts = 4)
        {
            float result = Rand.Range(-35f, 35f);
            interestScore[interestname] = result;
        }
        public float GetOrGenerateInterestScore(Interest key)
        {
            if (!interestScore.TryGetValue(key.name, out float score))
            {
                GenerateInterestScore(key.name);
                if (!interestScore.TryGetValue(key.name, out score))
                {
                    score = 0;
                }
            }
            return score;
        }

        public float GetOrGenerateAdjustedInterestScoreRaw(Interest key)
        {
            if (!adjustedInterestScore.TryGetValue(key.id, out float adjustedValue))
            {
                GenerateAdjustedInterestScoreForDomain(RimpsycheDatabase.InterestDomainIdDict[key.id]);
                if (!adjustedInterestScore.TryGetValue(key.id, out adjustedValue))
                {
                    adjustedValue = 50;
                }
            }
            return adjustedValue;
        }
        public float GetOrGenerateAdjustedInterestScore(Interest key)
        {
            return Mathf.Clamp(GetOrGenerateAdjustedInterestScoreRaw(key), 0.01f, 100f);
        }

        public void SetInterestScore(Interest interest, float score)
        {
            float delta = score - GetOrGenerateAdjustedInterestScoreRaw(interest);
            if (interestScore.TryGetValue(interest.name, out float originalScore))
            {
                if ((delta<0f && originalScore == -35f) || (delta>0f && originalScore == 35f)) return;
                float scoreAfter = Mathf.Clamp(interestScore[interest.name] + delta, -35f, 35f);
                interestScore[interest.name] = scoreAfter;
                adjustedInterestScore[interest.id] += scoreAfter - originalScore;
                cachedSampler = null;
            }
        }

        public Interest SampleInterest()
        {
            cachedSampler ??= new InterestSampler(RimpsycheDatabase.InterestList, this);
            return cachedSampler?.SampleInterest();
        }

        public Interest ChooseInterest(int poolIndex)
        {
            return RimpsycheDatabase.InterestList.RandomElementByWeight((Interest interest) => (interest.topicPool[poolIndex].Count > 0) ? GetOrGenerateAdjustedInterestScore(interest) : 0f);
        }

        public void NotifyPersonalityDirtied()
        {
            adjustedInterestScore.Clear();
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

        public InterestSampler(List<Interest> interestList, Pawn_InterestTracker tracker)
        {
            interestCount = interestList.Count;
            probArr = new float[interestCount];
            aliasArr = new int[interestCount];

            float[] weights = new float[interestCount];
            float sum = 0f;
            for (int i = 0; i < interestList.Count; i++)
            {
                float w = tracker.GetOrGenerateAdjustedInterestScore(interestList[i]);
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
