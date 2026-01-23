using HarmonyLib;
using System;
using System.Reflection;
using Verse;

namespace Maux36.RimPsyche.TIE
{
    [StaticConstructorOnStartup]
    public static class HarmonyInit
    {
        static HarmonyInit()
        {
            var harmony = new Harmony("Harmony_RimpsycheTIE");
            try
            {
                harmony.PatchAll(Assembly.GetExecutingAssembly());
                RPC_TIE_Utility.Init();
                Log.Message($"[Rimpsyche] Talking Isn't Everything patched");
            }
            catch (Exception e)
            {
                Log.Error($"[Rimpsyche] Talking Isn't Everything patch failed: {e}");
            }
        }
    }
}
