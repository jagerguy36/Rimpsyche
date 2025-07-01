using RimWorld;
using System;
using System.Linq;
using UnityEngine;
using Verse;

namespace Maux36.RimPsyche
{
    public class PsycheInfoCard
    {
        // Constants and style settings
        public static TextAnchor OldAnchor;
        public static int OldSmallFontSize = Text.fontStyles[1].fontSize;

        public static Rect PsycheRect = new Rect(0f, 0f, Mathf.Clamp(UI.screenWidth * 0.5f, 450f, 550f) , Mathf.Clamp(UI.screenHeight*0.5f,350f, 450f));
        public static bool ShowNumbersBool = false;
        public static GUIStyle style;
        public const int OptionFontSize = 16;
        public static float expandButtonSize = 8f;

        public static readonly Color LineColor = new Color(97f, 108f, 122f, 0.25f);
        public static float BoundaryPadding = 5f;
        public static Vector2 PersonalityScrollPosition = Vector2.zero;
        public static Vector2 InterestScrollPosition = Vector2.zero;

        public static bool rightPanelVisible = false;
        public static float rightPanelWidthConstant = 250f;

        //Assets
        public static Texture2D ViewButton = ContentFinder<Texture2D>.Get("Buttons/RimpsycheView", true);
        public static Texture2D PsycheButton = ContentFinder<Texture2D>.Get("Buttons/RimpsycheEdit", true);
        public static Texture2D RevealButton = ContentFinder<Texture2D>.Get("Buttons/RimpsycheReveal", true);
        public static Texture2D HideButton = ContentFinder<Texture2D>.Get("Buttons/RimpsycheHide", true);

