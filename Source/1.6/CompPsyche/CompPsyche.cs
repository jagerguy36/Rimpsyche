using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RimWorld;
using Verse;


namespace Maux36.RimPsyche
{
    public class CompPsyche : ThingComp
    {
        //Internals
        private Pawn parentPawnInt = null;
        private Pawn_PersonalityTracker personality;
        private Pawn_InterestTracker interests;
        private Pawn_SexualityTracker sexuality;

        public int lastProgressTick = -1;

        private Pawn parentPawn
        {
            get
            {
                parentPawnInt ??= parent as Pawn;
                return parentPawnInt;
            }
        }
        public Pawn_PersonalityTracker Personality
        {
            get
            {
                if (personality == null)
                {
                    personality = new Pawn_PersonalityTracker(parentPawn);
                    personality.Initialize();
                }
                return personality;
            }
            set => personality = value;
        }
        public Pawn_InterestTracker Interests
        {
            get
            {
                if (interests == null)
                {
                    interests = new Pawn_InterestTracker(parentPawn);
                    interests.Initialize();
                }
                return interests;
            }
            set => interests = value;
        }
        public Pawn_SexualityTracker Sexuality
        {
            get
            {
                if (sexuality == null)
                {
                    sexuality = GetSexualityTracker(parentPawn);
                    sexuality.Initialize(parentPawn);
                }
                return sexuality;
            }
            set => sexuality = value;
        }

        public void PsycheValueSetup()
        {
            if (personality == null)
            {
                personality = new Pawn_PersonalityTracker(parentPawn);
                personality.Initialize();
            }
        }
        public void InterestScoreSetup()
        {
            if (interests == null)
            {
                interests = new Pawn_InterestTracker(parentPawn);
                interests.Initialize();
            }
        }
        public void SexualitySetup()
        {
            if (sexuality == null)
            {
                sexuality = GetSexualityTracker(parentPawn);
            }
            //For save-game trait safety with Sexuality Module
            sexuality.Initialize(parentPawn);
        }
        public static Pawn_SexualityTracker GetSexualityTracker(Pawn pawn)
        {
            return new Pawn_SexualityTracker(pawn);
        }
        public void DirtyTraitCache()
        {
            if(personality != null)
            {
                personality.DirtyTraitCache();
            }
        }
        public bool AffectPawn(float resultOffset, float opinion, Topic topic, float direction = 1f)
        {
            float adultHoodAge = Rimpsyche_Utility.GetMinAdultAge(parentPawn);
            float pawnTrust = parentPawn.compPsyche().personality.GetPersonality(PersonalityDefOf.Rimpsyche_Trust); //-1~1
            float pawnAge = (float)parentPawn.ageTracker.AgeBiologicalYears; //0~100
            float score = resultOffset; //0~20
            float ageFactor = 8f * adultHoodAge / (pawnAge + 0.6f * adultHoodAge) - 5f; //8.3333~-3.70036
            float scoreBase = Mathf.Max(0f, score - 5f + pawnTrust * 2f + ageFactor);
            float influenceChance = Mathf.Clamp01(scoreBase * scoreBase * (0.15f + opinion * 0.03f) / (pawnAge + 1f));
            Log.Message($"{parentPawn.Name} affect pawn entered with {resultOffset}. scorebase: {scoreBase} direction: {direction}, chance {influenceChance}");
            if (Rand.Chance(influenceChance))
            {
                influenceChance *= direction;
                if (parentPawn.DevelopmentalStage.Juvenile())
                {
                    influenceChance *= 1.5f;
                }
                Log.Message($"Affect. magnitude: {influenceChance}");

                float totalWeight = topic.weights.Sum(w => Mathf.Abs(w.weight));
                if (totalWeight == 0f)
                    return false;

                var facetChanges = new Dictionary<Facet, float>();
                foreach (var weight in topic.weights)
                {
                    float contribution = influenceChance * (weight.weight / totalWeight);
                    if (contribution != 0f)
                        facetChanges[weight.facet] = contribution;
                }
                Personality.AffectFacetValue(facetChanges);

                return true;
            }
            return false;
        }
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Deep.Look(ref personality, "personality", new object[] { parent as Pawn });
            Scribe_Deep.Look(ref interests, "interests", new object[] { parent as Pawn });
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                PsycheValueSetup();
                InterestScoreSetup();
                SexualitySetup();
            }
        }

    }


}
