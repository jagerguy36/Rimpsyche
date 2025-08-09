using System.Collections.Generic;
using Verse;

namespace Maux36.RimPsyche.VREAndroid
{
    [StaticConstructorOnStartup]
    public class VRE_Android_TraitDatabaseProvider
    {
        static VRE_Android_TraitDatabaseProvider()
        {
            Log.Message("[Rimpsyche] VRE Android gate data added");
            RimpsycheDatabase.GeneGateDatabase["VREA_PsychologyDisabled"] = new List<FacetGate>
            {
                new FacetGate(Facet.Imagination, 0f, 0f, 10),
                new FacetGate(Facet.Intellect, 0f, 0f, 10),
                new FacetGate(Facet.Curiosity, 0f, 0f, 10),
                new FacetGate(Facet.Industriousness, 20f, 0f, 10),
                new FacetGate(Facet.Orderliness, 0f, 0f, 10),
                new FacetGate(Facet.Integrity, 20f, 0f, 10),
                new FacetGate(Facet.Sociability, 20f, 0f, 10),
                new FacetGate(Facet.Assertiveness, 0f, 0f, 10),
                new FacetGate(Facet.Enthusiasm, 0f, 0f, 10),
                new FacetGate(Facet.Compassion, 30f, 0f, 10),
                new FacetGate(Facet.Cooperation, 30f, 0f, 10),
                new FacetGate(Facet.Humbleness, 30f, 0f, 10),
                new FacetGate(Facet.Volatility, -30f, 0f, 10),
                new FacetGate(Facet.Pessimism, -30f, 0f, 10),
                new FacetGate(Facet.Insecurity, 0f, 0f, 10),
            };
        }
    }
}
