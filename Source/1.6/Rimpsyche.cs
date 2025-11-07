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

            if(SexualityModuleLoaded)
            {
                listing_Standard.Label("RimpsycheKinseyDistributionSettings".Translate());
                Rect sliderArea = listing_Standard.GetRect(200f);
                DrawKinseyDistributionSliders(sliderArea);
            }

            listing_Standard.End();
            Widgets.EndScrollView();
        }
        private void DrawKinseyDistributionSliders(Rect rect)
        {
            float total = RimpsycheSettings.KinseyDistributionSetting.Sum();
            const int numSliders = 7;
            float spacing = 20f;
            float sliderWidth = (rect.width - spacing * (numSliders - 1)) / numSliders;
            float sliderHeight = rect.height - 40f; // Leave space for labels

            Text.Anchor = TextAnchor.MiddleCenter;

            for (int i = 0; i < numSliders; i++)
            {
                float x = rect.x + i * (sliderWidth + spacing);
                Rect sliderRect = new Rect(x, rect.y + 20f, sliderWidth, sliderHeight);

                Widgets.Label(new Rect(x, rect.y, sliderWidth, 20f), i.ToString());

                RimpsycheSettings.KinseyDistributionSetting[i] = Mathf.Round(GUI.VerticalSlider(
                    sliderRect,
                    RimpsycheSettings.KinseyDistributionSetting[i],
                    100f,
                    0f
                ), 1f);

                string valueText = (RimpsycheSettings.KinseyDistributionSetting[i]/total).ToString("0.0");
                Widgets.Label(new Rect(x, rect.y + sliderHeight + 25f, sliderWidth, 20f), valueText);
            }

            Text.Anchor = TextAnchor.UpperLeft;
        }
    }
}
