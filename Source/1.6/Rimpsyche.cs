using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;


namespace Maux36.RimPsyche
{
    public class Rimpsyche : Mod
    {
        public static RimpsycheSettings settings;
        public const string minCompatibleSexualityVersion_string = "1.0.5";
        public static string currentVersion;
        public static bool DispositionModuleLoaded = false;
        public static bool SexualityModuleLoaded = false;
        public static bool RelationshipModuleLoaded = false;
        private static List<DescriptorType> toggleableDescriptorTypes = new();
        private static readonly HashSet<DescriptorType> coreTypes = [DescriptorType.Conversation, DescriptorType.Gameplay];
        private static readonly HashSet<DescriptorType> dispositionTypes = [DescriptorType.ThoughtTag];
        private static readonly HashSet<DescriptorType> sexualityTypes = [DescriptorType.Romance];
        public Rimpsyche(ModContentPack content) : base(content)
        {
            currentVersion = content.ModMetaData.ModVersion;
            Log.Message($"[Rimpsyche] Personality Core running with version {currentVersion}");
            settings = GetSettings<RimpsycheSettings>();

            foreach (var type in coreTypes)
            {
                toggleableDescriptorTypes.Add(type);
            }
            if (Rimpsyche_Utility.IsModActive("maux36.rimpsyche.disposition"))
            {
                DispositionModuleLoaded = true;

                foreach (var type in dispositionTypes)
                {
                    toggleableDescriptorTypes.Add(type);
                }
                var dispositionVersion_string = ModLister.GetModWithIdentifier("maux36.rimpsyche.disposition").ModVersion;
                Log.Message($"[Rimpsyche] Disposition module loaded with version {dispositionVersion_string}");
            }

            if (Rimpsyche_Utility.IsModActive("maux36.rimpsyche.sexuality"))
            {
                SexualityModuleLoaded = true;


                foreach (var type in sexualityTypes)
                {
                    toggleableDescriptorTypes.Add(type);
                }
                var sexualityVersion_string = ModLister.GetModWithIdentifier("maux36.rimpsyche.sexuality").ModVersion;
                Log.Message($"[Rimpsyche] Sexuality module loaded with version {sexualityVersion_string}");
                if (new Version(sexualityVersion_string) < new Version(minCompatibleSexualityVersion_string))
                {
                    Log.Error($"[Rimpsyche - Sexuality] Rimpsyche - Sexuality version {sexualityVersion_string} is outdated. Sexuality Module version {minCompatibleSexualityVersion_string} or above is required to run with Rimpsyche Core version {currentVersion}, else you will experience errors. If Steam does not automatically update your mod, you can try un-subbing and re-subbing to force the update.");
                    DelayedErrorWindowRequest.Add($"Rimpsyche - Sexuality version {sexualityVersion_string} is outdated.\n\nSexuality Module version {minCompatibleSexualityVersion_string} or above is required to run with Rimpsyche Core version {currentVersion}, else you will experience errors.\n\nIf Steam does not automatically update your mod, you can try un-subbing and re-subbing to force the update.", "[Rimpsyche - Sexuality] Outdated Module Version");
                }
            }

            if (Rimpsyche_Utility.IsModActive("maux36.rimpsyche.relationship"))
            {
                RelationshipModuleLoaded = true;
            }
            //if (!Rimpsyche_Utility.IsModActive("zetrith.prepatcher"))
            //{
            //    Log.Warning("[Rimpsyche] Prepatcher not detected. For optimal performance, Prepatcher is highly recommended.");
            //}
        }
        public override string SettingsCategory()
        {
            return "RimpsycheSettingCategory".Translate();
        }
        private static Vector2 scrollPosition = new Vector2(0f, 0f);
        private static float TotalContentHeight = 560f;
        private const float ScrollBarWidthMargin = 18f;

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Color oldColor = GUI.color;
            Rect outerRect = inRect.ContractedBy(10f);
            bool scrollBarVisible = TotalContentHeight > outerRect.height;
            var scrollViewTotal = new Rect(0f, 0f, outerRect.width - (scrollBarVisible ? ScrollBarWidthMargin : 0f), TotalContentHeight);

            Widgets.BeginScrollView(outerRect, ref scrollPosition, scrollViewTotal);

            var listing = new Listing_Standard();
            listing.Begin(scrollViewTotal);

