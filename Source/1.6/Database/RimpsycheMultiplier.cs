using System;

namespace Maux36.RimPsyche
{
    public class RimpsycheMultiplier
    {
        public string multiplierName;
        public Func<Pawn_PersonalityTracker, float> calculationFunction;
        public RimpsycheMultiplier(string name, Func<Pawn_PersonalityTracker, float> calculation)
        {
            multiplierName = name;
            calculationFunction = calculation;
        }
    }

    public class PersonalityWeight
    {
        public PersonalityDef personality;
        public float weight;
    }
}