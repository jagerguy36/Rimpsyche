using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
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
