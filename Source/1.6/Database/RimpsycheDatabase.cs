using KTrie;
using RimWorld;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Maux36.RimPsyche
{
    [StaticConstructorOnStartup]
    public class RimpsycheDatabase
    {
        public static HashSet<int> MindlessDefShorthashSet = new();
        public static Dictionary<string, InterestDomainDef> InterestDomainDict = new();
        public static Dictionary<int, InterestDomainDef> InterestDomainIdDict = new();
        public static Dictionary<int, string> InterstTopicStringDict = new();
        public static HashSet<Interest> InterestList = new();
        public static Dictionary<string, Topic> TopicDict = new();
        public static Dictionary<int, Topic> TopicIdDict = new();
        public static Dictionary<string, PersonalityDef> PersonalityDict = new();
        public static Dictionary<int, List<(int, float, float)>> TraitScopeDatabase = new();
        public static Dictionary<int, List<FacetGate>> TraitGateDatabase = new() { };
        public static Dictionary<int, List<FacetGate>> GeneGateDatabase = new() { };
        public static List<PreferenceDef> OrderedRomPreferenceDefs = new();
        public static List<PreferenceDef> OrderedSexPreferenceDefs = new();
        public static Facet[] AllFacets = (Facet[])Enum.GetValues(typeof(Facet));
        public static float maxFacetLabelWidth = 130f;
        public static float maxInterestLabelWidth = 130f;
        public static float maxSexualityLabelWidth = 70f;
        public static float maxRightsideLabelWidth = 130f;
        public static float maxPersonalityLabelWidth = 130f;
        public static float totalPreferenceEditorfHeight = 0f;

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
            RegisterBaseGates();
            ModCompat();
        }

        public static void Initialize()
        {
            InteractionDefOf.Chitchat = DefOfRimpsyche.Rimpsyche_Smalltalk;
            InteractionDefOf.DeepTalk = DefOfRimpsyche.Rimpsyche_StartConversation;

            //Sexuality Label consideration
            if (Rimpsyche.SexualityModuleLoaded)
            {
                var maleLabelWith = Text.CalcSize("RPC_AttractionMale".Translate()).x;
                maxSexualityLabelWidth = Mathf.Max(maleLabelWith, maxSexualityLabelWidth);
                var femaleLabelWith = Text.CalcSize("RPC_AttractionFemale".Translate()).x;
                maxSexualityLabelWidth = Mathf.Max(femaleLabelWith, maxSexualityLabelWidth);
                var driveLabelWith = Text.CalcSize("RPC_SexDrive".Translate()).x;
                maxSexualityLabelWidth = Mathf.Max(driveLabelWith, maxSexualityLabelWidth);
            }
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
                foreach (var interest in interestdomain.interests)
                {
                    List<string> topicStrings = [];
                    InterestList.Add(interest);
                    interest.id = InterestList.Count;
                    maxInterestLabelWidth = Mathf.Max(maxInterestLabelWidth, 5f + Text.CalcSize(interest.label).x);
                    InterestDomainDict.Add(interest.name, interestdomain);
                    InterestDomainIdDict.Add(interest.id, interestdomain);
                    foreach (var topic in interest.topics)
                    {
                        topicStrings.Add("  - " + topic.label.CapitalizeFirst());
                        topic.id = TopicIdDict.Count();
                        TopicDict[topic.name] = topic;
                        TopicIdDict[topic.id] = topic;
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
                    InterstTopicStringDict[interest.id] = string.Join("\n", topicStrings);
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
                    var seenTraits = new HashSet<int>();
                    foreach (var scopeData in scopeList)
                    {
                        var scopeCenter = scopeData.centerOffset;
                        var scopeRange = scopeData.range;
                        if (scopeRange <= 0 || scopeCenter - scopeRange < -1 || scopeCenter + scopeRange > 1)
                        {
                            Log.Error($"Error parsing Scope data of {personalityDef.label}. Either its range is not positive or Its range gets outside of -1 ~ 1.");
                            continue;
                        }
                        var traitDef = DefDatabase<TraitDef>.GetNamed(scopeData.traitDefname, false);
                        if (traitDef == null)
                        {
                            Log.Warning($"[Rimpsyche] Could not find TraitDef named '{scopeData.traitDefname}'.");
                            continue;
                        }
                        if (scopeData.degree < -256 || 256 < scopeData.degree)
                        {
                            Log.Error($"[Rimpsyche] A scope for {scopeData.traitDefname} has a degree of {scopeData.degree}. Rimpsyche only supports trait degree between -256 ~ 256. Report this to the mod author.");
                            continue;
                        }
                        int key = (traitDef.shortHash << 16) | (scopeData.degree + 256);
                        if (seenTraits.Contains(key))
                        {
                            Log.Error($"[Rimpsyche] PersonalityDef {personalityDef.defName} is being double-scoped by {scopeData.traitDefname} ({scopeData.degree}). It is possible multiple mods are trying to scope this personality using the same trait. This will incur inconsistency and critical error during Personality evaluation.");
                        }
                        seenTraits.Add(key);
                        if (!TraitScopeDatabase.ContainsKey(key))
                        {
                            TraitScopeDatabase[key] = new List<(int, float, float)>();
                        }
                        TraitScopeDatabase[key].Add((personalityDef.shortHash, scopeData.centerOffset, scopeData.range));
                    }
                }
                PersonalityDict[personalityDef.defName] = personalityDef;
            }

            if (Rimpsyche.SexualityModuleLoaded)
                maxRightsideLabelWidth = Mathf.Max(maxSexualityLabelWidth, maxInterestLabelWidth);
            //Preference
            var OrderedPreferenceDefs = DefDatabase<PreferenceDef>.AllDefs.OrderByDescending(prefDef => prefDef.priority).ToList();
            OrderedRomPreferenceDefs = OrderedPreferenceDefs.Where(prefDef => prefDef.category == RimpsychePrefCategory.Romantic).ToList();
            OrderedSexPreferenceDefs = OrderedPreferenceDefs.Where(prefDef => prefDef.category == RimpsychePrefCategory.Physical).ToList();
            foreach (var prefDef in OrderedPreferenceDefs)
            {
                totalPreferenceEditorfHeight += prefDef.worker.EditorHeight;
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

        public static RimpsycheFormula TalkFactor = new(
            "TalkFactor",
            (tracker) =>
            {
                float talkativeness = tracker.GetPersonality(PersonalityDefOf.Rimpsyche_Talkativeness);
                float playfulness = tracker.GetPersonality(PersonalityDefOf.Rimpsyche_Playfulness);
                float gravity = Mathf.Min(0, playfulness) * Mathf.Min(0f, talkativeness) * 0.75f;
                return 1.75f + (0.75f * talkativeness) + gravity;
            },
            RimpsycheFormulaManager.FormulaIdDict
        );

        public static RimpsycheFormula AssertBase = new(
            "AssertBase",
            (tracker) =>
            {
                float tact = tracker.GetPersonality(PersonalityDefOf.Rimpsyche_Tact);
                float talkativeness = tracker.GetPersonality(PersonalityDefOf.Rimpsyche_Talkativeness);
                return tact * (talkativeness + 1) * 0.5f;
            },
            RimpsycheFormulaManager.FormulaIdDict
        );

        public static RimpsycheFormula ReceiveBase = new(
            "ReceiveBase",
            (tracker) =>
            {
                float openness = tracker.GetPersonality(PersonalityDefOf.Rimpsyche_Openness);
                float trust = tracker.GetPersonality(PersonalityDefOf.Rimpsyche_Trust);
                return openness * (trust + 1) * 0.5f;
            },
            RimpsycheFormulaManager.FormulaIdDict
        );

        public static RimpsycheFormula Fervor = new(
            "Fervor",
            (tracker) =>
            {
                float passion = tracker.GetPersonality(PersonalityDefOf.Rimpsyche_Passion);
                float aggressiveness = tracker.GetPersonality(PersonalityDefOf.Rimpsyche_Aggressiveness);
                return 0.1f * (passion - aggressiveness);
            },
            RimpsycheFormulaManager.FormulaIdDict
        );

        public static void RegisterTraitGate(Pair<string, int> traitPair, List<FacetGate> gate)
        {
            string defName = traitPair.First;
            int degree = traitPair.Second;
            var traitDef = DefDatabase<TraitDef>.GetNamed(defName, false);
            if (traitDef != null)
            {
                if (degree < -256 || 256 < degree)
                {
                    Log.Error($"[Rimpsyche] A scope for {traitDef.defName} has a degree of {degree}. Rimpsyche only supports trait degree between -256 ~ 256. Report this to the mod author.");
                    return;
                }
                int key = (traitDef.shortHash << 16) | (degree + 256);
                TraitGateDatabase[key] = gate;
            }
            else
            {
                Log.Warning($"[Rimpsyche] Could not find TraitDef named '{defName}'.");
            }
        }
        public static void RegisterGeneGate(string defName, List<FacetGate> gate)
        {
            var geneDef = DefDatabase<GeneDef>.GetNamed(defName, false);
            if (geneDef != null)
            {
                GeneGateDatabase[geneDef.shortHash] = gate;
            }
            else
            {
                Log.Warning($"[Rimpsyche] Could not find GeneDef named '{defName}'.");
            }
        }
        public static void RegisterBaseGates()
        {
            RegisterTraitGate(new Pair<string, int>("Psychopath", 0), new List<FacetGate>
            {
                new(Facet.Compassion, -45f, 5f, 5),
                new(Facet.Humbleness, -25f, 25f, 5),
                new(Facet.Integrity, -45f, 5f, 5),
                new(Facet.Volatility, -45f, 5f, 5),
                new(Facet.Pessimism, -25f, 25f),
                new(Facet.Insecurity, -25f, 25f)
            });
            RegisterTraitGate(new Pair<string, int>("TooSmart", 0), new List<FacetGate>
            {
                new(Facet.Intellect, 25f, 25f)
            });
            RegisterTraitGate(new Pair<string, int>("Jealous", 0), new List<FacetGate>
            {
                new(Facet.Humbleness, -25f, 25f)
            });
            RegisterTraitGate(new Pair<string, int>("TorturedArtist", 0), new List<FacetGate>
            {
                new(Facet.Imagination, 25f, 25f)
            });
            RegisterTraitGate(new Pair<string, int>("NaturalMood", -2), new List<FacetGate>
            {
                new(Facet.Pessimism, 25f, 25f)
            });
            RegisterTraitGate(new Pair<string, int>("NaturalMood", -1), new List<FacetGate>
            {
                new(Facet.Pessimism, 25f, 25f)
            });
            RegisterTraitGate(new Pair<string, int>("NaturalMood", 1), new List<FacetGate>
            {
                new(Facet.Pessimism, -25f, 25f)
            });
            RegisterTraitGate(new Pair<string, int>("NaturalMood", 2), new List<FacetGate>
            {
                new(Facet.Pessimism, -25f, 25f)
            });
            RegisterTraitGate(new Pair<string, int>("Nerves", -2), new List<FacetGate>
            {
                new (Facet.Volatility, 25f, 25f)
            });
            RegisterTraitGate(new Pair<string, int>("Nerves", -1), new List<FacetGate>
            {
                new (Facet.Volatility, 25f, 25f)
            });
            RegisterTraitGate(new Pair<string, int>("Nerves", 1), new List<FacetGate>
            {
                new(Facet.Volatility, -25f, 25f)
            });
            RegisterTraitGate(new Pair<string, int>("Nerves", 2), new List<FacetGate>
            {
                new(Facet.Volatility, -25f, 25f)
            });
            RegisterTraitGate(new Pair<string, int>("Neurotic", 1), new List<FacetGate>
            {
                new(Facet.Orderliness, 25f, 25f)
            });
            RegisterTraitGate(new Pair<string, int>("Neurotic", 2), new List<FacetGate>
            {
                new(Facet.Orderliness, 25f, 25f)
            });
            RegisterTraitGate(new Pair<string, int>("Industriousness", -2), new List<FacetGate>
            {
                new(Facet.Industriousness, -25f, 25f)
            });
            RegisterTraitGate(new Pair<string, int>("Industriousness", -1), new List<FacetGate>
            {
                new(Facet.Industriousness, -25f, 25f)
            });
            RegisterTraitGate(new Pair<string, int>("Industriousness", 1), new List<FacetGate>
            {
                new(Facet.Industriousness, 25f, 25f)
            });
            RegisterTraitGate(new Pair<string, int>("Industriousness", 2), new List<FacetGate>
            {
                new(Facet.Industriousness, 25f, 25f)
            });
            RegisterTraitGate(new Pair<string, int>("Bloodlust", 0), new List<FacetGate>
            {
                new(Facet.Compassion, -25f, 25f)
            });
            RegisterTraitGate(new Pair<string, int>("Kind", 0), new List<FacetGate>
            {
                new(Facet.Compassion, 25f, 25f)
            });
        }

        public static void ModCompat()
        {
            if (ModsConfig.BiotechActive)
            {
                RegisterTraitGate(new Pair<string, int>("Recluse", 0), new List<FacetGate>
                {
                    new(Facet.Sociability, -25f, 25f)
                });
                //Genes
                RegisterGeneGate("Learning_Slow", new List<FacetGate>
                {
                    new(Facet.Intellect, -25f, 25f)
                });
                RegisterGeneGate("Learning_Fast", new List<FacetGate>
                {
                    new(Facet.Intellect, 25f, 25f)
                });
                RegisterGeneGate("Mood_Depressive", new List<FacetGate>
                {
                    new(Facet.Pessimism, 25f, 25f)
                });
                RegisterGeneGate("Mood_Pessimist", new List<FacetGate>
                {
                    new(Facet.Pessimism, 25f, 25f)
                });
                RegisterGeneGate("Mood_Optimist", new List<FacetGate>
                {
                    new(Facet.Pessimism, -25f, 25f)
                });
                RegisterGeneGate("Mood_Sanguine", new List<FacetGate>
                {
                    new(Facet.Pessimism, -25f, 25f)
                });
                RegisterGeneGate("Aggression_DeadCalm", new List<FacetGate>
                {
                    new(Facet.Assertiveness, -25f, 25f),
                    new(Facet.Volatility, -25f, 25f)
                });
                RegisterGeneGate("Aggression_Aggressive", new List<FacetGate>
                {
                    new(Facet.Assertiveness, 25f, 25f),
                    new(Facet.Volatility, 25f, 25f)
                });
                RegisterGeneGate("Aggression_HyperAggressive", new List<FacetGate>
                {
                    new(Facet.Assertiveness, 25f, 25f),
                    new(Facet.Volatility, 25f, 25f)
                });
                RegisterGeneGate("KillThirst", new List<FacetGate>
                {
                    new(Facet.Compassion, -25f, 25f)
                });
            }

            if (ModsConfig.IsActive("vanillaexpanded.vanillatraitsexpanded"))
            {
                Log.Message("[Rimpsyche] VTE gate data added");
                RegisterTraitGate(new Pair<string, int>("VTE_Eccentric", 0), new List<FacetGate>
                {
                    new FacetGate(Facet.Intellect, 25f, 25f)
                });
                RegisterTraitGate(new Pair<string, int>("VTE_Submissive", 0), new List<FacetGate>
                {
                    new FacetGate(Facet.Assertiveness, -25f, 25f)
                });
                RegisterTraitGate(new Pair<string, int>("VTE_Dunce", 0), new List<FacetGate>
                {
                    new FacetGate(Facet.Intellect, -25f, 25f)
                });
                RegisterTraitGate(new Pair<string, int>("VTE_Snob", 0), new List<FacetGate>
                {
                    new FacetGate(Facet.Humbleness, -25f, 25f)
                });
                RegisterTraitGate(new Pair<string, int>("VTE_Anxious", 0), new List<FacetGate>
                {
                    new FacetGate(Facet.Insecurity, 25f, 25f)
                });
                RegisterTraitGate(new Pair<string, int>("VTE_Prodigy", 0), new List<FacetGate>
                {
                    new FacetGate(Facet.Intellect, 25f, 25f)
                });
                RegisterTraitGate(new Pair<string, int>("VTE_MadSurgeon", 0), new List<FacetGate>
                {
                    new FacetGate(Facet.Compassion, -45f, 5f, 5)
                });
                RegisterTraitGate(new Pair<string, int>("VTE_WorldWeary", 0), new List<FacetGate>
                {
                    new FacetGate(Facet.Pessimism, 25f, 25f)
                });
                RegisterTraitGate(new Pair<string, int>("VTE_Academian", 0), new List<FacetGate>
                {
                    new FacetGate(Facet.Intellect, 25f, 25f)
                });
            }

            if (ModsConfig.IsActive("vanillaracesexpanded.android"))
            {
                Log.Message("[Rimpsyche] VRE Android gate data added");
                RegisterGeneGate("VREA_PsychologyDisabled", new List<FacetGate>
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
                });
            }

            if (ModsConfig.IsActive("consolidatedtraits.lc.rw"))
            {
                Log.Message("[Rimpsyche] ConsolidatedTraits gate data added");
                RegisterTraitGate(new Pair<string, int>("RCT_Aesthete", 0), new List<FacetGate>
                {
                    new FacetGate(Facet.Imagination, 40f, 10f)
                });
                RegisterTraitGate(new Pair<string, int>("RCT_Dunce", 0), new List<FacetGate>
                {
                    new FacetGate(Facet.Intellect, -25f, 25f)
                });
                RegisterTraitGate(new Pair<string, int>("RCT_Savant", 0), new List<FacetGate>
                {
                    new FacetGate(Facet.Intellect, 40f, 10f)
                });
            }

            if (ModsConfig.IsActive("avius.badpeople"))
            {
                Log.Message("[Rimpsyche] BadPeople gate data added");
                RegisterTraitGate(new Pair<string, int>("BadPeople_Evil", 0), new List<FacetGate>
                {
                    new FacetGate(Facet.Compassion, -35f, 15f),
                    new FacetGate(Facet.Integrity, -35f, 15f)
                });
                RegisterTraitGate(new Pair<string, int>("BadPeople_Kinslayer", 0), new List<FacetGate>
                {
                    new FacetGate(Facet.Compassion, -35f, 15f),
                    new FacetGate(Facet.Integrity, -35f, 15f)
                });
            }

            if (ModsConfig.IsActive("hautarche.hautstraits"))
            {
                RegisterTraitGate(new Pair<string, int>("HVT_Aestheticist", 0), new List<FacetGate>
                {
                    new FacetGate(Facet.Imagination, 25, 25)
                });
                RegisterTraitGate(new Pair<string, int>("HVT_Drudge", 0), new List<FacetGate>
                {
                    new FacetGate(Facet.Intellect, -25, 25)
                });
                RegisterTraitGate(new Pair<string, int>("HVT_Sadist", 0), new List<FacetGate>
                {
                    new FacetGate(Facet.Compassion, -35, 15),
                    new FacetGate(Facet.Integrity, -35, 15)
                });
                RegisterTraitGate(new Pair<string, int>("HVT_Staid", 0), new List<FacetGate>
                {
                    new FacetGate(Facet.Imagination, -35, 15)
                });
                RegisterTraitGate(new Pair<string, int>("HVT_Tempestophile", 0), new List<FacetGate>
                {
                    new FacetGate(Facet.Curiosity, 25, 25)
                });
                RegisterTraitGate(new Pair<string, int>("HVT_Tranquil", 0), new List<FacetGate>
                {
                    new FacetGate(Facet.Volatility, -35, 15)
                });
                RegisterTraitGate(new Pair<string, int>("HVT_Visionary", 0), new List<FacetGate>
                {
                    new FacetGate(Facet.Intellect, 25, 25)
                });
                RegisterTraitGate(new Pair<string, int>("HVT_Prideful", 1), new List<FacetGate>
                {
                    new FacetGate(Facet.Humbleness, -35, 15)
                });
                RegisterTraitGate(new Pair<string, int>("HVT_Prideful", 2), new List<FacetGate>
                {
                    new FacetGate(Facet.Humbleness, -35, 15)
                });
                RegisterTraitGate(new Pair<string, int>("HVT_Humble", 2), new List<FacetGate>
                {
                    new FacetGate(Facet.Humbleness, 35, 15)
                });

                //Royalty
                if (ModsConfig.RoyaltyActive)
                {
                    RegisterTraitGate(new Pair<string, int>("HVT_Anarchist", 0), new List<FacetGate>
                    {
                        new FacetGate(Facet.Cooperation, -25, 25)
                    });
                        RegisterTraitGate(new Pair<string, int>("HVT_Servile", 0), new List<FacetGate>
                    {
                        new FacetGate(Facet.Assertiveness, -35, 15),
                        new FacetGate(Facet.Humbleness, 25, 25)
                    });
                }

                //Ideology
                if (ModsConfig.IdeologyActive)
                {
                    RegisterTraitGate(new Pair<string, int>("HVT_Subjugator", 0), new List<FacetGate>
                    {
                        new FacetGate(Facet.Humbleness, -25, 25)
                    });
                        RegisterTraitGate(new Pair<string, int>("HVT_Conformist", 0), new List<FacetGate>
                    {
                        new FacetGate(Facet.Integrity, -25, 25)
                    });
                        RegisterTraitGate(new Pair<string, int>("HVT_Proclaimer", 0), new List<FacetGate>
                    {
                        new FacetGate(Facet.Assertiveness, 25, 25)
                    });
                        RegisterTraitGate(new Pair<string, int>("HVT_RadicalThinker", 0), new List<FacetGate>
                    {
                        new FacetGate(Facet.Intellect, 25, 25)
                    });
                }

                //Anomaly
                if (ModsConfig.AnomalyActive)
                {
                    RegisterTraitGate(new Pair<string, int>("HVT_Twisted", 0), new List<FacetGate>
                    {
                        new FacetGate(Facet.Sociability, -25, 25)
                    });
                }
            }

            if (ModsConfig.IsActive("goji.thesimstraits"))
            {
                Log.Message("[Rimpsyche] The Sims Traits gate data added");
                RegisterTraitGate(new Pair<string, int>("ST_Shy", 0), new List<FacetGate>
                {
                    new FacetGate(Facet.Assertiveness, -35f, 15f)
                });
                RegisterTraitGate(new Pair<string, int>("ST_Zen", 0), new List<FacetGate>
                {
                    new FacetGate(Facet.Imagination, 25f, 25f)
                });
                if (ModsConfig.RoyaltyActive)
                {
                    RegisterTraitGate(new Pair<string, int>("ST_Virtuoso", 0), new List<FacetGate>
                    {
                        new FacetGate(Facet.Imagination, 35f, 15f)
                    });
                }
                if (ModsConfig.IsActive("vanillaexpanded.vanillatraitsexpanded"))
                {
                    RegisterTraitGate(new Pair<string, int>("ST_Submissive", 0), new List<FacetGate>
                    {
                        new FacetGate(Facet.Assertiveness, -35f, 15f)
                    });
                }
                // MayRequire="VanillaExpanded.VanillaTraitsExpanded"
            }

            if (ModsConfig.IsActive("chjees.androids14"))
            {
                var BattleDroiddef = DefDatabase<ThingDef>.GetNamed("ChjBattleDroid", false);
                if (BattleDroiddef != null)
                    MindlessDefShorthashSet.Add(BattleDroiddef.shortHash);
                else
                    Log.Warning("[Rimpsyche] chjees.androids14 is loaded but ChjBattleDroid Def is not found.");
                var Droiddef = DefDatabase<ThingDef>.GetNamed("ChjDroid", false);
                if (Droiddef != null)
                    MindlessDefShorthashSet.Add(Droiddef.shortHash);
                else
                    Log.Warning("[Rimpsyche] chjees.androids14 is loaded but ChjDroid Def is not found.");
            }
        }
    }
}

