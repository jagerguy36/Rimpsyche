using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Maux36.RimPsyche
{
    public class Topic
    {
        public string name;
        public string label;
        public float controversiality = 1;
        public bool allowChild = true;
        public bool NSFW = false;
        public List<PersonalityWeight> weights;
        public float GetScore(Pawn initiator, Pawn recipient, out float initDirection)
        {
            initDirection = 1f;
            float score = 0f;
            float initiatorAttitude = 0f;
            float recipientAttitude = 0f;
            var initiatorPsyche = initiator.compPsyche();
            var recipientPsyche = recipient.compPsyche();
            if (weights != null)
            {
                foreach (PersonalityWeight weight in weights)
                {
                    initiatorAttitude += initiatorPsyche.Personality.GetPersonality(weight.personalityDefName) * weight.weight;
                    recipientAttitude += recipientPsyche.Personality.GetPersonality(weight.personalityDefName) * weight.weight;
                }
                initiatorAttitude = Rimpsyche_Utility.Boost2(Mathf.Clamp(initiatorAttitude, -1f, 1f));
                recipientAttitude = Rimpsyche_Utility.Boost2(Mathf.Clamp(recipientAttitude, -1f, 1f));
                score = Rimpsyche_Utility.SaddleShapeFunction(initiatorAttitude, recipientAttitude, controversiality);
                //Log.Message($"initiatorAttitude: {initiatorAttitude}. recipientAttitude: {recipientAttitude} | score: {score}");
            }
            else
            {
                //Log.Error($"Null weight on topic {name}");
                initiatorAttitude = initiatorPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Tact) + (0.1f * initiator.skills.GetSkill(SkillDefOf.Social).Level);
                recipientAttitude = recipientPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Tact) + (0.1f * recipient.skills.GetSkill(SkillDefOf.Social).Level);
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

}