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

        public static readonly Color LineColor = new Color(1f, 1f, 1f, 0.5f);
        public static Vector2 NodeScrollPosition = Vector2.zero;
        public static float BoundaryPadding = 5f;

        public static void DrawPsycheCard(Rect totalRect, Pawn pawn)
        {
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
            float sexualityPanelWidth = 150f;
            float sexualityPanelHeight = 200f;
            float kinseyPanelWidth = 100f;
            float highlightPadding = 5f;

            // === Text sizing for Kinsey label ===
            Vector2 kinseyTextSize = Text.CalcSize("KinseyRating".Translate() + " 0");
            float actualKinseyWidth = kinseyPanelWidth;

            // === Define the Kinsey (right side) panel rect ===
            Rect kinseyRect = new Rect(
                totalRect.xMax - sexualityPanelWidth - BoundaryPadding,
                totalRect.y + BoundaryPadding,
                actualKinseyWidth,
                kinseyTextSize.y
            );

            // === Define the personality (left side) panel rect ===
            Rect personalityRect = totalRect;
            personalityRect.xMax = kinseyRect.x - highlightPadding;
            personalityRect = personalityRect.ContractedBy(BoundaryPadding); // Add padding

            // === Define other helper rects ===
            Rect forbiddenRect = new Rect(
                personalityRect.xMax,
                0f,
                totalRect.width - personalityRect.xMax,
                10f + BoundaryPadding
            );

            Rect bigFiveRect = new Rect(
                forbiddenRect.center.x - 5f, // 10f width / 2
                10f + 2f * BoundaryPadding,
                10f,
                10f
            );

            // === Adjust personalityRect again to not collide with right section ===
            personalityRect.xMax = totalRect.xMax - sexualityPanelWidth - highlightPadding - 2f * BoundaryPadding;

            // === Draw separating lines between personality & sexuality sections ===
            GUI.color = LineColor;
            Widgets.DrawLineVertical(forbiddenRect.x, totalRect.y, totalRect.height); // Vertical divider
            Widgets.DrawLineHorizontal(forbiddenRect.x, sexualityPanelHeight + BoundaryPadding, forbiddenRect.width); // Horizontal divider
            GUI.color = Color.white;

            // === Draw content ===

            // Placeholder: Draw Big Five (currently commented out)
            // DrawBigFive(pawn, bigFiveRect, forbiddenRect);

            // Draw list of personality traits
            DrawPersonalityList(personalityRect, pawn);

            // === End group and restore state ===
            GUI.EndGroup();
        }

        public static void DrawPersonalityList(Rect personalityRect, Pawn pawn)
        {
            var personalityDefList = DefDatabase<PersonalityDef>.AllDefs;
            var compPsyche = pawn.compPsyche();
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
            Widgets.Label(titleRect, "Personality");

            // Icon on the right
            Texture2D PsycheButton = ContentFinder<Texture2D>.Get("Buttons/RimpsycheIcon", true);
            float iconSize = 24f;
            Rect iconRect = new Rect(headerRect.width - iconSize - 8f, (headerHeight - iconSize) / 2f, iconSize, iconSize);

            // Draw & handle click
            if (Prefs.DevMode)
            {
                if (Widgets.ButtonImage(iconRect, PsycheButton))
                {
                    Find.WindowStack.Add(new PsycheEditPopup(pawn));
                }
                TooltipHandler.TipRegion(iconRect, "RimpsycheEdit");
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

            Widgets.BeginScrollView(scrollRect, ref NodeScrollPosition, scrollContentRect);

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
    }
}