        public static void DrawPsycheCard(Rect totalRect, Pawn pawn)
        {
            totalRect.width -= (rightPanelVisible ? 0f : rightPanelWidthConstant);
            var compPsyche = pawn.compPsyche();
            bool showSexuality = compPsyche.Sexuality.ShowOnUI();
            // Save state
            OldAnchor = Text.Anchor;
            Text.Font = GameFont.Small;
            GUI.color = Color.white;

            // Setup font style
            style = Text.fontStyles[1];

            // All drawing will happen within this group
            GUI.BeginGroup(totalRect);
            totalRect.position = Vector2.zero;

            // === Layout constants ===
            float rightPanelWidth = rightPanelVisible ? rightPanelWidthConstant : 0f;
            float rightPanelHeight = 0f;
            if (showSexuality)
            {
                rightPanelHeight = 150f;
            }

            // === Define the sexuality (right side) panel rect ===
            Rect sexualityRect = new Rect(
                totalRect.xMax - rightPanelWidth,
                totalRect.y,
                rightPanelWidth,
                rightPanelHeight
            );

            // === Define the interest (right side) panel rect ===
            Rect interestRect = new Rect(
                totalRect.xMax - rightPanelWidth,
                sexualityRect.y + rightPanelHeight,
                rightPanelWidth,
                totalRect.height - rightPanelHeight
            );

            // === Define the personality (left side) panel rect ===
            Rect personalityRect = totalRect;
            personalityRect.xMax = sexualityRect.x;

            // === Draw separating lines between personality & sexuality sections ===
            if (rightPanelVisible)
            {
                GUI.color = LineColor;
                Widgets.DrawLineVertical(personalityRect.xMax - 1, totalRect.y + 1, totalRect.height - 2); // Vertical divider
                if (showSexuality)
                {
                    Widgets.DrawLineHorizontal(personalityRect.xMax, rightPanelHeight, totalRect.width - personalityRect.xMax-1); // Horizontal divider
                }
                GUI.color = Color.white;
            }

            // === Draw Expanding Button

            personalityRect.xMax -= expandButtonSize;
            Rect openButtonRect = new Rect(
                personalityRect.xMax-expandButtonSize/2, // Center the button in the buttonAreaWidth
                totalRect.y + (totalRect.height / 2) - (expandButtonSize / 2), // Vertically center the button
                expandButtonSize,
                expandButtonSize
            );
            if (rightPanelVisible)
            {
                if (Widgets.ButtonImage(openButtonRect, HideButton))
                {
                    rightPanelVisible = !rightPanelVisible; // Toggle visibility
                }
            }
            else
            {
                if (Widgets.ButtonImage(openButtonRect, RevealButton))
                {
                    rightPanelVisible = !rightPanelVisible; // Toggle visibility
                }
            }


            if (showSexuality)
            {
                sexualityRect = sexualityRect.ContractedBy(BoundaryPadding);
            }
            interestRect = interestRect.ContractedBy(BoundaryPadding);
            personalityRect = personalityRect.ContractedBy(BoundaryPadding); // Add padding

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

        public static void DrawPersonalityBox(Rect personalityRect, CompPsyche compPsyche,  Pawn pawn)
        {
            var personalityDefList = DefDatabase<PersonalityDef>.AllDefs;
            TextAnchor oldAnchor = Text.Anchor;
            GameFont oldFont = Text.Font;

            // === Header Config ===
            float headerHeight = 35f;
            float rowHeight = 28f;
            float labelPadding = 2f;
            float barWidth = 80f;
            float barHeight = 4f;

            // === Draw Header ===
            Rect headerRect = new Rect(personalityRect.x, personalityRect.y, personalityRect.width, headerHeight);
            GUI.BeginGroup(headerRect);

            // Title: "Personality"
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
            if (Widgets.ButtonImage(viewIconRect, ViewButton))
            {
                Log.Message("clicked view button");
            }
            TooltipHandler.TipRegion(viewIconRect, "RimpsycheView");

            float editIconX = viewIconRect.xMax + spacing;
            Rect editIconRect = new Rect(editIconX, (headerHeight - iconSize) / 2f, iconSize, iconSize);
            if (Prefs.DevMode)
            {
                if (Widgets.ButtonImage(editIconRect, PsycheButton))
                {
                    Find.WindowStack.Add(new PsycheEditPopup(pawn));
                }
                TooltipHandler.TipRegion(editIconRect, "RimpsycheEdit");
            }

            GUI.EndGroup();

            // === Scroll View Setup ===
            Text.Font = GameFont.Small;
            float viewHeight = personalityDefList.Count() * rowHeight + 3f;
            Rect scrollContentRect = new Rect(0f, 0f, personalityRect.width - 20f, viewHeight);

            Rect scrollRect = new Rect(
                personalityRect.x,
                personalityRect.y + headerHeight,
                personalityRect.width,
                personalityRect.height - headerHeight
            );

            Widgets.BeginScrollView(scrollRect, ref PersonalityScrollPosition, scrollContentRect);

            float y = 0f;

            foreach (var personality in personalityDefList)
            {
                var value = compPsyche.Personality.GetPersonality(personality);
                var (leftLabel, rightLabel, leftColor, rightColor) = (personality.low, personality.high, Color.red, Color.green);

                Rect rowRect = new Rect(0f, y, scrollContentRect.width, rowHeight);

                // Hover highlight + tooltip
                if (Mouse.IsOver(rowRect))
                {
                    Widgets.DrawHighlight(rowRect);
                    TooltipHandler.TipRegion(rowRect, $"{personality.label}: {Math.Round(value, 1)}");
                }

                float labelWidth = 130f;
                float barCenterX = rowRect.x + rowRect.width / 2f;
                float centerY = rowRect.y + rowRect.height / 2f;

                // Left label
                Rect leftRect = new Rect(rowRect.x + labelPadding, centerY - Text.LineHeight / 2f, labelWidth, Text.LineHeight);
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(leftRect, leftLabel);

                // Right label
                Rect rightRect = new Rect(rowRect.xMax - labelWidth - labelPadding, centerY - Text.LineHeight / 2f, labelWidth, Text.LineHeight);
                Text.Anchor = TextAnchor.MiddleRight;
                Widgets.Label(rightRect, rightLabel);

                // Bar background
                Rect barRect = new Rect(barCenterX - barWidth / 2f, centerY - barHeight / 2f, barWidth, barHeight);
                Widgets.DrawBoxSolid(barRect, new Color(0.2f, 0.2f, 0.2f, 0.5f));

                // Value bar
                float clamped = Mathf.Clamp(value, -1f, 1f); // now value is between -1 ~ 1
                float halfBar = Mathf.Abs(clamped) * (barWidth) / 2f;
                Rect valueRect = clamped >= 0
                    ? new Rect(barCenterX, barRect.y, halfBar, barHeight)
                    : new Rect(barCenterX - halfBar, barRect.y, halfBar, barHeight);

                // Color based on intensity (small = yellow, strong = green)
                float intensity = Mathf.Abs(clamped) * 2f; // maps 0–0.5 to 0–1
                Color barColor = Color.Lerp(Color.yellow, Color.green, intensity);
                Widgets.DrawBoxSolid(valueRect, barColor);

                y += rowHeight;
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
            float viewHeight = RimpsycheDatabase.InterestList.Count() * rowHeight + 3f;
            Rect scrollContentRect = new Rect(0f, 0f, interestRect.width - 10f, viewHeight);

            Rect scrollRect = new Rect(
                interestRect.x,
                interestRect.y + headerHeight,
                interestRect.width,
                interestRect.height - headerHeight
            );

            Widgets.BeginScrollView(scrollRect, ref InterestScrollPosition, scrollContentRect);

            float y = 0f;
            float barWidth = scrollContentRect.width - labelWidth - labelPadding-5f;

            foreach (var interest in RimpsycheDatabase.InterestList)
            {
                var value = compPsyche.Interests.GetOrCreateInterestScore(interest);
                Rect rowRect = new Rect(5f, y, scrollContentRect.width, rowHeight);

                // Hover highlight + tooltip
                if (Mouse.IsOver(rowRect))
                {
                    Widgets.DrawHighlight(rowRect);
                    TooltipHandler.TipRegion(rowRect, $"{interest.name}: {Math.Round(value, 1)}");
                }

                float barCenterX = rowRect.x + rowRect.width / 2f;
                float centerY = rowRect.y + rowRect.height / 2f;

                // Left label
                Rect leftRect = new Rect(rowRect.x + labelPadding, centerY - Text.LineHeight / 2f, labelWidth, Text.LineHeight);
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(leftRect, interest.name);

                // Bar background
                Rect barRect = new Rect(leftRect.x + labelWidth, centerY - barHeight / 2f, barWidth, barHeight);
                Widgets.DrawBoxSolid(barRect, new Color(0.2f, 0.2f, 0.2f, 0.5f));

                // Value bar
                float normalizedValue = Mathf.Clamp(value, 0f, 100f) / 100f; // Normalize value to 0-1 range
                float fillWidth = normalizedValue * barWidth; // Calculate the width of the filled part
                Rect valueRect = new Rect(barRect.x, barRect.y, fillWidth, barHeight); // Bar fills from the left

                // Color based on intensity (small = yellow, strong = green)
                Color barColor = Color.Lerp(Color.yellow, Color.green, normalizedValue);
                Widgets.DrawBoxSolid(valueRect, barColor);

                y += rowHeight;
            }

            Widgets.EndScrollView();

            // Restore previous text settings
            Text.Anchor = oldAnchor;
            Text.Font = oldFont;
        }
    }
}
