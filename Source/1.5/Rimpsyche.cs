using RimWorld;
using UnityEngine;
using Verse;


namespace Maux36.RimPsyche
{
    public class Rimpsyche : Mod
    {
        public static RimpsycheSettings settings;
        public Rimpsyche(ModContentPack content) : base(content)
        {

            settings = GetSettings<RimpsycheSettings>();

            if (!ModsConfig.IsActive("zetrith.prepatcher"))
            {
                Log.Error("Rimpsyche could not find its required dependency: Prepatcher. This is a critical component, and your game will not work without it.");
            }
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

            listing_Standard.End();
            Widgets.EndScrollView();
        }
    }
}
