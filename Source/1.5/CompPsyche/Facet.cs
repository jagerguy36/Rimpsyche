using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Verse;

namespace Maux36.RimPsyche
{
    public enum Facet : byte
    {
        //Openness
        Imagination,
        Intellect,
        Curiosity,

        //Conscientiousness
        Ambition,
        Order,
        Integrity,

        //Extraversion
        Sociability,
        Assertiveness,
        Enthusiasm,

        //Agreeableness
        Compassion,
        Cooperation,
        Humility,

        //Neuroticism
        Volatility,
        Pessimism,
        Insecurity
    }

    public class FacetWeight : IExposable
    {
        public Facet facet;
        public float weight;
        public void ExposeData()
        {
            Scribe_Values.Look(ref facet, "facet");
            Scribe_Values.Look(ref weight, "weight");
        }
    }
}
