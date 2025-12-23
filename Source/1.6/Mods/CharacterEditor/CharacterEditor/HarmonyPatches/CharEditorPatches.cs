using RimWorld;
using Verse;
using System;
using HarmonyLib;
using System.Reflection;

namespace Maux36.RimPsyche.CharacterEditor
{
    public class CharEditorPatches
    {
        [HarmonyPatch]
        public static class CE_AddTrait_Patch
        {
            static MethodBase TargetMethod()
            {
                var type = AccessTools.TypeByName("CharacterEditor.TraitTool");
                return AccessTools.Method(type, "AddTrait");
            }
            public static void Postfix(Pawn pawn, TraitDef traitDef)
            {
                var compPsyche = pawn.compPsyche();
                compPsyche?.DirtyTraitCache(traitDef);
            }
        }


        [HarmonyPatch]
        public static class CE_RemoveTrait_Patch
        {
            static MethodBase TargetMethod()
            {
                var type = AccessTools.TypeByName("CharacterEditor.TraitTool");
                return AccessTools.Method(type, "RemoveTrait");
            }

            public static void Postfix(Pawn pawn, Trait t)
            {
                var compPsyche = pawn.compPsyche();
                compPsyche?.DirtyTraitCache(t.def);
            }
        }

    }
}