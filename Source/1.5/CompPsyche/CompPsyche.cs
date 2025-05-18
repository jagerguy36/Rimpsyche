using RimWorld;
using Verse;


namespace Maux36.RimPsyche
{
    public class CompPsyche : ThingComp
    {
        //Internals
        private Pawn parentPawnInt = null;
        public bool PostGen = false;
        public int convoStartedTick = -1;
        public int convoCheckTick = -1;
        public Pawn convoPartner = null;
        private Pawn_PersonalityTracker personality;

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


        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            PsycheValueSetup();
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            PsycheValueSetup();
        }

        public void PsycheValueSetup(bool reset = false)
        {
            if (parentPawn != null || reset)
            {
                if(personality == null) personality = new Pawn_PersonalityTracker(parentPawn);
                GiveRandomPsycheValue();
            }
        }

        public void GiveRandomPsycheValue()
        {
            personality.Initialize();
        }
        public override void CompTick()
        {
            if (convoStartedTick > 0)
            {
                if (convoPartner?.Spawned != true || parentPawn.Spawned != true)
                {
                    var convoPartnerPsyche = convoPartner.compPsyche();
                    convoPartnerPsyche.EndConvo();
                    EndConvo();
                }
                if (parentPawn.IsHashIntervalTick(91)) //InteractionsTrackerTick checks every interval tick 91
                {
                    Log.Message($"{parentPawn.Name}'s curioisty: {Personality.curiosity}");
                    if (ShouldEndConvo())
                    {
                        var convoPartnerPsyche = convoPartner.compPsyche();
                        convoPartnerPsyche.EndConvo();
                        EndConvo();
                    }
                }
                if (convoCheckTick > 0 && convoCheckTick < Find.TickManager.TicksGame)
                {
                    Log.Message($"{parentPawn.Name} calcuates chance because gametick exceeded convocheck {convoCheckTick} : {(float)(Find.TickManager.TicksGame - convoStartedTick) / 2500f}");
                    var parentPawnStartanotherConvoChance = Rand.Chance(1f - (float)(Find.TickManager.TicksGame - convoStartedTick) / 2500f);
                    var partnerStartanotherConvoChance = Rand.Chance(1f - (float)(Find.TickManager.TicksGame - convoStartedTick) / 2500f);
                    if (parentPawnStartanotherConvoChance)
                    {
                        Log.Message($"another convo {DefOfRimpsyche.Rimpsyche_Conversation.defName} start from {parentPawn.Name} with {convoPartner.Name}.");
                        parentPawn.interactions.TryInteractWith(convoPartner, DefOfRimpsyche.Rimpsyche_Conversation);
                    }
                    else if (partnerStartanotherConvoChance)
                    {
                        Log.Message($"another convo {DefOfRimpsyche.Rimpsyche_Conversation.defName} start from {convoPartner.Name} with {parentPawn.Name}.");
                        convoPartner.interactions.TryInteractWith(parentPawn, DefOfRimpsyche.Rimpsyche_Conversation);
                    }
                else
                {
                    Log.Message($"stop chance true");
                    var convoPartnerPsyche = convoPartner.compPsyche();
                    convoPartnerPsyche.EndConvo();
                    EndConvo();
                }

                }
            }
        }

        public bool ShouldEndConvo()
        {
            if (!InteractionUtility.CanReceiveRandomInteraction(parentPawn) || !InteractionUtility.CanReceiveRandomInteraction(convoPartner))
            {
                return true;
            }
            if (!IsGoodPositionForInteraction(parentPawn.Position, convoPartner.Position, parentPawn.Map))
            {
                return true;
            }
            return false;
        }
        public void EndConvo()
        {
            Log.Message($"{parentPawn.Name} ending conversation with {convoPartner.Name}");
            convoStartedTick = -1;
            convoPartner = null;
        }
        public static bool IsGoodPositionForInteraction(IntVec3 cell, IntVec3 recipientCell, Map map)
        {
            if (cell.InHorDistOf(recipientCell, 12f))
            {
                return GenSight.LineOfSight(cell, recipientCell, map, skipFirstCell: true);
            }
            return false;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref PostGen, "PostGen");
            Scribe_Values.Look(ref convoStartedTick, "convoStartedTick");
            Scribe_References.Look(ref convoPartner, "convoPartner");
            Scribe_Deep.Look(ref personality, "personality", new object[] { parent as Pawn });
        }

    }


}
