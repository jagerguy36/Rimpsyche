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
        public static float maxFacetLabelWidth = 130f;
        public static float maxInterestLabelWidth = 130f;
        public static float maxPersonalityLabelWidth = 130f;

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
            ModCompat();
        }

        public static void Initialize()
        {
            InteractionDefOf.Chitchat = DefOfRimpsyche.Rimpsyche_Smalltalk;
            InteractionDefOf.DeepTalk = DefOfRimpsyche.Rimpsyche_StartConversation;
            if (LanguageDatabase.activeLanguage.HaveTextForKey("MemoryReportString"))
            {
                conversationMemoryString = "MemoryReportString".Translate();
            }

            foreach (var facet in InterfaceComponents.FacetNotation)
            {
                var (_, leftLabel, rightLabel, _, _) = facet.Value;
                maxFacetLabelWidth = Mathf.Max(maxFacetLabelWidth, 5f + Text.CalcSize(leftLabel.CapitalizeFirst()).x);
                maxFacetLabelWidth = Mathf.Max(maxFacetLabelWidth, 5f + Text.CalcSize(rightLabel.CapitalizeFirst()).x);
            }

            //Interest and Topic
            foreach (var interestdomain in DefDatabase<InterestDomainDef>.AllDefs)
            {
                InterestList.AddRange(interestdomain.interests);
                foreach (var interest in interestdomain.interests)
                {
                    maxInterestLabelWidth = Mathf.Max(maxInterestLabelWidth, 5f + Text.CalcSize(interest.label).x);
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
                maxPersonalityLabelWidth = Mathf.Max(maxPersonalityLabelWidth, 5f + Text.CalcSize(personalityDef.low.CapitalizeFirst()).x);
                maxPersonalityLabelWidth = Mathf.Max(maxPersonalityLabelWidth, 5f + Text.CalcSize(personalityDef.high.CapitalizeFirst()).x);
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
                float aggressiveness = 1f +  tracker.GetPersonality(PersonalityDefOf.Rimpsyche_Aggressiveness) * 0.4f; // 0.6~1.4
                float emotionality = 1f + tracker.GetPersonality(PersonalityDefOf.Rimpsyche_Emotionality) * 0.2f; // 0.8~1.2
                float compassion = tracker.GetPersonality(PersonalityDefOf.Rimpsyche_Compassion);
                float compassionMult = compassion > 0 ? 1f - compassion * 0.5f : 1f; //0.5~1
                return aggressiveness * emotionality * compassionMult;// 0.24~1.68
            },
            RimpsycheFormulaManager.FormulaIdDict
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
            ["KillThirst"] = new List<FacetGate>
            {
                new(Facet.Compassion, -25f, 25f)
            }
        };

        public static void ModCompat()
        {
            if (ModsConfig.IsActive("vanillaexpanded.vanillatraitsexpanded"))
            {
                Log.Message("[Rimpsyche] VTE gate data added");
                TraitGateDatabase[new Pair<string, int>("VTE_Eccentric", 0)] = new List<FacetGate>
                {
                    new FacetGate(Facet.Intellect, 25f, 25f)
                };
                TraitGateDatabase[new Pair<string, int>("VTE_Submissive", 0)] = new List<FacetGate>
                {
                    new FacetGate(Facet.Assertiveness, -25f, 25f)
                };
                TraitGateDatabase[new Pair<string, int>("VTE_Dunce", 0)] = new List<FacetGate>
                {
                    new FacetGate(Facet.Intellect, -25f, 25f)
                };
                 TraitGateDatabase[new Pair<string, int>("VTE_Snob", 0)] = new List<FacetGate>
                {
                    new FacetGate(Facet.Humbleness, -25f, 25f)
                };
                 TraitGateDatabase[new Pair<string, int>("VTE_Anxious", 0)] = new List<FacetGate>
                {
                    new FacetGate(Facet.Insecurity, 25f, 25f)
                };
                 TraitGateDatabase[new Pair<string, int>("VTE_Prodigy", 0)] = new List<FacetGate>
                {
                    new FacetGate(Facet.Intellect, 25f, 25f)
                };
                TraitGateDatabase[new Pair<string, int>("VTE_MadSurgeon", 0)] = new List<FacetGate>
                {
                    new FacetGate(Facet.Compassion, -45f, 5f, 5)
                };
                TraitGateDatabase[new Pair<string, int>("VTE_WorldWeary", 0)] = new List<FacetGate>
                {
                    new FacetGate(Facet.Pessimism, 25f, 25f)
                };
                TraitGateDatabase[new Pair<string, int>("VTE_Academian", 0)] = new List<FacetGate>
                {
                    new FacetGate(Facet.Intellect, 25f, 25f)
                };
            }

            if (ModsConfig.IsActive("vanillaracesexpanded.android"))
            {
                Log.Message("[Rimpsyche] VRE Android gate data added");
                GeneGateDatabase["VREA_PsychologyDisabled"] = new List<FacetGate>
                {
                    new FacetGate(Facet.Imagination, 0f, 0f, 10),
                    new FacetGate(Facet.Intellect, 0f, 0f, 10),
                    new FacetGate(Facet.Curiosity, 0f, 0f, 10),
                    new FacetGate(Facet.Industriousness, 20f, 0f, 10),
                    new FacetGate(Facet.Orderliness, 0f, 0f, 10),
                    new FacetGate(Facet.Integrity, 20f, 0f, 10),
                    new FacetGate(Facet.Sociability, 20f, 0f, 10),
                    new FacetGate(Facet.Assertiveness, 0f, 0f, 10),
                    new FacetGate(Facet.Enthusiasm, 0f, 0f, 10),
                    new FacetGate(Facet.Compassion, 30f, 0f, 10),
                    new FacetGate(Facet.Cooperation, 30f, 0f, 10),
                    new FacetGate(Facet.Humbleness, 30f, 0f, 10),
                    new FacetGate(Facet.Volatility, -30f, 0f, 10),
                    new FacetGate(Facet.Pessimism, -30f, 0f, 10),
                    new FacetGate(Facet.Insecurity, 0f, 0f, 10),
                };
            }


            if (ModsConfig.IsActive("consolidatedtraits.lc.rw"))
            {
                Log.Message("[Rimpsyche] ConsolidatedTraits gate data added");
                TraitGateDatabase[new Pair<string, int>("RCT_Aesthete", 0)] = new List<FacetGate>
                {
                    new FacetGate(Facet.Imagination, 40f, 10f)
                };
                TraitGateDatabase[new Pair<string, int>("RCT_Dunce", 0)] = new List<FacetGate>
                {
                    new FacetGate(Facet.Intellect, -25f, 25f)
                };
                TraitGateDatabase[new Pair<string, int>("RCT_Savant", 0)] = new List<FacetGate>
                {
                    new FacetGate(Facet.Intellect, 40f, 10f)
                };
            }


            if (ModsConfig.IsActive("avius.badpeople"))
            {
                Log.Message("[Rimpsyche] BadPeople gate data added");
                TraitGateDatabase[new Pair<string, int>("BadPeople_Evil", 0)] = new List<FacetGate>
                {
                    new FacetGate(Facet.Compassion, -35f, 15f),
                    new FacetGate(Facet.Integrity, -35f, 15f)
                };
                TraitGateDatabase[new Pair<string, int>("BadPeople_Kinslayer", 0)] = new List<FacetGate>
                {
                    new FacetGate(Facet.Compassion, -35f, 15f),
                    new FacetGate(Facet.Integrity, -35f, 15f)
                };
            }
        }
    }
}

