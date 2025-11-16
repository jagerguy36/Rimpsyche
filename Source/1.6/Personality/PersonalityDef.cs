using System.Collections.Generic;
using Verse;

namespace Maux36.RimPsyche
{
    public class PersonalityDef : Def
    {
        public string high;
        public string low;
        public List<FacetWeight> scoreWeight;
        public List<Scope> scopes = null;
        public float preferenceBias = 0f;
    }

    public class Scope
    {
        public string traitDefname;
        public int degree;
        public float centerOffset;
        public float range;
    }
    public struct PersonalityWeight
    {
        public string personalityDefName;
        public float weight;
    }
}
