using System.Collections.Generic;
using Verse;

namespace Maux36.RimPsyche
{
    public static class PsycheCacheManager
    {
        public static readonly Dictionary<Pawn, CompPsyche> CompPsycheCache = new Dictionary<Pawn, CompPsyche>();
        public static HashSet<string> TrackingDef = new HashSet<string> { };
        public static CompPsyche GetCompPsycheCached(Pawn pawn)
        {
            if (pawn == null) return null;
            if (CompPsycheCache.TryGetValue(pawn, out CompPsyche comp))
            {
                return comp;
            }
            comp = pawn.GetComp<CompPsyche>();
            if (comp != null)
            {
                CompPsycheCache.Add(pawn, comp);
            }
            return comp;
        }

        public static void ClearCacheForPawn(Pawn pawn)
        {
            CompPsycheCache.Remove(pawn);
        }

        public static void ClearAllCache()
        {
            CompPsycheCache.Clear();
        }
    }
}
