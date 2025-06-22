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
            { Facet.Imagination, Tuple.Create("Realistic", "Imaginative", Color.magenta, Color.cyan) },
            { Facet.Intellect, Tuple.Create("Practical", "Philosophical", Color.magenta, Color.cyan) },
            { Facet.Curiosity, Tuple.Create("Conventional", "Explorative", Color.magenta, Color.cyan) },

            // Conscientiousness
            { Facet.Industriousness, Tuple.Create("Unmotivated", "Industrious", Color.gray, Color.yellow) },
            { Facet.Orderliness, Tuple.Create("Disorganized", "Organized", Color.gray, Color.yellow) },
            { Facet.Integrity, Tuple.Create("Inconsistent", "Reliable", Color.gray, Color.yellow) },

            // Extraversion
            { Facet.Sociability, Tuple.Create("Aloof", "Friendly", Color.magenta, Color.green) },
            { Facet.Assertiveness, Tuple.Create("Timid", "Assertive", Color.magenta, Color.green) },
            { Facet.Enthusiasm, Tuple.Create("Stoic", "Cheerful", Color.magenta, Color.green) },

            // Agreeableness
            { Facet.Compassion, Tuple.Create("Detached", "Compassionate", Color.blue, Color.green) },
            { Facet.Cooperation, Tuple.Create("Stubborn", "Accommodating", Color.blue, Color.green) },
            { Facet.Humbleness, Tuple.Create("Arrogant", "Humble", Color.blue, Color.green) },

            // Neuroticism
            { Facet.Volatility, Tuple.Create("Stable", "Volatile", Color.green, Color.red) },
            { Facet.Pessimism, Tuple.Create("Optimistic", "Pessimistic", Color.green, Color.red) },
            { Facet.Insecurity, Tuple.Create("Confident", "Insecure", Color.green, Color.red) }
        };

        public static Dictionary<Facet, string> FacetDescription = new()
        {
            // Openness
            { Facet.Imagination, "Reflects a vivid inner world and emotional sensitivity to art and beauty. Imaginative pawns are drawn to daydreams, music, and Realistic pawns are more grounded and practical." },
            { Facet.Intellect, "Captures interest in abstract thinking and complex ideas. Philosophical pawns enjoy deep conversations and philosophical questions; Practical pawns prefer concrete, straightforward thinking." },
            { Facet.Curiosity, "Describes a drive for novelty, diversity, and exploration. Explorative pawns seek new experiences and challenge norms; Conventional pawns favor routine and familiar settings." },

            // Conscientiousness
            { Facet.Industriousness, "Tooltip1" },
            { Facet.Orderliness, "Tooltip1" },
            { Facet.Integrity, "Tooltip1" },

            // Extraversion
            { Facet.Sociability, "Tooltip1" },
            { Facet.Assertiveness, "Tooltip1" },
            { Facet.Enthusiasm, "Tooltip1" },

            // Agreeableness
            { Facet.Compassion, "Tooltip1" },
            { Facet.Cooperation, "Tooltip1" },
            { Facet.Humbleness, "Tooltip1" },

            // Neuroticism
            { Facet.Volatility, "Tooltip1" },
            { Facet.Pessimism, "Tooltip1" },
            { Facet.Insecurity, "Tooltip1" }
        };
    }
}
