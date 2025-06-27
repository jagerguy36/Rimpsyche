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
        public static Dictionary<Pair<string, int>, List<(Facet, float, float)>> TraitGateDatabase = new();
        public static Dictionary<Pair<string, int>, List<(string, float, float)>> TraitScopeDatabase = new();
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
                            Log.Error($"Facet weight absolute sum for topic {topic.name} is not 1. It is {absoluteWeightSum}");
                        }
                    }
                }
            }

            foreach (var personalityDef in DefDatabase<PersonalityDef>.AllDefs)
            {
                var scopeList = personalityDef.scopes;
                if(scopeList != null)
                {
                    foreach (var scopeData in scopeList)
                    {
                        var key = new Pair<string, int>(scopeData.traitDefname, scopeData.degree);
                        if (!TraitScopeDatabase.ContainsKey(key))
                        {
                            TraitScopeDatabase[key] = new List<(string, float, float)>();
                        }
                        TraitScopeDatabase[key].Add((personalityDef.defName, scopeData.lowEnd, scopeData.highEnd));
                    }
                }
            }
        }


        //public static Dictionary<Pair<string, int>, List<(Facet, float, float)>> TraitGateDatabase = new()
        //{
        //    [new Pair<string, int>("NaturalMood", 2)] = new List<(Facet, float, float)>
        //    {
        //        (Facet.Pessimism, -50f, -25f)
        //    },
        //    [new Pair<string, int>("NaturalMood", 1)] = new List<(Facet, float, float)>
        //    {
        //        (Facet.Pessimism, -50f, 0f)
        //    },
        //    [new Pair<string, int>("NaturalMood", -1)] = new List<(Facet, float, float)>
        //    {
        //        (Facet.Pessimism, 0f, 50f)
        //    },
        //    [new Pair<string, int>("NaturalMood", -2)] = new List<(Facet, float, float)>
        //    {
        //        (Facet.Pessimism, 25f, 50f)
        //    },
        //    //[new Pair<string, int>("Kind", 0)] = new List<(Facet, float, float)>
        //    //{
        //    //    (Facet.Cooperation, 0f, 50f)
        //    //},
        //};

        //public static Dictionary<Pair<string, int>, List<(string, float, float)>> TraitScopeDatabase = new()
        //{
        //    [new Pair<string, int>("Kind", 0)] = new List<(string, float, float)>
        //    {
        //       ("Rimpsyche_Aggressiveness", -1f, 0f)
        //    },
        //};
    }
}

