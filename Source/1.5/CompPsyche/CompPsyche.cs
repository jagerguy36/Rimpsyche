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
        public float topicScore;

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

        public override void CompTick()
        {
            base.CompTick();
            if (convoStartedTick > 0)
            {
                if (ShouldEndConvoImmediately())
                {
                    var convoPartnerPsyche = convoPartner.compPsyche();
                    convoPartnerPsyche?.EndConvo();
                    EndConvo();
                    return;
                }
                if (parentPawn.IsHashIntervalTick(200)) //InteractionsTrackerTick checks every interval tick 91
                {
                    Log.Message($"{parentPawn.Name} checking conversation validity with {convoPartner.Name} on topic {topic.name}, in {topic.category} with score {topicScore}");
                    if (ShouldEndConvo())
                    {
                        var convoPartnerPsyche = convoPartner.compPsyche();
                        convoPartnerPsyche?.EndConvo();
                        EndConvo();
                        return;
                    }
                }
                if (convoCheckTick > 0)
                {
                    if (convoCheckTick < Find.TickManager.TicksGame)
                    {
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
                        var convoPartnerPsyche = convoPartner.compPsyche();
                        convoPartnerPsyche?.EndConvo();
                        EndConvo();
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
        public void EndConvo()
        {
            Log.Message($"{parentPawn.Name} ending conversation with {convoPartner.Name}");
            float finalScore = topicScore;
            //Tolerance is used for opinion. This should use switch.
            if (finalScore < 0)
            {
                finalScore = Mathf.Min(finalScore + parentPawn.compPsyche().Personality.Tolerance, 0f);
            }
            if (convoPartner != null)
            {
                ThoughtDef newDef = Rimpsyche_Utility.CreateSocialThought(
                    parentPawn.GetHashCode() + "Conversation" + topic.name,
                    "ConversationStage".Translate() + " " + topic.name,
                    topicScore * 10);
                parentPawn.needs?.mood?.thoughts?.memories?.TryGainMemory(newDef, convoPartner);
            }
            convoStartedTick = -1;
            convoCheckTick = -1;
            convoPartner = null;
            topic = null;
            topicScore = 0;
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
            Scribe_Values.Look(ref topicScore, "topicScore");
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
