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

            if(SexualityModuleLoaded)
            {
                listing_Standard.Gap(12f);
                listing_Standard.Label("RimpsycheKinseyDistributionSettings".Translate());
                Rect sliderArea = listing_Standard.GetRect(200f);
                DrawKinseyDistributionSliders(sliderArea);
            }

            listing_Standard.End();
            Widgets.EndScrollView();
        }
        private void DrawKinseyDistributionSliders(Rect rect)
        {
            float total = 0f;
            foreach (var value in RimpsycheSettings.KinseyDistributionSetting)
            {
                total += value;
            }
            const int numSliders = 7;
            float spacing = 5f;
            float sliderWidth = 50f;
            float sliderHeight = rect.height - 100f; // Leave space for labels
            string pcText;

            for (int i = 0; i < numSliders; i++)
            {
                float x = rect.x + i * (sliderWidth + spacing);
                Widgets.Label(new Rect(x, rect.y, sliderWidth, 20f), i.ToString());
                Rect sliderRect = new Rect(x, rect.y+20f, sliderWidth, sliderHeight);
                RimpsycheSettings.KinseyDistributionSetting[i] = (int)(GUI.VerticalSlider(
                    sliderRect,
                    RimpsycheSettings.KinseyDistributionSetting[i],
                    100f,
                    0f
                ));

                Rect valLabelRect = new Rect(x, sliderRect.yMax, sliderWidth, 20f);
                Widgets.Label(valLabelRect, RimpsycheSettings.KinseyDistributionSetting[i].ToString());
                Rect pcLabelRect = new Rect(x, valLabelRect.yMax, sliderWidth, 20f);
                if (total == 0f) pcText = "(" + (1f / 7f).ToStringPercent() + ")";
                else pcText = "(" + (RimpsycheSettings.KinseyDistributionSetting[i] / total).ToStringPercent() + ")";
                Widgets.Label(pcLabelRect, pcText);
            }
            Rect buttonRect = new Rect(rect.x, rect.yMax-30f, 300f, 30f);
            if(Widgets.ButtonText(buttonRect, "RimpsycheKinseyDefaultSetting".Translate()))
            {

                RimpsycheSettings.KinseyDistributionSetting = [60, 10, 10, 5, 5, 5, 5];
            }
        }
    }
}
