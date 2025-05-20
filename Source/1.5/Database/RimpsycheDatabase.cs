using System.Collections.Generic;
using Verse;

namespace Maux36.RimPsyche
{
    [StaticConstructorOnStartup]
    public class RimpsycheDatabase
    {
        public static HashSet<Interest> InterestList = new();
        public static HashSet<string> InterestNameList = new();
        public static Dictionary<Interest, InterestDomainDef> InterestDomainDict = new();
        public static HashSet<Topic> TopicList = new();
        public static HashSet<string> TopicNameList = new();
        static RimpsycheDatabase()
        {
            foreach (var interestdomain in DefDatabase<InterestDomainDef>.AllDefs)
            {
                InterestList.AddRange(interestdomain.interests);
                foreach (var interest in interestdomain.interests)
                {
                    InterestDomainDict.Add(interest, interestdomain);
                    InterestNameList.Add(interest.name);
                    TopicList.AddRange(interest.topics);
                    foreach(var topic in interest.topics)
                    {
                        TopicNameList.Add(topic.name);
                    }
                }
            }
        }
    }
}