            // TabSetting | PsycheInfo Section
            float halfWidth = (scrollViewTotal.width - 17f) / 2f;
            float blockHeight = 124f;
            Rect rowRect = listing.GetRect(blockHeight);

            Rect leftRect = new Rect(rowRect.x, rowRect.y, halfWidth, rowRect.height);
            Rect rightRect = new Rect(rowRect.x + halfWidth + 10f, rowRect.y, halfWidth, rowRect.height);

            // Left: Tab
            Listing_Standard subListingTab = new Listing_Standard();
            subListingTab.Begin(leftRect);
            subListingTab.Label("RimpsycheTabSetting".Translate());
            subListingTab.GapLine(6f);
            subListingTab.CheckboxLabeled("RimpsycheUsePsycheTab".Translate(), ref RimpsycheSettings.usePsycheTab, "RimpsycheUsePsycheTabTooltip".Translate());
            subListingTab.Gap(6f);
            if (RimpsycheSettings.usePsycheTab)
            {
                subListingTab.CheckboxLabeled("RimpsycheHideSummaryInBio".Translate(), ref RimpsycheSettings.hideSummaryInBio, "RimpsycheHideSummaryInBioTooltip".Translate());
                subListingTab.Gap(6f);
            }
            else
            {
                GUI.color = Color.grey;
                subListingTab.Label("RimpsycheHideSummaryInBio".Translate(), tooltip: "RimpsycheHideSummaryInBioDisabledTooltip".Translate());
                GUI.color = oldColor;
                subListingTab.Gap(6f);
            }
            if (RimpsycheSettings.ShowSummaryInBio)
            {
                subListingTab.CheckboxLabeled("RimpsycheShowSideInfoInSummary".Translate(), ref RimpsycheSettings.showSideInfoInSummary, "RimpsycheShowSideInfoInSummaryTooltip".Translate());
                subListingTab.Gap(6f);
            }
            subListingTab.End();

            // Right: Psyche UI Settings
            Listing_Standard subListingPsyche = new Listing_Standard();
            subListingPsyche.Begin(rightRect);
            subListingPsyche.Label("RimpsychePsycheUISetting".Translate());
            subListingPsyche.GapLine(6f);
            if (DispositionModuleLoaded)
            {
                subListingPsyche.CheckboxLabeled("RimpsycheShowDispositionInUI".Translate(), ref RimpsycheSettings.showDispositionInUI, "RimpsycheShowDispositionInUITooltip".Translate());
                subListingPsyche.Gap(6f);
            }
            subListingPsyche.CheckboxLabeled("RimpsycheShowFacetInUI".Translate(), ref RimpsycheSettings.showFacetInUI, "RimpsycheShowFacetInUITooltip".Translate());
            subListingPsyche.Gap(6f);
            if (RimpsycheSettings.showFacetInUI)
            {
                subListingPsyche.CheckboxLabeled("RimpsycheAllowFacetEdit".Translate(), ref RimpsycheSettings.allowFacetEdit, "RimpsycheAllowFacetEditTooltip".Translate());
                subListingPsyche.Gap(6f);
            }
            else
            {
                GUI.color = Color.grey;
                subListingPsyche.Label("RimpsycheAllowFacetEdit".Translate(), tooltip: "RimpsycheAllowFacetEditDisabledTooltip".Translate());
                GUI.color = oldColor;
                subListingPsyche.Gap(6f);
            }
            //subListingPsyche.CheckboxLabeled("RimpsycheShowFacetGraph".Translate(), ref RimpsycheSettings.showFacetGraph, "RimpsycheShowFacetGraphTooltip".Translate());
            subListingPsyche.End();
            listing.Gap(16f);

            // Personality View Section
            listing.Label("RimpsychePersonalityViewSetting".Translate());
            listing.GapLine(6f);

