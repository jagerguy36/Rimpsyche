using Prepatcher;
using Verse;

namespace Maux36.RimPsyche
{
    public static class PawnExtensions
    {
        [PrepatcherField]
        [InjectComponent]
        public static CompPsyche compPsyche(this Pawn pawn)
        {
            return CompCacheManager.GetCompPsycheCached(pawn);
        }

    }
}
