using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RimWorld;
using Verse;


namespace Maux36.RimPsyche
{
    public class CompPsyche : ThingComp
    {
        //Internals
        private static int maxConvoOpinions = 10;
        private Pawn parentPawnInt = null;
        private Pawn_PersonalityTracker personality;
        private Pawn_InterestTracker interests;
        private Pawn_SexualityTracker sexuality;

        public bool PostGen = false;
        public int convoStartedTick = -1;
        public int convoCheckTick = -1;
        public Pawn convoPartner = null;
        public Topic topic = null;
        public float topicAlignment;
        public float continuationChance = 0;

        private Pawn parentPawn
        {
            get
            {
                parentPawnInt ??= parent as Pawn;
                return parentPawnInt;
            }
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
                    sexuality = GetSexualityTracker(parentPawn);
                    sexuality.Initialize(parentPawn);
                }
                return sexuality;
            }
            set => sexuality = value;
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
                sexuality = GetSexualityTracker(parentPawn);
            }
            sexuality.Initialize(parentPawn);
        }
        public static Pawn_SexualityTracker GetSexualityTracker(Pawn pawn)
        {
            return new Pawn_SexualityTracker(pawn);
        }
        public void DirtyTraitCache()
        {
            if(personality != null)
            {
                personality.DirtyTraitCache();
            }
        }
        public override void CompTick()
        {
            base.CompTick();
            if (convoStartedTick > 0)
            {
                if (convoPartner == null)
                {
                    CleanUp();
                    return;
                }
                if (ShouldEndConvoImmediately())
                {
                    FinishConvo();
                    return;
                }
                if (parentPawn.IsHashIntervalTick(200)) //InteractionsTrackerTick checks every interval tick 91
                {
                    Log.Message($"{parentPawn.Name} checking conversation validity with {convoPartner.Name} on topic {topic.name} with topicAlignment {topicAlignment}");
                    if (ShouldEndConvo(out bool showMote))
                    {
                        FinishConvo(showMote);
                        return;
                    }
                }
                if (convoCheckTick > 0)
                {
                    if (convoCheckTick <= Find.TickManager.TicksGame)
                    {
                        //TODO: Check conversation continue chance. If that's the case, then increase the check tick
                        if(continuationChance > 0 && Rand.Chance(continuationChance))
                        {
                            Log.Message($"continue for {convoCheckTick - convoStartedTick} more ticks");
                            convoCheckTick += convoCheckTick - convoStartedTick;
                            continuationChance = 0;
                        }
                        else
                        {
                            Log.Message($"end convo.");
                            FinishConvo(true);
                            return;
                        }
                    }
                    if ((Find.TickManager.TicksGame - convoStartedTick) % 200 == 199)
                    {
                        if (convoPartner.Map != null && parentPawn.Map != null)
                        {
                            MoteMaker.MakeInteractionBubble(parentPawn, convoPartner, DefOfRimpsyche.Rimpsyche_Conversation.interactionMote, DefOfRimpsyche.Rimpsyche_Conversation.GetSymbol());
                        }
                    }
                }
            }
        }
        public bool ShouldEndConvoImmediately()//Simple checks
        {
            if (parentPawn.Spawned != true || convoPartner.Spawned != true) return true;
            if (parentPawn.Downed || convoPartner.Downed) return true;
            if (parentPawn.Dead || convoPartner.Dead) return true;
            if (parentPawn.IsMutant || convoPartner.IsMutant) return true;
            if (parentPawn.InAggroMentalState || convoPartner.InAggroMentalState) return true;
            if (parentPawn.Map == null && convoPartner.Map == null) return true;
            return false;
        }
        public bool ShouldEndConvo(out bool showMote)
        {
            showMote = false;
            if (!parentPawn.Awake() || !convoPartner.Awake()) return true;
            if (parentPawn.IsBurning() || convoPartner.IsBurning()) return true;
            if (!IsGoodPositionForInteraction(parentPawn.Position, convoPartner.Position, parentPawn.Map))
            {
                showMote = true;
                return true;
            }
            return false;
        }
        public void CleanUp()
        {
            convoStartedTick = -1;
            convoCheckTick = -1;
            convoPartner = null;
            topic = null;
            topicAlignment = 0;
        }
        public void FinishConvo(bool showMote = false)
        {
            GetConvoResult(out float pawnScore, out float partnerScore);
            Log.Message($"GetConvoResult: {parentPawn.Name}: {pawnScore} | {convoPartner.Name}: {partnerScore}");
            float lengthMult = Mathf.Max(0, Find.TickManager.TicksGame - convoStartedTick - 200) * 0.002f + 1f; // 1~2 ~ 4
            var intDef = DefOfRimpsyche.Rimpsyche_EndConversation;
            var entry = new PlayLogEntry_InteractionConversation(intDef, parentPawn, convoPartner, topic.name, null);
            if (showMote)
            {
                if (convoPartner.Map != null && parentPawn.Map != null)
                {
                    MoteMaker.MakeInteractionBubble(parentPawn, convoPartner, intDef.interactionMote, intDef.GetSymbol(parentPawn.Faction, parentPawn.Ideo), intDef.GetSymbolColor(parentPawn.Faction));
                }
            }
            Find.PlayLog.Add(entry);

            var convoPartnerPsyche = convoPartner.compPsyche();
            convoPartnerPsyche?.EndConvo(lengthMult * partnerScore);
            EndConvo(lengthMult * pawnScore);
        }
        public void EndConvo(float opinionOffset = 0)
        {
            Log.Message($"{parentPawn.Name} ending conversation with {convoPartner.Name} and getting opinion {opinionOffset}");
            if (convoPartner != null)
            {
                if(opinionOffset != 0)
                {
                    //Affect here
                    
                    ThoughtDef newDef = Rimpsyche_Utility.CreateSocialThought(
                        "Rimpsyche_Conversation" + parentPawn.GetHashCode() + topic.name,
                        "ConversationStage".Translate() + " " + topic.name,
                        opinionOffset);

                    //Use custom Gain Memory
                    GainCoversationMemoryFast(ThoughtMaker.MakeThought(newDef, null), opinionOffset, convoPartner);
                    AffectPawn(opinionOffset);
                }
            }
            //initiator.skills.Learn(intDef.initiatorXpGainSkill, intDef.initiatorXpGainAmount);
            convoStartedTick = -1;
            convoCheckTick = -1;
            convoPartner = null;
            topic = null;
            topicAlignment = 0;
        }
        public void GetConvoResult(out float pawnScore, out float partnerScore)
        {
            pawnScore = 0f;
            partnerScore = 0f;
            
            var partnerPsyche = convoPartner.compPsyche();
            if (partnerPsyche != null)
            {
                // -1 ~ 1
                float pawnOpinion = parentPawn.relations.OpinionOf(convoPartner) * 0.01f;
                float pawnTact = Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Tact); //(0.1f * recipient.skills.GetSkill(SkillDefOf.Social).Level)
                pawnTact = Mathf.Clamp(pawnTact + (0.1f * parentPawn.skills.GetSkill(SkillDefOf.Social).Level), -1f, 1f);
                float pawnOpenness = Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Openness);
                float pawnTrust = Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Trust);
                float pawnPassion = Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Passion);
                float pawnTalkativeness = Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Talkativeness);

                float partnerOpinion = convoPartner.relations.OpinionOf(parentPawn) * 0.01f;
                float partnerTact = partnerPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Tact);
                partnerTact = Mathf.Clamp(partnerTact + (0.1f * convoPartner.skills.GetSkill(SkillDefOf.Social).Level), -1f, 1f);
                float partnerOpenness = partnerPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Openness);
                float partnerTrust = partnerPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Trust);
                float partnerPassion = partnerPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Passion);
                float partnerTalkativeness = partnerPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Talkativeness);

                
                float talkRand = Rand.Value;

                if (topicAlignment > 0)
                {
                    Log.Message($"positive alignment");
                    float partnerScoreBase = 1f + (0.5f * partnerOpinion) + (2f * topicAlignment); //0.5[2]3.5
                    float partnerScoreModifier = (0.2f * pawnTact) + (0.2f * pawnPassion); //-0.4~[0]~0.4
                    partnerScoreModifier = (1f + talkRand) * partnerScoreModifier; // -0.8~[0]~0.8
                    partnerScore = (partnerScoreBase + partnerScoreModifier); // -0.3[2]4.3

                    float pawnScoreBase = 1f + (0.5f * pawnOpinion) + (2f * topicAlignment); //0.5[2]3.5
                    float pawnScoreModifier = (0.2f * partnerTact) + (0.2f * partnerPassion); //-0.4~[0]~0.4
                    pawnScoreModifier = (1f + talkRand) * pawnScoreModifier; // -0.8~[0]~0.8
                    pawnScore = (pawnScoreBase + pawnScoreModifier); // -0.3[2]4.3
                    return;
                }
                //Negative Alignment
                float pawnReceiveScore = (partnerTact * (partnerTalkativeness + 1) * 0.5f) + (pawnOpenness * (pawnTrust + 1) * 0.5f) + pawnOpinion; // -3~[0]~3
                float partnerReceiveScore = (pawnTact * (pawnTalkativeness + 1) * 0.5f) + (partnerOpenness * (partnerTrust + 1) * 0.5f) + partnerOpinion; // -3~[0]~3
                Log.Message($"negative alignment. pawnReceiveScore = {pawnReceiveScore}, partnerReceiveScore: {partnerReceiveScore}");
                if (pawnReceiveScore > 0f && partnerReceiveScore > 0f)
                {
                    //If both receiveScore is positive then there is a chance it's a good talk even if the alignment is negative
                    float goodTalkChance = (3f + pawnReceiveScore + partnerReceiveScore) * (0.10f + (0.05f * topicAlignment)); // (3 ~ 9)  * (0.05 ~ 0.1) = 0.15 ~ 0.9
                    Log.Message($"goodTalkChance = {goodTalkChance}, talkRand: {talkRand}");
                    if (talkRand > 1f - goodTalkChance)
                    {
                        partnerScore = partnerReceiveScore * talkRand;
                        pawnScore = pawnReceiveScore * talkRand;
                        return;
                    }
                }
                //Bad Talk
                float negativeScoreBase = 2f * topicAlignment * (1f - talkRand); // -2~[-1]~0
                Log.Message($"Bad talk. talkRand: {talkRand}. negativeScoreBase: {negativeScoreBase}");
                partnerScore = negativeScoreBase * (1f - (0.3f * partnerReceiveScore)); //(-2~0) * 0.1~1.9 = -3.8 ~[-1]~ 0
                pawnScore = negativeScoreBase * (1f - (0.3f * pawnReceiveScore)); //-3.8 ~ 0
                return;
            }
            return;
        }
        public bool AffectPawn(float resultOffset)
        {
            float adultHoodAge = Rimpsyche_Utility.GetMinAdultAge(parentPawn);
            float pawnTrust = parentPawn.compPsyche().personality.GetPersonality(PersonalityDefOf.Rimpsyche_Trust); //-1~1
            int pawnAge = parentPawn.ageTracker.AgeBiologicalYears; //0~100
            float opinion = parentPawn.relations.OpinionOf(convoPartner) * 0.01f;
            float score = Mathf.Abs(resultOffset); //0~20
            float ageFactor = 0.48f * adultHoodAge / (pawnAge + 0.6f * adultHoodAge) - 0.3f; //0.43333~-0.31072
            float scoreBase = Mathf.Max(0f,score-11f+pawnTrust*2f+ageFactor*10f);
            float influenceChance = scoreBase*scoreBase * (1f + opinion*0.2f) * 0.0025f;
            Log.Message($"affect pawn with chance {influenceChance}");
            if (Rand.Chance(influenceChance))
            {
                if (parentPawn.DevelopmentalStage.Juvenile())
                {
                    influenceChance *= 1.5f;
                }
                Log.Message($"Affect. magnitude: {influenceChance}");

                float totalWeight = topic.weights.Sum(w => Mathf.Abs(w.weight));
                if (totalWeight == 0f)
                    return false;

                var facetChanges = new Dictionary<Facet, float>();
                foreach (var weight in topic.weights)
                {
                    float contribution = influenceChance * (weight.weight / totalWeight);
                    if (contribution != 0f)
                        facetChanges[weight.facet] = contribution;
                }
                Personality.AffectFacetValue(facetChanges);

                return true;
            }
            return false;
        }
        public static bool IsGoodPositionForInteraction(IntVec3 cell, IntVec3 recipientCell, Map map)
        {
            if (cell.InHorDistOf(recipientCell, 12f)) return GenSight.LineOfSight(cell, recipientCell, map, skipFirstCell: true);
            return false;
        }
        public void GainCoversationMemoryFast(Thought_Memory newThought, float opinionOffset, Pawn otherPawn = null)
        {
            if (newThought.otherPawn == null && otherPawn == null)
            {
                Log.Error(string.Concat("Can't gain social thought ", newThought.def, " because its otherPawn is null and otherPawn passed to this method is also null. Social thoughts must have otherPawn."));
                return;
            }
            otherPawn = otherPawn ?? newThought.otherPawn;
            if (!newThought.def.socialTargetDevelopmentalStageFilter.Has(otherPawn.DevelopmentalStage))
            {
                return;
            }
            newThought.pawn = parentPawn;
            newThought.otherPawn = otherPawn;
            List<Thought_MemoryPostDefined> currentConvoMemories = parentPawn.needs.mood.thoughts.memories.Memories
                .OfType<Thought_MemoryPostDefined>()
                .Where(m => m.otherPawn == otherPawn)
                .ToList();

            if (currentConvoMemories.Count < maxConvoOpinions)
            {
                parentPawn.needs?.mood?.thoughts?.memories?.Memories.Add(newThought);
            }
            else
            {
                currentConvoMemories.Sort((m1, m2) => Mathf.Abs(m2.OpinionOffset()).CompareTo(Mathf.Abs(m1.OpinionOffset())));
                Thought_MemoryPostDefined memoryToCompareWith = currentConvoMemories[maxConvoOpinions - 1];
                if (Mathf.Abs(opinionOffset) < Mathf.Abs(memoryToCompareWith.OpinionOffset()))
                {
                    Log.Message("It's smaller actually. so no adding for you");
                    return;
                }
                for (int i = maxConvoOpinions - 1; i < currentConvoMemories.Count; i++)
                {
                    Thought_MemoryPostDefined m = currentConvoMemories[i];
                    Log.Message($"{m.def.defName} will be removed");
                    m.age = m.DurationTicks + 300;
                }
                parentPawn.needs?.mood?.thoughts?.memories?.Memories.Add(newThought);
            }
        }
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref PostGen, "PostGen", true);
            Scribe_Values.Look(ref convoStartedTick, "convoStartedTick", -1);
            Scribe_Values.Look(ref convoCheckTick, "convoCheckTick", -1);
            Scribe_References.Look(ref convoPartner, "convoPartner");
            Scribe_Deep.Look(ref topic, "topic");
            Scribe_Values.Look(ref topicAlignment, "topicAlignment");
            Scribe_Deep.Look(ref personality, "personality", new object[] { parent as Pawn });
            Scribe_Deep.Look(ref interests, "interests", new object[] { parent as Pawn });
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                PsycheValueSetup();
                InterestScoreSetup();
                SexualitySetup();
            }
        }

    }


}
