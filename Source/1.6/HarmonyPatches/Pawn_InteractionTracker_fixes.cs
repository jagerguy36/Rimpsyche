using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Maux36.RimPsyche
{
    [HarmonyPatch(typeof(Pawn_InteractionsTracker), "SocialFightChance")]
    public static class Pawn_InteractionTracker_SocialFightChance
    {
        public static void Postfix(Pawn_InteractionsTracker __instance, ref float __result, Pawn ___pawn)
        {
            if (__result > 0f)
            {
                var compPsyche = ___pawn.compPsyche();
                if (compPsyche != null)
                {
                    __result = Mathf.Clamp01(__result * compPsyche.Personality.Evaluate(RimpsycheDatabase.SocialFightChanceMultiplier));
                }
            }
        }
    }
}
