using HarmonyLib;
using System.Collections.Generic;
using Verse;

namespace Maux36.RimPsyche.VanillaTraitExpanded
{
    [StaticConstructorOnStartup]
    public class VET_TraitDatabaseProvider
    {
        static VET_TraitDatabaseProvider()
        {
            Log.Message("[Rimpsyche] VTE gate data added");
            RimpsycheDatabase.TraitGateDatabase[new Pair<string, int>("VTE_Eccentric", 0)] = new List<(Facet, float, float)>
            {
                (Facet.Intellect, 25f, 25f)
            };
            RimpsycheDatabase.TraitGateDatabase[new Pair<string, int>("VTE_Submissive", 0)] = new List<(Facet, float, float)>
            {
                (Facet.Assertiveness, -25f, 25f)
            };
            RimpsycheDatabase.TraitGateDatabase[new Pair<string, int>("VTE_Dunce", 0)] = new List<(Facet, float, float)>
            {
                (Facet.Intellect, -25f, 25f)
            };
            RimpsycheDatabase.TraitGateDatabase[new Pair<string, int>("VTE_Snob", 0)] = new List<(Facet, float, float)>
            {
                (Facet.Humbleness, -25f, 25f)
            };
            RimpsycheDatabase.TraitGateDatabase[new Pair<string, int>("VTE_Anxious", 0)] = new List<(Facet, float, float)>
            {
                (Facet.Insecurity, 25f, 25f)
            };
            RimpsycheDatabase.TraitGateDatabase[new Pair<string, int>("VTE_Prodigy", 0)] = new List<(Facet, float, float)>
            {
                (Facet.Intellect, 25f, 25f)
            };
            RimpsycheDatabase.TraitGateDatabase[new Pair<string, int>("VTE_MadSurgeon", 0)] = new List<(Facet, float, float)>
            {
                (Facet.Compassion, -45f, 5f)
            };
            RimpsycheDatabase.TraitGateDatabase[new Pair<string, int>("VTE_WorldWeary", 0)] = new List<(Facet, float, float)>
            {
                (Facet.Pessimism, 25f, 25f)
            };
            RimpsycheDatabase.TraitGateDatabase[new Pair<string, int>("VTE_Academian", 0)] = new List<(Facet, float, float)>
            {
                (Facet.Intellect, 25f, 25f)
            };
        }
    }
}
