using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Maux36.RimPsyche
{
    public class InteractionWorker_StartConversation : InteractionWorker
    {
        public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
        {
            if (!initiator.health.capacities.CapableOf(PawnCapacityDefOf.Talking) || !recipient.health.capacities.CapableOf(PawnCapacityDefOf.Talking))
            {
                return 0f;
            }
            var initiatorPsyche = initiator.compPsyche();
            var recipientPsyche = recipient.compPsyche();
            if (initiatorPsyche != null && recipientPsyche != null)
            {

                float initSociability = initiatorPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Sociability);
                float initSpontaneity = initiatorPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Spontaneity);
                float initTalkativeness = initiatorPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Talkativeness);
                float initOpinion = (initiator.relations.OpinionOf(recipient)) * 0.01f; //-1~1

                if (initOpinion < 0f)
                {
                    bool giveupConverse = initOpinion + initSociability + initSpontaneity + Rand.Value < 0f;
                    if (giveupConverse) return 0f;
                }
                float convoChance = 1f + initTalkativeness; // 0~[1]~2
                float relationshipOffset = 1f + initOpinion; // 0~[1]~2 
                convoChance += relationshipOffset; //0~[2]~4
                return 0.8f * convoChance; //0~[1.6]~3.2
            }
            else
            {
                return 0f;
            }           
        }


        public override void Interacted(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks, out string letterText, out string letterLabel, out LetterDef letterDef, out LookTargets lookTargets)
        {
            //Log.Message($"Interacted called by {initiator.Name} with {recipient.Name}.");
            letterText = null;
            letterLabel = null;
            letterDef = null;
            lookTargets = null;
            var initiatorPsyche = initiator.compPsyche();
            var recipientPsyche = recipient.compPsyche();
            if (initiatorPsyche != null && recipientPsyche != null)
            {
                // -1 ~ 1
                float initOpinion = initiator.relations.OpinionOf(recipient) * 0.01f;
                float initTalkativeness = initiatorPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Talkativeness);
                float initPassion = initiatorPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Passion);
                float initInquisitiveness = initiatorPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Inquisitiveness);
                float initSpontaneity = initiatorPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Spontaneity);

                float reciOpinion = recipient.relations.OpinionOf(recipient) * 0.01f;
                float reciSociability = recipientPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Sociability);
                float reciTalkativeness = recipientPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Talkativeness);
                float reciPassion = recipientPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Passion);
                float reciInquisitiveness = recipientPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Inquisitiveness);
                float reciSpontaneity = recipientPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Spontaneity);


                //Select the convo interest area by initiator. See if the recipient is willing to talk to the initiator about that area.
                Interest convoInterest = initiatorPsyche.Interests.ChoseInterest();
                // 0 ~ 1
                float initInterestScore = recipientPsyche.Interests.GetOrCreateInterestScore(convoInterest) * 0.01f;
                float reciInterestScore = recipientPsyche.Interests.GetOrCreateInterestScore(convoInterest) * 0.01f;

                //If the opinion is negative, there is a chance for the pawn to brush off the conversation.
                if (reciOpinion < 0)
                {
                    float participateFactor = (reciInterestScore + reciSociability + reciOpinion + 2f) * 0.2f; // 0 ~ 1
                    if (Rand.Chance(1 - participateFactor))
                    {
                        extraSentencePacks.Add(DefOfRimpsyche.Sentence_RimpsycheConversationFail);
                        initiator.needs?.mood?.thoughts?.memories?.TryGainMemory(DefOfRimpsyche.Rimpsyche_ConvoIgnored, recipient);
                        return;
                    }
                }

                //Conversation.
                Topic convoTopic = convoInterest.GetRandomTopic();
                float topicAlignment = convoTopic.GetScore(initiator, recipient, out float initDirection); // -1~1 [0]
                float tAbs = Mathf.Abs(topicAlignment);
                float initInterestF = (1f + (0.5f * initOpinion)) + (initInterestScore * (1f + (0.5f * initPassion))) + ((1f - initInterestScore) * (1f + (0.5f * initInquisitiveness))); // 0.5~3 [1.5]
                float reciInterestF = (1f + (0.5f * reciOpinion)) + (reciInterestScore * (1f + (0.5f * reciPassion))) + ((1f - reciInterestScore) * (1f + (0.5f * reciInquisitiveness))); ; // 0.5~3 [1.5]
                float initTalkF = (1.5f + initTalkativeness) * initInterestF; // 0.25~7.5 [2.25]
                float reciTalkF = (1.5f + reciTalkativeness) * reciInterestF; // 0.25~7.5 [2.25]
                float spontaneousF = (initSpontaneity + reciSpontaneity + 2f) * 0.05f; // 0~0.2 [0.1]
                float aligntmentLengthFactor = -1.5f * tAbs * (tAbs - 2f) + 1f;
                int convoLength = (int)((4f + initTalkF + reciTalkF) * 25f * aligntmentLengthFactor * Rand.Range(1f - spontaneousF, 1f + spontaneousF)); //25 * (4.5~[8.5]~19)*([1]~2.5) || 90(112.5~[212.5]~1187.5)1425 [2min~30min]

                //continuation
                float continuationChance = 0f;
                continuationChance += 0.1f; // 10% base chance
                continuationChance += ((initTalkativeness + reciTalkativeness) / 2f) * 0.1f; // -0.1 ~ 0.1
                continuationChance += tAbs * 0.3f; // 0 ~ 0.3
                continuationChance += (initInterestF * reciInterestF) * 0.01f; // 0.075 ~ 0.027
                continuationChance = Mathf.Clamp01(continuationChance);
                continuationChance *= Rand.Range(1f - spontaneousF, 1f + spontaneousF);
                Log.Message($"{initiator.Name} started conversation with {recipient.Name}. convoTopic: {convoTopic.name}. topicAlignment: {topicAlignment}. convoLength = {convoLength}. continuationChance: {continuationChance}");

                if (initiatorPsyche.convoStartedTick < 0) initiatorPsyche.convoStartedTick = Find.TickManager.TicksGame;
                initiatorPsyche.topic = convoTopic;
                initiatorPsyche.topicAlignment = topicAlignment;
                initiatorPsyche.direction = initDirection;
                initiatorPsyche.convoPartner = recipient;
                initiatorPsyche.convoCheckTick = Find.TickManager.TicksGame + convoLength;
                initiatorPsyche.continuationChance = continuationChance;

                if (recipientPsyche.convoStartedTick < 0) recipientPsyche.convoStartedTick = Find.TickManager.TicksGame;
                recipientPsyche.topic = convoTopic;
                recipientPsyche.topicAlignment = topicAlignment;
                recipientPsyche.direction = -initDirection;
                recipientPsyche.convoPartner = initiator;
            }
        }
    }
}
