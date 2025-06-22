using HarmonyLib;
using System.Reflection;
using Verse;

namespace Maux36.RimPsyche
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            var harmony = new Harmony("rimworld.mod.Maux.RimPsyche");
            harmony.PatchAllUncategorized(Assembly.GetExecutingAssembly());
        }
    }
}
