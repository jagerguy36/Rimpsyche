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
        private static float TotalContentHeight => RimpsycheSettings.showEffectInDescription ? 560f : 540f;
        private const float ScrollBarWidthMargin = 18f;

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Rect outerRect = inRect.ContractedBy(10f);
            bool scrollBarVisible = TotalContentHeight > outerRect.height;
            var scrollViewTotal = new Rect(0f, 0f, outerRect.width - (scrollBarVisible ? ScrollBarWidthMargin : 0f), TotalContentHeight);

            Widgets.BeginScrollView(outerRect, ref scrollPosition, scrollViewTotal);

            var listing = new Listing_Standard();
            listing.Begin(scrollViewTotal);

            // PsycheTab Toggle
            listing.CheckboxLabeled("RimpsycheUsePsycheTab".Translate(), ref RimpsycheSettings.usePsycheTab, "RimpsycheUsePsycheTabTooltip".Translate());
            listing.Gap(12f);

            // BioTab | PsycheTab Section
            if (RimpsycheSettings.usePsycheTab)
            {
                float halfWidth = (scrollViewTotal.width - 17f) / 2f;
                float blockHeight = 100f;
                Rect rowRect = listing.GetRect(blockHeight);

                Rect leftRect = new Rect(rowRect.x, rowRect.y, halfWidth, rowRect.height);
                Rect rightRect = new Rect(rowRect.x + halfWidth + 10f, rowRect.y, halfWidth, rowRect.height);

                // Left: Bio & Summary
                Listing_Standard subListingBio = new Listing_Standard();
                subListingBio.Begin(leftRect);
                subListingBio.Label("RimpsycheBioTabSetting".Translate());
                subListingBio.GapLine(6f);
                subListingBio.CheckboxLabeled("RimpsycheHideSummaryInBio".Translate(), ref RimpsycheSettings.hideSummaryInBio, "RimpsycheHideSummaryInBioTooltip".Translate());

                if (DispositionModuleLoaded && !RimpsycheSettings.hideSummaryInBio)
                {
                    subListingBio.CheckboxLabeled("RimpsycheShowDispositionInSummary".Translate(), ref RimpsycheSettings.showDispositionInSummary, "RimpsycheShowDispositionInSummaryTooltip".Translate());
                }
                subListingBio.End();

                // Right: Psyche Tab Settings
                Listing_Standard subListingPsyche = new Listing_Standard();
                subListingPsyche.Begin(rightRect);
                subListingPsyche.Label("RimpsychePsycheTabSetting".Translate());
                subListingPsyche.GapLine(6f);
                if (DispositionModuleLoaded)
                {
                    subListingPsyche.CheckboxLabeled("RimpsycheShowDispositionInTab".Translate(), ref RimpsycheSettings.showDispositionInTab, "RimpsycheShowDispositionInTabTooltip".Translate());
                }
                subListingPsyche.CheckboxLabeled("RimpsycheShowFacetInMenu".Translate(), ref RimpsycheSettings.showFacetInMenu, "RimpsycheShowFacetInMenuTooltip".Translate());
                subListingPsyche.CheckboxLabeled("RimpsycheShowFacetGraph".Translate(), ref RimpsycheSettings.showFacetGraph, "RimpsycheShowFacetGraphTooltip".Translate());
                subListingPsyche.End();
                listing.Gap(16f);
            }
            // Full width when Psyche Tab is disabled
            else if (DispositionModuleLoaded)
            {
                listing.Label("RimpsycheBioTabSetting".Translate());
                listing.GapLine(6f);
                listing.CheckboxLabeled("RimpsycheShowDispositionInSummary".Translate(), ref RimpsycheSettings.showDispositionInSummary, "RimpsycheShowDispositionInSummaryTooltip".Translate());
                listing.Gap(16f);
            }


            // Personality View Section
            listing.Label("RimpsychePersonalityViewSetting".Translate());
            listing.GapLine(6f);

            listing.CheckboxLabeled("RimpsychePersonalityAsBars".Translate(), ref RimpsycheSettings.personalityAsBar, "RimpsychePersonalityAsBarsTooltip".Translate());
            listing.Gap(6f);
            listing.CheckboxLabeled("RimpsycheShowEffectInDescription".Translate(), ref RimpsycheSettings.showEffectInDescription, "RimpsycheShowEffectInDescriptionTooltip".Translate());
            listing.Gap(6f);
            if (RimpsycheSettings.showEffectInDescription)
            {
                listing.Label("RimpsycheDescriptionEffectToggle".Translate());
                float lineHeight = 24f;
                float boxHeight = 12f + (toggleableDescriptorTypes.Count * lineHeight) + 12f;

                Rect boxRect = listing.GetRect(boxHeight);
                Widgets.DrawMenuSection(boxRect);

                Rect innerBoxRect = boxRect.ContractedBy(12f);

                Listing_Standard subListingBox = new Listing_Standard();
                subListingBox.Begin(innerBoxRect);

                foreach (DescriptorType descriptorType in toggleableDescriptorTypes)
                {
                    bool enabled = RimpsycheSettings.DescriptorTypesToShow.Contains(descriptorType);
                    bool before = enabled;
                    subListingBox.CheckboxLabeled(("RP_Desc_"+descriptorType.ToString()).Translate(), ref enabled);
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
            listing.CheckboxLabeled("RimpsycheAllowFacetEdit".Translate(), ref RimpsycheSettings.allowFacetEdit, "RimpsycheAllowFacetEditTooltip".Translate());
            listing.Gap(6f);
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
            RimpsycheSettings.showDispositionInSummary = true;
            RimpsycheSettings.showEffectInDescription = true;
            RimpsycheSettings.showThoughtTagEffects = false;
            RimpsycheSettings.showDispositionInTab = false;
            RimpsycheSettings.personalityAsBar = true;
            RimpsycheSettings.allowFacetEdit = false;
            RimpsycheSettings.showFacetInMenu = false;
            RimpsycheSettings.showFacetGraph = false;
            RimpsycheSettings.confirmLoadSave = true;
            RimpsycheSettings.DescriptorTypesToShow = [.. (DescriptorType[])Enum.GetValues(typeof(DescriptorType))];
            ToggleDescriptors();
        }
        public static void ToggleDescriptors()
        {
            foreach (var descDef in DefDatabase<PsycheDescriptorDef>.AllDefsListForReading)
            {
                if (coreTypes.Contains(descDef.type))
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
