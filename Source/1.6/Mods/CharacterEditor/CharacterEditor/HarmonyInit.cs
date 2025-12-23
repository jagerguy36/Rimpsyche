using HarmonyLib;
using System;
using System.Reflection;
using Verse;

namespace Maux36.RimPsyche.CharacterEditor
{
    [StaticConstructorOnStartup]
    public static class HarmonyInit
    {
        static HarmonyInit()
        {
            var harmony = new Harmony("Harmony_RimpsycheCE");
            try
            {
                harmony.PatchAll(Assembly.GetExecutingAssembly());
                Log.Message($"[Rimpsyche] Character Editor patched");
            }
            catch (Exception e)
            {
                Log.Error($"[Rimpsyche] Character Editor patch failed: {e}");
            }
        }
    }
}