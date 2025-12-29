using System.Collections.Generic;
using System;
using Verse;

namespace Maux36.RimPsyche
{
    [Flags]
    public enum ParticipantMask : byte
    {
        None = 0,
        AA   = 1 << 0,
        AAs  = 1 << 1,
        AC   = 1 << 2,
        CA   = 1 << 3,
        CC   = 1 << 4,
        All  = AA | AAs | AC | CA | CC
    }
    public class InterestDomainDef : Def
    {
        public List<FacetWeight> scoreWeight;
        public List<Interest> interests;
    }
    public class Interest
    {
        [NoTranslate]
        public string name;
        public int id = -1;
        public string label;
        public string description;
        public List<FacetWeight> scoreWeight;
        public List<Topic> topics;
        // Index 0=AA, 1=AAs, 2=AC, 3=CA, 4=CCI
        public List<int>[] topicPool;

        public Topic GetRandomTopic(bool initAdult, bool reciAdult, bool limitNSFW = false)
        {
            int bits = (initAdult ? 2 : 0) | (reciAdult ? 1 : 0);
            int poolIndex = bits switch
            {
                0b11 => limitNSFW ? 1 : 0, // 1 = AAs, 0 = AA
                0b10 => 2, // 2 = AC
                0b01 => 3, // 3 = CA
                0b00 => 4, // 4 = CC
                _ => -1
            };
            if (topicPool[poolIndex].Count == 0) return null;
            var pool = topicPool[poolIndex];
            int randomIndex = Rand.Range(0, pool.Count);
            return topics[pool[randomIndex]];
        }
        public float GetAverageAlignment(CompPsyche pawnPsyche, CompPsyche otherPawnPsyche, bool weedOutlier = true)
        {
            int topicCount = topics.Count;
            float outlier = 0f;
            float result = 0f;
            for (int i = 0; i < topicCount; i++)
            {
                var tScore = topics[i].GetScore(pawnPsyche, otherPawnPsyche, out _);
                if(weedOutlier && tScore < outlier)
                {
                    outlier = tScore;
                }
                result += tScore;
            }
            result -= outlier;
            if (weedOutlier)
                return result / (topicCount - 1);
            return result / topicCount;
        }
    }
}
