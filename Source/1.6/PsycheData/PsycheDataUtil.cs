using System.Collections.Generic;
using Verse;

namespace Maux36.RimPsyche
{
    public static class PsycheDataUtil
    {
        public static PsycheData GetPsycheData(Pawn pawn, bool preserveMemory=false)
        {
            var compPsyche = pawn?.compPsyche();
            if (compPsyche == null) return null;

            //New PsycheData
            var psyche = new PsycheData();

            //Personality
            var personality = compPsyche.Personality;
            psyche.imagination = personality.GetFacetValueRaw(Facet.Imagination);
            psyche.intellect = personality.GetFacetValueRaw(Facet.Intellect);
            psyche.curiosity = personality.GetFacetValueRaw(Facet.Curiosity);

            psyche.industriousness = personality.GetFacetValueRaw(Facet.Industriousness);
            psyche.orderliness = personality.GetFacetValueRaw(Facet.Orderliness);
            psyche.integrity = personality.GetFacetValueRaw(Facet.Integrity);

            psyche.sociability = personality.GetFacetValueRaw(Facet.Sociability);
            psyche.assertiveness = personality.GetFacetValueRaw(Facet.Assertiveness);
            psyche.enthusiasm = personality.GetFacetValueRaw(Facet.Enthusiasm);

            psyche.compassion = personality.GetFacetValueRaw(Facet.Compassion);
            psyche.cooperation = personality.GetFacetValueRaw(Facet.Cooperation);
            psyche.humbleness = personality.GetFacetValueRaw(Facet.Humbleness);

            psyche.volatility = personality.GetFacetValueRaw(Facet.Volatility);
            psyche.pessimism = personality.GetFacetValueRaw(Facet.Pessimism);
            psyche.insecurity = personality.GetFacetValueRaw(Facet.Insecurity);

            //Interests
            var interests = compPsyche.Interests;
            psyche.interestScore = new Dictionary<string, float>(interests.interestScore);

            //Sexuality
            var sexuality = compPsyche.Sexuality;
            psyche.orientationCategory = sexuality.orientationCategory;
            psyche.mKinsey = sexuality.MKinsey;
            psyche.attraction = sexuality.Attraction;
            psyche.sexDrive = sexuality.SexDrive;
            if (preserveMemory)
            {
                psyche.knownOrientation = [.. sexuality.knownOrientation];
                psyche.relationship = new Dictionary<int, float>(sexuality.relationship);
            }
            else
            {
                psyche.knownOrientation = [];
                psyche.relationship = [];
            }
            //psyche.acquaintanceship = new Dictionary<int, float>(sexuality.acquaintanceship);
            psyche.preference = new Dictionary<string, List<PrefEntry>>(sexuality.GetPreferenceRaw());

            return psyche;
        }

        public static void InjectPsycheData(Pawn pawn, PsycheData psyche, bool preserveMemory)
        {
            if(psyche == null) return;
            var compPsyche = pawn?.compPsyche();
            if (compPsyche == null) return;
            compPsyche.InjectPsycheData(psyche, preserveMemory);
        }
    }
}
