using HarmonyLib;
using RimWorld;
using Verse;

namespace Maux36.RimPsyche
{
    [HarmonyPatch(typeof(TraitSet), "GainTrait")]
    public static class TraitSet_GainTrait_Patch
    {
        static void Postfix(Pawn ___pawn, Trait trait)
        {
            var compPsyche = ___pawn.compPsyche();
            if (compPsyche != null)
            {
                compPsyche.DirtyTraitCache(trait.def);
            }
        }
    }

    [HarmonyPatch(typeof(TraitSet), "RemoveTrait")]
    public static class TraitSet_RemoveTrait_Patch
    {
        static void Postfix(Pawn ___pawn, Trait trait)
        {
            var compPsyche = ___pawn.compPsyche();
            if (compPsyche != null)
            {
                compPsyche.DirtyTraitCache(trait.def);
            }
        }
    }
}
