using System;
using System.Collections.Generic;
using Verse;

namespace Maux36.RimPsyche
{
    public static class RimpsycheFormulaManager
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
            if (registry.ContainsKey(name))
            {
                formulaName = name+(Rand.Value).ToStringSafe();
                Log.Error($"[Rimpsyche] Duplicate keys initiated for: {name}. This is the author's oversight and needs to be reported.");
            }
            else
            {
                formulaName = name;
            }
            int new_id = registry.Count;
            registry.Add(formulaName, new_id);
            formulaId = new_id;
            calculationFunction = calculation;
        }
    }
}