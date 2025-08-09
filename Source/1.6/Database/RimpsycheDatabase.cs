using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Maux36.RimPsyche
{
    [StaticConstructorOnStartup]
    public class RimpsycheDatabase
    {
        public static HashSet<Interest> InterestList = new();
        public static Dictionary<Interest, InterestDomainDef> InterestDomainDict = new();
        public static Dictionary<string, PersonalityDef> PersonalityDict = new();
        public static Dictionary<Pair<string, int>, List<(string, float, float)>> TraitScopeDatabase = new();
        public static Facet[] AllFacets = (Facet[])Enum.GetValues(typeof(Facet));


        public static Dictionary<string, string> IntensityKeysDefault = new Dictionary<string, string>()
        {
            { "RimPsycheIntensityExtremely", "Extremely" },
            { "RimPsycheIntensityVery", "Very" },
            { "RimPsycheIntensitySomewhat", "Somewhat" },
            { "RimPsycheIntensityMarginally", "Marginally" },
            { "RimPsycheIntensityNeutral", "Neutral" }
        };
        public static string conversationMemoryString = "Conversation about {0}";

        static RimpsycheDatabase()
        {
            Initialize();
        }

        public static void Initialize()
        {
            InteractionDefOf.Chitchat = DefOfRimpsyche.Rimpsyche_Smalltalk;
            InteractionDefOf.DeepTalk = DefOfRimpsyche.Rimpsyche_StartConversation;
            if (LanguageDatabase.activeLanguage.HaveTextForKey("MemoryReportString"))
            {
                conversationMemoryString = "MemoryReportString".Translate();
            }

            //Interest and Topic
            foreach (var interestdomain in DefDatabase<InterestDomainDef>.AllDefs)
            {
                InterestList.AddRange(interestdomain.interests);
                foreach (var interest in interestdomain.interests)
                {
                    InterestDomainDict.Add(interest, interestdomain);
                    foreach (var topic in interest.topics)
                    {
                        //TopicNameList.Add(topic.name);
                        float absoluteWeightSum = 0f;
                        foreach (var fw in topic.weights)
                        {
                            absoluteWeightSum += Mathf.Abs(fw.weight);
                        }
                        if (Math.Abs(absoluteWeightSum - 1) > 0.001f) // Use a small tolerance due to floating-point precision
                        {
                            Log.Error($"Personality weight absolute sum for topic {topic.name} is not 1. It is {absoluteWeightSum}");
                        }
                    }
                }
            }

            //Scope
            foreach (var personalityDef in DefDatabase<PersonalityDef>.AllDefs)
            {
                //Check Personality weight sum
                float absoluteWeightSum = 0f;
                foreach (var fw in personalityDef.scoreWeight)
                {
                    absoluteWeightSum += Mathf.Abs(fw.weight);
                }
                if (Math.Abs(absoluteWeightSum - 1) > 0.0001f) // Use a small tolerance due to floating-point precision
                {
                    Log.Error($"Facet weight absolute sum for topic {personalityDef.label} is not 1. It is {absoluteWeightSum}");
                }

                var scopeList = personalityDef.scopes;
                if (scopeList != null)
                {
                    foreach (var scopeData in scopeList)
                    {
                        var scopeCenter = scopeData.centerOffset;
                        var scopeRange = scopeData.range;
                        if (scopeRange <= 0 || scopeCenter - scopeRange < -1 || scopeCenter + scopeRange > 1)
                        {
                            Log.Error($"Error parsing Scope data of {personalityDef.label}. Either its range is not positive or Its range gets outside of -1 ~ 1.");
                            continue;
                        }
                        var key = new Pair<string, int>(scopeData.traitDefname, scopeData.degree);
                        if (!TraitScopeDatabase.ContainsKey(key))
                        {
                            TraitScopeDatabase[key] = new List<(string, float, float)>();
                        }
                        TraitScopeDatabase[key].Add((personalityDef.defName, scopeData.centerOffset, scopeData.range));
                    }
                }
                PersonalityDict[personalityDef.defName] = personalityDef;
            }
        }

        public static RimpsycheFormula SocialFightChanceMultiplier = new(
            "SocialFightChanceMultiplier",
            (tracker) =>
            {
                float aggressiveness = 1f +  tracker.GetPersonality(PersonalityDefOf.Rimpsyche_Aggressiveness) * 0.4f;
                float emotionality = 1f + tracker.GetPersonality(PersonalityDefOf.Rimpsyche_Emotionality) * 0.2f;
                float compassion = tracker.GetPersonality(PersonalityDefOf.Rimpsyche_Compassion);
                float compassionMult = compassion > 0 ? 1f - compassion * 0.5f : 1f;
                return aggressiveness * emotionality * compassionMult;
            }
        );


        public static Dictionary<Pair<string, int>, List<FacetGate>> TraitGateDatabase = new()
        {
            [new Pair<string, int>("Psychopath", 0)] = new List<FacetGate>
            {
                new(Facet.Compassion, -45f, 5f, 5),
                new(Facet.Humbleness, -25f, 25f, 5),
                new(Facet.Integrity, -45f, 5f, 5),
                new(Facet.Volatility, -45f, 5f, 5),
                new(Facet.Pessimism, -25f, 25f),
                new(Facet.Insecurity, -25f, 25f)
            },
            [new Pair<string, int>("TooSmart", 0)] = new List<FacetGate>
            {
                new(Facet.Intellect, 25f, 25f)
            },
            [new Pair<string, int>("Jealous", 0)] = new List<FacetGate>
            {
                new(Facet.Humbleness, -25f, 25f)
            },
            [new Pair<string, int>("TorturedArtist", 0)] = new List<FacetGate>
            {
                new(Facet.Imagination, 25f, 25f)
            },
            [new Pair<string, int>("NaturalMood", -2)] = new List<FacetGate>
            {
                new(Facet.Pessimism, 25f, 25f)
            },
            [new Pair<string, int>("NaturalMood", -1)] = new List<FacetGate>
            {
                new(Facet.Pessimism, 25f, 25f)
            },
            [new Pair<string, int>("NaturalMood", 1)] = new List<FacetGate>
            {
                new(Facet.Pessimism, -25f, 25f)
            },
            [new Pair<string, int>("NaturalMood", 2)] = new List<FacetGate>
            {
                new(Facet.Pessimism, -25f, 25f)
            },
            [new Pair<string, int>("Nerves", -2)] = new List<FacetGate>
            {
                new (Facet.Volatility, 25f, 25f)
            },
            [new Pair<string, int>("Nerves", -1)] = new List<FacetGate>
            {
                new (Facet.Volatility, 25f, 25f)
            },
            [new Pair<string, int>("Nerves", 1)] = new List<FacetGate>
            {
                new(Facet.Volatility, -25f, 25f)
            },
            [new Pair<string, int>("Nerves", 2)] = new List<FacetGate>
            {
                new(Facet.Volatility, -25f, 25f)
            },
            [new Pair<string, int>("Neurotic", 1)] = new List<FacetGate>
            {
                new(Facet.Orderliness, 25f, 25f)
            },
            [new Pair<string, int>("Neurotic", 2)] = new List<FacetGate>
            {
                new(Facet.Orderliness, 25f, 25f)
            },
            [new Pair<string, int>("Industriousness", -2)] = new List<FacetGate>
            {
                new(Facet.Industriousness, -25f, 25f)
            },
            [new Pair<string, int>("Industriousness", -1)] = new List<FacetGate>
            {
                new(Facet.Industriousness, -25f, 25f)
            },
            [new Pair<string, int>("Industriousness", 1)] = new List<FacetGate>
            {
                new(Facet.Industriousness, 25f, 25f)
            },
            [new Pair<string, int>("Industriousness", 2)] = new List<FacetGate>
            {
                new(Facet.Industriousness, 25f, 25f)
            },
            [new Pair<string, int>("Recluse", 0)] = new List<FacetGate>
            {
                new(Facet.Sociability, -25f, 25f)
            },
            [new Pair<string, int>("Bloodlust", 0)] = new List<FacetGate>
            {
                new(Facet.Compassion, -25f, 25f)
            },
            [new Pair<string, int>("Kind", 0)] = new List<FacetGate>
            {
                new(Facet.Compassion, 25f, 25f)
            }
        };
        public static Dictionary<string, List<FacetGate>> GeneGateDatabase = new()
        {
            ["Learning_Slow"] = new List<FacetGate>
            {
                new(Facet.Intellect, -25f, 25f)
            },
            ["Learning_Fast"] = new List<FacetGate>
            {
                new(Facet.Intellect, 25f, 25f)
            },
            ["Mood_Depressive"] = new List<FacetGate>
            {
                new(Facet.Pessimism, 25f, 25f)
            },
            ["Mood_Pessimist"] = new List<FacetGate>
            {
                new(Facet.Pessimism, 25f, 25f)
            },
            ["Mood_Optimist"] = new List<FacetGate>
            {
                new(Facet.Pessimism, -25f, 25f)
            },
            ["Mood_Sanguine"] = new List<FacetGate>
            {
                new(Facet.Pessimism, -25f, 25f)
            },
            ["Aggression_DeadCalm"] = new List<FacetGate>
            {
                new(Facet.Assertiveness, -25f, 25f),
                new(Facet.Volatility, -25f, 25f)
            },
            ["Aggression_Aggressive"] = new List<FacetGate>
            {
                new(Facet.Assertiveness, 25f, 25f),
                new(Facet.Volatility, 25f, 25f)
            },
            ["Aggression_HyperAggressive"] = new List<FacetGate>
            {
                new(Facet.Assertiveness, 25f, 25f),
                new(Facet.Volatility, 25f, 25f)
            },
        };
    }
}

