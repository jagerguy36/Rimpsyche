using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Maux36.RimPsyche
{
    public class PsycheEditPopup : Window
    {
        private Pawn editFor;
        private const float baseMargin = 36f; //Margin is set to 18f.
        private bool editMode;
        static PsycheEditPopup()
        {
            facetLabelWidth = RimpsycheDatabase.maxFacetLabelWidth;
            facetWidthDiff = 2f * (facetLabelWidth - 130f);

            personalityLabelWidth = RimpsycheDatabase.maxPersonalityLabelWidth;
            personalityWidthDiff = 2f * (personalityLabelWidth - 130f);
            personalityViewHeight = DefDatabase<PersonalityDef>.AllDefsListForReading.Count * personalityRowHeight;
            intensityRectWidth = RimpsycheDatabase.dispositionIntensityWidth;

            interestLabelWidth = RimpsycheDatabase.maxInterestLabelWidth;
            interestWidthDiff = (interestLabelWidth - 130f);
            interestViewHeight = RimpsycheDatabase.InterestList.Count * interestRowHeight;

            leftRectWidth = 330f + facetWidthDiff;
            midRectWidth = 380f + personalityWidthDiff;
            kinseyLabelWidth = Text.CalcSize("RPC_Kinsey".Translate()).x + 20f;
            var sexualityLabelDiff = RimpsycheDatabase.maxEditSexualityLabelWidth - 100f;
            rightRectWidth = 240f + Mathf.Max(interestWidthDiff, sexualityLabelDiff);
            rightRectWidth = Mathf.Max(kinseyLabelWidth + RimpsycheDatabase.orientationLabelWidth + 10f, rightRectWidth);
            //rightRectWidth = Mathf.Max(rightRectWidth, RimpsycheDatabase.maxSexualityLabelWidth + 10f + RimpsycheDatabase.orientationLabelWidth);
            totalBaseSize = midRectWidth + rightRectWidth + baseMargin;
            totalFullSize = leftRectWidth + midRectWidth + rightRectWidth + baseMargin;
        }
        public PsycheEditPopup(Pawn editFor, bool enableEdit = false)
        {
            this.editMode = enableEdit;
            this.editFor = editFor;
            soundClose = SoundDefOf.InfoCard_Close;
            closeOnClickedOutside = true;
            absorbInputAroundWindow = false;
            forcePause = true;
            preventCameraMotion = false;
            doCloseX = true;
            closeOnAccept = true;
            closeOnCancel = true;
        }

        // Settings
        private static readonly bool usePreference = RimpsycheSexualitySettings.usePreferenceSystem;
        public override Vector2 InitialSize
        {
            get
            {
                // Get the current screen width and height
                float screenWidth = UI.screenWidth;
                float screenHeight = UI.screenHeight;

                // Calculate desired width and height based on screen size
                float desiredWidth = screenWidth * 0.55f;
                float desiredHeight = screenHeight * 0.5f;

                float minWidth = RimpsycheSettings.showFacetInUI ? totalFullSize : totalBaseSize;
                float minHeight = 400f;

                return new Vector2(minWidth, Mathf.Max(desiredHeight, minHeight));
            }
        }
        public static readonly float leftRectWidth;
        public static readonly float midRectWidth;
        public static readonly float rightRectWidth;
        public static readonly float totalBaseSize;
        public static readonly float totalFullSize;
        //Shared
        public static readonly float innerPadding = 5f;
        public static readonly float titleHeight = 35f;
        public static readonly float scrollBarWidth = 20f;
        public static readonly float titleContentSpacing = 5f;
        public static readonly float iconSize = 15f;
        public static readonly float iconSpacing = 6f;
        public static readonly float resetButtonSize = 20f;
        public static readonly float resetButtonMargin = 5f;
        public static readonly Color barBackgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);

        //Facet
        public static bool editFacetOn = false;
        public static readonly float facetLabelWidth;
        public static readonly float facetWidthDiff;
        public static readonly float facetRowHeight = 28f;
        public static readonly float facetViewHeight = 15f * facetRowHeight;
        public static readonly float facetLabelPadding = 2f;
        public static readonly float facetBarWidth = 80f;
        public static readonly float facetBarHeight = 4f;

        //Personality

        public static ShowMode showMode = ShowMode.Personality;
        public static bool editPersonalityOn = false;
        public static readonly float personalityLabelWidth;
        public static readonly float personalityWidthDiff;
        public static readonly float personalityRowHeight = 32f;
        public static readonly float personalityViewHeight;
        public static readonly float intensityRectWidth;
        public static readonly float personalityLabelPadding = 2f;
        public static readonly float personalityBarWidth = 90f;
        public static readonly float personalityBarHeight = 4f;
        public static Color LowValueColor = Color.grey;
        public static Color HighValueColor = Color.green;
        public static Color lightGreyColor = new Color(0.5f, 0.5f, 0.5f, 0.75f);
        private struct PersonalityDisplayDataForEditor
        {
            public PersonalityDef Personality;
            public string CachedDescription;
            public string CachedFullDescription;
        }
        public struct BehaviorDataForEditor
        {
            public string Label;
            public string Intensity;
            public string Description;
            public string Tooltip;
            public float NormalizedAbsValue;
            public bool IsSignificant;
        }
        private static List<PersonalityDisplayDataForEditor> cachedPersonalityData = null;
        private static Dictionary<ushort, List<string>> cachedPersonalityEffects = new();
        private static List<BehaviorDataForEditor> cachedBehaviorData = null;
        private static List<BehaviorDataForEditor> GetBehaviorData(CompPsyche compPsyche, Pawn currentPawn)
        {
            if (cachedBehaviorData == null)
            {
                GeneratePersonalityData(compPsyche, currentPawn);
                return cachedBehaviorData;
            }
            return cachedBehaviorData;
        }
        private static List<PersonalityDisplayDataForEditor> GetPersonalityData(CompPsyche compPsyche, Pawn currentPawn)
        {
            if (cachedPersonalityData == null)
            {
                GeneratePersonalityData(compPsyche, currentPawn);
                return cachedPersonalityData;
            }
            return cachedPersonalityData;
        }
        private static void GeneratePersonalityData(CompPsyche compPsyche, Pawn currentPawn)
        {
            GenerateBehaviorData(compPsyche);
            var personalityDefList = DefDatabase<PersonalityDef>.AllDefsListForReading;
            var rawData = new List<PersonalityDisplayDataForEditor>();
            foreach (var personality in personalityDefList)
            {
                float value = compPsyche.Personality.GetPersonality(personality);
                string cachedLabelText = ((value >= 0) ? personality.high : personality.low).CapitalizeFirst();
                var personalityShortDesc = (value >= 0f ? personality.highDescription : personality.lowDescription);
                var personalityFullDesc = $"{personality.label.CapitalizeFirst()}: {value * 100f:F1}\n\n{personality.description}";
                var effectString = string.Empty;
                if (cachedPersonalityEffects.TryGetValue(personality.shortHash, out var effectList))
                {
                    effectString = effectHeaderString + string.Join("\n", effectList);
                }
                if (effectString != string.Empty && RimpsycheSettings.showEffectInDescription)
                {
                    personalityFullDesc += effectString;
                    personalityShortDesc += effectString;
                }
                if (compPsyche.Personality.scopeInfoCache.TryGetValue(personality.shortHash, out string explanation))
                {
                    personalityShortDesc += $"\n\n{explanation}";
                    personalityFullDesc += $"\n\n{explanation}";
                }
                var personalityDesc = $"{cachedLabelText}\n\n{personalityShortDesc}" + shiftForFullString;
                rawData.Add(new PersonalityDisplayDataForEditor
                {
                    Personality = personality,
                    CachedDescription = personalityDesc,
                    CachedFullDescription = personalityFullDesc,
                });
            }
            cachedPersonalityData = rawData;
        }
        private static void GenerateBehaviorData(CompPsyche compPsyche)
        {
            cachedPersonalityEffects.Clear();
            var sortedData = new List<BehaviorDataForEditor>();

            foreach (PsycheDescriptorDef descDef in DefDatabase<PsycheDescriptorDef>.AllDefs)
            {
                var dWorker = descDef.Worker;
                dWorker.Build(compPsyche);

                foreach ((ushort shortHash, var impactDesc) in dWorker.bImpactRegistry)
                {
                    if (!cachedPersonalityEffects.TryGetValue(shortHash, out List<string> list))
                    {
                        list = new List<string>();
                        cachedPersonalityEffects.Add(shortHash, list);
                    }
                    list.Add(impactDesc);
                }
                if (string.IsNullOrEmpty(dWorker.bLabel))
                    continue;
                var normalizedAbsValue = dWorker.bNormalizedAbsValue;
                var result = new BehaviorDataForEditor
                {
                    Label = dWorker.bLabel,
                    Intensity = dWorker.bIntensityString,
                    Description = dWorker.bDescription,
                    Tooltip = dWorker.bToolTip,
                    NormalizedAbsValue = normalizedAbsValue,
                    IsSignificant = normalizedAbsValue >= 1f
                };
                sortedData.Add(result);
            }

            // Sort descending by strength
            sortedData.Sort((a, b) => b.NormalizedAbsValue.CompareTo(a.NormalizedAbsValue));
            cachedBehaviorData = sortedData;
        }
        //Interest
        public static bool editInterestOn = false;
        public static readonly float interestLabelWidth;
        public static readonly float interestWidthDiff;
        public static readonly float interestRowHeight = 32f;
        public static readonly float interestViewHeight;
        public static readonly float interestLabelPadding = 2f;
        public static readonly float interestBarHeight = 4f;
        public static Color LowInterestColor = new Color(0.6f, 0.55f, 0.65f, 0.5f);
        public static Color HighInterestColor = new Color(0.95f, 0.9f, 0.1f, 1f);

        public static Vector2 FacetNodeScrollPosition = Vector2.zero;
        public static Vector2 PersonalityNodeScrollPosition = Vector2.zero;
        public static Vector2 InterestNodeScrollPosition = Vector2.zero;

        //Sexuality
        public static bool editSexualityOn = false;
        public static readonly float kinseyLabelWidth;
        public static readonly float sexualityContentHeight = 160f;
        public static readonly float sexualityRowHeight = 30f;
        public static readonly float sexualityBarHeight = 4f;
        public static Color LowAttractionColor = Color.grey;
        public static Color HighAttractionBarColor = Color.green;
        public static Color HyperAttractionBarColor = Color.cyan;
        public static Color LowLibidoBarColor = new Color(0.75f, 0.65f, 0.8f, 0.5f);
        public static Color HighLibidoBarColor = new Color(1f, 0.4f, 0.6f, 1f);
        public static Color HyperLibidoBarColor = new Color(0.9f, 0.15f, 0.25f, 1f);
        public static Color maleHighColor = new Color(0.1f, 0.3f, 0.7f, 1f);
        public static Color maleLowolor = new Color(0.4f, 0.7f, 0.9f, 1f);
        public static Color femaleHighColor = new Color(0.7f, 0.1f, 0.3f, 1f);
        public static Color femaleLowolor = new Color(0.9f, 0.4f, 0.7f, 1f);

        //Preference
        private static bool resetPreferenceHeights = true;
        private static List<float> cachedViewerHeights = null;
        private static List<float> GetViewerHeights(Pawn currentPawn)
        {
            //List<(string, float)> cachedPreferenceReport
            if (resetPreferenceHeights == false && cachedViewerHeights != null)
            {
                return cachedViewerHeights;
            }
            GenerateViewerHeights(currentPawn);
            return cachedViewerHeights;
        }
        private static void GenerateViewerHeights(Pawn currentPawn)
        {
            cachedViewerHeights = new();
            var allPrefDefs = DefDatabase<PreferenceDef>.AllDefsListForReading;
            for (int i = 0; i < allPrefDefs.Count; i++)
            {
                var prefDef = allPrefDefs[i];
                if (!prefDef.isActive)
                {
                    cachedViewerHeights.Add(0f);
                }
                else
                {
                    float viewerHeight = prefDef.worker.GetViewerHeight(currentPawn);
                    cachedViewerHeights.Add(viewerHeight);
                }
            }
            resetPreferenceHeights = false;
        }

        // Labels
        public static readonly string effectHeaderString = $"\n\n{"RP_PsycheEffects".Translate()}:\n";
        public static readonly string shiftForFullString = $"\n\n<i><color=#808080BF>{"RP_ShiftForFull".Translate()}</color></i>";
        public static readonly string kinseyLabel = "RPC_Kinsey".Translate();
        public static readonly string maleAttractionLabel = "RPC_AttractionMale".Translate();
        public static readonly string femaleAttractionLabel = "RPC_AttractionFemale".Translate();
        public static readonly string maxAttractionLabel = "RPC_MaxAttraction".Translate();
        public static readonly string sexDriveLabel = "RPC_SexDrive".Translate();

        public static bool showPreference = false;
        public static float preferenceViewHeight = RimpsycheDatabase.totalPreferenceEditorfHeight;

        public override void PreOpen()
        {
            base.PreOpen();
            editFacetOn = false;
            editPersonalityOn = false;
            editInterestOn = false;
            editSexualityOn = false;
            FacetNodeScrollPosition = Vector2.zero;
            PersonalityNodeScrollPosition = Vector2.zero;
            InterestNodeScrollPosition = Vector2.zero;
            resetPreferenceHeights = true;
        }

        public override void DoWindowContents(Rect inRect)
        {
            if (!IsOpen) return;
            // Get pawn
            Pawn pawn = editFor;
            bool allowEdit = editMode || Prefs.DevMode;

            if (pawn == null) return;
            var compPsyche = pawn.compPsyche();
            if (compPsyche == null) return;

            float leftWidth = (RimpsycheSettings.showFacetInUI) ? leftRectWidth : 0f;
            float restTotalWidth = inRect.width - leftWidth;
            float midWidth = midRectWidth;
            float rightWidth = rightRectWidth;

            Rect leftRect = new Rect(inRect.x, inRect.y, leftWidth, inRect.height);
            Rect middleRect = new Rect(leftRect.xMax, inRect.y, midWidth, inRect.height);

            Rect rightTopRect = new Rect(middleRect.xMax, inRect.y, rightWidth, compPsyche.Sexuality.ShowOnUI() ? sexualityContentHeight : 0f);
            Rect rightBottomRect = new Rect(middleRect.xMax, rightTopRect.yMax, rightWidth, inRect.height- rightTopRect.height);
            bool showSexuality = compPsyche.Sexuality.ShowOnUI();
            bool showPref = showPreference && showSexuality;

            if (RimpsycheSettings.showFacetInUI)
                DrawFacetCard(leftRect, allowEdit, pawn, compPsyche);

            DrawPersonalityEditcard(middleRect, allowEdit, pawn, compPsyche);
            if (showSexuality)
            {
                DrawSexualityEditCard(rightTopRect, allowEdit, pawn, compPsyche);
            }
            if (showPref) DrawPreferenceEditCard(rightBottomRect, allowEdit, pawn, compPsyche);
            else DrawInterestEditCard(rightBottomRect, allowEdit, pawn, compPsyche, showSexuality);

            if (compPsyche?.Enabled != true)
            {
                TextAnchor oldAnchor = Text.Anchor;
                GameFont oldFont = Text.Font;
                Widgets.DrawHighlight(inRect);
                Text.Anchor = TextAnchor.MiddleCenter;
                Text.Font = GameFont.Medium;
                GUI.color = new Color(1f, 0f, 0f, 0.80f);
                Widgets.Label(inRect, "PsycheDisabled".Translate());
                GUI.color = Color.white;
                Text.Font = oldFont;
                Text.Anchor = oldAnchor;
            }
        }

        public static void DrawSexualityEditCard(Rect rect, bool allowEdit, Pawn pawn, CompPsyche compPsyche)
        {
            var psycheEnabled = compPsyche?.Enabled == true;
            TextAnchor oldAnchor = Text.Anchor;
            GameFont oldFont = Text.Font;
            Rect innerRect = rect.ContractedBy(innerPadding);
            var sexuality = compPsyche.Sexuality;

            // Title
            Rect titleRect = new Rect(innerRect.x, innerRect.y, innerRect.width, titleHeight);
            Text.Anchor = TextAnchor.MiddleCenter;
            Text.Font = GameFont.Medium;
            string titleString = "RPC_Sexuality".Translate();
            Widgets.Label(titleRect, titleString);
            Vector2 titleTextSize = Text.CalcSize(titleString);
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;

            // Icon on the right
            float editIconX = titleRect.x + (titleRect.width / 2f) + (titleTextSize.x / 2f) + iconSpacing;
            Rect editIconRect = new Rect(editIconX, titleRect.y + (titleHeight - iconSize) / 2f, iconSize, iconSize);

            // Draw & handle click
            if (psycheEnabled && allowEdit)
            {
                if (Widgets.ButtonImage(editIconRect, Rimpsyche_UI_Utility.EditButton))
                {
                    editSexualityOn = !editSexualityOn;
                }
                TooltipHandler.TipRegion(editIconRect, "RimpsycheEdit".Translate());
            }


            Rect ContentRect = new Rect(innerRect.x, titleRect.yMax, innerRect.width, innerRect.height - titleHeight);
            float maxSexualityLabelWidth = RimpsycheDatabase.maxEditSexualityLabelWidth + 5f;
            float sliderWidth = ContentRect.width - maxSexualityLabelWidth;

            Rect KinseyLabelRect = new Rect(ContentRect.x, ContentRect.y, kinseyLabelWidth, sexualityRowHeight);
            Widgets.Label(KinseyLabelRect, kinseyLabel + ": ");
            Rect KinseyReportRect = new Rect(KinseyLabelRect.xMax, ContentRect.y, sliderWidth, sexualityRowHeight);
            Widgets.Label(KinseyReportRect, (compPsyche.Sexuality.GetOrientationReport() + $" ({compPsyche.Sexuality.GetKinseyReport()})"));// + "(" + sexuality.kinsey.ToString("F2") + ")"

            Rect sliderRect1 = new Rect(ContentRect.x, KinseyLabelRect.yMax, innerRect.width, sexualityRowHeight);
            if (editSexualityOn)
            {
                float newMValue = Widgets.HorizontalSlider(sliderRect1, sexuality.MKinsey, 0f, 1f, true, leftAlignedLabel: femaleAttractionLabel, rightAlignedLabel: maleAttractionLabel);
                if (newMValue != sexuality.MKinsey) sexuality.SetmKinsey(newMValue);
            }
            else
            {
                float leftLabelWidth = Text.CalcSize(femaleAttractionLabel).x;
                float rightLabelWidth = Text.CalcSize(maleAttractionLabel).x;
                Rect leftLabelRect = new Rect(sliderRect1.x, sliderRect1.y,leftLabelWidth + 8f, sexualityRowHeight);
                Rect rightLabelRect = new Rect(sliderRect1.xMax - (rightLabelWidth + 8f), sliderRect1.y, rightLabelWidth + 8f, sexualityRowHeight);

                Widgets.Label(leftLabelRect, femaleAttractionLabel);
                Text.Anchor = TextAnchor.MiddleRight;
                Widgets.Label(rightLabelRect, maleAttractionLabel);
                Text.Anchor = TextAnchor.UpperLeft;

                Rect barRect = new Rect(leftLabelRect.xMax, sliderRect1.y + (sexualityRowHeight - sexualityBarHeight) / 2f, sliderRect1.width - (leftLabelRect.width + rightLabelRect.width), sexualityBarHeight);

                Widgets.DrawBoxSolid(barRect, barBackgroundColor);
                float clamped = (sexuality.MKinsey - 0.5f) * 2f;
                float barCenterX = barRect.center.x;
                float halfBarWidth = Mathf.Abs(clamped) * (barRect.width / 2f);
                Rect valueRect = clamped >= 0f
                    ? new Rect(barCenterX, barRect.y, halfBarWidth, sexualityBarHeight)
                    : new Rect(barCenterX - halfBarWidth, barRect.y, halfBarWidth, sexualityBarHeight);

                Color barColor = clamped >= 0f
                    ? Color.Lerp(maleLowolor, maleHighColor, Mathf.Abs(clamped))
                    : Color.Lerp(femaleLowolor, femaleHighColor, Mathf.Abs(clamped));
                Widgets.DrawBoxSolid(valueRect, barColor);
                Widgets.DrawLineVertical(barRect.x + barRect.width * 0.5f, barRect.y - 1, barRect.height + 2);
            }
            TooltipHandler.TipRegion(sliderRect1, "RPS_KinseySliderTooltip".Translate());

            Rect labelRect2 = new Rect(ContentRect.x, sliderRect1.yMax, maxSexualityLabelWidth, sexualityRowHeight);
            Widgets.Label(labelRect2, maxAttractionLabel);
            Rect sliderRect2 = new Rect(labelRect2.xMax, labelRect2.y, sliderWidth, sexualityRowHeight);
            if (editSexualityOn)
            {
                float newAttraction = Widgets.HorizontalSlider(sliderRect2, sexuality.Attraction, 0f, 1f, true, null, null, (2f * sexuality.Attraction).ToString("F2"));
                if (newAttraction != sexuality.Attraction) sexuality.SetAttraction(newAttraction);
            }
            else
            {
                // Using your custom style with surplus and divider line
                Rect attractionBarRect = new Rect(sliderRect2.x, sliderRect2.y + (sexualityRowHeight - sexualityBarHeight) / 2f, sliderWidth, sexualityBarHeight);
                Widgets.DrawBoxSolid(attractionBarRect, barBackgroundColor);
                float attractionVal = sexuality.Attraction;
                Rect attrValueRect = new Rect(attractionBarRect.x, attractionBarRect.y, attractionVal * attractionBarRect.width, sexualityBarHeight);
                Color attrColor = attractionVal <= 0.5f
                    ? Color.Lerp(LowAttractionColor, HighAttractionBarColor, attractionVal * 2f)
                    : Color.Lerp(HighAttractionBarColor, HyperAttractionBarColor, (attractionVal - 0.5f) * 2f);

                Widgets.DrawBoxSolid(attrValueRect, attrColor);
                Widgets.DrawLineVertical(attractionBarRect.x + attractionBarRect.width * 0.5f, attractionBarRect.y - 1, attractionBarRect.height + 2);
            }
            Rect attractionRect = new Rect(ContentRect.x, labelRect2.y, ContentRect.width, sexualityRowHeight);
            TooltipHandler.TipRegion(attractionRect, "RPS_AttractionTooltip".Translate());

            Rect labelRect3 = new Rect(ContentRect.x, labelRect2.yMax, maxSexualityLabelWidth, sexualityRowHeight);
            Widgets.Label(labelRect3, sexDriveLabel);
            Rect sliderRect3 = new Rect(labelRect3.xMax, labelRect2.yMax, sliderWidth, sexualityRowHeight);
            if (editSexualityOn)
            {
                float newDrive = Widgets.HorizontalSlider(sliderRect3, sexuality.SexDrive, 0f, 1f, true, null, null, (2f * sexuality.SexDrive).ToString("F2"));
                if (newDrive != sexuality.SexDrive) sexuality.SetSexdrive(newDrive);
            }
            else
            {
                Rect sexDriveRect = new Rect(sliderRect3.x, sliderRect3.y + (sexualityRowHeight - sexualityBarHeight) / 2f, sliderWidth, sexualityBarHeight);
                Widgets.DrawBoxSolid(sexDriveRect, barBackgroundColor);
                float sexDrive = sexuality.SexDrive;
                Rect sdValueRect = new Rect(sexDriveRect.x, sexDriveRect.y, sexDrive * sexDriveRect.width, sexualityBarHeight);
                Color sdColor = sexDrive <= 0.5f
                    ? Color.Lerp(LowLibidoBarColor, HighLibidoBarColor, sexDrive * 2f)
                    : Color.Lerp(HighLibidoBarColor, HyperLibidoBarColor, (sexDrive - 0.5f) * 2f);

                Widgets.DrawBoxSolid(sdValueRect, sdColor);
                Widgets.DrawLineVertical(sexDriveRect.x + sexDriveRect.width * 0.5f, sexDriveRect.y - 1, sexDriveRect.height + 2);
            }

            Text.Anchor = oldAnchor;
            Text.Font = oldFont;
        }
        public static void DrawPreferenceEditCard(Rect rect, bool allowEdit, Pawn pawn, CompPsyche compPsyche)
        {
            var psycheEnabled = compPsyche?.Enabled == true;
            TextAnchor oldAnchor = Text.Anchor;
            GameFont oldFont = Text.Font;
            Rect innerRect = rect.ContractedBy(innerPadding);

            // Title
            Rect titleRect = new Rect(innerRect.x, innerRect.y, innerRect.width, titleHeight);
            Text.Anchor = TextAnchor.MiddleCenter;
            Text.Font = GameFont.Medium;
            string titleString = "RPC_Preference".Translate();
            Widgets.Label(titleRect, titleString);
            Vector2 titleTextSize = Text.CalcSize(titleString);

            // Icon on the right
            float editIconX = titleRect.x + (titleRect.width / 2f) + (titleTextSize.x / 2f) + 8f;
            Rect editIconRect = new Rect(editIconX, titleRect.y + (titleHeight - iconSize) / 2f, iconSize, iconSize);

            // Draw & handle click
            if (psycheEnabled && allowEdit)
            {
                if (Widgets.ButtonImage(editIconRect, Rimpsyche_UI_Utility.EditButton))
                {
                    editInterestOn = !editInterestOn;
                    if (!editInterestOn && showPreference)
                    {
                        resetPreferenceHeights = true;
                    }
                }
                TooltipHandler.TipRegion(editIconRect, "RimpsycheEdit".Translate());
            }

            //Mode switcher
            if (Rimpsyche.SexualityModuleLoaded && usePreference)
            {
                float viewIconX = (titleRect.xMax - innerPadding - iconSize);
                Rect viewIconRect = new Rect(viewIconX, titleRect.y + (titleRect.height - iconSize) / 2f, iconSize, iconSize);

                // Draw & handle click
                if (Widgets.ButtonImage(viewIconRect, showPreference ? Rimpsyche_UI_Utility.InterestButton : Rimpsyche_UI_Utility.PreferenceButton))
                {
                    showPreference = !showPreference;
                }
                TooltipHandler.TipRegion(viewIconRect, showPreference ? "RimpsycheShowInterest".Translate() : "RimpsycheShowPreference".Translate());
            }

            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;

            Rect scrollRect = new Rect(innerRect.x, titleRect.yMax + titleContentSpacing, innerRect.width, innerRect.height - (titleRect.height + titleContentSpacing));
            var allPrefDefs = DefDatabase<PreferenceDef>.AllDefsListForReading;
            if (editInterestOn || RimpsycheSettings.showDeatiledPreference)
            {
                Rect viewRect = new Rect(0f, 0f, scrollRect.width - scrollBarWidth, preferenceViewHeight);

                Widgets.BeginScrollView(scrollRect, ref InterestNodeScrollPosition, viewRect);
                float y = 0f;
                for (int i = 0; i < allPrefDefs.Count; i++)
                {
                    var pref = allPrefDefs[i];
                    if (!pref.isActive)
                        continue;
                    var worker = pref.worker;
                    var rectHeight = worker.EditorHeight;
                    Rect prefRect = new Rect(0f, y, viewRect.width, rectHeight);
                    worker.DrawEditor(prefRect, pawn, editInterestOn);
                    y += rectHeight + RimpsycheDatabase.preferenceGap;
                }
            }
            else
            {
                float totalContentHeight = 0f;
                var viewerHeights = GetViewerHeights(pawn);
                foreach (var height in viewerHeights)
                {
                    totalContentHeight += height + 5f;
                }
                Rect scrollContentRect = new Rect(0f, 0f, scrollRect.width - scrollBarWidth, totalContentHeight);
                Widgets.BeginScrollView(scrollRect, ref InterestNodeScrollPosition, scrollContentRect);
                float y = 0f;
                for (int i = 0; i < allPrefDefs.Count; i++)
                {
                    if (!allPrefDefs[i].isActive)
                        continue;
                    var worker = allPrefDefs[i].worker;
                    float viewerHeight = viewerHeights[i];
                    Rect prefExplanationRect = new Rect(0f, y, scrollRect.width, viewerHeight);
                    worker.DrawViewer(prefExplanationRect, pawn);
                    y += viewerHeight + 5f;
                }
            }
            Widgets.EndScrollView();
            Text.Anchor = oldAnchor;
            Text.Font = oldFont;
        }

        public static void DrawInterestEditCard(Rect rect, bool allowEdit, Pawn pawn, CompPsyche compPsyche, bool showSexuality)
        {
            var psycheEnabled = compPsyche?.Enabled == true;
            TextAnchor oldAnchor = Text.Anchor;
            GameFont oldFont = Text.Font;
            Rect innerRect = rect.ContractedBy(innerPadding);

            // Title
            Rect titleRect = new Rect(innerRect.x, innerRect.y, innerRect.width, titleHeight);
            Text.Anchor = TextAnchor.MiddleCenter;
            Text.Font = GameFont.Medium;
            string titleString = "RPC_Interest".Translate();
            Widgets.Label(titleRect, titleString);
            Vector2 titleTextSize = Text.CalcSize(titleString);

            // Icon on the right
            float editIconX = titleRect.x + (titleRect.width / 2f) + (titleTextSize.x / 2f) + 8f;
            Rect editIconRect = new Rect(editIconX, titleRect.y + (titleHeight - iconSize) / 2f, iconSize, iconSize);

            // Draw & handle click
            if (psycheEnabled && allowEdit)
            {
                if (Widgets.ButtonImage(editIconRect, Rimpsyche_UI_Utility.EditButton))
                {
                    editInterestOn = !editInterestOn;
                }
                TooltipHandler.TipRegion(editIconRect, "RimpsycheEdit".Translate());
            }

            //Mode switcher
            if (Rimpsyche.SexualityModuleLoaded && usePreference && showSexuality)
            {
                float viewIconX = (titleRect.xMax - innerPadding - iconSize);
                Rect viewIconRect = new Rect(viewIconX, titleRect.y + (titleRect.height - iconSize) / 2f, iconSize, iconSize);

                // Draw & handle click
                if (Widgets.ButtonImage(viewIconRect, showPreference ? Rimpsyche_UI_Utility.InterestButton : Rimpsyche_UI_Utility.PreferenceButton))
                {
                    showPreference = !showPreference;
                }
                TooltipHandler.TipRegion(viewIconRect, showPreference ? "RimpsycheShowInterest".Translate() : "RimpsycheShowPreference".Translate());
            }

            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;

            Rect scrollRect = new Rect(innerRect.x, titleRect.yMax + titleContentSpacing, innerRect.width, innerRect.height - (titleRect.height + titleContentSpacing));
            Rect viewRect = new Rect(0f, 0f, scrollRect.width - scrollBarWidth, interestViewHeight);
            var interestBarWidth = viewRect.width - interestLabelWidth - 2 * interestLabelPadding;

            Widgets.BeginScrollView(scrollRect, ref InterestNodeScrollPosition, viewRect);
            float y = 0f;
            foreach (var interest in RimpsycheDatabase.InterestList)
            {
                float currentValue = compPsyche.Interests.GetOrGenerateAdjustedInterestScoreRaw(interest);
                Rect rowRect = new Rect(0f, y, viewRect.width, interestRowHeight);

                // Hover highlight + tooltip
                if (Mouse.IsOver(rowRect))
                {
                    Widgets.DrawHighlight(rowRect);
                    TooltipHandler.TipRegion(rowRect, $"{interest.label}: {Math.Round(currentValue, 1)}\n{interest.description}\n\n{"RimpsycheTopicHeader".Translate()}\n{RimpsycheDatabase.InterstTopicStringDict[interest.id]}");
                }
                float centerY = rowRect.y + rowRect.height / 2f;

                // Left label
                Rect leftRect = new Rect(rowRect.x + interestLabelPadding, centerY - Text.LineHeight / 2f, interestLabelWidth, Text.LineHeight);
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(leftRect, interest.label);
                if (editInterestOn)
                {
                    float minValue = 0f;
                    float maxValue = 100f;
                    Rect sliderRect = new Rect(leftRect.x + interestLabelWidth, centerY - interestBarHeight / 2f, interestBarWidth, interestRowHeight);
                    float newValue = Widgets.HorizontalSlider(sliderRect, currentValue, minValue, maxValue);
                    if (newValue != currentValue)
                    {
                        compPsyche.Interests.SetInterestScore(interest, newValue);
                    }
                }
                else
                {
                    Rect barRect = new Rect(leftRect.x + interestLabelWidth, centerY - interestBarHeight / 2f, interestBarWidth, interestBarHeight);
                    Widgets.DrawBoxSolid(barRect, barBackgroundColor);

                    float normalizedValue = currentValue * 0.01f;
                    float fillWidth = normalizedValue * interestBarWidth;
                    Rect valueRect = new Rect(barRect.x, barRect.y, fillWidth, interestBarHeight);
                    Color barColor = Color.Lerp(LowInterestColor, HighInterestColor, normalizedValue);
                    Widgets.DrawBoxSolid(valueRect, barColor);
                }

                y += interestRowHeight;
            }

            Widgets.EndScrollView();
            Text.Anchor = oldAnchor;
            Text.Font = oldFont;
        }

        public static void DrawPersonalityEditcard(Rect rect, bool allowEdit, Pawn pawn, CompPsyche compPsyche)
        {
            var psycheEnabled = compPsyche?.Enabled == true;
            var scope = compPsyche.Personality.scopeCache;
            TextAnchor oldAnchor = Text.Anchor;
            GameFont oldFont = Text.Font;
            Rect innerRect = rect.ContractedBy(innerPadding);
            if (!RimpsycheSettings.ShowDispositionInUI && showMode == ShowMode.Behavior)
            {
                showMode = ShowMode.Personality;
            }

            // Title
            Rect titleRect = new Rect(innerRect.x, innerRect.y, innerRect.width, titleHeight);
            Text.Anchor = TextAnchor.MiddleCenter;
            Text.Font = GameFont.Medium;
            string titleString;
            if (showMode == ShowMode.Personality)
                titleString = "RPC_Personality".Translate();
            else
                titleString = "RPC_Disposition".Translate();
            Widgets.Label(titleRect, titleString);
            Vector2 titleTextSize = Text.CalcSize(titleString);
            Rect resetButtonRect = new Rect(
                innerRect.x + resetButtonMargin,
                titleRect.y + (titleRect.height - resetButtonSize) / 2f,
                resetButtonSize,
                resetButtonSize
            );
            if (!RimpsycheSettings.showFacetInUI && psycheEnabled && allowEdit)
            {
                if (Widgets.ButtonImage(resetButtonRect, Rimpsyche_UI_Utility.resetIcon))
                {
                    compPsyche.Personality.Initialize();
                }
                TooltipHandler.TipRegion(resetButtonRect, "ResetPsycheTooltip".Translate());
            }

            // Icon on the right
            float iconX = titleRect.x + (titleRect.width / 2f) + (titleTextSize.x / 2f) + 8f;
            Rect infoIconRect = new Rect(iconX, titleRect.y + (titleHeight - iconSize) / 2f, iconSize, iconSize);

            // Draw & handle click
            if (Mouse.IsOver(infoIconRect))
            {
                GUI.DrawTexture(infoIconRect, Rimpsyche_UI_Utility.InfoHLButton);
            }
            else
            {
                GUI.DrawTexture(infoIconRect, Rimpsyche_UI_Utility.InfoButton);
            }
            TooltipHandler.TipRegion(infoIconRect, "RimpsychePersonalityInfo".Translate());

            iconX = infoIconRect.xMax + iconSpacing;
            if (RimpsycheSettings.ShowDispositionInUI)
            {
                Rect modeChangeIcon = new Rect(iconX, titleRect.y + (titleHeight - iconSize) / 2f, iconSize, iconSize);
                var (icon, modeTooltipKey, nextMode) = showMode switch
                {
                    ShowMode.Personality => (Rimpsyche_UI_Utility.ViewBehaviorButton, "RimpsycheShowDisposition", ShowMode.Behavior),
                    ShowMode.Behavior => (RimpsycheSettings.personalityAsBar ? Rimpsyche_UI_Utility.ViewBarButton : Rimpsyche_UI_Utility.ViewListButton, "RimpsycheShowPersonality", ShowMode.Personality),
                    _ => (RimpsycheSettings.personalityAsBar ? Rimpsyche_UI_Utility.ViewBarButton : Rimpsyche_UI_Utility.ViewListButton, "RimpsycheShowPersonality", ShowMode.Personality)
                };
                if (Widgets.ButtonImage(modeChangeIcon, icon))
                {
                    showMode = nextMode;
                    PersonalityNodeScrollPosition = Vector2.zero;
                }
                TooltipHandler.TipRegion(modeChangeIcon, modeTooltipKey.Translate());
                iconX = modeChangeIcon.xMax + iconSpacing;
            }

            Rect editIconRect = new Rect(iconX, titleRect.y + (titleHeight - iconSize) / 2f, iconSize, iconSize);

            // Draw & handle click
            if (psycheEnabled && allowEdit)
            {
                if (Widgets.ButtonImage(editIconRect, Rimpsyche_UI_Utility.EditButton))
                {
                    editPersonalityOn = !editPersonalityOn;
                    if(!editPersonalityOn)
                    {
                        PersonalityViewCacheClean();
                    }
                    if (showMode == ShowMode.Behavior) PersonalityNodeScrollPosition = Vector2.zero;
                }
                TooltipHandler.TipRegion(editIconRect, "RimpsycheEdit".Translate());
            }

            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;

            if (!RimpsycheSettings.showFacetInUI && allowEdit)
            {
                Rect saveButtonRect = new Rect(
                    titleRect.xMax - resetButtonSize - resetButtonMargin - scrollBarWidth,
                    titleRect.y + (titleRect.height - resetButtonSize) / 2f,
                    resetButtonSize,
                    resetButtonSize
                );
                if (Widgets.ButtonImage(saveButtonRect, Rimpsyche_UI_Utility.SaveLoadButton))
                {
                    ShowSlotSelectMenu(pawn);
                }
                TooltipHandler.TipRegion(saveButtonRect, "SavePsycheTooltip".Translate());
            }

            // Scroll view
            var personalityList = GetPersonalityData(compPsyche, pawn);
            var behaviorsList = GetBehaviorData(compPsyche, pawn);
            var viewHeight = showMode switch
            {
                ShowMode.Personality => personalityViewHeight,
                ShowMode.Behavior => behaviorsList.Count() * personalityRowHeight + 3f,
                _ => personalityViewHeight,
            };
            Rect scrollRect = new Rect(innerRect.x, titleRect.yMax + titleContentSpacing, innerRect.width, innerRect.height - (titleRect.height + titleContentSpacing));
            Rect viewRect = new Rect(0f, 0f, scrollRect.width - scrollBarWidth, viewHeight);

            Widgets.BeginScrollView(scrollRect, ref PersonalityNodeScrollPosition, viewRect);

            float y = 0f;
            if (showMode == ShowMode.Personality || (psycheEnabled && editPersonalityOn))
            {
                float barCenterX = viewRect.width * 0.5f;
                for (int i = 0; i < personalityList.Count; i++)
                {
                    var pData = personalityList[i];
                    var def = pData.Personality;
                    float currentValue = compPsyche.Personality.GetPersonalityDirect(def);
                    var (leftLabel, rightLabel) = (def.low.CapitalizeFirst(), def.high.CapitalizeFirst());

                    Rect rowRect = new Rect(0f, y, viewRect.width, personalityRowHeight);

                    if (Mouse.IsOver(rowRect))
                    {
                        if (Mouse.IsOver(rowRect))
                        {
                            Widgets.DrawHighlight(rowRect);
                            int uniqueId = i;
                            TipSignal tip = new TipSignal(() => Event.current.shift ? pData.CachedFullDescription : pData.CachedDescription, uniqueId);
                            TooltipHandler.TipRegion(rowRect, tip);
                        }
                    }
                    float centerY = rowRect.y + rowRect.height / 2f;
                    // Left label
                    Rect leftRect = new Rect(rowRect.x + personalityLabelPadding, centerY - Text.LineHeight / 2f, personalityLabelWidth, Text.LineHeight);
                    Text.Anchor = TextAnchor.MiddleLeft;
                    Widgets.Label(leftRect, leftLabel);

                    // Right label
                    Rect rightRect = new Rect(rowRect.xMax - personalityLabelWidth - personalityLabelPadding, centerY - Text.LineHeight / 2f, personalityLabelWidth, Text.LineHeight);
                    Text.Anchor = TextAnchor.MiddleRight;
                    Widgets.Label(rightRect, rightLabel);
                    if (psycheEnabled && editPersonalityOn)
                    {
                        float highend = 1f;
                        float lowend = -1f;
                        if (!scope.NullOrEmpty())
                        {
                            if (scope.TryGetValue(def.shortHash, out var range))
                            {
                                (lowend, highend) = range;
                            }
                        }
                        Rect sliderRect = new Rect(barCenterX - personalityBarWidth / 2f, centerY - personalityBarHeight / 2f, personalityBarWidth, personalityRowHeight);
                        float newValue = Widgets.HorizontalSlider(sliderRect, currentValue, lowend, highend);
                        //newValue = Mathf.Clamp(newValue, lowend, highend);
                        if (newValue != currentValue)
                        {
                            compPsyche.Personality.SetPersonalityRating(def, newValue);
                        }
                    }
                    else
                    {
                        // Bar background
                        Rect barRect = new Rect(barCenterX - personalityBarWidth / 2f, centerY - personalityBarHeight / 2f, personalityBarWidth, personalityBarHeight);
                        Widgets.DrawBoxSolid(barRect, barBackgroundColor);

                        // Value bar
                        float clamped = Mathf.Clamp(currentValue, -1f, 1f);
                        float halfBar = Mathf.Abs(clamped) * (personalityBarWidth) / 2f;
                        Rect valueRect = clamped >= 0
                            ? new Rect(barCenterX, barRect.y, halfBar, personalityBarHeight)
                            : new Rect(barCenterX - halfBar, barRect.y, halfBar, personalityBarHeight);

                        // Color based on intensity (small = yellow, strong = green)
                        float intensity = Mathf.Abs(clamped) * 2f;
                        Color barColor = Color.Lerp(LowValueColor, HighValueColor, intensity);
                        Widgets.DrawBoxSolid(valueRect, barColor);
                    }

                    y += personalityRowHeight;
                }
            }
            else
            {
                Color originalColor = GUI.color;
                TextAnchor originalAnchor = Text.Anchor;
                if (behaviorsList.Count == 0)
                {
                    Rect emptyRect = new Rect(10f, y, viewRect.width - 10f, personalityRowHeight);
                    GUI.color = Color.gray;
                    Widgets.Label(emptyRect, "RPC_NoBehavior".Translate());
                    GUI.color = originalColor;
                    y += personalityRowHeight;
                }
                else
                {
                    int firstIndex = Mathf.FloorToInt(PersonalityNodeScrollPosition.y / personalityRowHeight);
                    int lastIndex = Mathf.FloorToInt((PersonalityNodeScrollPosition.y + scrollRect.height) / personalityRowHeight);
                    firstIndex = Mathf.Clamp(firstIndex, 0, behaviorsList.Count - 1);
                    lastIndex = Mathf.Clamp(lastIndex, 0, behaviorsList.Count - 1);
                    for (int i = firstIndex; i <= lastIndex; i++)
                    {
                        var entry = behaviorsList[i];
                        y = i * personalityRowHeight;
                        Rect outerRowRect = new Rect(0f, y, viewRect.width, personalityRowHeight);
                        Rect rowRect = new Rect(personalityLabelPadding, y, outerRowRect.width - 2 * personalityLabelPadding, personalityRowHeight);
                        Rect intensityRect = new Rect(rowRect.xMax - intensityRectWidth, rowRect.y, intensityRectWidth, rowRect.height);
                        Widgets.DrawHighlightIfMouseover(outerRowRect);

                        if (!entry.IsSignificant)
                            GUI.color = lightGreyColor;
                        else
                            GUI.color = Color.white;
                        Text.Anchor = TextAnchor.MiddleLeft;
                        Widgets.Label(rowRect, entry.Label);
                        Widgets.Label(intensityRect, entry.Intensity);

                        // Dynamic breakdown tooltip containing what personality properties formed this descriptor
                        TooltipHandler.TipRegion(outerRowRect, entry.Tooltip);
                    }
                }
                GUI.color = originalColor;
                Text.Anchor = originalAnchor;
            }
            Widgets.EndScrollView();
            Text.Anchor = oldAnchor;
            Text.Font = oldFont;
        }

        public static void DrawFacetCard(Rect rect, bool allowEdit, Pawn pawn, CompPsyche compPsyche)
        {
            var psycheEnabled = compPsyche?.Enabled == true;
            var gate = compPsyche.Personality.gateCache;
            Text.Font = GameFont.Small;
            TextAnchor oldAnchor = Text.Anchor;
            Rect innerRect = rect.ContractedBy(innerPadding);

            // Title for the Facet Card
            Text.Font = GameFont.Medium;
            Rect titleRect = new Rect(innerRect.x, innerRect.y, innerRect.width, 35f);
            Text.Anchor = TextAnchor.MiddleCenter;
            string titleString = "RPC_Facets".Translate();
            Widgets.Label(titleRect, titleString);
            Vector2 titleTextSize = Text.CalcSize(titleString);
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;

            // Left Rest button
            Rect resetButtonRect = new Rect(
                innerRect.x + resetButtonMargin,
                titleRect.y + (titleRect.height - resetButtonSize) / 2f,
                resetButtonSize,
                resetButtonSize
            );
            if (psycheEnabled && allowEdit)
            {
                if (Widgets.ButtonImage(resetButtonRect, Rimpsyche_UI_Utility.resetIcon))
                {
                    compPsyche.Personality.Initialize();
                }
                TooltipHandler.TipRegion(resetButtonRect, "ResetPsycheTooltip".Translate());
            }

            // Icon on the right
            float infoIconX = titleRect.x + (titleRect.width / 2f) + (titleTextSize.x / 2f) + 8f;
            Rect infoIconRect = new Rect(infoIconX, titleRect.y + (titleHeight - iconSize) / 2f, iconSize, iconSize);

            // Draw & handle click
            if (Mouse.IsOver(infoIconRect))
            {
                GUI.DrawTexture(infoIconRect, Rimpsyche_UI_Utility.InfoHLButton);
            }
            else
            {
                GUI.DrawTexture(infoIconRect, Rimpsyche_UI_Utility.InfoButton);
            }
            TooltipHandler.TipRegion(infoIconRect, "RimpsycheFacetInfo".Translate());


            Rect editIconRect = new Rect(infoIconRect.xMax + iconSpacing, titleRect.y + (titleHeight - iconSize) / 2f, iconSize, iconSize);

            // Draw & handle click
            if (psycheEnabled && RimpsycheSettings.allowFacetEdit && allowEdit)
            {
                if (Widgets.ButtonImage(editIconRect, Rimpsyche_UI_Utility.EditButton))
                {
                    editFacetOn = !editFacetOn;
                }
                TooltipHandler.TipRegion(editIconRect, "RimpsycheEdit".Translate());
            }

            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;
            if (allowEdit)
            {
                Rect saveButtonRect = new Rect(
                    titleRect.xMax - resetButtonSize - resetButtonMargin - scrollBarWidth,
                    titleRect.y + (titleRect.height - resetButtonSize) / 2f,
                    resetButtonSize,
                    resetButtonSize
                );
                if (Widgets.ButtonImage(saveButtonRect, Rimpsyche_UI_Utility.SaveLoadButton))
                {
                    ShowSlotSelectMenu(pawn);
                }
                TooltipHandler.TipRegion(saveButtonRect, "SavePsycheTooltip".Translate());
            }

            Rect viewRect = new Rect(0f, 0f, innerRect.width - scrollBarWidth, facetViewHeight);
            Rect scrollRect = new Rect(innerRect.x, titleRect.yMax + 5f, innerRect.width, innerRect.height - (titleRect.height + 5f));
            Widgets.BeginScrollView(scrollRect, ref FacetNodeScrollPosition, viewRect);

            float y = 0f;
            foreach (Facet facet in RimpsycheDatabase.AllFacets)
            {
                var value = compPsyche.Personality.GetFacetValue(facet);
                var (facetlabel, leftLabel, rightLabel, lefColor, rightColor) = InterfaceComponents.FacetNotation[facet];
                Rect rowRect = new Rect(0f, y, viewRect.width, facetRowHeight);
                if (Mouse.IsOver(rowRect))
                {
                    Widgets.DrawHighlight(rowRect);
                    string tooltipString = $"{facetlabel}: {(value * 2f).ToString("F1")}\n\n{InterfaceComponents.FacetDescription[facet]}";
                    if (compPsyche.Personality.gateInfoCache.TryGetValue(facet, out string explanation))
                    {
                        tooltipString += $"\n\n{explanation}";
                    }
                    TooltipHandler.TipRegion(rowRect, tooltipString);
                }

                float barCenterX = rowRect.x + rowRect.width / 2f;
                float centerY = rowRect.y + rowRect.height / 2f;

                // Left label
                Rect leftRect = new Rect(rowRect.x + facetLabelPadding, centerY - Text.LineHeight / 2f, facetLabelWidth, Text.LineHeight);
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(leftRect, leftLabel);

                // Right label
                Rect rightRect = new Rect(rowRect.xMax - facetLabelWidth - facetLabelPadding, centerY - Text.LineHeight / 2f, facetLabelWidth, Text.LineHeight);
                Text.Anchor = TextAnchor.MiddleRight;
                Widgets.Label(rightRect, rightLabel);

                if (psycheEnabled && editFacetOn)
                {
                    float highend;
                    float lowend;
                    if (!gate.NullOrEmpty() && gate.TryGetValue(facet, out var range))
                    {
                        (lowend, highend) = range;
                    }
                    else
                    {
                        highend = 50f;
                        lowend = -50f;
                    }
                    //Rect sliderRect = new Rect(barCenterX + barWidth / 2f * lowend , centerY - barHeight / 2f, barWidth*(highend-lowend)*0.5f, 24f);?
                    Rect sliderRect = new Rect(barCenterX - (facetBarWidth) / 2f, centerY - facetBarHeight / 2f, facetBarWidth, facetRowHeight);
                    float newValue = Widgets.HorizontalSlider(sliderRect, value, lowend, highend);
                    //newValue = Mathf.Clamp(newValue, lowend, highend);
                    if (newValue != value)
                    {
                        if (highend != 50f || lowend != 50f)
                        {
                            newValue = Rimpsyche_Utility.RestoreGatedValue(newValue, lowend, highend);
                        }
                        if (compPsyche.Personality.SetFacetValue(facet, newValue))
                        {
                            compPsyche.Personality.DirtyCache();
                        }
                    }

                }
                else
                {
                    // Bar (centered vertically)
                    Rect barRect = new Rect(barCenterX - facetBarWidth / 2f, centerY - facetBarHeight / 2f, facetBarWidth, facetBarHeight);
                    Widgets.DrawBoxSolid(barRect, barBackgroundColor);

                    // Value bar
                    float halfBar = (Mathf.Abs(value) / 50f) * (facetBarWidth / 2f);
                    Rect valueRect;

                    if (value >= 0)
                    {
                        valueRect = new Rect(barCenterX, barRect.y, halfBar, facetBarHeight);
                    }
                    else
                    {
                        valueRect = new Rect(barCenterX - halfBar, barRect.y, halfBar, facetBarHeight);
                    }

                    // Color gradient: red → green
                    Color barColor = Color.Lerp(lefColor, rightColor, (value + 50f) / 100f);
                    Widgets.DrawBoxSolid(valueRect, barColor);
                }

                y += facetRowHeight;
            }

            Widgets.EndScrollView();
            Text.Anchor = oldAnchor;
        }

        private static void ShowSlotSelectMenu(Pawn pawn)
        {
            List<FloatMenuOption> options = new();

            for (int i = 0; i < PsycheSaveManager.Slots.Count; i++)
            {
                int slotIndex = i;
                string label = PsycheSaveManager.Slots[i] == null
                    ? $"Slot {i} (empty)"
                    : PsycheSaveManager.Slots[i].name;

                options.Add(new FloatMenuOption(label, () =>
                {
                    ShowSlotActionMenu(pawn, slotIndex);
                }));
            }

            Find.WindowStack.Add(new FloatMenu(options));
        }

        private static void ShowSlotActionMenu(Pawn pawn, int index)
        {
            List<FloatMenuOption> options = new();

            // Save
            if (RimpsycheSettings.confirmLoadSave && PsycheSaveManager.Slots[index] != null)
            {
                options.Add(new FloatMenuOption("SaveToSlot".Translate(), () =>
                {
                    Find.WindowStack.Add(
                        new Dialog_ConfirmClosable(
                            "ConfirmSaveToSlot".Translate(index),
                            () =>
                            {
                                PsycheSaveManager.Slots[index] = new PsycheSlot(pawn.LabelShort, PsycheDataUtil.GetPsycheData(pawn, false));
                                PsycheSaveManager.Save();
                            },
                            destructive: false
                        )
                    );
                }));
            }
            else
            {
                options.Add(new FloatMenuOption("SaveToSlot".Translate(), () =>
                {
                    PsycheSaveManager.Slots[index] = new PsycheSlot(pawn.LabelShort, PsycheDataUtil.GetPsycheData(pawn, false));
                    PsycheSaveManager.Save();
                }));
            }

            // Load
            if (PsycheSaveManager.Slots[index] != null)
            {
                if (RimpsycheSettings.confirmLoadSave)
                {
                    options.Add(new FloatMenuOption("LoadFromSlot".Translate(), () =>
                    {
                        bool randomizeSexuality = false;
                        var loadingKinsey = PsycheSaveManager.Slots[index].data.mKinsey;
                        var pawnKinsey = pawn.compPsyche().Sexuality.MKinsey;
                        if (Rimpsyche.SexualityModuleLoaded && (loadingKinsey < 0f) && (pawnKinsey >= 0f))
                            randomizeSexuality = true;
                        Find.WindowStack.Add(
                            new Dialog_ConfirmClosable(
                                (randomizeSexuality ? "ConfirmLoadFromSlot_RandomOrientation" : "ConfirmLoadFromSlot").Translate(index, PsycheSaveManager.Slots[index].name),
                                () =>
                                {
                                    PsycheDataUtil.InjectPsycheData(pawn, PsycheSaveManager.Slots[index].data, false, true);
                                },
                                destructive: false
                            )
                        );
                    }));
                }
                else
                {
                    options.Add(new FloatMenuOption("LoadFromSlot".Translate(), () =>
                    {
                        PsycheDataUtil.InjectPsycheData(pawn, PsycheSaveManager.Slots[index].data, false, true);
                    }));
                }
            }

            // Delete
            if (PsycheSaveManager.Slots[index] != null)
            {
                if (RimpsycheSettings.confirmLoadSave)
                {
                    options.Add(new FloatMenuOption("DeleteSlot".Translate(), () =>
                    {
                        Find.WindowStack.Add(
                            new Dialog_ConfirmClosable(
                                "ConfirmDeleteSlot".Translate(index),
                                () =>
                                {
                                    PsycheSaveManager.Slots[index] = null;
                                    PsycheSaveManager.Save();
                                },
                                destructive: true
                            )
                        );
                    }));
                }
                else
                {
                    options.Add(new FloatMenuOption("DeleteSlot".Translate(), () =>
                    {
                        PsycheSaveManager.Slots[index] = null;
                        PsycheSaveManager.Save();
                    }));
                }
            }
            Find.WindowStack.Add(new FloatMenu(options));
        }

        public override void PostClose()
        {
            base.PostClose();
            PsycheInfoCard.CacheClean();
            PersonalityViewCacheClean();
        }
        private static void PersonalityViewCacheClean()
        {
            cachedPersonalityData = null;
            cachedBehaviorData = null;
            cachedPersonalityEffects.Clear();
            foreach (var pref in DefDatabase<PreferenceDef>.AllDefsListForReading)
            {
                pref.worker.ClearEditorCache();
            }
        }
    }

    public class Dialog_ConfirmClosable : Dialog_MessageBox
    {
        public Dialog_ConfirmClosable(TaggedString text, Action confirmedAction, bool destructive = false, string title = null, WindowLayer layer = WindowLayer.Dialog)
            : base(text, "Confirm".Translate(), confirmedAction, "GoBack".Translate(), null, title, destructive, confirmedAction, null, layer)
        {
            closeOnClickedOutside = true;
            absorbInputAroundWindow = false;
        }
    }
}
