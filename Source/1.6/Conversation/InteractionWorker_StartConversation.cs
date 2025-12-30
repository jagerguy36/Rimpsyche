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
            var initiatorPsyche = initiator.compPsyche();
            var recipientPsyche = recipient.compPsyche();
            if (initiatorPsyche?.Enabled != true || recipientPsyche?.Enabled != true) return 0;
            var initPersonality = initiatorPsyche.Personality;
            float initSociability = initPersonality.GetPersonality(PersonalityDefOf.Rimpsyche_Sociability);
            float initSpontaneity = initPersonality.GetPersonality(PersonalityDefOf.Rimpsyche_Spontaneity);
            float initTalkativeness = initPersonality.GetPersonality(PersonalityDefOf.Rimpsyche_Talkativeness);
            float initOpinion = (initiator.relations.OpinionOf(recipient)) * 0.01f; //-1~1

            if (initOpinion < 0f)
            {
                bool giveupConverse = initOpinion + initSociability + (1f + initSpontaneity) * Rand.ValueSeeded(Find.TickManager.TicksGame) < 0f;
                if (giveupConverse) return 0f;
            }
            float convoChance = 1f + initTalkativeness; // 0~[1]~2
            convoChance +=  1f + initOpinion; //0~[2]~4
            return 0.3f * convoChance; //0~[0.6]~1.2
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
                float initRelationship = initiatorPsyche.Sexuality.GetRelationshipWith(recipient);
                var initPersonality = initiatorPsyche.Personality;
                float initTact = initPersonality.GetPersonality(PersonalityDefOf.Rimpsyche_Tact);
                initTact = Mathf.Clamp(initTact + (0.1f * initiator.skills.GetSkill(SkillDefOf.Social).Level), -1f, 1f);
                float initPassion = initPersonality.GetPersonality(PersonalityDefOf.Rimpsyche_Passion);
                float initInquisitiveness = initPersonality.GetPersonality(PersonalityDefOf.Rimpsyche_Inquisitiveness);
                float initSpontaneity = initPersonality.GetPersonality(PersonalityDefOf.Rimpsyche_Spontaneity);

                float reciOpinion = recipient.relations.OpinionOf(initiator) * 0.01f;
                float reciRelationship = recipientPsyche.Sexuality.GetRelationshipWith(initiator);
                var reciPersonality = recipientPsyche.Personality;
                float reciTact = reciPersonality.GetPersonality(PersonalityDefOf.Rimpsyche_Tact);
                reciTact = Mathf.Clamp(reciTact + (0.1f * recipient.skills.GetSkill(SkillDefOf.Social).Level), -1f, 1f);
                float reciSociability = reciPersonality.GetPersonality(PersonalityDefOf.Rimpsyche_Sociability);
                float reciPassion = reciPersonality.GetPersonality(PersonalityDefOf.Rimpsyche_Passion);
                float reciInquisitiveness = reciPersonality.GetPersonality(PersonalityDefOf.Rimpsyche_Inquisitiveness);
                float reciSpontaneity = reciPersonality.GetPersonality(PersonalityDefOf.Rimpsyche_Spontaneity);

                //Select the convo interest area by initiator. See if the recipient is willing to talk to the initiator about that area.
                bool limitNSFW = false //TODO: NSFW check with proriety
                int participantIndex = GetParticipantIndex(initiator.DevelopmentalStage.Adult(), recipient.DevelopmentalStage.Adult(), limitNSFW);
                Interest convoInterest = initiatorPsyche.Interests.ChooseInterest(participantIndex);
                Topic convoTopic = convoInterest.GetRandomTopic(participantIndex);
                //Topic null case (Should not happen): Add log no available topic

                // 0 ~ 1
                float initInterestScore = initiatorPsyche.Interests.GetOrCreateInterestScore(convoInterest) * 0.01f;
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
                float topicAlignment = convoTopic.GetScore(initiatorPsyche, recipientPsyche, out float initDirection); // -1~1 [0]
                float tAbs = Mathf.Abs(topicAlignment);
                float initInterestF = (1f + (0.5f * Mathf.Min(initOpinion + initRelationship, 1f))) + (initInterestScore * (1f + (0.5f * initPassion))) + 0.25f * ((1f - initInterestScore) * (1f + initInquisitiveness)); //0.5~1.5+ 0~1.5 => 0.5~3 [1.5]
                float reciInterestF = (1f + (0.5f * Mathf.Min(reciOpinion + reciRelationship, 1f))) + (reciInterestScore * (1f + (0.5f * reciPassion))) + 0.25f * ((1f - reciInterestScore) * (1f + reciInquisitiveness)); //0.5~1.5+ 0~1.5 => 0.5~3 [1.5]
                float initTalkF = initiatorPsyche.Evaluate(RimpsycheDatabase.TalkFactor) * initInterestF; //1~2.5 [1.75] * 0.5~3 [1.5] ||  0.5~7.5 [2.625]
                float reciTalkF = recipientPsyche.Evaluate(RimpsycheDatabase.TalkFactor) * reciInterestF; //1~2.5 [1.75] * 0.5~3 [1.5] ||  0.5~7.5 [2.625]
                float spontaneousF = (initSpontaneity + reciSpontaneity + 2f) * 0.05f; // 0~0.2 [0.1]
                float aligntmentLengthFactor = -1f * tAbs * (tAbs - 2f) + 1f; //1~2
                float lengthMult = 0.1f * (5f + initTalkF + reciTalkF) * aligntmentLengthFactor * Rand.Range(1f - spontaneousF, 1f + spontaneousF); // 0.1f * (6~[10.25]~20) * ([1]~2) || 0.6~[1.025]~4

                //GetResult
                bool startFight = false;
                bool startedByInitiator = false;
                float scoreBoost = 1f;
                float pawnScore;
                float partnerScore;
                float talkRand = Rand.Value;

                if (topicAlignment > 0)
                {
                    float initAcceptance = 1f + (0.5f * initOpinion) + (4f * topicAlignment); //0.5[2.5]5.5
                    float recipientEloquence = (0.2f * reciTact) + recipientPsyche.Evaluate(RimpsycheDatabase.Fervor);  //-0.4~[0]~0.4
                    recipientEloquence = (1f + talkRand) * recipientEloquence; // -0.8~[0]~0.8
                    pawnScore = (initAcceptance + recipientEloquence + 0.5f * initRelationship); // -0.3[2.5]6.8

                    float reciAcceptance = 1f + (0.5f * reciOpinion) + (4f * topicAlignment); //0.5[2.5]5.5
                    float initiatorEloquence = (0.2f * initTact) + initiatorPsyche.Evaluate(RimpsycheDatabase.Fervor); //-0.4~[0]~0.4
                    initiatorEloquence = (1f + talkRand) * initiatorEloquence; // -0.8~[0]~0.8
                    partnerScore = (reciAcceptance + initiatorEloquence + 0.5f * reciRelationship); // -0.3[2.5]6.8

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
                    float pawnReceiveScore = recipientPsyche.Evaluate(RimpsycheDatabase.AssertBase) + initiatorPsyche.Evaluate(RimpsycheDatabase.ReceiveBase) + initOpinion + 0.5f * initRelationship; // -3~[0]~3.5
                    float partnerReceiveScore = initiatorPsyche.Evaluate(RimpsycheDatabase.AssertBase) + recipientPsyche.Evaluate(RimpsycheDatabase.ReceiveBase) + reciOpinion + 0.5f * reciRelationship; // -3~[0]~3.5

                    float goodTalkChance = (3f + pawnReceiveScore + partnerReceiveScore) * (0.10f + (0.05f * topicAlignment)); // (3 ~ 10)  * (0.05 ~ 0.1) = 0.15 ~ 1 (when both score>0)
                    if (pawnReceiveScore > 0f && partnerReceiveScore > 0f && talkRand > 1f - goodTalkChance)
                    {
                        scoreBoost = 4f;
                        pawnScore = pawnReceiveScore * talkRand;// 0~3.5
                        partnerScore = partnerReceiveScore * talkRand; // 0~3.5
                        extraSentencePacks.Add(DefOfRimpsyche.Sentence_RimpsycheConversationNegativeGood);
                    }
                    else
                    {
                        //Bad Talk
                        float negativeScoreBase = (topicAlignment - 1f) * (1.5f - talkRand); // -3~[-1.5]~-0.5
                        pawnScore = negativeScoreBase * (1f - (0.2f * pawnReceiveScore)); //( -3~[-1.5]~-0.5) * 0.3~1.6 = -4.8 ~[-1.5]~ -0.15
                        partnerScore = negativeScoreBase * (1f - (0.2f * partnerReceiveScore)); //( -3~[-1.5]~-0.5) * 0.3~1.6 = -4.8 ~[-1.5]~ -0.15
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
                    if (initOpinionOffset > 0) initiatorPsyche.AffectPawn(initOpinionOffset, initOpinion, convoTopic, initDirection, scoreBoost);
                }
                if (reciOpinionOffset != 0)
                {
                    Rimpsyche_Utility.GainCoversationMemoryFast(convoTopic.name, convoTopic.label, reciOpinionOffset, recipient, initiator);
                    if (reciOpinionOffset > 0) recipientPsyche.AffectPawn(reciOpinionOffset, reciOpinion, convoTopic, -initDirection, scoreBoost);
                }

                entry = new PlayLogEntry_InteractionConversation(DefOfRimpsyche.Rimpsyche_Conversation, initiator, recipient, convoTopic.name, convoTopic.label, extraSentencePacks);
                Find.PlayLog.Add(entry);
                InteractionHook(initiator, recipient, convoTopic, topicAlignment, initOpinionOffset, reciOpinionOffset);
                convoTopic.result?.ApplyEffect(initiator, recipient, topicAlignment, initOpinionOffset, reciOpinionOffset);
            }
        }

        //Harmony hook for mod compatibility.
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void InteractionHook(Pawn initiator, Pawn recipient, Topic convoTopic, float alignment, float initOpinionOffset, float reciOpinionOffset) { }
    }
}
