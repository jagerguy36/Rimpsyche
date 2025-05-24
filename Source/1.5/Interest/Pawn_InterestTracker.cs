using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Maux36.RimPsyche
{
    public class Pawn_InterestTracker : IExposable
    {
        private Pawn pawn;
        public Dictionary<string, float> interestScore = new Dictionary<string, float>();

        public Pawn_InterestTracker(Pawn p)
        {
            pawn = p;
        }
        public void Initialize(int inputSeed = 0)
        {
            foreach (InterestDomainDef interestdomainDef in DefDatabase<InterestDomainDef>.AllDefs)
            {
                GenerateInterestScoresForDomain(interestdomainDef);
            }
        }

        public void GenerateInterestScoresForDomain(InterestDomainDef interestdomainDef)
        {
            //Log.Message($"Generating interest for {interestdomainDef.label}");
            float baseValue = 50;
            if (interestdomainDef.scoreWeight != null)
            {
                baseValue = 50; //get base value from facets
            }
            foreach (var interest in interestdomainDef.interests)
            {

                float result;
                int attempts = 0;
                do
                {
                    result = Rand.Gaussian(baseValue, 5f); // center at basevalue, 3widthfactor == 15
                    attempts++;
                }
                while ((result < 0f || result > 100f) && attempts < 2);
                if (result < 0f || result > 100f)
                {
                    result = Mathf.Clamp(result, 0f, 100f);
                    //Log.Warning($"GenerateInterestScores failed to get valid value in {2} attempts. Clamped to {result}.");
                }
                interestScore[interest.name] = result;
            }
        }

        public float GetOrCreateInterestScore(Interest key)
        {
            if (!interestScore.TryGetValue(key.name, out float value))
            {
                GenerateInterestScoresForDomain(RimpsycheDatabase.InterestDomainDict[key]);
                if (!interestScore.TryGetValue(key.name, out value))
                {
                    value = 50;
                }
            }
            return value;
        }
        public Interest ChoseInterest()
        {
            return GenCollection.RandomElementByWeight(RimpsycheDatabase.InterestList, GetOrCreateInterestScore);
        }
        public Topic GetConvoTopic()
        {
            Interest chosenInterest = GenCollection.RandomElementByWeight(RimpsycheDatabase.InterestList, GetOrCreateInterestScore);
            Topic chosenTopic = chosenInterest.GetRandomTopic();
            return chosenTopic;
        }

        // Save
        public void ExposeData()
        {
            Scribe_Collections.Look(ref interestScore, "interestScore", LookMode.Value, LookMode.Value);
        }
    }
}