            listing.CheckboxLabeled("RimpsychePersonalityAsBars".Translate(), ref RimpsycheSettings.personalityAsBar, "RimpsychePersonalityAsBarsTooltip".Translate());
            listing.Gap(6f);
            var showEffectValueBefore = RimpsycheSettings.showEffectInDescription;
            listing.CheckboxLabeled("RimpsycheShowEffectInDescription".Translate(), ref RimpsycheSettings.showEffectInDescription, "RimpsycheShowEffectInDescriptionTooltip".Translate());
            if (showEffectValueBefore != RimpsycheSettings.showEffectInDescription)
            {
                ToggleDescriptors();
            }
            listing.Gap(6f);
            if (RimpsycheSettings.showEffectInDescription)
            {
                float lineHeight = 24f;
                float boxHeight = 12f + ((toggleableDescriptorTypes.Count + 1) * lineHeight) + 12f;

                Rect boxRect = listing.GetRect(boxHeight);
                Widgets.DrawMenuSection(boxRect);

                Rect innerBoxRect = boxRect.ContractedBy(12f);

                Listing_Standard subListingBox = new Listing_Standard();
                subListingBox.Begin(innerBoxRect);
                subListingBox.Label("RimpsycheDescriptionEffectToggle".Translate());
                foreach (DescriptorType descriptorType in toggleableDescriptorTypes)
                {
                    bool enabled = RimpsycheSettings.DescriptorTypesToShow.Contains(descriptorType);
                    bool before = enabled;
                    subListingBox.CheckboxLabeled(("RP_Desc_"+descriptorType.ToString()).Translate(), ref enabled, tabIn: 8f);
                    if (before != enabled)
                    {
                        if (enabled)
                        {
                            RimpsycheSettings.DescriptorTypesToShow.Add(descriptorType);
                            ToggleDescriptors();
                            //Log.Message("Enabled. DescriptorTypesToShow: " + string.Join(", ", RimpsycheSettings.DescriptorTypesToShow));
                        }
                        else
                        {
                            RimpsycheSettings.DescriptorTypesToShow.Remove(descriptorType);
                            ToggleDescriptors();
                            //Log.Message("Disabled. DescriptorTypesToShow: " + string.Join(", ", RimpsycheSettings.DescriptorTypesToShow));
                        }
                    }
                }

                subListingBox.End();
            }

            listing.Gap(20f);

            // Misc Section
            listing.Label("RimpsycheMiscSetting".Translate());
            listing.GapLine(6f);
            listing.CheckboxLabeled("RimpsycheConfirmLoadSave".Translate(), ref RimpsycheSettings.confirmLoadSave, "RimpsycheConfirmLoadSaveTooltip".Translate());

            listing.Gap(20f);

            // Reset
            if (listing.ButtonText("RimpsycheDefaultSetting".Translate()))
            {
                ResetDefaults();
            }

            listing.End();
            Widgets.EndScrollView();
        }
        private static void ResetDefaults()
        {
            RimpsycheSettings.usePsycheTab = false;
            RimpsycheSettings.hideSummaryInBio = false;
            RimpsycheSettings.showSideInfoInSummary = true;

            RimpsycheSettings.showDispositionInUI = false;
            RimpsycheSettings.showFacetInUI = false;

            RimpsycheSettings.showEffectInDescription = true;
            RimpsycheSettings.showThoughtTagEffects = false;
            RimpsycheSettings.personalityAsBar = true;

            RimpsycheSettings.allowFacetEdit = false;
            RimpsycheSettings.showFacetGraph = false;
            RimpsycheSettings.confirmLoadSave = true;
            RimpsycheSettings.DescriptorTypesToShow = [.. (DescriptorType[])Enum.GetValues(typeof(DescriptorType))];
            ToggleDescriptors();
        }
        public static void ToggleDescriptors()
        {
            foreach (var descDef in DefDatabase<PsycheDescriptorDef>.AllDefsListForReading)
            {
                if (!RimpsycheSettings.showEffectInDescription)
                {
                    descDef.showEffect = false;
                }
                else if (coreTypes.Contains(descDef.type))
                {
                    if (RimpsycheSettings.DescriptorTypesToShow.Contains(descDef.type)) descDef.showEffect = true;
                    else descDef.showEffect = false;
                }
                else if (dispositionTypes.Contains(descDef.type))
                {
                    if (DispositionModuleLoaded && (RimpsycheSettings.DescriptorTypesToShow.Contains(descDef.type))) descDef.showEffect = true;
                    else descDef.showEffect = false;
                }
                else if (sexualityTypes.Contains(descDef.type))
                {
                    if (SexualityModuleLoaded && (RimpsycheSettings.DescriptorTypesToShow.Contains(descDef.type))) descDef.showEffect = true;
                    else descDef.showEffect = false;
                }
            }
            PsycheInfoCard.CacheClean();
        }
    }
}
