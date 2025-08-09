using HarmonyLib;
using System;
using System.Reflection;
using Verse;

namespace Maux36.RimPsyche.VREAndroid
{
    [StaticConstructorOnStartup]
    public static class HarmonyInit
    {
        static HarmonyInit()
        {
            var harmony = new Harmony("Harmony_RimpsycheVREAndroid");
            try
            {
                harmony.PatchAll(Assembly.GetExecutingAssembly());
                Log.Message($"[Rimpsyche] VREAndroid patched");
            }
            catch (Exception e)
            {
                Log.Error($"[Rimpsyche] VREAndroid patch failed: {e}");
            }
        }
    }
}
