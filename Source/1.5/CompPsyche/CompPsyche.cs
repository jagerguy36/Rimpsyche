using RimWorld;
using UnityEngine;
using Verse;


namespace Maux36.RimPsyche
{
    public class CompPsyche : ThingComp
    {
        //Internals
        private Pawn parentPawnInt = null;
        private Pawn_PersonalityTracker personality;
        private Pawn_InterestTracker interests;
        public bool PostGen = false;
        public int convoStartedTick = -1;
        public int convoCheckTick = -1;
        public Pawn convoPartner = null;
        public Topic topic = null;
        public float topicAlignment;

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
                if (ShouldEndConvoImmediately())
                {
                    FinishConvo();
                    return;
                }
                if (parentPawn.IsHashIntervalTick(200)) //InteractionsTrackerTick checks every interval tick 91
                {
                    Log.Message($"{parentPawn.Name} checking conversation validity with {convoPartner.Name} on topic {topic.name}, in {topic.category} with topicAlignment {topicAlignment}");
                    if (ShouldEndConvo())
                    {
                        FinishConvo();
                        return;
                    }
                }
                if (convoCheckTick > 0)
                {
                    if (convoCheckTick < Find.TickManager.TicksGame)
                    {
                        //TODO: Check conversation continue chance. If that's the case, then increase the check tick


                        //var parentPawnStartanotherConvoChance = Rand.Chance(1f - (float)(Find.TickManager.TicksGame - convoStartedTick) / 2500f);
                        //var partnerStartanotherConvoChance = Rand.Chance(1f - (float)(Find.TickManager.TicksGame - convoStartedTick) / 2500f);
                        //if (parentPawnStartanotherConvoChance)
                        //{
                        //    Log.Message($"another convo {DefOfRimpsyche.Rimpsyche_Conversation.defName} start from {parentPawn.Name} with {convoPartner.Name}.");
                        //    parentPawn.interactions.TryInteractWith(convoPartner, DefOfRimpsyche.Rimpsyche_Conversation);
                        //}
                        //else if (partnerStartanotherConvoChance)
                        //{
                        //    Log.Message($"another convo {DefOfRimpsyche.Rimpsyche_Conversation.defName} start from {convoPartner.Name} with {parentPawn.Name}.");
                        //    convoPartner.interactions.TryInteractWith(parentPawn, DefOfRimpsyche.Rimpsyche_Conversation);
                        //}
                        Log.Message($"end convo.");
                        FinishConvo();
                        return;
                    }
                    //else if (convoStartedTick+300 < convoCheckTick && parentPawn.IsHashIntervalTick(200))
                    //{
                    //    MoteMaker.MakeInteractionBubble(parentPawn, convoPartner, DefOfRimpsyche.Rimpsyche_Conversation.interactionMote, DefOfRimpsyche.Rimpsyche_Conversation.GetSymbol());
                    //}
                }
            }
        }
        public bool ShouldEndConvoImmediately()//Simple checks
        {
            if (parentPawn?.Spawned != true || convoPartner.Spawned != true) return true;
            if (parentPawn.Downed || convoPartner.Downed) return true;
            if (parentPawn.Dead || convoPartner.Dead) return true;
            if (parentPawn.IsMutant || convoPartner.IsMutant) return true;
            if (parentPawn.InAggroMentalState || convoPartner.InAggroMentalState) return true;
            if (parentPawn.Map == null && convoPartner.Map == null) return true;
            return false;
        }
        public bool ShouldEndConvo()
        {
            if (!parentPawn.Awake() || !convoPartner.Awake()) return true;
            if (parentPawn.IsBurning() || convoPartner.IsBurning()) return true;
            if (!IsGoodPositionForInteraction(parentPawn.Position, convoPartner.Position, parentPawn.Map)) return true;
            return false;
        }
        public void FinishConvo()
        {
            float pawnScore = GetConvoResult(out float partnerScore);
            Log.Message($"GetConvoResult: {parentPawn.Name}: {pawnScore} | {convoPartner.Name}: {partnerScore}");
            float lengthMult = Mathf.Max(0, Find.TickManager.TicksGame - convoStartedTick - 200) * 0.002f + 1f; // 1~2 ~ 4

            //TODO : Check if mattered (affect personality)

            var convoPartnerPsyche = convoPartner.compPsyche();
            convoPartnerPsyche?.EndConvo(lengthMult * partnerScore);
            EndConvo(lengthMult * pawnScore);
        }
        public void EndConvo(float moodOffset = 0)
        {
            Log.Message($"{parentPawn.Name} ending conversation with {convoPartner.Name} and getting mood {moodOffset}");
            if (convoPartner != null && moodOffset!=0)
            {
                ThoughtDef newDef = Rimpsyche_Utility.CreateSocialThought(
                    parentPawn.GetHashCode() + "Conversation" + topic.name,
                    "ConversationStage".Translate() + " " + topic.name,
                    moodOffset);
                parentPawn.needs?.mood?.thoughts?.memories?.TryGainMemory(newDef, convoPartner);
            }
            convoStartedTick = -1;
            convoCheckTick = -1;
            convoPartner = null;
            topic = null;
            topicAlignment = 0;
        }
        public float GetConvoResult(out float partnerScore)
        {
            float pawnScore = 0f;
            partnerScore = 0f;
            
            var partnerPsyche = convoPartner.compPsyche();
            if (partnerPsyche != null)
            {
                // -1 ~ 1
                float pawnOpinion = parentPawn.relations.OpinionOf(convoPartner) * 0.01f;
                float pawnTact = personality.GetPersonality(PersonalityDefOf.Rimpsyche_Tact); //(0.1f * recipient.skills.GetSkill(SkillDefOf.Social).Level)
                pawnTact = Mathf.Clamp(pawnTact + (0.1f * parentPawn.skills.GetSkill(SkillDefOf.Social).Level), -1f, 1f);
                float pawnOpenness = personality.GetPersonality(PersonalityDefOf.Rimpsyche_Openness);
                float pawnTrust = personality.GetPersonality(PersonalityDefOf.Rimpsyche_Trust);
                float pawnPassion = personality.GetPersonality(PersonalityDefOf.Rimpsyche_Passion);
                float pawnTalkativeness = personality.GetPersonality(PersonalityDefOf.Rimpsyche_Talkativeness);

                float partnerOpinion = convoPartner.relations.OpinionOf(parentPawn) * 0.01f;
                float partnerTact = partnerPsyche.personality.GetPersonality(PersonalityDefOf.Rimpsyche_Tact);
                partnerTact = Mathf.Clamp(partnerTact + (0.1f * convoPartner.skills.GetSkill(SkillDefOf.Social).Level), -1f, 1f);
                float partnerOpenness = partnerPsyche.personality.GetPersonality(PersonalityDefOf.Rimpsyche_Openness);
                float partnerTrust = partnerPsyche.personality.GetPersonality(PersonalityDefOf.Rimpsyche_Trust);
                float partnerPassion = partnerPsyche.personality.GetPersonality(PersonalityDefOf.Rimpsyche_Passion);
                float partnerTalkativeness = partnerPsyche.personality.GetPersonality(PersonalityDefOf.Rimpsyche_Talkativeness);

                
                float talkQuality = Rand.Value;

                if (topicAlignment > 0)
                {
                    Log.Message($"positive alignment");
                    float partnerScoreBase = 1f + (1f * partnerOpinion) + (2f * topicAlignment); //base 1, can scale up to 4 with alignment score
                    float partnerScoreModifier = (0.2f * pawnTact) + (0.4f * (pawnPassion + 1f)); //-0.2~1
                    partnerScoreModifier = (1f + talkQuality) * partnerScoreModifier; // -0.4~2
                    partnerScore = (partnerScoreBase + partnerScoreModifier); //0.6~6 * 1~2(~4)

                    float pawnScoreBase = 1f + (1f * pawnOpinion) + (2f * topicAlignment); //base 1, can scale up to 4 with alignment score
                    float pawnScoreModifier = (0.2f * partnerTact) + (0.4f * (partnerPassion+1f)); //-0.2~1
                    pawnScoreModifier = (1f + talkQuality) * pawnScoreModifier; // -0.4~2
                    pawnScore = (pawnScoreBase + pawnScoreModifier); //0.6~6 * 1~2(~4)

                    return pawnScore;
                }
                //Negative Alignment
                float pawnReceiveScore = (partnerTact * (partnerTalkativeness + 1) * 0.5f) + pawnOpenness + pawnOpinion; // -3~3
                float partnerReceiveScore = (pawnTact * (pawnTalkativeness + 1) * 0.5f) + partnerOpenness + partnerOpinion; // -3~3
                Log.Message($"negative alignment. pawnReceiveScore = {pawnReceiveScore}, partnerReceiveScore: {partnerReceiveScore}");
                if (pawnReceiveScore > 0f && partnerReceiveScore > 0f)
                {
                    //If both receiveScore is positive then there is a chance it's a good talk even if the alignment is negative
                    float goodTalkChance = (3f + pawnReceiveScore + partnerReceiveScore) * (0.10f + (0.05f * topicAlignment)); // (3 ~ 9)  * (0.05 ~ 0.1) = 0.15 ~ 0.9
                    Log.Message($"goodTalkChance = {goodTalkChance}, talkQuality: {talkQuality}");
                    if (talkQuality > 1f - goodTalkChance)
                    {
                        partnerScore = partnerReceiveScore * talkQuality;
                        return pawnReceiveScore * talkQuality;
                    }
                }
                //Bad Talk
                float negativeScoreBase = topicAlignment * talkQuality * 4f; // -4~-0 * (1~2) 
                partnerScore = negativeScoreBase * (1f + (0.3f * partnerReceiveScore)); //0.1~1.9
                return negativeScoreBase * (1f + (0.3f * pawnReceiveScore)); //-7.6 ~ 0
            }
            partnerScore = 0f;
            return pawnScore;
        }
        public static bool IsGoodPositionForInteraction(IntVec3 cell, IntVec3 recipientCell, Map map)
        {
            if (cell.InHorDistOf(recipientCell, 12f)) return GenSight.LineOfSight(cell, recipientCell, map, skipFirstCell: true);
            return false;
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
            }
        }

    }


}
