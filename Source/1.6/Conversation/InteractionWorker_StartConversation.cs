using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

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
                float initTact = initiatorPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Tact);
                initTact = Mathf.Clamp(initTact + (0.1f * initiator.skills.GetSkill(SkillDefOf.Social).Level), -1f, 1f);
                float initTalkativeness = initiatorPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Talkativeness);
                float initPassion = initiatorPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Passion);
                float initInquisitiveness = initiatorPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Inquisitiveness);
                float initSpontaneity = initiatorPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Spontaneity);
                float initOpenness = initiatorPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Openness);
                float initTrust = initiatorPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Trust);

                float reciOpinion = recipient.relations.OpinionOf(recipient) * 0.01f;
                float reciTact = recipientPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Tact);
                reciTact = Mathf.Clamp(reciTact + (0.1f * recipient.skills.GetSkill(SkillDefOf.Social).Level), -1f, 1f);
                float reciSociability = recipientPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Sociability);
                float reciTalkativeness = recipientPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Talkativeness);
                float reciPassion = recipientPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Passion);
                float reciInquisitiveness = recipientPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Inquisitiveness);
                float reciSpontaneity = recipientPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Spontaneity);
                float reciOpenness = recipientPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Openness);
                float reciTrust = recipientPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Trust);


                //Select the convo interest area by initiator. See if the recipient is willing to talk to the initiator about that area.
                Interest convoInterest = initiatorPsyche.Interests.ChoseInterest();
                Topic convoTopic = convoInterest.GetRandomTopic((initiator.DevelopmentalStage.Juvenile() || recipient.DevelopmentalStage.Juvenile()), true); //TODO: NSFW check
                // 0 ~ 1
                float initInterestScore = recipientPsyche.Interests.GetOrCreateInterestScore(convoInterest) * 0.01f;
                float reciInterestScore = recipientPsyche.Interests.GetOrCreateInterestScore(convoInterest) * 0.01f;

                //Add hailing log first
                PlayLogEntry_InteractionConversation entry = new PlayLogEntry_InteractionConversation(DefOfRimpsyche.Rimpsyche_ReportConversation, initiator, recipient, convoTopic.name, convoTopic.label, null);
                Find.PlayLog.Add(entry);

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
                extraSentencePacks.Add(DefOfRimpsyche.Sentence_RimpsycheConversationSuccess);

                //Conversation.
                float topicAlignment = convoTopic.GetScore(initiator, recipient, out float initDirection); // -1~1 [0]
                float tAbs = Mathf.Abs(topicAlignment);
                float initInterestF = (1f + (0.5f * initOpinion)) + (initInterestScore * (1f + (0.5f * initPassion))) + 0.25f * ((1f - initInterestScore) * (1f + initInquisitiveness)); //0.5~1.5+ 0~1.5 => 0.5~3 [1.5]
                float reciInterestF = (1f + (0.5f * reciOpinion)) + (reciInterestScore * (1f + (0.5f * reciPassion))) + 0.25f * ((1f - reciInterestScore) * (1f + reciInquisitiveness)); //0.5~1.5+ 0~1.5 => 0.5~3 [1.5]
                float initTalkF = (1.5f + initTalkativeness) * initInterestF; // 0.25~7.5 [2.25]
                float reciTalkF = (1.5f + reciTalkativeness) * reciInterestF; // 0.25~7.5 [2.25]
                float spontaneousF = (initSpontaneity + reciSpontaneity + 2f) * 0.05f; // 0~0.2 [0.1]
                float aligntmentLengthFactor = -1.5f * tAbs * (tAbs - 2f) + 1f;
                float lengthMult = 0.1f * (5f + initTalkF + reciTalkF) * aligntmentLengthFactor * Rand.Range(1f - spontaneousF, 1f + spontaneousF); // 0.1f * (5.5~[9.5]~20) * ([1]~2.5) || 0.55~[0.95]~5

                //GetResult
                bool startFight = false;
                bool startedByParentPawn = false;
                float pawnScore;
                float partnerScore;
                float talkRand = Rand.Value;

                if (topicAlignment > 0)
                {
                    float partnerScoreBase = 1f + (0.5f * reciOpinion) + (2f * topicAlignment); //0.5[2]3.5
                    float partnerScoreModifier = (0.2f * initTact) + (0.2f * initPassion); //-0.4~[0]~0.4
                    partnerScoreModifier = (1f + talkRand) * partnerScoreModifier; // -0.8~[0]~0.8
                    partnerScore = (partnerScoreBase + partnerScoreModifier); // -0.3[2]4.3

                    float pawnScoreBase = 1f + (0.5f * initOpinion) + (2f * topicAlignment); //0.5[2]3.5
                    float pawnScoreModifier = (0.2f * reciTact) + (0.2f * reciPassion); //-0.4~[0]~0.4
                    pawnScoreModifier = (1f + talkRand) * pawnScoreModifier; // -0.8~[0]~0.8
                    pawnScore = (pawnScoreBase + pawnScoreModifier); // -0.3[2]4.3

                    if (partnerScore < 0f || pawnScore < 0f)
                    {
                        extraSentencePacks.Add(DefOfRimpsyche.Sentence_RimpsycheConversationPositiveBad);
                    }
                    else
                    {
                        extraSentencePacks.Add(DefOfRimpsyche.Sentence_RimpsycheConversationPositiveGood);
                    }
                }
                else
                {
                    //Negative Alignment
                    float pawnReceiveScore = (reciTact * (reciTalkativeness + 1) * 0.5f) + (initOpenness * (initTrust + 1) * 0.5f) + initOpinion; // -3~[0]~3
                    float partnerReceiveScore = (initTact * (initTalkativeness + 1) * 0.5f) + (reciOpenness * (reciTrust + 1) * 0.5f) + initOpinion; // -3~[0]~3

                    float goodTalkChance = (3f + pawnReceiveScore + partnerReceiveScore) * (0.10f + (0.05f * topicAlignment)); // (3 ~ 9)  * (0.05 ~ 0.1) = 0.15 ~ 0.9
                    if (pawnReceiveScore > 0f && partnerReceiveScore > 0f && talkRand > 1f - goodTalkChance)
                    {
                        partnerScore = partnerReceiveScore * talkRand; // 0~3
                        pawnScore = pawnReceiveScore * talkRand;// 0~3
                        extraSentencePacks.Add(DefOfRimpsyche.Sentence_RimpsycheConversationNegativeGood);
                    }
                    else
                    {
                        //Bad Talk
                        float negativeScoreBase = 2f * topicAlignment * (1f - talkRand); // -2~[-1]~0
                        pawnScore = negativeScoreBase * (1f - (0.3f * pawnReceiveScore)); //-3.8 ~ 0
                        partnerScore = negativeScoreBase * (1f - (0.3f * partnerReceiveScore)); //(-2~0) * 0.1~1.9 = -3.8 ~[-1]~ 0
                        //Calcualte fight Chance
                        float pawnStartCandBaseChance = -0.005f * pawnScore * lengthMult * initiatorPsyche.Personality.GetMultiplier(RimpsycheDatabase.SocialFightChanceMultiplier);
                        float partnerStartCandBaseChance = -0.005f * partnerScore * lengthMult * recipientPsyche.Personality.GetMultiplier(RimpsycheDatabase.SocialFightChanceMultiplier);
                        if (pawnStartCandBaseChance >= 0.005f)
                        {

                            float pawnStartFightChance = Rimpsyche_Utility.ConvoSocialFightChance(initiator, recipient, pawnStartCandBaseChance, initOpinion);
                            if (Rand.Chance(pawnStartFightChance))
                            {
                                startFight = true;
                            }
                        }
                        else if (partnerStartCandBaseChance >= 0.005f)
                        {

                            float partnerStartFightChance = Rimpsyche_Utility.ConvoSocialFightChance(initiator, recipient, partnerStartCandBaseChance, reciOpinion);
                            if (Rand.Chance(partnerStartFightChance))
                            {
                                startFight = true;
                                startedByParentPawn = true;
                            }
                        }
                        
                        if (startFight)
                        {
                            if (startedByParentPawn)
                            {
                                initiator.interactions.StartSocialFight(recipient);
                                extraSentencePacks.Add(DefOfRimpsyche.Sentence_RimpsycheSocialFightConvoInitiatorStarted);
                            }
                            else
                            {
                                recipient.interactions.StartSocialFight(initiator);
                                extraSentencePacks.Add(DefOfRimpsyche.Sentence_RimpsycheSocialFightConvoRecipientStarted);
                            }
                        }
                        else
                        {
                            extraSentencePacks.Add(DefOfRimpsyche.Sentence_RimpsycheConversationNegativeBad);
                        }
                    }
                }

                float lengthOpinionMult = (6f * lengthMult) / (lengthMult + 2f);
                float initOpinionOffset = pawnScore * lengthOpinionMult;
                float reciOpinionOffset = partnerScore * lengthOpinionMult;
                Log.Message($"GetConvoResult: {initiator.Name}: {pawnScore} | {recipient.Name}: {partnerScore} | lengthOpinionMult: {lengthOpinionMult}");
                if (initOpinionOffset != 0)
                {
                    Rimpsyche_Utility.GainCoversationMemoryFast(convoTopic.name, convoTopic.label, initOpinionOffset, initiator, recipient);
                    if (initOpinionOffset > 0) initiatorPsyche.AffectPawn(initOpinionOffset, initOpinion, convoTopic, initDirection);
                }
                if (reciOpinionOffset != 0)
                {
                    Rimpsyche_Utility.GainCoversationMemoryFast(convoTopic.name, convoTopic.label, reciOpinionOffset, recipient, initiator);
                    if (reciOpinionOffset > 0) recipientPsyche.AffectPawn(reciOpinionOffset, reciOpinion, convoTopic, -initDirection);
                }
            }
        }
    }
}
