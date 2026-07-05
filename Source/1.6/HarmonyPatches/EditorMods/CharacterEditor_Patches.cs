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
                __result = AsBase64(text, Encoding.UTF8);
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
                    string text = Base64ToString(data, Encoding.UTF8);
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


        internal static byte[] AsBytes(string text, Encoding enc)
        {
            if (text == null)
            {
                return null;
            }
            if (enc == null)
            {
                return Encoding.Default.GetBytes(text);
            }
            return enc.GetBytes(text);
        }

        internal static string AsBase64(string text, Encoding enc)
        {
            byte[] inArray = AsBytes(text, enc);
            return Convert.ToBase64String(inArray);
        }
        internal static byte[] Base64ToBytes(string base64)
        {
            byte[] result = new byte[0];
            if (!string.IsNullOrEmpty(base64))
            {
                try
                {
                    result = Convert.FromBase64String(base64);
                }
                catch
                {
                }
            }
            return result;
        }
        internal static string Base64ToString(string base64, Encoding enc)
        {
            if (base64 == null)
            {
                return null;
            }
            if (enc == null)
            {
                return Encoding.Default.GetString(Base64ToBytes(base64));
            }
            return enc.GetString(Base64ToBytes(base64));
        }
    }
}