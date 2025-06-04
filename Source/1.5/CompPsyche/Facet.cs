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
        Imagination, // Imaginative | Realistic
        Intellect, // Philosophical | Unreflective
        Curiosity, // Explorative | Conventional

        //Conscientiousness
        Industriousness, // Persistent | Unmotivated
        Orderliness, // Organized | Disorganized
        Integrity, // Reliable | Inconsistent

        //Extraversion
        Sociability, // Friendly | Aloof
        Assertiveness, // Assertive | Timid
        Enthusiasm, // Cheerful | Stoic

        //Agreeableness
        Compassion, // Compassionate | Cold
        Cooperation, // Accommodating | Stubborn
        Humbleness, // Humble | Arrogant

        //Neuroticism
        Volatility, //Volatile | Stable
        Pessimism, //Pessimistic | Optimistic
        Insecurity // Insecure | Confident
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
