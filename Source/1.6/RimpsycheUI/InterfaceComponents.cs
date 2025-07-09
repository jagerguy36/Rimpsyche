using System;
using System.Collections.Generic;
using UnityEngine;

namespace Maux36.RimPsyche
{
    public class InterfaceComponents
    {
        public static Dictionary<Facet, Tuple<string, string, Color, Color>> FacetNotation = new()
        {
            // Openness
            { Facet.Imagination, Tuple.Create("RPC_Realistic".Translate(), "RPC_Imaginative".Translate(), Color.magenta, Color.cyan) },
            { Facet.Intellect, Tuple.Create("RPC_Practical".Translate(), "RPC_Philosophical".Translate(), Color.magenta, Color.cyan) },
            { Facet.Curiosity, Tuple.Create("RPC_Conventional".Translate(), "RPC_Explorative".Translate(), Color.magenta, Color.cyan) },

            // Conscientiousness
            { Facet.Industriousness, Tuple.Create("RPC_Unmotivated".Translate(), "RPC_Industrious".Translate(), Color.gray, Color.yellow) },
            { Facet.Orderliness, Tuple.Create("RPC_Disorganized".Translate(), "RPC_Organized".Translate(), Color.gray, Color.yellow) },
            { Facet.Integrity, Tuple.Create("RPC_Inconsistent".Translate(), "RPC_Reliable".Translate(), Color.gray, Color.yellow) },

            // Extraversion
            { Facet.Sociability, Tuple.Create("RPC_Aloof".Translate(), "RPC_Friendly".Translate(), Color.magenta, Color.green) },
            { Facet.Assertiveness, Tuple.Create("RPC_Timid".Translate(), "RPC_Assertive".Translate(), Color.magenta, Color.green) },
            { Facet.Enthusiasm, Tuple.Create("RPC_Stoic".Translate(), "RPC_Cheerful".Translate(), Color.magenta, Color.green) },

            // Agreeableness
            { Facet.Compassion, Tuple.Create("RPC_Detached".Translate(), "RPC_Compassionate".Translate(), Color.blue, Color.green) },
            { Facet.Cooperation, Tuple.Create("RPC_Stubborn".Translate(), "RPC_Accommodating".Translate(), Color.blue, Color.green) },
            { Facet.Humbleness, Tuple.Create("RPC_Arrogant".Translate(), "RPC_Humble".Translate(), Color.blue, Color.green) },

            // Neuroticism
            { Facet.Volatility, Tuple.Create("RPC_Stable".Translate(), "RPC_Volatile".Translate(), Color.green, Color.red) },
            { Facet.Pessimism, Tuple.Create("RPC_Optimistic".Translate(), "RPC_Pessimistic".Translate(), Color.green, Color.red) },
            { Facet.Insecurity, Tuple.Create("RPC_Confident".Translate(), "RPC_Insecure".Translate(), Color.green, Color.red) }
        };

        public static Dictionary<Facet, string> FacetDescription = new()
        {
            // Openness
            { Facet.Imagination, "RPC_Imagination_Tooltip".Translate() },
            { Facet.Intellect, "RPC_Intellect_Tooltip".Translate()},
            { Facet.Curiosity, "RPC_Curiosity_Tooltip".Translate() },

            // Conscientiousness
            { Facet.Industriousness, "RPC_Industriousness_Tooltip".Translate() },
            { Facet.Orderliness, "RPC_Orderliness_Tooltip".Translate() },
            { Facet.Integrity, "RPC_Integrity_Tooltip".Translate() },

            // Extraversion
            { Facet.Sociability, "RPC_Sociability_Tooltip".Translate() },
            { Facet.Assertiveness, "RPC_Assertiveness_Tooltip".Translate() },
            { Facet.Enthusiasm, "RPC_Enthusiasm_Tooltip".Translate() },

            // Agreeableness
            { Facet.Compassion, "RPC_Compassion_Tooltip".Translate() },
            { Facet.Cooperation, "RPC_Cooperation_Tooltip".Translate() },
            { Facet.Humbleness, "RPC_Humbleness_Tooltip".Translate() },

            // Neuroticism
            { Facet.Volatility, "RPC_Volatility_Tooltip".Translate() },
            { Facet.Pessimism, "RPC_Pessimism_Tooltip".Translate()},
            { Facet.Insecurity, "RPC_Insecurity_Tooltip".Translate() }
        };
    }
}
