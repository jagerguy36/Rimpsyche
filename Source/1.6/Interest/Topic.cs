using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Maux36.RimPsyche
{
    public class Topic
    {
        [NoTranslate]
        public string name;
        public int id = -1;
        public string label;
        public float controversiality = 1;
        public bool allowChild = true;
        public bool NSFW = false;
        public List<PersonalityWeight> weights;
        public ConvoResultBase result;
        public float GetScore(CompPsyche initPsyche, CompPsyche reciPsyche, out float initDirection)
        {
            initDirection = 1f;
            float score = 0f;
            float initiatorAttitude = 0f;
            float recipientAttitude = 0f;
            if (weights != null)
            {
                foreach (PersonalityWeight weight in weights)
                {
                    initiatorAttitude += initPsyche.Personality.GetPersonality(weight.personalityDefName) * weight.weight;
                    recipientAttitude += reciPsyche.Personality.GetPersonality(weight.personalityDefName) * weight.weight;
                }
                initiatorAttitude = Rimpsyche_Utility.Boost2(Mathf.Clamp(initiatorAttitude, -1f, 1f));
                recipientAttitude = Rimpsyche_Utility.Boost2(Mathf.Clamp(recipientAttitude, -1f, 1f));
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
    }

    public abstract class ConvoResultBase
    {
        public abstract void ApplyEffect(Pawn initiator, Pawn recipient, float alignment, float initOpinionOffset, float reciOpinionOffset);
    }

}