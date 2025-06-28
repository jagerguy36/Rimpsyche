using RimWorld;
using Verse;

namespace Maux36.RimPsyche
{
    [DefOf] 
    public static class PersonalityDefOf
    {
        public static PersonalityDef Rimpsyche_Talkativeness;
        public static PersonalityDef Rimpsyche_Sociability;
        public static PersonalityDef Rimpsyche_Tact;
        public static PersonalityDef Rimpsyche_Openness;
        public static PersonalityDef Rimpsyche_Confidence;
        public static PersonalityDef Rimpsyche_Inquisitiveness;
        public static PersonalityDef Rimpsyche_Prudishness;
        public static PersonalityDef Rimpsyche_Aggressiveness;

        public static PersonalityDef Rimpsyche_Emotionality;
        public static PersonalityDef Rimpsyche_Compassion;


        public static PersonalityDef Rimpsyche_Passion;
        public static PersonalityDef Rimpsyche_Optimism;
        public static PersonalityDef Rimpsyche_Spontaneity;
        public static PersonalityDef Rimpsyche_Trust;

        static PersonalityDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(PersonalityDefOf));
        }

    }
}
