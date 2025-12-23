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

        public Topic GetRandomTopic(bool isInitiatorAdult, bool isRecipientAdult, bool allowNSWF = false)
        {
            int topicCount = topics.Count;
            int eligibleCount = 0;
            bool childInvolved = !isInitiatorAdult | !isRecipientAdult;

            for (int i = 0; i < topicCount; i++)
            {
                var t = topics[i];
                if (!IsValid(isInitiatorAdult, isRecipientAdult, allowNSWF, t.disallowedInit, t.disallowedReci, t.isNSFW)) continue;
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
                if (!IsValid(isInitiatorAdult, isRecipientAdult, allowNSWF, t.disallowedInit, t.disallowedReci, t.isNSFW)) continue;

                if (currentEligibleIndex == randomIndex)
                    return t;

                currentEligibleIndex++;
            }
            return null;
        }
        public static bool IsValid(bool isInitiatorAdult, bool isRecipientAdult, bool allowNSFW, Demographic initException, Demographic reciException, bool isNSFW)
        {
            if (isNSFW && !allowNSFW) return false;

            int nsfwMask = isNSFW ? (int)Exception.Child : 0;
            
            int iBit = (int)(isInitiatorAdult ? Exception.Adult : Exception.Child);
            int rBit = (int)(isRecipientAdult ? Exception.Adult : Exception.Child);

            return ((iBit & ((int)initException | nsfwMask)) | 
                    (rBit & ((int)reciException | nsfwMask))) == 0;
        }
        public float GetAverageAlignment(CompPsyche pawnPsyche, CompPsyche otherPawnPsyche, bool tamperNeg = true)
        {
            int topicCount = topics.Count;
            float result = 0f;
            for (int i = 0; i < topicCount; i++)
            {
                var tScore = topics[i].GetScore(pawnPsyche, otherPawnPsyche, out _);
                if(tamperNeg && tScore < 0f)
                {
                    tScore *= 0.2f;
                }
                result += tScore;
            }
            return result / topicCount;
        }
    }
}
