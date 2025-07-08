using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Maux36.RimPsyche
{
    public class PsycheInfoCard
    {
        // Constants and style settings
        public static Rect PsycheRect = new Rect(0f, 0f, Mathf.Clamp(UI.screenWidth * 0.5f, 450f, 550f) , Mathf.Clamp(UI.screenHeight*0.5f,350f, 450f));
        public static GUIStyle style;
        public static Vector2 PersonalityScrollPosition = Vector2.zero;
        public static Vector2 InterestScrollPosition = Vector2.zero;

        public static readonly float expandButtonSize = 8f;
        public static readonly float rightPanelWidthConstant = 250f;
        public static readonly Color LineColor = new Color(97f, 108f, 122f, 0.25f);
        public static readonly float headerHeight = 35f;
        public static readonly float labelPadding = 2f;
        public static readonly float innerPadding = 5f;
        public static readonly float scrollWidth = 15f;

        //Cache
        private static List<PersonalityDisplayData> cachedPersonalityData = new List<PersonalityDisplayData>();
        private static List<InterestDisplayData> cachedInterestData = new List<InterestDisplayData>();
        private static Pawn lastPawn;
        private struct PersonalityDisplayData
        {
            public PersonalityDef Personality;
            public float Value;
            public float AbsValue;
            public string CachedLabelText;
            public string CachedDescription;
            public Color CachedLabelColor;
        }
        private struct InterestDisplayData
        {
            public Interest Interest;
            public float Value;
            public string CachedLabelText;
            public string CachedDescription;
            public Color CachedLabelColor;
        }

        public static void CacheClean()
        {
            cachedPersonalityData = null;
        }

        //Options
        public static bool rightPanelVisible = false;
        public static bool showBothSide = false;
        public static bool showNumbers = false;


        public static void DrawPsycheCard(Rect totalRect, Pawn pawn)
        {
            totalRect.width -= (rightPanelVisible ? 0f : rightPanelWidthConstant);
            var compPsyche = pawn.compPsyche();
            bool showSexuality = compPsyche.Sexuality.ShowOnUI();

             // Save state           
            TextAnchor oldAnchor = Text.Anchor;
            GameFont oldFont = Text.Font;

            // Setup font style
            Text.Font = GameFont.Small;
            GUI.color = Color.white;
            style = Text.fontStyles[1];

            // All drawing will happen within this group
            GUI.BeginGroup(totalRect);
            totalRect.position = Vector2.zero;

            // Layout constants
            float rightPanelWidth = rightPanelVisible ? rightPanelWidthConstant : 0f;
            float rightTopPanelHeight = 0f;
            if (showSexuality)
            {
                rightTopPanelHeight = 150f;
            }

            // Define the sexuality panel rect
            Rect sexualityRect = new Rect(
                totalRect.xMax - rightPanelWidth,
                totalRect.y,
                rightPanelWidth,
                rightTopPanelHeight
            );

            // Define the interest panel rect
            Rect interestRect = new Rect(
                totalRect.xMax - rightPanelWidth,
                sexualityRect.y + rightTopPanelHeight,
                rightPanelWidth,
                totalRect.height - rightTopPanelHeight
            );

            // Define the personality panel rect
            Rect personalityRect = totalRect;
            personalityRect.xMax = sexualityRect.x;

            // Draw separating lines between personality & sexuality sections
            if (rightPanelVisible)
            {
                GUI.color = LineColor;
                Widgets.DrawLineVertical(personalityRect.xMax - 1, totalRect.y + 1, totalRect.height - 2); // Vertical divider
                if (showSexuality)
                {
                    Widgets.DrawLineHorizontal(personalityRect.xMax, rightTopPanelHeight, totalRect.width - personalityRect.xMax-1); // Horizontal divider
                }
                GUI.color = Color.white;
            }

            // Draw Expanding Button
            personalityRect.xMax -= expandButtonSize;
            Rect openButtonRect = new Rect(
                personalityRect.xMax-expandButtonSize/2, // Center the button in the buttonAreaWidth
                totalRect.y + (totalRect.height / 2) - (expandButtonSize / 2), // Vertically center the button
                expandButtonSize,
                expandButtonSize
            );
            if (rightPanelVisible)
            {
                if (Widgets.ButtonImage(openButtonRect, Rimpsyche_UI_Utility.HideButton))
                {
                    rightPanelVisible = !rightPanelVisible;
                }
            }
            else
            {
                if (Widgets.ButtonImage(openButtonRect, Rimpsyche_UI_Utility.RevealButton))
                {
                    rightPanelVisible = !rightPanelVisible;
                }
            }


            if (showSexuality)
            {
                sexualityRect = sexualityRect.ContractedBy(innerPadding);
            }
            interestRect = interestRect.ContractedBy(innerPadding);
            personalityRect = personalityRect.ContractedBy(innerPadding); // Add padding

            // === Draw content ===
            DrawPersonalityBox(personalityRect, compPsyche, pawn);
            if (rightPanelVisible)
            {
                if (showSexuality)
                {
                    DrawSexaulityBox(sexualityRect, compPsyche);
                }
                DrawInterestBox(interestRect, compPsyche);
            }
            

            // === End group and restore state ===
            GUI.EndGroup();
        }

        private static List<PersonalityDisplayData> GetSortedPersonalityData(CompPsyche compPsyche, Pawn currentPawn)
        {
            if (currentPawn == lastPawn && cachedPersonalityData != null)
            {
                return cachedPersonalityData;
            }

            lastPawn = currentPawn;

            var personalityDefList = DefDatabase<PersonalityDef>.AllDefs;
            var sortedData = new List<PersonalityDisplayData>();

            foreach (var personality in personalityDefList)
            {
                float value = compPsyche.Personality.GetPersonality(personality);
                float absValue = Mathf.Abs(value);

                string cachedLabelText = "";
                Color cachedLabelColor = Color.white;

                string intensityKey = "RimPsycheIntensityNeutral";
                if (absValue >= 0.75f)
                {
                    intensityKey = "RimPsycheIntensityExtremely";
                }
                else if (absValue >= 0.5f)
                {
                    intensityKey = "RimPsycheIntensityVery";
                }
                else if (absValue >= 0.25f)
                {
                    intensityKey = "RimPsycheIntensitySomewhat";
                }
                else if (absValue > 0f)
                {
                    intensityKey = "RimPsycheIntensityMarginally";
                }

                string personalityName = (value >= 0) ? personality.high : personality.low;

                if (LanguageDatabase.activeLanguage.HaveTextForKey(intensityKey))
                {
                    cachedLabelText = intensityKey.Translate(personalityName);
                }
                else
                {
                    cachedLabelText = RimpsycheDatabase.IntensityKeysDefault[intensityKey] + " " + personalityName;
                }
                cachedLabelColor = Color.Lerp(Color.yellow, Color.green, absValue);
                sortedData.Add(new PersonalityDisplayData
                {
                    Personality = personality,
                    Value = value,
                    AbsValue = absValue,
                    CachedLabelText = cachedLabelText,
                    CachedLabelColor = cachedLabelColor
                    CachedDescription = $"{personality.label}: {Math.Round(value, 1)}\n{personality.description}"
                });
            }
            sortedData = sortedData.OrderByDescending(p => p.AbsValue).ToList();
            cachedPersonalityData = sortedData;
            return sortedData;
        }

        public static readonly float personalityRowHeight = 28f;
        public static readonly float personalityBarWidth = 80f;
        public static readonly float personalityBarHeight = 4f;

        public static void DrawPersonalityBox(Rect personalityRect, CompPsyche compPsyche,  Pawn pawn)
        {
            TextAnchor oldAnchor = Text.Anchor;
            GameFont oldFont = Text.Font;

            // Draw Header
            Rect headerRect = new Rect(personalityRect.x, personalityRect.y, personalityRect.width, headerHeight);
            GUI.BeginGroup(headerRect);
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleCenter;
            Rect titleRect = new Rect(0f, 0f, headerRect.width, headerRect.height);
            Widgets.Label(titleRect, "Personality".Translate());
            Vector2 titleTextSize = Text.CalcSize("Personality".Translate());

            // Icon on the right
            float iconSize = 24f;
            float spacing = 2;
            float viewIconX = (headerRect.width / 2f) + (titleTextSize.x / 2f) + 8f;
            Rect viewIconRect = new Rect(viewIconX, (headerHeight - iconSize) / 2f, iconSize, iconSize);

            // Draw & handle click
            if (Widgets.ButtonImage(viewIconRect, Rimpsyche_UI_Utility.ViewButton))
            {
                showBothSide = !showBothSide;
            }
            TooltipHandler.TipRegion(viewIconRect, "RimpsycheView");

            Rect editIconRect = new Rect(viewIconRect.xMax + spacing, (headerHeight - iconSize) / 2f, iconSize, iconSize);
            if (Prefs.DevMode)
            {
                if (Widgets.ButtonImage(editIconRect, Rimpsyche_UI_Utility.EditButton))
                {
                    Find.WindowStack.Add(new PsycheEditPopup(pawn));
                }
                TooltipHandler.TipRegion(editIconRect, "RimpsycheEdit");
            }

            GUI.EndGroup();

            // Scroll View Setup
            Text.Font = GameFont.Small;
            var personalitiesToDisplay = GetSortedPersonalityData(compPsyche, pawn);

            float viewHeight = personalitiesToDisplay.Count() * personalityRowHeight + 3f;
            Rect scrollContentRect = new Rect(0f, 0f, personalityRect.width - scrollWidth, viewHeight);

            Rect scrollRect = new Rect(
                personalityRect.x,
                personalityRect.y + headerHeight,
                personalityRect.width,
                personalityRect.height - headerHeight
            );

            Widgets.BeginScrollView(scrollRect, ref PersonalityScrollPosition, scrollContentRect);

            float y = 0f;

            if (showBothSide)
            {
                foreach (var pData in personalitiesToDisplay)
                {
                    var personality = pData.Personality;
                    var value = pData.Value;
                    var (leftLabel, rightLabel, leftColor, rightColor) = (personality.low, personality.high, Color.red, Color.green);

                    Rect rowRect = new Rect(0f, y, scrollContentRect.width, rowHeight);

                    // Hover highlight + tooltip
                    if (Mouse.IsOver(rowRect))
                    {
                        Widgets.DrawHighlight(rowRect);
                        TooltipHandler.TipRegion(rowRect, CachedDescription);
                    }

                    float labelWidth = 130f;
                    float barCenterX = rowRect.x + rowRect.width / 2f;
                    float centerY = rowRect.y + rowRect.height / 2f;
                    float textY = centerY - Text.LineHeight / 2f;
                    float barY = centerY - personalityBarHeight / 2f

                    // Left label
                    Rect leftRect = new Rect(rowRect.x + labelPadding, textY, labelWidth, Text.LineHeight);
                    Text.Anchor = TextAnchor.MiddleLeft;
                    Widgets.Label(leftRect, leftLabel);

                    // Right label
                    Rect rightRect = new Rect(rowRect.xMax - labelWidth - labelPadding, textY, labelWidth, Text.LineHeight);
                    Text.Anchor = TextAnchor.MiddleRight;
                    Widgets.Label(rightRect, rightLabel);

                    // Bar background
                    Rect barRect = new Rect(barCenterX - personalityBarWidth / 2f, barY, personalityBarWidth, personalityBarHeight);
                    Widgets.DrawBoxSolid(barRect, new Color(0.2f, 0.2f, 0.2f, 0.5f));

                    // Value bar
                    float halfBar = pData.absValue * (personalityBarWidth) / 2f;
                    Rect valueRect = value >= 0
                        ? new Rect(barCenterX, barRect.y, halfBar, personalityBarHeight)
                        : new Rect(barCenterX - halfBar, barRect.y, halfBar, personalityBarHeight);

                    // Color based on intensity (small = yellow, strong = green)
                    Widgets.DrawBoxSolid(valueRect, pData.cachedLabelColor);

                    y += rowHeight;
                }
            }
            else
            {
                foreach (var pData in personalitiesToDisplay)
                {
                    Rect rowRect = new Rect(0f, y, scrollContentRect.width, rowHeight);

                    // Hover highlight + tooltip
                    if (Mouse.IsOver(rowRect))
                    {
                        Widgets.DrawHighlight(rowRect);
                        TooltipHandler.TipRegion(rowRect, CachedDescription);
                    }

                    // Draw label
                    Text.Anchor = TextAnchor.MiddleLeft;
                    GUI.color = pData.CachedLabelColor;
                    Rect labelRect = new Rect(rowRect.x + labelPadding, rowRect.y, scrollContentRect.width - (2 * labelPadding), rowHeight);
                    Widgets.Label(labelRect, pData.CachedLabelText);
                    GUI.color = Color.white; // Reset color

                    y += rowHeight;
                }
            }

            Widgets.EndScrollView();

            // Restore previous text settings
            Text.Anchor = oldAnchor;
            Text.Font = oldFont;
        }

        public static void DrawSexaulityBox(Rect sexualityRect, CompPsyche compPsyche)
        {
            TextAnchor oldAnchor = Text.Anchor;
            GameFont oldFont = Text.Font;

            // === Header Config ===
            float headerHeight = 35f;
            float lineHeight = 25f; // Standard height for each line of text
            float contentStartY = sexualityRect.y + headerHeight; // Starting Y for content below header

            // === Draw Header ===
            Rect headerRect = new Rect(sexualityRect.x, sexualityRect.y, sexualityRect.width, headerHeight);
            GUI.BeginGroup(headerRect);

            // Title: "Sexuality"
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleCenter;
            Rect titleRect = new Rect(0f, 0f, headerRect.width, headerRect.height);
            Widgets.Label(titleRect, "Sexuality");

            GUI.EndGroup();

            // === Draw Details ===
            GUI.BeginGroup(new Rect(sexualityRect.x, contentStartY, sexualityRect.width, sexualityRect.height - headerHeight));

            Text.Font = GameFont.Small; // Set font for the details
            Text.Anchor = TextAnchor.MiddleLeft; // Align text to the left

            // Name
            float y = 0f;
            // Sexuality
            Rect sexualityDetailRect = new Rect(0f, y, sexualityRect.width, lineHeight);
            Widgets.Label(sexualityDetailRect, "Sexuality: " + compPsyche.Sexuality.GetOrientationCategory());
            y += lineHeight;

            Rect sexualityKinseyRect = new Rect(0f, y, sexualityRect.width, lineHeight);
            Widgets.Label(sexualityKinseyRect, "kinsey: " + (compPsyche.Sexuality.kinsey).ToString("F0"));
            y += lineHeight;

            Rect sexDriveRect = new Rect(0f, y, sexualityRect.width, lineHeight);
            Widgets.Label(sexDriveRect, "Sex Drive: " + compPsyche.Sexuality.sexDrive);
            y += lineHeight;

            //// Sexuality
            //Rect sexualityAttrMRect = new Rect(0f, y, sexualityRect.width, lineHeight);
            //Widgets.Label(sexualityAttrMRect, "Male Attraction: " + compPsyche.Sexuality.attractionM);
            //y += lineHeight;

            //Rect sexualityAttrFRect = new Rect(0f, y, sexualityRect.width, lineHeight);
            //Widgets.Label(sexualityAttrFRect, "Female Attraction: " + compPsyche.Sexuality.attractionF);
            //y += lineHeight;

            GUI.EndGroup();

            // Reset text settings
            Text.Anchor = oldAnchor;
            Text.Font = oldFont;

            Text.Anchor = oldAnchor;
            Text.Font = oldFont;
        }

        public static List<InterestDisplayData> GetSortedInterestData(CompPsyche compPsyche, Pawn currentPawn)
        {
            if (currentPawn == lastPawn && cachedInterestData != null)
            {
                return cachedInterestData;
            }

            lastPawn = currentPawn;

            var interestList = RimpsycheDatabase.InterestList;
            var sortedData = new List<InterestDisplayData>();

            foreach (var interest in interestList)
            {
                float value = compPsyche.Interests.GetOrCreateInterestScore(interest);
                float absValue = Mathf.Abs(value);

                string cachedLabelText = interest.label;
                Color cachedLabelColor = Color.Lerp(Color.yellow, Color.green, absValue);
                sortedData.Add(new InterestDisplayData
                {
                    Interest = interest,
                    Value = value,
                    AbsValue = absValue,
                    CachedLabelText = cachedLabelText,
                    CachedLabelColor = cachedLabelColor
                    CachedDescription = $"{interest.label}: {Math.Round(value, 1)}\n{interest.description}"
                });
            }
            sortedData = sortedData.OrderByDescending(p => p.AbsValue).ToList();
            cachedPersonalityData = sortedData;
            return sortedData;
        }


        public static void DrawInterestBox(Rect interestRect, CompPsyche compPsyche)
        {
            TextAnchor oldAnchor = Text.Anchor;
            GameFont oldFont = Text.Font;

            // === Header Config ===
            float headerHeight = 35f;
            float labelWidth = 150f;
            float rowHeight = 28f;
            float labelPadding = 2f;
            float barHeight = 4f;

            // === Draw Header ===
            Rect headerRect = new Rect(interestRect.x, interestRect.y, interestRect.width, headerHeight);
            GUI.BeginGroup(headerRect);

            // Title: "Interest"
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleCenter;
            Rect titleRect = new Rect(0f, 0f, headerRect.width, headerRect.height);
            Widgets.Label(titleRect, "Interest");

            GUI.EndGroup();

            Text.Anchor = oldAnchor;
            Text.Font = oldFont;


            // === Scroll View Setup ===
            Text.Font = GameFont.Small;
            var interestsToDisplay = GetSortedInterestData(compPsyche, pawn);
            float viewHeight = interestsToDisplay.Count() * rowHeight + 3f;
            Rect scrollContentRect = new Rect(0f, 0f, interestRect.width - scrollWidth, viewHeight);

            Rect scrollRect = new Rect(
                interestRect.x,
                interestRect.y + headerHeight,
                interestRect.width,
                interestRect.height - headerHeight
            );

            Widgets.BeginScrollView(scrollRect, ref InterestScrollPosition, scrollContentRect);

            float y = 0f;
            float barWidth = scrollContentRect.width - labelWidth - labelPadding-5f;

            foreach (var interestData in interestsToDisplay)
            {
                var value = interestData.Value;
                Rect rowRect = new Rect(5f, y, scrollContentRect.width, rowHeight);

                // Hover highlight + tooltip
                if (Mouse.IsOver(rowRect))
                {
                    Widgets.DrawHighlight(rowRect);
                    TooltipHandler.TipRegion(rowRect, interestData.description);
                }

                float barCenterX = rowRect.x + rowRect.width / 2f;
                float centerY = rowRect.y + rowRect.height / 2f;

                // Left label
                Rect leftRect = new Rect(rowRect.x + labelPadding, centerY - Text.LineHeight / 2f, labelWidth, Text.LineHeight);
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(leftRect, interestData.CachedLabelText);

                // Bar background
                Rect barRect = new Rect(leftRect.x + labelWidth, centerY - barHeight / 2f, barWidth, barHeight);
                Widgets.DrawBoxSolid(barRect, new Color(0.2f, 0.2f, 0.2f, 0.5f));

                // Value bar
                float normalizedValue = value * 0.01f; // Normalize value to 0-1 range
                float fillWidth = normalizedValue * barWidth; // Calculate the width of the filled part
                Rect valueRect = new Rect(barRect.x, barRect.y, fillWidth, barHeight); // Bar fills from the left

                // Color based on intensity (small = yellow, strong = green)
                Widgets.DrawBoxSolid(valueRect, interestData.cachedLabelColor);

                y += rowHeight;
            }

            Widgets.EndScrollView();

            // Restore previous text settings
            Text.Anchor = oldAnchor;
            Text.Font = oldFont;
        }
    }
}
