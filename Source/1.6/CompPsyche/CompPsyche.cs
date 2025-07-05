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
        private Pawn parentPawnInt = null;
        private Pawn_PersonalityTracker personality;
        private Pawn_InterestTracker interests;
        private Pawn_SexualityTracker sexuality;

        public int lastProgressTick = -1;

        public int convoStartedTick = -1;
        public int convoCheckTick = -1;
        public Pawn convoPartner = null;
        public Topic topic = null;
        public float topicAlignment;
        public float direction;
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
            //For save-game trait safety with Sexuality Module
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
                        Log.Message($"{parentPawn.Name} ending convo after validation check");
                        FinishConvo(showMote);
                        return;
                    }
                }
                if (convoCheckTick > 0)
                {
                    if (convoCheckTick <= Find.TickManager.TicksGame)
                    {
                        if(continuationChance > 0 && Rand.Chance(continuationChance))
                        {
                            Log.Message($"continue for {convoCheckTick - convoStartedTick} more ticks");
                            convoCheckTick += convoCheckTick - convoStartedTick;
                            continuationChance = 0;
                        }
                        else
                        {
                            Log.Message($"{parentPawn.Name} : end convo.");
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
            if (!Rimpsyche_Utility.IsGoodPositionForInteraction(parentPawn.Position, convoPartner.Position, parentPawn.Map))
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
            topicAlignment = 0f;
            direction = 0f;
        }
        public void FinishConvo(bool showMote = false)
        {
            float lengthMult = (Find.TickManager.TicksGame - convoStartedTick - 212.5f) * 0.002f + 1f; // 0.8~[1]~2.95 || 5.325
            bool startedSocialFight = GetConvoResult(lengthMult, out float pawnScore, out float partnerScore, out bool startedByParentPawn);
            Log.Message($"GetConvoResult: {parentPawn.Name}: {pawnScore} | {convoPartner.Name}: {partnerScore}");
            var intDef = DefOfRimpsyche.Rimpsyche_EndConversation;
            List<RulePackDef> extraSents = [];
            //If partnerScore<0 or pawnScore <0 check social fight chance.
            if(startedSocialFight)
            {
                extraSents.Add(DefOfRimpsyche.Sentence_RimpsycheSocialFightConvoInitiatorStarted);
                if (startedByParentPawn)
                {
                    parentPawn.interactions.StartSocialFight(convoPartner);
                }
                else
                {
                    convoPartner.interactions.StartSocialFight(parentPawn);
                }
            }
            PlayLogEntry_InteractionConversation entry = new PlayLogEntry_InteractionConversation(intDef, parentPawn, convoPartner, topic.name, extraSents);
            if (showMote)
            {
                if (convoPartner.Map != null && parentPawn.Map != null)
                {
                    if (startedSocialFight) MoteMaker.MakeInteractionBubble(parentPawn, convoPartner, intDef.interactionMote, intDef.GetSymbol(parentPawn.Faction, parentPawn.Ideo), intDef.GetSymbolColor(parentPawn.Faction));
                    else MoteMaker.MakeInteractionBubble(parentPawn, convoPartner, intDef.interactionMote, intDef.GetSymbol(parentPawn.Faction, parentPawn.Ideo), intDef.GetSymbolColor(parentPawn.Faction));
                }
            }
            Find.PlayLog.Add(entry);
            var convoPartnerPsyche = convoPartner.compPsyche();
            convoPartnerPsyche?.EndConvo(partnerScore, lengthMult);
            EndConvo(pawnScore, lengthMult);
        }
        public void EndConvo(float score, float lengthMult)
        {
            float opinionOffset = score * (6f * lengthMult) / (lengthMult + 2f); //1.714~[2]~3.5757 || 4.36177
            Log.Message($"{parentPawn.Name} ending conversation with {convoPartner?.Name} and getting opinion {opinionOffset}.");
            if (convoPartner != null)
            {
                if (opinionOffset != 0)
                {
                    ThoughtDef newDef = Rimpsyche_Utility.CreateSocialThought(
                        "Rimpsyche_Conversation" + parentPawn.GetHashCode() + topic.name,
                        string.Format("ConversationStage {0}".Translate(), topic.name),
                        opinionOffset);

                    //Use custom Gain Memory
                    Rimpsyche_Utility.GainCoversationMemoryFast(ThoughtMaker.MakeThought(newDef, null), opinionOffset, parentPawn, convoPartner);
                    if(opinionOffset>0) AffectPawn(opinionOffset);
                }
            }
            convoStartedTick = -1;
            convoCheckTick = -1;
            convoPartner = null;
            topic = null;
            topicAlignment = 0f;
            direction = 0f;
        }
        public bool GetConvoResult(float lengthMult, out float pawnScore, out float partnerScore, out bool startedByParentPawn)
        {
            bool startFight = false;
            startedByParentPawn = false;
            pawnScore = 0f;
            partnerScore = 0f;
            
            var partnerPsyche = convoPartner.compPsyche();
            if (partnerPsyche != null)
            {
                // -1 ~ 1
                float pawnOpinion = parentPawn.relations.OpinionOf(convoPartner); //-100~100
                float pawnTact = Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Tact);
                pawnTact = Mathf.Clamp(pawnTact + (0.1f * parentPawn.skills.GetSkill(SkillDefOf.Social).Level), -1f, 1f);
                float pawnOpenness = Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Openness);
                float pawnTrust = Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Trust);
                float pawnPassion = Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Passion);
                float pawnTalkativeness = Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Talkativeness);

                float partnerOpinion = convoPartner.relations.OpinionOf(parentPawn); //-100~100
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
                    float partnerScoreBase = 1f + (0.005f * partnerOpinion) + (2f * topicAlignment); //0.5[2]3.5
                    float partnerScoreModifier = (0.2f * pawnTact) + (0.2f * pawnPassion); //-0.4~[0]~0.4
                    partnerScoreModifier = (1f + talkRand) * partnerScoreModifier; // -0.8~[0]~0.8
                    partnerScore = (partnerScoreBase + partnerScoreModifier); // -0.3[2]4.3

                    float pawnScoreBase = 1f + (0.005f * pawnOpinion) + (2f * topicAlignment); //0.5[2]3.5
                    float pawnScoreModifier = (0.2f * partnerTact) + (0.2f * partnerPassion); //-0.4~[0]~0.4
                    pawnScoreModifier = (1f + talkRand) * pawnScoreModifier; // -0.8~[0]~0.8
                    pawnScore = (pawnScoreBase + pawnScoreModifier); // -0.3[2]4.3
                    return startFight;
                }
                //Negative Alignment
                float pawnReceiveScore = (partnerTact * (partnerTalkativeness + 1) * 0.5f) + (pawnOpenness * (pawnTrust + 1) * 0.5f) + (pawnOpinion * 0.01f); // -3~[0]~3
                float partnerReceiveScore = (pawnTact * (pawnTalkativeness + 1) * 0.5f) + (partnerOpenness * (partnerTrust + 1) * 0.5f) + (partnerOpinion * 0.01f); // -3~[0]~3
                Log.Message($"negative alignment. pawnReceiveScore = {pawnReceiveScore}, partnerReceiveScore: {partnerReceiveScore}");
                if (pawnReceiveScore > 0f && partnerReceiveScore > 0f)
                {
                    //If both receiveScore is positive then there is a chance it's a good talk even if the alignment is negative
                    float goodTalkChance = (3f + pawnReceiveScore + partnerReceiveScore) * (0.10f + (0.05f * topicAlignment)); // (3 ~ 9)  * (0.05 ~ 0.1) = 0.15 ~ 0.9
                    Log.Message($"goodTalkChance = {goodTalkChance}, talkRand: {talkRand}");
                    if (talkRand > 1f - goodTalkChance)
                    {
                        partnerScore = partnerReceiveScore * talkRand; // 0~3
                        pawnScore = pawnReceiveScore * talkRand;// 0~3
                        return startFight;
                    }
                }
                //Bad Talk
                float negativeScoreBase = 2f * topicAlignment * (1f - talkRand); // -2~[-1]~0
                Log.Message($"Bad talk. talkRand: {talkRand}. negativeScoreBase: {negativeScoreBase}.");
                pawnScore = negativeScoreBase * (1f - (0.3f * pawnReceiveScore)); //-3.8 ~ 0
                partnerScore = negativeScoreBase * (1f - (0.3f * partnerReceiveScore)); //(-2~0) * 0.1~1.9 = -3.8 ~[-1]~ 0
                Log.Message($"pawnScore: {pawnScore}.partnerScore: {partnerScore}.");
                //Calcualte fight Chance
                float pawnStartFightChance = Rimpsyche_Utility.ConvoSocialFightChance(parentPawn, convoPartner, -0.005f * pawnScore * lengthMult * Personality.GetMultiplier(RimpsycheDatabase.SocialFightChanceMultiplier), pawnOpinion);
                float partnerStartFightChance = Rimpsyche_Utility.ConvoSocialFightChance(convoPartner, parentPawn, -0.005f * partnerScore * lengthMult * partnerPsyche.Personality.GetMultiplier(RimpsycheDatabase.SocialFightChanceMultiplier), partnerOpinion);
                Log.Message($"pawnStartFightChance: {pawnStartFightChance}. partnerStartFightChance: {partnerStartFightChance}.");
                if (Rand.Chance(pawnStartFightChance))
                {
                    startFight = true;
                    startedByParentPawn = true;
                }
                else if (Rand.Chance(pawnStartFightChance))
                {
                    startFight = true;
                    startedByParentPawn = false;
                }
                return startFight;
            }
            return startFight;
        }
        public bool AffectPawn(float resultOffset)
        {
            float adultHoodAge = Rimpsyche_Utility.GetMinAdultAge(parentPawn);
            float pawnTrust = parentPawn.compPsyche().personality.GetPersonality(PersonalityDefOf.Rimpsyche_Trust); //-1~1
            float pawnAge = (float)parentPawn.ageTracker.AgeBiologicalYears; //0~100
            float opinion = parentPawn.relations.OpinionOf(convoPartner) * 0.01f;
            float score = resultOffset; //0~20
            float ageFactor = 0.48f * adultHoodAge / (pawnAge + 0.6f * adultHoodAge) - 0.3f; //0.43333~-0.31072
            float scoreBase = Mathf.Max(0f,score-11f+pawnTrust*2f+ageFactor*10f);
            float influenceChance = scoreBase*scoreBase * (1f + opinion*0.2f) * 0.0025f;
            Log.Message($"{parentPawn.Name} affect pawn entered with {resultOffset}. scorebase: {scoreBase} direction: {direction}, chance {influenceChance}");
            if (Mathf.Approximately(influenceChance, 0f)) return false;
            if (Rand.Chance(Mathf.Clamp01(influenceChance)))
            {
                influenceChance *= direction;
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
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref lastProgressTick, "lastProgressTick", -1);
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
