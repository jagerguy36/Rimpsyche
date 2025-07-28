using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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
        public float roomRoleFactor = 1f;
        public int organizedMood = -1;

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
                    sexuality = new Pawn_SexualityTracker(parentPawn);
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
                sexuality = new Pawn_SexualityTracker(parentPawn);
            }
            //Initialize even when not null for save-game trait safety with Sexuality Module.
            sexuality.Initialize(parentPawn);
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
            float ageFactor = 8f * adultHoodAge / (pawnAge + 0.6f * adultHoodAge) - 5f; //8.3333 ~ -5
            float scoreBase = Mathf.Max(0f, score - 5f + pawnTrust * 2f + ageFactor);
            float influenceChance = Mathf.Clamp01(scoreBase * scoreBase * (0.15f + opinion * 0.05f) / (pawnAge + 1f));
            if (Rand.Chance(influenceChance))
            {
                influenceChance *= direction;
                if (parentPawn.DevelopmentalStage.Juvenile())
                {
                    influenceChance *= 1.5f;
                }
                //Log.Message($"Affect. magnitude: {influenceChance}");

                float totalWeight = topic.weights.Sum(w => Mathf.Abs(w.weight));
                if (totalWeight == 0f)
                    return false;

                var facetChanges = new Dictionary<Facet, float>();
                foreach (var personalityWeight in topic.weights)
                {
                    float contribution = influenceChance * (personalityWeight.weight / totalWeight);
                    if (contribution != 0f)
                    {
                        var personality = RimpsycheDatabase.PersonalityDict[personalityWeight.personalityDefName];
                        foreach (var weight in personality.scoreWeight)
                        {
                            if (facetChanges.ContainsKey(weight.facet))
                            {
                                facetChanges[weight.facet] += weight.weight * contribution;
                            }
                            else
                            {
                                facetChanges[weight.facet] = weight.weight * contribution;
                            }
                        }
                    }
                }
                Personality.AffectFacetValue(facetChanges);

                return true;
            }
            return false;
        }
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref lastProgressTick, "lastProgressTick", -1);
            Scribe_Values.Look(ref roomRoleFactor, "roomRoleFactor", 1f);
            Scribe_Values.Look(ref organizedMood, "organizedMood", -1);

            Scribe_Deep.Look(ref personality, "personality", new object[] { parent as Pawn });
            Scribe_Deep.Look(ref interests, "interests", new object[] { parent as Pawn });
            Scribe_Deep.Look(ref sexuality, "sexuality", new object[] { parent as Pawn });
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                PsycheValueSetup();
                InterestScoreSetup();
                SexualitySetup();
            }
        }

    }


}
