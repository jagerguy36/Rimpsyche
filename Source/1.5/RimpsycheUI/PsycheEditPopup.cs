using RimWorld;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Verse;
using static RimWorld.ColonistBar;
using Verse.Sound;

namespace Maux36.RimPsyche
{
    public class PsycheEditPopup(Pawn editFor) : Window
    {
        public override Vector2 InitialSize
        {
            get
            {
                // Get the current screen width and height
                float screenWidth = UI.screenWidth;
                float screenHeight = UI.screenHeight;

                // Calculate desired width and height based on screen size
                float desiredWidth = screenWidth * 0.5f;
                float desiredHeight = screenHeight * 0.5f;

                // You might want to set a minimum size to prevent it from becoming too small
                float minWidth = 800f; // Example minimum width
                float minHeight = 400f; // Example minimum height

                return new Vector2(Mathf.Max(desiredWidth, minWidth), Mathf.Max(desiredHeight, minHeight));
            }
        }
        public static Vector2 FacetNodeScrollPosition = Vector2.zero;
        public static Vector2 EditNodeScrollPosition = Vector2.zero;
        public static bool editModeOn = false;

        public override void DoWindowContents(Rect inRect)
        {
            soundClose = SoundDefOf.InfoCard_Close;
            closeOnClickedOutside = true;
            absorbInputAroundWindow = false;
            forcePause = true;
            preventCameraMotion = false;
            doCloseX = true;
            closeOnAccept = true;
            closeOnCancel = true;

            // Get pawn
            Pawn pawn = Find.WindowStack.IsOpen(typeof(Dialog_Trade)) || Current.ProgramState != ProgramState.Playing
                ? editFor
                : Find.Selector.SingleSelectedThing as Pawn;

            if (pawn == null) return;

            // Divide window into two horizontal parts: 2:3 ratio
            float totalWidth = inRect.width;
            float spacing = 10f; // Optional spacing between panels

            float leftWidth = totalWidth * 2f / 5f - spacing / 2f;
            float rightWidth = totalWidth * 3f / 5f - spacing / 2f;

            Rect leftRect = new Rect(inRect.x, inRect.y, leftWidth, inRect.height);
            Rect rightRect = new Rect(leftRect.xMax + spacing, inRect.y, rightWidth, inRect.height);

            // Left: Edit Personality
            DrawFacetCard(leftRect, pawn);

            // Right: Facet card (to be defined later)
            DrawPsycheEditcard(rightRect, pawn); // You will define this method
        }

