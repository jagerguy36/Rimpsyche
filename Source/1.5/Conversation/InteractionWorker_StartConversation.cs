using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace Maux36.RimPsyche
{
    public class InteractionWorker_StartConversation : InteractionWorker
    {
        public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
        {
            //if (initiator.compPsyche()?.convoStartedTick > 0)
            //{
            //    //Log.Message($"initiator {initiator.Name} already holding a conversation");
            //    return 0f;
            //}
            //if (recipient.compPsyche()?.convoStartedTick > 0)
            //{
            //    //Log.Message($"recipient {recipient.Name} already holding a conversation");
            //    return 0f;
            //}
            if (!initiator.health.capacities.CapableOf(PawnCapacityDefOf.Talking) || !recipient.health.capacities.CapableOf(PawnCapacityDefOf.Talking))
            {
                return 0f;
            }
            var initiatorCompPsyche = initiator.compPsyche();
            if (initiatorCompPsyche != null)
            {
                float convoChance = 1f + initiatorCompPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Talkativeness); // 0~2
                float relationshipOffset = 1f + 0.01f * initiator.relations.OpinionOf(recipient); // 0~2 
                convoChance *= relationshipOffset;
                //Log.Message($"{initiator.Name} weight {recipient.Name} : 0.5f * {convoChance}.");
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

                //Select the convo interest area by initiator. See if the recipient is willing to talk to the initiator about that area.
                Interest convoInterest = initiatorPsyche.Interests.ChoseInterest();

                float opinion = (recipient.relations.OpinionOf(initiator)) * 0.01f;
                if (opinion < 0)
                {
                    float recipientInterestScore = recipientPsyche.Interests.GetOrCreateInterestScore(convoInterest) * 0.01f;
                    float recipientEngagement = recipientPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_SocialIntelligence);
                    float rejectionFactor = (recipientInterestScore + recipientEngagement + opinion) * 0.5f;
                    if (rejectionFactor < 0 && rejectionFactor * rejectionFactor * 0.95f < Rand.Value)
                    {
                        extraSentencePacks.Add(DefOfRimpsyche.Sentence_ConversationFail);
                        initiator.needs?.mood?.thoughts?.memories?.TryGainMemory(DefOfRimpsyche.Rimpsyche_ConvoIgnored, recipient);
                        return;
                    }
                }

                //Conversation.
                Topic convoTopic = convoInterest.GetRandomTopic();
                float topicScore = convoTopic.GetScore(initiator, recipient); // -1~1


                //Topic score.
                //chose interaction time based on the score. This should also differ based on the category
                //- > if recipient is not interested: less time, But if explorative: more time.
                //tolerance factors in for the recipient.
                int convoLength = 450 + 100*(int)topicScore + Rand.Range(-75, 75);//25tick == 36sec. || 275tick(6min 36sec) ~ 625tick(15min)
                Log.Message($"{initiator.Name} started conversation with {recipient.Name}. convoTopic: {convoTopic.name}. topicScore: {topicScore}. convoLength = {convoLength}");

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
                initiatorPsyche.topicScore = topicScore;
                initiatorPsyche.convoPartner = recipient;
                initiatorPsyche.convoCheckTick = Find.TickManager.TicksGame + convoLength;

                if (recipientPsyche.convoStartedTick < 0) recipientPsyche.convoStartedTick = Find.TickManager.TicksGame;
                recipientPsyche.topic = convoTopic;
                recipientPsyche.topicScore = topicScore;
                recipientPsyche.convoPartner = initiator;
            }
        }
    }
}
