using System.Collections.Generic;
using Verse;

namespace Maux36.RimPsyche
{
    public class Interest
    {
        public string name;
        public List<FacetWeight> scoreWeight;
        public List<Topic> topics;

        public Topic GetRandomTopic()
        {
            int topicIndex = Rand.Range(0, topics.Count);
            return topics[topicIndex];
        }
    }
}
