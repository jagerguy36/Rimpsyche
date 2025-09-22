using RimWorld;
using UnityEngine;
using Verse;


namespace Maux36.RimPsyche
{
    public class Rimpsyche : Mod
    {
        public static RimpsycheSettings settings;
        public static string currentVersion;
        public static bool SexualityModuleLoaded = false;
        public static bool DispositionModuleLoaded = false;
        public Rimpsyche(ModContentPack content) : base(content)
        {
            currentVersion = content.ModMetaData.ModVersion;
            settings = GetSettings<RimpsycheSettings>();

            if (ModsConfig.IsActive("maux36.rimpsyche.sexuality"))
            {
                SexualityModuleLoaded = true;
            }

            if (ModsConfig.IsActive("maux36.rimpsyche.disposition"))
            {
                DispositionModuleLoaded = true;
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
        private static float totalContentHeight = ModsConfig.BiotechActive ? 770f : 720f;
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

            listing_Standard.End();
            Widgets.EndScrollView();
        }
    }
}
