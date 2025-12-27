using RimWorld;
using System;
using UnityEngine;
using Verse;


namespace Maux36.RimPsyche
{
    public class Rimpsyche : Mod
    {
        public static RimpsycheSettings settings;
        public const string requiredSexualityVersion_string = "1.0.4";
        public static string currentVersion;
        public static bool DispositionModuleLoaded = false;
        public static bool SexualityModuleLoaded = false;
        public static bool RelationshipModuleLoaded = false;
        public Rimpsyche(ModContentPack content) : base(content)
        {
            currentVersion = content.ModMetaData.ModVersion;
            Log.Message($"[Rimpsyche] Personality Core running with version {currentVersion}");
            settings = GetSettings<RimpsycheSettings>();

            if (ModsConfig.IsActive("maux36.rimpsyche.disposition"))
            {
                DispositionModuleLoaded = true;
                Log.Message($"[Rimpsyche] Disposition Active");
            }

            if (ModsConfig.IsActive("maux36.rimpsyche.sexuality"))
            {
                SexualityModuleLoaded = true;
                Log.Message($"[Rimpsyche] Sexuality Active");

                var sexualityVersion_string = ModLister.GetModWithIdentifier("maux36.rimpsyche.sexuality").ModVersion;
                if (new Version(sexualityVersion_string) < new Version(requiredSexualityVersion_string))
                {
                    Log.Error($"[Rimpsyche] Rimpsyche Core version {currentVersion} requires Rimpsyche - Sexuality version {requiredSexualityVersion_string} or above. Sexuality Module ({sexualityVersion_string}) needs to be updated or you will experience errors. If Steam does not automatically update your mod, you can try un-subbing and re-subbing to force the update.");
                    DelayedErrorWindowRequest.Add($"Rimpsyche Core version {currentVersion} requires Rimpsyche - Sexuality version {requiredSexualityVersion_string} or above.\n\nYour Sexuality Module ({sexualityVersion_string}) needs to be updated or you will experience errors.\n\nIf Steam does not automatically update your mod, you can try un-subbing and re-subbing to force the update.", "[Rimpsyche] Outdated Sexuality Module Version");
                }
            }

            if (ModsConfig.IsActive("maux36.rimpsyche.relationship"))
            {
                RelationshipModuleLoaded = true;
            }

            //if (!ModsConfig.IsActive("zetrith.prepatcher"))
            //{
            //    Log.Warning("[Rimpsyche] Prepatcher not detected. For optimal performance, Prepatcher is highly recommended.");
            //}
        }
        public override string SettingsCategory()
        {
            return "RimpsycheSettingCategory".Translate();
        }
        private static Vector2 scrollPosition = new Vector2(0f, 0f);
        private static float totalContentHeight = 400f;
        private const float ScrollBarWidthMargin = 18f;
        public override void DoSettingsWindowContents(Rect inRect)
        {
            Rect outerRect = inRect.ContractedBy(10f);
            bool scrollBarVisible = totalContentHeight > outerRect.height;
            var scrollViewTotal = new Rect(0f, 0f, outerRect.width - (scrollBarVisible ? ScrollBarWidthMargin : 0), totalContentHeight);
            Widgets.BeginScrollView(outerRect, ref scrollPosition, scrollViewTotal);

            var listing_Standard = new Listing_Standard();
            listing_Standard.Begin(new Rect(0f, 0f, scrollViewTotal.width, 9999f));
            listing_Standard.Gap(12f);

            listing_Standard.Label("RimpsycheGeneralSetting".Translate());
            listing_Standard.Gap(12f);
            listing_Standard.CheckboxLabeled("RimpsycheAllowFacetEdit".Translate(), ref RimpsycheSettings.allowFacetEdit, "RimpsycheAllowFacetEditTooltip".Translate());
            listing_Standard.Gap(6f);
            listing_Standard.CheckboxLabeled("RimpsycheShowFacetInMenu".Translate(), ref RimpsycheSettings.showFacetInMenu, "RimpsycheShowFacetInMenuTooltip".Translate());
            listing_Standard.Gap(6f);
            listing_Standard.CheckboxLabeled("RimpsycheShowFacetGraph".Translate(), ref RimpsycheSettings.showFacetGraph, "RimpsycheShowFacetGraphTooltip".Translate());
            listing_Standard.Gap(6f);
            listing_Standard.CheckboxLabeled("RimpsycheConfirmLoadSave".Translate(), ref RimpsycheSettings.confirmLoadSave, "RimpsycheConfirmLoadSaveTooltip".Translate());

            listing_Standard.End();
            Widgets.EndScrollView();
        }
    }
}
