using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Maux36.RimPsyche
{
    [Flags]
    public enum Demographic : byte
    {
        None = 0,  // 0
        Adult = 1 << 0,  // 1 
        Child = 1 << 1,  // 2
    }

    public class Topic
    {
        [NoTranslate]
        public string name;
        public int id = -1;
        public string label;
        public float controversiality = 1;
        public Demographic disallowedInit = Demographic.None;
        public Demographic disallowedReci = Demographic.None;
        public bool NSFW = false;
        public List<PersonalityWeight> weights;
        public ConvoResultBase result;
        public float GetScore(CompPsyche initPsyche, CompPsyche reciPsyche, out float initDirection)
        {
            initDirection = 1f;
            float score;
            float initiatorAttitude;
            float recipientAttitude;
            if (weights != null)
            {
                int indicator = 0;
                if (initPsyche.TopicOpinionCache.TryGetValue(id, out initiatorAttitude))
                    indicator += 2;
                if (reciPsyche.TopicOpinionCache.TryGetValue(id, out recipientAttitude))
                    indicator += 1;
                switch (indicator) //0: need both | 1: need init | 2: need reci | 3: need none
                {
                    case 0: // need both
                        foreach (PersonalityWeight weight in weights)
                        {
                            initiatorAttitude += initPsyche.Personality.GetPersonality(weight.personalityDefName) * weight.weight;
                            recipientAttitude += reciPsyche.Personality.GetPersonality(weight.personalityDefName) * weight.weight;
                        }
                        //Log.Message($"Case 0 [{name}] {initPsyche.parentPawn.Name} ![{initiatorAttitude}], {reciPsyche.parentPawn.Name} ![{recipientAttitude}]");
                        initiatorAttitude = Rimpsyche_Utility.Boost2(Mathf.Clamp(initiatorAttitude, -1f, 1f));
                        recipientAttitude = Rimpsyche_Utility.Boost2(Mathf.Clamp(recipientAttitude, -1f, 1f));
                        initPsyche.TopicOpinionCache[id] = initiatorAttitude;
                        reciPsyche.TopicOpinionCache[id] = recipientAttitude;
                        break;
                    case 1: // need init
                        foreach (PersonalityWeight weight in weights)
                        {
                            initiatorAttitude += initPsyche.Personality.GetPersonality(weight.personalityDefName) * weight.weight;
                        }
                        //Log.Message($"Case 1 [{name}] {initPsyche.parentPawn.Name} ![{initiatorAttitude}] | {reciPsyche.parentPawn.Name}({recipientAttitude})");
                        initiatorAttitude = Rimpsyche_Utility.Boost2(Mathf.Clamp(initiatorAttitude, -1f, 1f));
                        initPsyche.TopicOpinionCache[id] = initiatorAttitude;
                        break;
                    case 2: // need reci
                        foreach (PersonalityWeight weight in weights)
                        {
                            recipientAttitude += reciPsyche.Personality.GetPersonality(weight.personalityDefName) * weight.weight;
                        }
                        //Log.Message($"Case 2 [{name}] {initPsyche.parentPawn.Name}({initiatorAttitude}) | {reciPsyche.parentPawn.Name} ![{recipientAttitude}]");
                        recipientAttitude = Rimpsyche_Utility.Boost2(Mathf.Clamp(recipientAttitude, -1f, 1f));
                        reciPsyche.TopicOpinionCache[id] = recipientAttitude;
                        break;
                    case 3: // need none
                        //Log.Message($"Case 3 [{name}] {initPsyche.parentPawn.Name}({initiatorAttitude}) | {reciPsyche.parentPawn.Name}({recipientAttitude})");
                        break;
                    default:
                        throw new InvalidOperationException("Unexpected outcome.");
                }
                score = Rimpsyche_Utility.SaddleShapeFunction(initiatorAttitude, recipientAttitude, controversiality);
                //Log.Message($"initiatorAttitude: {initiatorAttitude}. recipientAttitude: {recipientAttitude} | score: {score}");
            }
            else
            {
                Log.Error($"[Rimpsyche] Null weight value on topic {name}. Using default attitude to prevent critical failure.");
                initiatorAttitude = initPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Tact) + (0.1f * initPsyche.parentPawn.skills.GetSkill(SkillDefOf.Social).Level);
                recipientAttitude = reciPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Tact) + (0.1f * reciPsyche.parentPawn.skills.GetSkill(SkillDefOf.Social).Level);
                initiatorAttitude = Mathf.Clamp(initiatorAttitude, -1f, 1f);
                recipientAttitude = Mathf.Clamp(recipientAttitude, -1f, 1f);
                score = (initiatorAttitude + recipientAttitude) * 0.5f;
            }
            if(recipientAttitude < initiatorAttitude)
            {
                initDirection = -1f;
            }
            else if (recipientAttitude == initiatorAttitude)
            {
                initDirection = 0f;
            }
            return Mathf.Clamp(score, -1f, 1f);
        }

        public ParticipantMask GetValidParticipants()
        {
            if (isNSFW) return ParticipantMask.AA;
            int key = ((int)disallowedInit << 2) | (int)disallowedReci;
            return key switch
            {
                0b_0101 => ParticipantMask.CC, // (Adult, Adult) => 01 01 = 5
                0b_0110 => ParticipantMask.CA, // (Adult, Child) => 01 10 = 6
                0b_0100 => ParticipantMask.CA | ParticipantMask.CC, // (Adult, None)  => 01 00 = 4
                0b_1001 => ParticipantMask.AC, // (Child, Adult) => 10 01 = 9
                0b_1010 => ParticipantMask.AA | ParticipantMask.AAs, // (Child, Child) => 10 10 = 10
                0b_1000 => ParticipantMask.AA | ParticipantMask.AAs | ParticipantMask.AC, // (Child, None)  => 10 00 = 8
                0b_0001 => ParticipantMask.AC | ParticipantMask.CC, // (None, Adult)  => 00 01 = 1
                0b_0010 => ParticipantMask.AA | ParticipantMask.AAs | ParticipantMask.CA, // (None, Child)  => 00 10 = 2
                _ => ParticipantMask.All // (None, None)   => 00 00 = 0
            };
        }
    }

    public abstract class ConvoResultBase
    {
        public abstract void ApplyEffect(Pawn initiator, Pawn recipient, float alignment, float initOpinionOffset, float reciOpinionOffset);
    }

}