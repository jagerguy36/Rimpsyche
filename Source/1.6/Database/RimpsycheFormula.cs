using System;
using System.Collections.Generic;

namespace Maux36.RimPsyche
{
    public class RimpsycheFormulaManager
    {
        public static Dictionary<string, int> FormulaIdDict = new();
    }

    public class RimpsycheFormula
    {
        public string formulaName;
        public readonly int formulaId;
        public Func<Pawn_PersonalityTracker, float> calculationFunction;
        public RimpsycheFormula(string name, Func<Pawn_PersonalityTracker, float> calculation, Dictionary<string, int> registry)
        {
            formulaName = name;
            if (registry.ContainsKey(formulaName)) formulaId = registry[formulaName];
            else
            {
                int new_id = registry.Count;
                registry.Add(formulaName, new_id);
                formulaId = new_id;
            }
            calculationFunction = calculation;
        }
    }
}