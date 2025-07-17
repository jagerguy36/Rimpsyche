using System;

namespace Maux36.RimPsyche
{
    public class RimpsycheFormula
    {
        public string formulaName;
        public Func<Pawn_PersonalityTracker, float> calculationFunction;
        public RimpsycheFormula(string name, Func<Pawn_PersonalityTracker, float> calculation)
        {
            formulaName = name;
            calculationFunction = calculation;
        }
    }
}