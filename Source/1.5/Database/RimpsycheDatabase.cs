using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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
                        float absoluteWeightSum = topic.weights.Sum(fw => Mathf.Abs(fw.weight));
                        if (Math.Abs(absoluteWeightSum - 1) > 0.001f) // Use a small tolerance due to floating-point precision
                        {
                            // Log an error if the absolute weight sum is not 1
                            Log.Error($"Facet weight absolute sum for topic {topic.name} is not 1. It is {absoluteWeightSum}");
                        }
                    }
                }
            }
        }
    }
}

