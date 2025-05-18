using RimWorld;
using Prepatcher;
using Verse;

namespace Maux36.RimPsyche
{
    public static class PawnExtensions
    {
        [PrepatcherField]
        [InjectComponent]
        public static extern CompPsyche compPsyche(this Pawn pawn);

    }
}