        public static void DrawPsycheEditcard(Rect rect, Pawn pawn)
        {
            if (pawn == null) return;
            var compPsyche = pawn.compPsyche();
            if (compPsyche == null) return;

            var personalityDefList = DefDatabase<PersonalityDef>.AllDefs;
            float rowHeight = 32f;
            float viewHeight = personalityDefList.Count() * rowHeight + 40f;
            float labelPadding = 0f;
            float barWidth = 80f;
            float barHeight = 4f;

            // Define internal padding/margins if desired
            float innerPadding = 10f;
            Rect innerRect = new Rect(rect.x + innerPadding, rect.y + innerPadding,
                                      rect.width - (innerPadding * 2), rect.height - (innerPadding * 2));

            // Title
            Text.Font = GameFont.Medium;
            // titleRect should be relative to the innerRect or directly to rect
            Rect titleRect = new Rect(innerRect.x, innerRect.y, innerRect.width, 35f);
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(titleRect, "Personality");
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;

            // --- ADDED EDIT MODE TOGGLE BOX ---
            float editModeBoxWidth = 160f;
            float editModeBoxHeight = 40f;
            float editModeBoxMargin = 5f;

            // Position the clickable box to the right of the title
            Rect editModeRect = new Rect(
                titleRect.xMax - editModeBoxWidth - editModeBoxMargin, // Align right, with a margin
                titleRect.y + (titleRect.height - editModeBoxHeight) / 2f, // Center vertically
                editModeBoxWidth,
                editModeBoxHeight
            );

            // Draw the background box and text
            //Widgets.DrawBox(editModeRect); // Or Widgets.DrawBoxSolid(editModeRect, Color.gray) for a filled box

            //// Set text color based on editModeOn status
            //Color originalColor = GUI.color;
            //if (editModeOn)
            //{
            //    GUI.color = Color.green; // Highlight green if on
            //}
            //else
            //{
            //    GUI.color = Color.red; // Highlight red if off
            //}

            //Text.Anchor = TextAnchor.MiddleCenter;
            //Widgets.Label(editModeRect, "Edit Mode");
            //Text.Anchor = TextAnchor.UpperLeft; // Reset anchor
            //GUI.color = originalColor; // Reset color

            //// Make the box clickable to toggle editModeOn
            //if (Widgets.ButtonInvisible(editModeRect)) // Invisible button over the label
            //{
            //    editModeOn = !editModeOn; // Toggle the boolean
            //    // You can add a sound here if you want
            //    // SoundDefOf.Click.PlayOneShotOnCamera();
            //}
            //TooltipHandler.TipRegion(editModeRect, "Toggle edit mode for personality values.");
            string editmodeText = "Rimbody_EditModeOn".Translate();
            if (editModeOn)
            {
                editmodeText = "Rimbody_EditModeOff".Translate();
            }
            if (!Widgets.ButtonText(editModeRect, editmodeText))
            {
                SoundDefOf.Tick_High.PlayOneShotOnCamera();
                editModeOn = !editModeOn;
            }
            // --- END ADDED EDIT MODE TOGGLE BOX ---

            // Scroll view
            // The scrollRect's position and size must be relative to the 'rect' parameter.
            // x = rect.x + padding (or just rect.x if you want it flush)
            // y = rect.y + titleRect.height + some_spacing
            Rect scrollRect = new Rect(innerRect.x, titleRect.yMax + 5f, innerRect.width, innerRect.height - (titleRect.height + 5f));

            // viewRect defines the *total scrollable content size*, not its position.
            // Its x and y should typically be 0, as it's the internal coordinate system of the scroll view.
            Rect viewRect = new Rect(0f, 0f, scrollRect.width - 16f, viewHeight); // 16f for scrollbar width

            Widgets.BeginScrollView(scrollRect, ref EditNodeScrollPosition, viewRect);

            float y = 0f; // This 'y' is correct as it's relative to the *inside* of the scroll view.
            float labelWidth = 130f;

            foreach (var def in personalityDefList)
            {
                float currentValue = compPsyche.Personality.GetPersonalityDirect(def);
                var (leftLabel, rightLabel, leftColor, rightColor) = (def.low, def.high, Color.red, Color.green);

                // rowRect and its sub-rects are correctly relative to 'y' which is inside viewRect
                Rect rowRect = new Rect(0f, y, viewRect.width, rowHeight);

                if (Mouse.IsOver(rowRect))
                {
                    Widgets.DrawHighlight(rowRect);
                    TooltipHandler.TipRegion(rowRect, $"{def.label}: {Mathf.Round(currentValue * 100f) / 100f}");
                }
                if (editModeOn)
                {
                    Rect labelRect = new Rect(10f, y + 6f, 160f, 24f);
                    Widgets.Label(labelRect, def.label);

                    Rect sliderRect = new Rect(labelRect.xMax + 10f, y + 8f, viewRect.width - labelRect.width - 30f, 24f);
                    float newValue = Widgets.HorizontalSlider(sliderRect, currentValue, -1f, 1f);

                    if (newValue != currentValue)
                    {
                        compPsyche.Personality.SetPersonalityRating(def, newValue);
                    }

                    y += rowHeight;
                }
                else
                {

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
                    float clamped = Mathf.Clamp(currentValue, -0.5f, 0.5f); // now value is between -0.5 ~ 0.5
                    float halfBar = Mathf.Abs(clamped) * (barWidth / 0.5f) / 2f;
                    Rect valueRect = clamped >= 0
                        ? new Rect(barCenterX, barRect.y, halfBar, barHeight)
                        : new Rect(barCenterX - halfBar, barRect.y, halfBar, barHeight);

                    // Color based on intensity (small = yellow, strong = green)
                    float intensity = Mathf.Abs(clamped) * 2f; // maps 0–0.5 to 0–1
                    Color barColor = Color.Lerp(Color.yellow, Color.green, intensity);
                    Widgets.DrawBoxSolid(valueRect, barColor);

                    y += rowHeight;
                }
            }

            Widgets.EndScrollView();
            Text.Anchor = TextAnchor.UpperLeft;
        }

