using System.Collections.Generic;
using Verse;

namespace Maux36.RimPsyche
{
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

        public Topic GetRandomTopic(bool childInvolved = false, bool allowNSWF = false)
        {
            int topicCount = topics.Count;
            int eligibleCount = 0;
            for (int i = 0; i < topicCount; i++)
            {
                var t = topics[i];
                if (childInvolved && (!t.allowChild || t.NSFW)) continue;
                if (!allowNSWF && t.NSFW) continue;
                eligibleCount++;
            }

            if (eligibleCount == 0)
            {
                return null;
            }
            int randomIndex = Rand.Range(0, eligibleCount);

            int currentEligibleIndex = 0;
            for (int i = 0; i < topicCount; i++)
            {
                var t = topics[i];
                if (childInvolved && (!t.allowChild || t.NSFW)) continue;
                if (!allowNSWF && t.NSFW) continue;

                if (currentEligibleIndex == randomIndex)
                    return t;

                currentEligibleIndex++;
            }
            return null;
        }
        public float GetAverageAlignment(CompPsyche pawnPsyche, CompPsyche otherPawnPsyche)
        {
            float result = 0f;
            int topicCount = topics.Count;
            for (int i = 0; i < topicCount; i++)
            {
                var t = topics[i];
                result += t.GetScore(pawnPsyche, otherPawnPsyche, out _);
            }
            return result;
        }
    }
}
