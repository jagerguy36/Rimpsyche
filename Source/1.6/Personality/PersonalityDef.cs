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
    }

    public class Scope
    {
        public string traitDefname;
        public int degree;
        public float ceterOffset;
        public float range;
    }
}
