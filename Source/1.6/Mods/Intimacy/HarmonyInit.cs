using HarmonyLib;
using System;
using System.Reflection;
using LoveyDoveySexWithEuterpe;
using Verse;

namespace Maux36.RimPsyche.Intimacy
{
    [StaticConstructorOnStartup]
    public static class HarmonyInit
    {
        static HarmonyInit()
        {
            var harmony = new Harmony("Harmony_RimpsycheIntimacy");
            try
            {
                harmony.PatchAll(Assembly.GetExecutingAssembly());
                CommonChecks.TryAddInteraction("Rimpsyche_Smalltalk", CommonChecks.IntimateInteractions);
                Log.Message($"[Rimpsyche] Intimacy patched");
            }
            catch (Exception e)
            {
                Log.Error($"[Rimpsyche] Intimacy patch failed: {e}");
            }
        }
    }
}