        public static void DrawFacetCard(Rect rect, Pawn pawn) // Renamed personalityRect to rect for consistency
        {
            var compPsyche = pawn.compPsyche();
            if (compPsyche == null) return; // Added null check for compPsyche

            List<Tuple<Facet, float>> FacetList = new();
            foreach (Facet facet in Enum.GetValues(typeof(Facet)))
            {
                // No need for notation here, it's used in DrawTraitList
                FacetList.Add(new Tuple<Facet, float>(facet, compPsyche.Personality.GetFacetValue(facet)));
            }

            Text.Font = GameFont.Small;
            TextAnchor oldAnchor = Text.Anchor;
            float rowHeight = 28f;
            float labelPadding = 2f;
            float barWidth = 80f;
            float barHeight = 4f;

            // Define internal padding/margins if desired
            float innerPadding = 10f;
            Rect innerRect = new Rect(rect.x + innerPadding, rect.y + innerPadding,
                                      rect.width - (innerPadding * 2), rect.height - (innerPadding * 2));

            // Title for the Facet Card
            Text.Font = GameFont.Medium;
            Rect titleRect = new Rect(innerRect.x, innerRect.y, innerRect.width, 35f);
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(titleRect, "Facets"); // Assuming this card is for Facets
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;

            // --- ADDED RESET BUTTON ---
            float resetButtonSize = 24f; // Standard button size
            float resetButtonMargin = 5f;

            // Position the button on the right side of the title area
            Rect resetButtonRect = new Rect(
                titleRect.xMax - resetButtonSize - resetButtonMargin, // Align right, with a margin
                titleRect.y + (titleRect.height - resetButtonSize) / 2f, // Center vertically within titleRect
                resetButtonSize,
                resetButtonSize
            );

            Texture2D resetIcon = ContentFinder<Texture2D>.Get("Buttons/RimpsycheRefresh", true); // Ensure the path is correct

            if (Widgets.ButtonImage(resetButtonRect, resetIcon))
            {
                compPsyche.Personality.Initialize(); // Call the instance method
            }
            TooltipHandler.TipRegion(resetButtonRect, "Psyche_ResetTooltip");

            // The viewRect defines the total scrollable content area within the scroll view.
            // Its x and y should be 0, as it's the internal coordinate system of the scroll view.
            float viewHeight = FacetList.Count * rowHeight + 3f;
            Rect viewRect = new Rect(0f, 0f, innerRect.width - 16f, viewHeight); // 16f for scrollbar width

            // The scrollRect defines the *visible area* of the scroll view, and its position
            // and size must be relative to the 'rect' parameter.
            Rect scrollRect = new Rect(innerRect.x, titleRect.yMax + 5f, innerRect.width, innerRect.height - (titleRect.height + 5f));
            Widgets.BeginScrollView(scrollRect, ref FacetNodeScrollPosition, viewRect);

            float y = 0f; // This 'y' is correct as it's relative to the *inside* of the scroll view.

            for (int i = 0; i < FacetList.Count; i++)
            {
                var (facet, value) = FacetList[i];
                // Ensure InterfaceComponents.FacetNotation is accessible and correctly returns the tuple
                var (leftLabel, rightLabel, lefColor, rightColor) = InterfaceComponents.FacetNotation[facet];

                // rowRect and its sub-rects are correctly relative to 'y' which is inside viewRect
                Rect rowRect = new Rect(0f, y, viewRect.width, rowHeight);

                // Hover & tooltip
                if (Mouse.IsOver(rowRect))
                {
                    Widgets.DrawHighlight(rowRect);
                    TooltipHandler.TipRegion(rowRect, $"{facet}: {Math.Round(value, 1)} \n\n" + InterfaceComponents.FacetDescription[facet]);
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

                // Bar (centered vertically)
                Rect barRect = new Rect(barCenterX - barWidth / 2f, centerY - barHeight / 2f, barWidth, barHeight);
                Widgets.DrawBoxSolid(barRect, new Color(0.2f, 0.2f, 0.2f, 0.5f)); // Bar background

                // Value bar
                float clamped = Mathf.Clamp(value, -50f, 50f);
                float halfBar = (Mathf.Abs(clamped) / 50f) * (barWidth / 2f);
                Rect valueRect;

                if (clamped >= 0)
                {
                    valueRect = new Rect(barCenterX, barRect.y, halfBar, barHeight);
                }
                else
                {
                    valueRect = new Rect(barCenterX - halfBar, barRect.y, halfBar, barHeight);
                }

                // Color gradient: red → green
                Color barColor = Color.Lerp(lefColor, rightColor, (clamped + 50f) / 100f);
                Widgets.DrawBoxSolid(valueRect, barColor);

                y += rowHeight * 1f;
            }

            Widgets.EndScrollView();
            Text.Anchor = oldAnchor;
        }
    }
}
