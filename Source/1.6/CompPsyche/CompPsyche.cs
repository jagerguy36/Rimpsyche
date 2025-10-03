using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;


namespace Maux36.RimPsyche
{
    public class CompPsyche : ThingComp
    {
        //Internals
        public Pawn parentPawn;
        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            parentPawn = parent as Pawn;
        }
        private bool? psycheEnabledInternal = null;
        public bool Enabled
        {
            get
            {
                psycheEnabledInternal ??= CheckEnabled();
                if (psycheEnabledInternal == true)
                {
                    return !parentPawn.IsSubhuman;
                }
                return false;
            }
        }
        public bool IsAdult => parentPawn.DevelopmentalStage == DevelopmentalStage.Adult;
        private Pawn_PersonalityTracker personality;
        private Pawn_InterestTracker interests;
        private Pawn_SexualityTracker sexuality;

        //Progress
        public int progressTick = -1;
        public string progressLastCause = null;
        public int progressLastCauseIndex = 1;//Skill=1, Quality=2, ColonyEvent=3

        //Room
        public float roomRoleFactor = 1f;
        public int organizedMood = -1;
        
        //Resilience
        public int lastResilientSpiritTick = -3600000;

        //Shame
        public float shame = 0f;
        public int tickOverwhelmed = 0;
        public Dictionary<ThoughtDef, int> activeShameThoughts = null;
        public Dictionary<ThoughtDef, int> ShameThoughts
        {
            get
            {
                if(activeShameThoughts == null)
                {
                    RefreshShameThoughts();
                }
                return activeShameThoughts;
            }
        }
        public void RefreshShameThoughts()
        {
            activeShameThoughts = new();
            var allMoodThoughts = new List<Thought>();
            parentPawn.needs.mood.thoughts.GetAllMoodThoughts(allMoodThoughts);
            for (int i = 0; i < allMoodThoughts.Count; i++)
            {
                if (allMoodThoughts[i] is Thought_Situational_Shame shamethought)
                {
                    if (activeShameThoughts.TryGetValue(shamethought.def, out int count))
                    {
                        activeShameThoughts[shamethought.def] = count + 1;
                    }
                    else
                    {
                        activeShameThoughts[shamethought.def] = 1;
                    }
                }
            }
        }
        public void CleanShame()
        {
            activeShameThoughts = null;
        }

        public Pawn_PersonalityTracker Personality
        {
            get
            {
                if (personality == null)
                {
                    personality = new Pawn_PersonalityTracker(parentPawn);
                    personality.Initialize();
                }
                return personality;
            }
            set => personality = value;
        }
        public Pawn_InterestTracker Interests
        {
            get
            {
                if (interests == null)
                {
                    interests = new Pawn_InterestTracker(parentPawn);
                    interests.Initialize();
                }
                return interests;
            }
            set => interests = value;
        }
        public Pawn_SexualityTracker Sexuality
        {
            get
            {
                if (sexuality == null)
                {
                    sexuality = new Pawn_SexualityTracker(parentPawn);
                    sexuality.Initialize(parentPawn);
                }
                return sexuality;
            }
            set => sexuality = value;
        }
        public bool CheckEnabled()
        {
            //Core only checks for inhumanized. Other mods should postfix this method and add nullifyCheck method to add their own restrictions.
            if (parentPawn.Inhumanized()) return false;
            return true;
        }
        public void NullifyCheck()
        {
            psycheEnabledInternal = null;
        }


        public readonly Dictionary<int, float> EvaluationCache = new();
        public readonly Dictionary<int, float> ThoughtEvaluationCache = new();
        public readonly Dictionary<int, float> OpinionEvaluationCache = new();
        public float Evaluate(RimpsycheFormula rimpsycheMultiplier)
        {
            if (EvaluationCache.TryGetValue(rimpsycheMultiplier.formulaId, out float cachedValue))
            {
                return cachedValue;
            }
            else
            {
                float calculatedValue = rimpsycheMultiplier.calculationFunction(this.Personality);
                EvaluationCache[rimpsycheMultiplier.formulaId] = calculatedValue;
                //Log.Message($"calculating {pawn.Name}'s {rimpsycheMultiplier.formulaName} : {calculatedValue} || {nameof(rimpsycheMultiplier)}");
                return calculatedValue;
            }
        }


