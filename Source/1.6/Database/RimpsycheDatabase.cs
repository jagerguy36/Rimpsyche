using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

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
        public static Facet[] AllFacets = (Facet[])Enum.GetValues(typeof(Facet));
        //public static Dictionary<Facet, float> facetAccumulated = new();
        static RimpsycheDatabase()
        {
            //Interest and Topic
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
                        //FOR DEVELOPMENT
                        //foreach(var eachweight in topic.weights)
                        //{
                        //    if (facetAccumulated.ContainsKey(eachweight.facet))
                        //    {
                        //        facetAccumulated[eachweight.facet] += Mathf.Abs(eachweight.weight);
                        //    }
                        //    else
                        //    {
                        //        facetAccumulated.Add(eachweight.facet, Mathf.Abs(eachweight.weight));
                        //    }
                        //}
                    }
                }
                //FOR DEVELOPMENT
                //Log.Message(string.Join("\n", facetAccumulated.Select(pair => $"{pair.Key}: {pair.Value}")));
            }

            //Scope
            foreach (var personalityDef in DefDatabase<PersonalityDef>.AllDefs)
            {
                //Check Personality weight sum
                float absoluteWeightSum = personalityDef.scoreWeight.Sum(fw => Mathf.Abs(fw.weight));
                if (Math.Abs(absoluteWeightSum - 1) > 0.0001f) // Use a small tolerance due to floating-point precision
                {
                    Log.Error($"Facet weight absolute sum for topic {personalityDef.label} is not 1. It is {absoluteWeightSum}");
                }

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

        public static RimpsycheMultiplier SocialFightChanceMultiplier = new(
            "SocialFightChanceMultiplier",
            (tracker) =>
            {
                float aggressiveness = 1f +  tracker.GetPersonality(PersonalityDefOf.Rimpsyche_Aggressiveness) * 0.9f;
                float emotionality = 1f + tracker.GetPersonality(PersonalityDefOf.Rimpsyche_Emotionality) * 0.4f;
                float compassion = tracker.GetPersonality(PersonalityDefOf.Rimpsyche_Compassion);
                float compassionMult = compassion > 0 ? 1f - compassion * 0.5f : 1f;
                return aggressiveness * emotionality * compassionMult;
            }
        );


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

