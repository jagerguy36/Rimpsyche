using RimWorld;
using Verse;

namespace Maux36.RimPsyche
{
    [DefOf]
    public static class PersonalityDefOf
    {
        public static PersonalityDef Rimpsyche_Sociability;
        public static PersonalityDef Rimpsyche_Talkativeness;
        public static PersonalityDef Rimpsyche_SocialIntelligence;
        public static PersonalityDef Rimpsyche_Openness;

        static PersonalityDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(PersonalityDefOf));
        }

    }
}