        public void PsycheValueSetup()
        {
            if (personality == null)
            {
                personality = new Pawn_PersonalityTracker(parentPawn);
                personality.Initialize();
            }
        }
        public void InterestScoreSetup()
        {
            if (interests == null)
            {
                interests = new Pawn_InterestTracker(parentPawn);
                interests.Initialize();
            }
        }
        public void SexualitySetup()
        {
            if (sexuality == null)
            {
                sexuality = new Pawn_SexualityTracker(parentPawn);
            }
            //Initialize even when not null for save-game trait safety with Sexuality Module.
            sexuality.Initialize(parentPawn);
        }
        public void DirtyTraitCache()
        {
            if(personality != null)
            {
                personality.DirtyTraitCache();
            }
        }
        public bool AffectPawn(float resultOffset, float opinion, Topic topic, float direction = 1f)
        {
            float adultHoodAge = Rimpsyche_Utility.GetMinAdultAge(parentPawn);
            float pawnTrust = parentPawn.compPsyche().personality.GetPersonality(PersonalityDefOf.Rimpsyche_Trust); //-1~1
            float pawnAge = (float)parentPawn.ageTracker.AgeBiologicalYears; //0~100
            float score = resultOffset; //0~20
            float ageFactor = 8f * adultHoodAge / (pawnAge + 0.6f * adultHoodAge) - 5f; //8.3333 ~ -5
            float scoreBase = Mathf.Max(0f, score - 5f + pawnTrust * 2f + ageFactor);
            float influenceChance = Mathf.Clamp01(scoreBase * scoreBase * (0.15f + opinion * 0.05f) / (pawnAge + 1f));
            if (Rand.Chance(influenceChance))
            {
                influenceChance *= direction;
                if (parentPawn.DevelopmentalStage.Juvenile())
                {
                    influenceChance *= 1.5f;
                }
                //Log.Message($"Affect. magnitude: {influenceChance}");
                var facetChanges = new Dictionary<Facet, float>();
                foreach (var personalityWeight in topic.weights)
                {
                    float contribution = influenceChance * personalityWeight.weight;
                    if (contribution != 0f)
                    {
                        var personality = RimpsycheDatabase.PersonalityDict[personalityWeight.personalityDefName];
                        foreach (var weight in personality.scoreWeight)
                        {
                            if (facetChanges.ContainsKey(weight.facet))
                            {
                                facetChanges[weight.facet] += weight.weight * contribution;
                            }
                            else
                            {
                                facetChanges[weight.facet] = weight.weight * contribution;
                            }
                        }
                    }
                }
                Personality.AffectFacetValue(facetChanges);

                return true;
            }
            return false;
        }
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref progressTick, "progressTick", -1);
            Scribe_Values.Look(ref progressLastCause, "progressLastCause", null);
            Scribe_Values.Look(ref progressLastCauseIndex, "progressLastCauseIndex", 1);
            Scribe_Values.Look(ref roomRoleFactor, "roomRoleFactor", 1f);
            Scribe_Values.Look(ref organizedMood, "organizedMood", -1);
            Scribe_Values.Look(ref lastResilientSpiritTick, "lastResilientSpiritTick", -3600000);
            Scribe_Values.Look(ref shame, "shame", 0f);
            Scribe_Values.Look(ref tickOverwhelmed, "tickOverwhelmed", 0);

            Scribe_Deep.Look(ref personality, "personality", new object[] { parent as Pawn });
            Scribe_Deep.Look(ref interests, "interests", new object[] { parent as Pawn });
            Scribe_Deep.Look(ref sexuality, "sexuality", new object[] { parent as Pawn });
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                PsycheValueSetup();
                InterestScoreSetup();
                SexualitySetup();
                if (Rimpsyche.DispositionModuleLoaded)
                {
                    if (progressTick < 0)
                    {
                        progressTick = Find.TickManager.TicksGame-1;
                    }
                }
                else
                {
                    progressTick = -1;
                    progressLastCause = null;
                    progressLastCauseIndex = 1;
                    roomRoleFactor = 1f;
                    organizedMood = -1;
                    //lastResilientSpiritTick = -3600000; Keep this in memory
                    tickOverwhelmed = 0;
                }
            }
        }

    }


}
