using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Maux36.RimPsyche
{
    public class Topic : IExposable
    {
        public string name;
        public TopicCategory category = TopicCategory.Socializing;
        public List<FacetWeight> weights;
        public float GetScore(Pawn initiator, Pawn recipient)
        {
            float score = 0f;
            switch (category)
            {
                case TopicCategory.Socializing:
                    var recipientPsyche = recipient.compPsyche();
                    score = recipientPsyche.Personality.SocialIntelligence + (0.1f * recipient.skills.GetSkill(SkillDefOf.Social).Level);
                    if(weights != null)
                    {
                        foreach (FacetWeight weight in weights)
                        {
                            score += recipientPsyche.Personality.GetFacetValue(weight.facet) * weight.weight;
                        }
                    }
                    break;
                case TopicCategory.Opinion:
                    break;
                case TopicCategory.Topical:
                    break;
            }
            return Mathf.Clamp(score, -1f, 1f);
        }
        public void ExposeData()
        {
            Scribe_Values.Look(ref name, "name");
            Scribe_Values.Look(ref category, "category", TopicCategory.Socializing);
            Scribe_Collections.Look(ref weights, "weights", LookMode.Deep);
        }
    }
    public enum TopicCategory : byte
    {
        Socializing,
        Opinion,
        Topical
    }

}