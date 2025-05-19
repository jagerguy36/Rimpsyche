using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
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
        public Dictionary<string, float> interestScore;
        public Pawn_PersonalityTracker personality;

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
            InterestScoreSetup();
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            PsycheValueSetup();
            InterestScoreSetup();
        }

        public void PsycheValueSetup()
        {
            if (personality == null)
            {
                personality = new Pawn_PersonalityTracker(parentPawn);
                GiveRandomPsycheValue();
            }
        }
        public void InterestScoreSetup()
        {
            if (interestScore == null)
            {
                interestScore = new Dictionary<string, float>();
                GenerateInterestScores();

            }
        }

        public void GiveRandomPsycheValue()
        {
            personality.Initialize();
        }
        public void GenerateInterestScores()
        {
            foreach (InterestDomainDef interestdomainDef in DefDatabase<InterestDomainDef>.AllDefs)
            {
                Log.Message($"{interestdomainDef.label}");
                float baseValue = 5;
                if (interestdomainDef.scoreWeight != null)
                {
                    baseValue = 7; //get base value from facets
                }
                float result;
                int attempts = 0;
                do
                {
                    result = Rand.Gaussian(baseValue, 0.5f); // center at basevalue, 3widthfactor == 1.5
                    attempts++;
                }
                while ((result < 0f || result > 10f) && attempts < 2);

                // Optional: Clamp to valid range if all attempts fail
                if (result < 0f || result > 10f)
                {
                    result = Mathf.Clamp(result, 0f, 10f);
                    Log.Warning($"GenerateInterestScores failed to get valid value in {2} attempts. Clamped to {result}.");
                }
                interestScore[interestdomainDef.defName] = result;
            }
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
        public float GetOrCreateInterestScore(string key, Func<float> valueFactory)
        {
            if (!interestScore.TryGetValue(key, out float value))
            {
                value = valueFactory();             // Generate the value
                interestScore[key] = value;         // Store it in the dictionary
            }
            return value;
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
            Scribe_Collections.Look(ref interestScore, "interestScore", LookMode.Value, LookMode.Value);
        }

    }


}
