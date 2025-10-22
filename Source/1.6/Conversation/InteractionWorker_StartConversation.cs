using RimWorld;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Verse;

namespace Maux36.RimPsyche
{
    public class InteractionWorker_StartConversation : InteractionWorker
    {
        public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
        {
            if (initiator.Inhumanized())
			{
				return 0f;
			}
            var initiatorPsyche = initiator.compPsyche();
            var recipientPsyche = recipient.compPsyche();
            if (initiatorPsyche?.Enabled == true && recipientPsyche?.Enabled == true)
            {

                float initSociability = initiatorPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Sociability);
                float initSpontaneity = initiatorPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Spontaneity);
                float initTalkativeness = initiatorPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Talkativeness);
                float initOpinion = (initiator.relations.OpinionOf(recipient)) * 0.01f; //-1~1

                //TODO: elaborate the logic about initSpontaneity
                if (initOpinion < 0f)
                {
                    bool giveupConverse = initOpinion + initSociability + initSpontaneity + Rand.Value < 0f;
                    if (giveupConverse) return 0f;
                }
                float convoChance = 1f + initTalkativeness; // 0~[1]~2
                float relationshipOffset = 1f + initOpinion; // 0~[1]~2 
                convoChance += relationshipOffset; //0~[2]~4
                return 0.3f * convoChance; //0~[0.6]~1.2
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
            if (initiatorPsyche?.Enabled == true && recipientPsyche?.Enabled == true)
            {
                PlayLogEntry_InteractionConversation entry;

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
                float initAggressiveness = initiatorPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Aggressiveness);

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
                float reciAggressiveness = recipientPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Aggressiveness);


                //Select the convo interest area by initiator. See if the recipient is willing to talk to the initiator about that area.
                Interest convoInterest = initiatorPsyche.Interests.ChooseInterest();
                Topic convoTopic = convoInterest.GetRandomTopic((initiator.DevelopmentalStage.Juvenile() || recipient.DevelopmentalStage.Juvenile()), true); //TODO: NSFW check
                // 0 ~ 1
                float initInterestScore = recipientPsyche.Interests.GetOrCreateInterestScore(convoInterest) * 0.01f;
                float reciInterestScore = recipientPsyche.Interests.GetOrCreateInterestScore(convoInterest) * 0.01f;

                //If the opinion is negative, there is a chance for the pawn to brush off the conversation.
                if (reciOpinion < 0)
                {
                    float participateFactor = (reciInterestScore + reciSociability + reciOpinion + 2f) * 0.2f; // 0 ~ 1
                    if (Rand.Chance(1 - participateFactor))
                    {
                        
                        initiator.needs?.mood?.thoughts?.memories?.TryGainMemory(DefOfRimpsyche.Rimpsyche_ConvoIgnored, recipient);

                        extraSentencePacks.Add(DefOfRimpsyche.Sentence_RimpsycheConversationFail);
                        entry = new PlayLogEntry_InteractionConversation(DefOfRimpsyche.Rimpsyche_ConversationAttempt, initiator, recipient, convoTopic.name, convoTopic.label, extraSentencePacks);
                        Find.PlayLog.Add(entry);
                        return;
                    }
                }

                //Conversation.
                float topicAlignment = convoTopic.GetScore(initiator, recipient, out float initDirection); // -1~1 [0]
                float tAbs = Mathf.Abs(topicAlignment);
                float initInterestF = (1f + (0.5f * initOpinion)) + (initInterestScore * (1f + (0.5f * initPassion))) + 0.25f * ((1f - initInterestScore) * (1f + initInquisitiveness)); //0.5~1.5+ 0~1.5 => 0.5~3 [1.5]
                float reciInterestF = (1f + (0.5f * reciOpinion)) + (reciInterestScore * (1f + (0.5f * reciPassion))) + 0.25f * ((1f - reciInterestScore) * (1f + reciInquisitiveness)); //0.5~1.5+ 0~1.5 => 0.5~3 [1.5]
                float initTalkF = (1.75f + (0.75f * initTalkativeness)) * initInterestF; // 0.5~7.5 [2.625]
                float reciTalkF = (1.75f + (0.75f * reciTalkativeness)) * reciInterestF; // 0.5~7.5 [2.625]
                float spontaneousF = (initSpontaneity + reciSpontaneity + 2f) * 0.05f; // 0~0.2 [0.1]
                float aligntmentLengthFactor = -1f * tAbs * (tAbs - 2f) + 1f;
                float lengthMult = 0.1f * (5f + initTalkF + reciTalkF) * aligntmentLengthFactor * Rand.Range(1f - spontaneousF, 1f + spontaneousF); // 0.1f * (6~[10.25]~20) * ([1]~2) || 0.6~[1.025]~4

                //GetResult
                bool startFight = false;
                bool startedByInitiator = false;
                float pawnScore;
                float partnerScore;
                float talkRand = Rand.Value;

                if (topicAlignment > 0)
                {
                    float partnerScoreBase = 1f + (0.5f * reciOpinion) + (4f * topicAlignment); //0.5[2.5]5.5
                    float partnerScoreModifier = (0.2f * initTact) + (0.1f * (initPassion - initAggressiveness)); //-0.4~[0]~0.4
                    partnerScoreModifier = (1f + talkRand) * partnerScoreModifier; // -0.8~[0]~0.8
                    partnerScore = (partnerScoreBase + partnerScoreModifier); // -0.3[2.5]6.3

                    float pawnScoreBase = 1f + (0.5f * initOpinion) + (4f * topicAlignment); //0.5[2.5]5.5
                    float pawnScoreModifier = (0.2f * reciTact) + (0.1f * (reciPassion - reciAggressiveness)); //-0.4~[0]~0.4
                    pawnScoreModifier = (1f + talkRand) * pawnScoreModifier; // -0.8~[0]~0.8
                    pawnScore = (pawnScoreBase + pawnScoreModifier); // -0.3[2.5]6.3

                    if (partnerScore < 0f || pawnScore < 0f)
                    {
                        extraSentencePacks.Add(DefOfRimpsyche.Sentence_RimpsycheConversationPositiveBad);
                    }
                    else if (partnerScore > 3f || pawnScore > 3f)
                    {
                        extraSentencePacks.Add(DefOfRimpsyche.Sentence_RimpsycheConversationPositiveGreat);
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
                        float negativeScoreBase = (topicAlignment - 1f) * (1f - talkRand); // -2~[-1.5]~-1
                        pawnScore = negativeScoreBase * (1f - (0.2f * pawnReceiveScore)); // -3.2~[-1.5]~-0.4
                        partnerScore = negativeScoreBase * (1f - (0.2f * partnerReceiveScore)); //(-2~-1) * 0.4~1.6 = -3.2 ~[-1.5]~ -0.4
                        //Calcualte fight Chance
                        // 0.002 * opScore * 0.24~[1]~1.68
                        float pawnStartCandBaseChance = -0.002f * pawnScore * lengthMult * initiatorPsyche.Evaluate(RimpsycheDatabase.SocialFightChanceMultiplier);
                        float partnerStartCandBaseChance = -0.002f * partnerScore * lengthMult * recipientPsyche.Evaluate(RimpsycheDatabase.SocialFightChanceMultiplier);
                        //opScore to go over 0.005 ranges from -10.41 ~ [-2.5] ~ -1.488
                        //Initiator has the first chance to start the fight
                        if (pawnStartCandBaseChance >= 0.005f)
                        {
                            float pawnStartFightChance = Rimpsyche_Utility.ConvoSocialFightChance(initiator, recipient, pawnStartCandBaseChance, initOpinion);
                            if (Rand.Chance(pawnStartFightChance))
                            {
                                startFight = true;
                                startedByInitiator = true;
                            }
                        }
                        //If initiator didn't start the fight, check the recipient for fight
                        if (!startFight && partnerStartCandBaseChance >= 0.005f)
                        {
                            float partnerStartFightChance = Rimpsyche_Utility.ConvoSocialFightChance(recipient, initiator, partnerStartCandBaseChance, reciOpinion);
                            if (Rand.Chance(partnerStartFightChance))
                            {
                                startFight = true;
                            }
                        }
                        
                        if (startFight)
                        {
                            if (startedByInitiator)
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

                float lengthOpinionMult = (6f * lengthMult) / (lengthMult + 2f); //boost lower/middle part while maintaining the range(valid between 0~4). 1.38 ~ 4
                float initOpinionOffset = pawnScore * lengthOpinionMult;
                float reciOpinionOffset = partnerScore * lengthOpinionMult;
                //Log.Message($"GetConvoResult: {initiator.Name}: {initOpinionOffset} | {recipient.Name}: {reciOpinionOffset} | lengthOpinionMult: {lengthOpinionMult}");
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

                entry = new PlayLogEntry_InteractionConversation(DefOfRimpsyche.Rimpsyche_Conversation, initiator, recipient, convoTopic.name, convoTopic.label, extraSentencePacks);
                Find.PlayLog.Add(entry);
                InteractionHook(initiator, recipient, convoTopic, topicAlignment, initOpinionOffset, reciOpinionOffset);
            }
        }

        //Harmony hook for mod compatibility.
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void InteractionHook(Pawn initiator, Pawn recipient, Topic convoTopic, float alignment, float initOpinionOffset, float reciOpinionOffset) { }
    }
}
