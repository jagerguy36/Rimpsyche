using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maux36.RimPsyche
{
    public enum Facet
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
        Impulsivity,
        Insecurity
    }

    public class FacetWeight
    {
        Facet facet;
        float weight;
    }
}
