using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using Verse;

namespace Maux36.RimPsyche
{
    public static class PsycheCacheManager
    {
        public static readonly Dictionary<Pawn, CompPsyche> CompPsycheCache = new Dictionary<Pawn, CompPsyche>();
        public static HashSet<int> TrackingDefHash = new HashSet<int> { };
        public static CompPsyche GetCompPsycheCached(Pawn pawn)
        {
            if (pawn == null) return null;
            if (CompPsycheCache.TryGetValue(pawn, out CompPsyche comp))
            {
                return comp;
            }
            if (!TrackingDefHash.Contains(pawn.def.shortHash))
            {
                return null;
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

    public class RimPsycheWorldComp : WorldComponent
    {
        public static PsycheData tempData = null;
        public RimPsycheWorldComp(World world) : base(world)
        {
        }

        public override void FinalizeInit(bool fromload)
        {
            base.FinalizeInit(fromload);
            try
            {
                PsycheCacheManager.ClearAllCache(); //clear any pawns from a previous world 
            }
            catch (Exception)
            {
                Log.Error($"[Rimpsyche] failed to clear caches on world initialization.");
            }
            try
            {
                //Make sure hijacking is still in place
                InteractionDefOf.Chitchat = DefOfRimpsyche.Rimpsyche_Smalltalk;
                InteractionDefOf.DeepTalk = DefOfRimpsyche.Rimpsyche_StartConversation;
            }
            catch (Exception)
            {
                Log.Error($"[Rimpsyche] failed to hijack chitchat and deeptalk defs.");
            }
        }
    }
}
