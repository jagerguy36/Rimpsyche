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
        private float? _minAdultAge = null;
        private float? _fullAdultAge = null;
        public float MinAdultAge
        {
            get
            {
                if (!_minAdultAge.HasValue)
                {
                    _minAdultAge = Rimpsyche_Utility.GetMinAdultAge(parentPawn);
                }
                return _minAdultAge.Value;
            }
        }
        public float FullAdultAge
        {
            get
            {
                if (!_fullAdultAge.HasValue)
                {
                    _fullAdultAge = Rimpsyche_Utility.GetFullAdultAge(parentPawn);
                }
                return _fullAdultAge.Value;
            }
        }
        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            parentPawn = parent as Pawn;
        }
        private bool psycheShouldCheckEnabled = true;
        private bool psycheEnabledInternal = false;
        public bool Enabled
        {
            get
            {
                if (psycheShouldCheckEnabled)
                {
                    psycheEnabledInternal = CheckEnabled();
                    psycheShouldCheckEnabled = false;
                }
                if (!psycheEnabledInternal) return false;
                return !parentPawn.IsSubhuman;
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
        private bool shamethoughtDirty = true;
        private Dictionary<ThoughtDef, int> activeShameThoughts = new();
        public Dictionary<ThoughtDef, int> ShameThoughts
        {
            get
            {
                if(shamethoughtDirty)
                {
                    RefreshShameThoughts();
                }
                return activeShameThoughts;
            }
        }
        private List<Thought> temp_allMoodThoughts = new();
        public void RefreshShameThoughts()
        {
            activeShameThoughts.Clear();
            temp_allMoodThoughts.Clear();
            parentPawn.needs.mood.thoughts.GetAllMoodThoughts(temp_allMoodThoughts);
            for (int i = 0; i < temp_allMoodThoughts.Count; i++)
            {
                if (temp_allMoodThoughts[i] is Thought_Situational_Shame shamethought)
                {
                    activeShameThoughts.TryGetValue(shamethought.def, out int count);
                    activeShameThoughts[shamethought.def] = count + 1;
                }
            }
            temp_allMoodThoughts.Clear();
            shamethoughtDirty = false;
        }
        public void Notify_ShameThoughtBecameActive(ThoughtDef def)
        {
            if (shamethoughtDirty) return;
            activeShameThoughts.TryGetValue(def, out int count);
            activeShameThoughts[def] = count + 1;
        }
        public void Notify_ShameThoughtBecameInactive(ThoughtDef def)
        {
            if (shamethoughtDirty) return;
            if (activeShameThoughts.TryGetValue(def, out int count))
            {
                if (count > 1)
                {
                    activeShameThoughts[def] = count - 1;
                }
                else
                {
                    activeShameThoughts.Remove(def);
                }
            }
            else
            {
                shamethoughtDirty = true;
            }
        }
        public void CleanShame()
        {
            shamethoughtDirty = true;
            activeShameThoughts.Clear();
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
                    sexuality.Initialize();
                }
                if (sexuality.shouldValidate) sexuality.Validate();
                return sexuality;
            }
            set => sexuality = value;
        }
        public bool CheckEnabled()
        {
            //Core only checks for inhumanized and shambler.
            //Other mods should postfix this method and add nullifyCheck method to add their own restrictions.
            if (parentPawn.IsMutant && parentPawn.mutant.Def == MutantDefOf.Shambler) return false;
            if (parentPawn.Inhumanized()) return false;
            return true;
        }
        public void NullifyCheck()
        {
            psycheShouldCheckEnabled = true;
        }


        public readonly Dictionary<int, float> EvaluationCache = new();
        public readonly Dictionary<int, float> ThoughtEvaluationCache = new();
        public readonly Dictionary<int, float> OpinionEvaluationCache = new();
        public readonly Dictionary<int, float> JoyChanceEvaluationCache = new();
        public readonly Dictionary<int, float> TopicOpinionCache = new();
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


        public void PersonalitySetup()
        {
            if (personality == null)
            {
                personality = new Pawn_PersonalityTracker(parentPawn);
                personality.Initialize();
            }
        }
        public void InterestSetup()
        {
            if (interests == null)
            {
                interests = new Pawn_InterestTracker(parentPawn);
                interests.Initialize();
            }
        }
        public void SexualitySetup(bool generate = false, bool allowGay = true)
        {
            sexuality ??= new Pawn_SexualityTracker(parentPawn);
            //Initialize even when not null for save-game trait safety with Sexuality Module.
            sexuality.Initialize(generate, allowGay);
        }
        public void InjectPsycheData(PsycheData psyche, bool preserveMemory)
        {
            personality ??= new Pawn_PersonalityTracker(parentPawn);
            personality.Initialize(psyche);
            interests ??= new Pawn_InterestTracker(parentPawn);
            interests.Initialize(psyche);
            if (Rimpsyche.SexualityModuleLoaded)
            {
                sexuality ??= new Pawn_SexualityTracker(parentPawn);
                sexuality.InjectData(psyche, preserveMemory);
            }
        }

        public void DirtyTraitCache(TraitDef def)
        {
            personality?.DirtyTraitCache();
            if(Rimpsyche.SexualityModuleLoaded) sexuality?.DirtyTraitCache(def);
        }
        private static readonly Dictionary<Facet, float> facetChanges = new Dictionary<Facet, float>();
        public bool AffectPawn(float resultOffset, float opinion, Topic topic, float direction = 1f, float scoreBoost = 1f)
        {
            float pawnTrust = parentPawn.compPsyche().personality.GetPersonality(PersonalityDefOf.Rimpsyche_Trust); //-1~1
            float pawnAge = Rimpsyche_Utility.GetPawnAge(parentPawn); //0~100
            //score boost is for negative good talk.
            //multiplying resultOffset by 4 will negative good talk value 3.5(max for negGood) act similar to 14 positive good talk
            float score = resultOffset * scoreBoost; //0~20
            float ageFactor = 8f * MinAdultAge / (pawnAge + 0.6f * MinAdultAge) - 5f; //8.3333 ~ -5
            float scoreBase = Mathf.Max(0f, score - 5f + pawnTrust * 2f + ageFactor);
            float influenceChance = 0.5f * Mathf.Clamp01(scoreBase * scoreBase * (0.15f + opinion * 0.05f) / (pawnAge + 1f));
            //Log.Message($"{parentPawn.Name}| score: {score} (boost: {scoreBoost}) | scoreBase: {scoreBase} | influenceChance: {influenceChance} || opinion: {opinion} | direction: {direction}");
            if (Rand.Chance(influenceChance))
            {
                influenceChance *= direction;
                if (parentPawn.DevelopmentalStage.Juvenile())
                {
                    influenceChance *= 0.75f;
                }
                else
                {
                    influenceChance *= 0.5f;
                }
                //Log.Message($"Affect. magnitude: {influenceChance}");
                facetChanges.Clear();
                //If direction is bigger than 0, then the facetChange should move towards the direction that will make the attitude about the topic more positive
                //If direction is smaller than 0, then the facetChange should move towards the direction that will make the attitude about the topic more negative
                var topicWeights = topic.weights;
                for (int i = 0; i < topicWeights.Count; i++)
                {
                    var personalityWeight = topicWeights[i];
                    float contribution = influenceChance * personalityWeight.weight;
                    if (contribution != 0f)
                    {
                        var scores = personalityWeight.personalityDef.scoreWeight;
                        for (int j = 0; j < scores.Count; j++)
                        {
                            var scoreW = scores[j];
                            float totalWeight = scoreW.weight * contribution;
                            if (facetChanges.TryGetValue(scoreW.facet, out float current))
                                facetChanges[scoreW.facet] = current + totalWeight;
                            else
                                facetChanges[scoreW.facet] = totalWeight;
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
                PersonalitySetup();
                InterestSetup();
                SexualitySetup(generate: false);
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
