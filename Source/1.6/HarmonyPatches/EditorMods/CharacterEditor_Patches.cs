using HarmonyLib;
using RimWorld;
using System;
using System.Reflection;
using System.Text;
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
                if (Rimpsyche_Utility.IsModActive("void.charactereditor"))
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
                if (Rimpsyche_Utility.IsModActive("void.charactereditor"))
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

        [HarmonyPatch]
        public static class CE_GetPsyche_Patch
        {
            public static bool Prepare()
            {
                if (Rimpsyche_Utility.IsModActive("void.charactereditor"))
                    return true;
                return false;
            }
            static MethodBase TargetMethod()
            {
                var type = AccessTools.TypeByName("CharacterEditor.CompatibilityTool");
                return AccessTools.Method(type, "GetPsyche");
            }

            public static bool Prefix(ref string __result, Pawn pawn)
            {
                string text = "";
                var compPsyche = pawn?.compPsyche();
                if (compPsyche != null)
                {
                    text = PsycheDataUtil.GetSerializedStringPsycheData(pawn);
                }
                if (Prefs.DevMode)
                {
                    Log.Message("getting psyche of " + pawn.LabelShort + " =" + text);
                }
                __result = ToBase64(text);
                return false;
            }
        }

        [HarmonyPatch]
        public static class CE_SetPsyche_Patch
        {
            public static bool Prepare()
            {
                if (Rimpsyche_Utility.IsModActive("void.charactereditor"))
                    return true;
                return false;
            }
            static MethodBase TargetMethod()
            {
                var type = AccessTools.TypeByName("CharacterEditor.CompatibilityTool");
                return AccessTools.Method(type, "SetPsyche");
            }

            public static bool Prefix(Pawn pawn, string data)
            {
                var compPsyche = pawn?.compPsyche();
                if (compPsyche == null)
                    return false;
                try
                {
                    string text = FromBase64(data);
                    if (Prefs.DevMode)
                    {
                        Log.Message("getting psyche of " + pawn.LabelShort + " =" + text);
                    }
                    PsycheDataUtil.InjectSerializedStringPsycheData(pawn, text, false, false);
                }
                catch (Exception e)
                {
                    if (Prefs.DevMode)
                    {
                        Log.Message(e.Message + "\n" + e.StackTrace);
                    }
                }
                return false;
            }
        }

        internal static string ToBase64(string text)
        {
            if (text is null)
                return null;
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(text));
        }

        internal static string FromBase64(string base64)
        {
            if (base64 is null)
                return null;

            return Encoding.UTF8.GetString(Convert.FromBase64String(base64));
        }
    }
}