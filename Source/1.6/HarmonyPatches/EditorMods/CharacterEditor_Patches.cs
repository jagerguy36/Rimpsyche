using HarmonyLib;
using RimWorld;
using System.Reflection;
using Verse;

namespace Maux36.RimPsyche
{
    public class CharEditorPatches
    {
        [HarmonyPatch]
        public static class CE_AddTrait_Patch
        {
            public static bool Prepare()
            {
                if (ModsConfig.IsActive("void.charactereditor"))
                    return true;
                return false;
            }
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
            public static bool Prepare()
            {
                if (ModsConfig.IsActive("void.charactereditor"))
                    return true;
                return false;
            }
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