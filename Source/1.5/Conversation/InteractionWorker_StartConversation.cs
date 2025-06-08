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
                float initSpontaneousness = initiatorPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Spontaneousness);
                float initTalkativeness = initiatorPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Talkativeness);
                float initOpinion = (initiator.relations.OpinionOf(recipient)) * 0.01f; //-1~1

                if (initOpinion < 0f)
                {
                    bool giveupConverse = initOpinion + initSociability + initSpontaneousness + Rand.Value < 0f;
                    if (giveupConverse) return 0f;
                }
                float convoChance = 1f + initTalkativeness; // 0~2
                float relationshipOffset = 1f + initOpinion; // 0~2 
                convoChance *= relationshipOffset; //0~4
                return 0.5f * convoChance;
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
                float initSociability = initiatorPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Sociability);
                float initTalkativeness = initiatorPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Talkativeness);
                float initTact = initiatorPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Tact);

                float reciOpinion = recipient.relations.OpinionOf(recipient) * 0.01f;
                float reciSociability = recipientPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Sociability);
                float reciTalkativeness = recipientPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Talkativeness);
                float reciTact = recipientPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Tact);


                //Select the convo interest area by initiator. See if the recipient is willing to talk to the initiator about that area.
                Interest convoInterest = initiatorPsyche.Interests.ChoseInterest();
                // 0 ~ 1
                float initInterestScore = recipientPsyche.Interests.GetOrCreateInterestScore(convoInterest) * 0.01f;
                float reciInterestScore = recipientPsyche.Interests.GetOrCreateInterestScore(convoInterest) * 0.01f;

                //If the opinion is negative, there is a chance for the pawn to brush off the conversation.
                if (reciOpinion < 0)
                {
                    float participateFactor = (reciInterestScore + reciTact + reciOpinion + 2f) * 0.2f; // 0 ~ 1
                    if (Rand.Chance(1-participateFactor))
                    {
                        extraSentencePacks.Add(DefOfRimpsyche.Sentence_ConversationFail);
                        initiator.needs?.mood?.thoughts?.memories?.TryGainMemory(DefOfRimpsyche.Rimpsyche_ConvoIgnored, recipient);
                        return;
                    }
                }

                //Conversation.
                Topic convoTopic = convoInterest.GetRandomTopic();
                float topicAlignment = convoTopic.GetScore(initiator, recipient); // -1~1
                float initTalk = initInterestScore * (initTalkativeness + 1.0f); //0~2
                float reciTalk = reciInterestScore * (reciTalkativeness + 1.0f); //0~2
                int convoLength = (int)((8f + initTalk + reciTalk) * 20f * (2f+Mathf.Abs(topicAlignment)) * Rand.Range(0.8f, 1.2f));  //(8~12)*20*(2~3)*(0.8~1.2) = 256(320~720)864  || approx. 250tick(5min) ~ 850tick(20min 24sec)
                Log.Message($"{initiator.Name} started conversation with {recipient.Name}. convoTopic: {convoTopic.name}. topicAlignment: {topicAlignment}. convoLength = {convoLength}");

                //Precalcualte result chance (so that we don't have to calculate opinion again)



                //- > also get talk outcome chance.
                //at the end of the conversation, the chance will be calculated.
                //if chance yields true -> more conversation is possible, forming a conversation chain
                //(the chain chance is determined by the outcome and total length of the conversation
                //At the end of the chain, the total score is considered to get the 'mattered' bool
                //- > as a result, facets maybe influenced. (if matterred==true)
                //- > long convo -> high chance of matterred=true

                //- > Personality core only : general topics.
                //- > Interest/Hobbies : Interests include topics
                //- > What topic is talked about is based on their interests. More social pawns can bring up topics the other might be interested in.
                //- > Topics have 'attitude' that gets generated by vectormulting its weights*facets + social skill level influence.
                //- > based on this attitude, convolength, conversation result varies.

                //- > See if it's too heavy. If it is, truncate the logic until it's light.


                if (initiatorPsyche.convoStartedTick < 0) initiatorPsyche.convoStartedTick = Find.TickManager.TicksGame;
                initiatorPsyche.topic = convoTopic;
                initiatorPsyche.topicAlignment = topicAlignment;
                initiatorPsyche.convoPartner = recipient;
                initiatorPsyche.convoCheckTick = Find.TickManager.TicksGame + convoLength;

                if (recipientPsyche.convoStartedTick < 0) recipientPsyche.convoStartedTick = Find.TickManager.TicksGame;
                recipientPsyche.topic = convoTopic;
                recipientPsyche.topicAlignment = topicAlignment;
                recipientPsyche.convoPartner = initiator;
            }
        }
    }
}
