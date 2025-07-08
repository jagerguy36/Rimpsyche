using RimWorld;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;
using static UnityEngine.GUI;

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
                float desiredWidth = screenWidth * 0.55f;
                float desiredHeight = screenHeight * 0.5f;

                float minWidth = 900f;
                float minHeight = 400f;

                return new Vector2(Mathf.Max(desiredWidth, minWidth), Mathf.Max(desiredHeight, minHeight));
            }
        }
        //Shared
        public static readonly float innerPadding = 5f;
        public static readonly float titleHeight = 35f;
        public static readonly float scrollBarWidth = 20f;
        public static readonly float titleContentSpacing = 5f;
        public static readonly Color barBackgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);

        //Facet

        //Personality
        public static readonly IEnumerable<PersonalityDef> personalityDefList = DefDatabase<PersonalityDef>.AllDefs;
        public static readonly float personalityRowHeight = 32f;
        public static float personalityViewHeight = personalityDefList.Count() * personalityRowHeight;
        public static readonly float personalityLabelPadding = 2f;
        public static readonly float personalityLabelWidth = 130f;
        public static readonly float personalityBarWidth = 120f;
        public static readonly float personalityBarHeight = 4f;

        //Interest
        public static readonly HashSet<Interest> interestList = RimpsycheDatabase.InterestList;
        public static readonly float interestRowHeight = 32f;
        public static readonly float interestViewHeight = interestList.Count() * interestRowHeight;
        public static readonly float interestLabelPadding = 2f;
        public static readonly float interestLabelWidth = 130f;
        public static readonly float interestBarWidth = 120f;
        public static readonly float interestBarHeight = 4f;

        public static Vector2 FacetNodeScrollPosition = Vector2.zero;
        public static Vector2 PersonalityNodeScrollPosition = Vector2.zero;
        public static Vector2 InterestNodeScrollPosition = Vector2.zero;
        public static bool editPersonalityOn = true;
        public static bool editInterestOn = true;
        

        public override void PreOpen()
        {
            base.PreOpen();
            FacetNodeScrollPosition = Vector2.zero;
            PersonalityNodeScrollPosition = Vector2.zero;
            InterestNodeScrollPosition = Vector2.zero;
        }

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
            var compPsyche = pawn.compPsyche();
            if (compPsyche == null) return;

            // Divide window into two horizontal parts: 2:3 ratio
            float totalWidth = inRect.width;

            float leftWidth = totalWidth * 0.375f;
            float midWidth = totalWidth * 0.375f;
            float rightWidth = totalWidth * 0.25f;

            Rect leftRect = new Rect(inRect.x, inRect.y, leftWidth, inRect.height);
            Rect middleRect = new Rect(leftRect.xMax, inRect.y, midWidth, inRect.height);

            Rect rightTopRect = new Rect(middleRect.xMax, inRect.y, rightWidth, compPsyche.Sexuality.ShowOnUI() ? 100f : 0f);
            Rect rightBottomRect = new Rect(middleRect.xMax, rightTopRect.yMax, rightWidth, inRect.height- rightTopRect.height);
            bool showSexuality = compPsyche.Sexuality.ShowOnUI();


            DrawFacetCard(leftRect, pawn, compPsyche);

            DrawPersonalityEditcard(middleRect, pawn, compPsyche);
            if (showSexuality)
            {
                DrawSexualityEditCard(rightTopRect, pawn, compPsyche);
            }
            DrawInterestEditCard(rightBottomRect, pawn, compPsyche);
        }
        public static void DrawSexualityEditCard(Rect rect, Pawn pawn, CompPsyche compPsyche)
        {
            TextAnchor oldAnchor = Text.Anchor;
            GameFont oldFont = Text.Font;
            Rect innerRect = rect.ContractedBy(innerPadding);
            
            // Title
            Rect titleRect = new Rect(innerRect.x, innerRect.y, innerRect.width, titleHeight);
            Text.Anchor = TextAnchor.MiddleCenter;
            Text.Font = GameFont.Medium;
            Widgets.Label(titleRect, "Sexuality");
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;
            Rect ContentRect = new Rect(innerRect.x, titleRect.yMax, innerRect.width, innerRect.height - titleHeight);
            Widgets.DrawBoxSolid(ContentRect, new Color(0.2f, 0.2f, 0.2f, 0.5f));
            Text.Anchor = oldAnchor;
            Text.Font = oldFont;
        }

        public static void DrawInterestEditCard(Rect rect, Pawn pawn, CompPsyche compPsyche)
        {
            TextAnchor oldAnchor = Text.Anchor;
            GameFont oldFont = Text.Font;
            Rect innerRect = rect.ContractedBy(innerPadding);

            // Title
            Rect titleRect = new Rect(innerRect.x, innerRect.y, innerRect.width, titleHeight);
            Text.Anchor = TextAnchor.MiddleCenter;
            Text.Font = GameFont.Medium;
            string titleString = "Interest".Translate();
            Widgets.Label(titleRect, titleString);
            Vector2 titleTextSize = Text.CalcSize(titleString);
            // Text.Anchor = oldAnchor;
            // Text.Font = oldFont;

            // Icon on the right
            float iconSize = 24f;
            float editIconX = titleRect.x + (titleRect.width / 2f) + (titleTextSize.x / 2f) + 8f;
            Rect editIconRect = new Rect(editIconX, titleRect.y + (titleHeight - iconSize) / 2f, iconSize, iconSize);

            // Draw & handle click
            if (Widgets.ButtonImage(editIconRect, Rimpsyche_UI_Utility.EditButton))
            {
                editInterestOn = !editInterestOn;
            }
            TooltipHandler.TipRegion(editIconRect, "RimpsycheEdit");


            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;

            Rect scrollRect = new Rect(innerRect.x, titleRect.yMax + titleContentSpacing, innerRect.width, innerRect.height - (titleRect.height + titleContentSpacing));
            Rect viewRect = new Rect(0f, 0f, scrollRect.width - scrollBarWidth, interestViewHeight);

            Widgets.BeginScrollView(scrollRect, ref InterestNodeScrollPosition, viewRect);

            float y = 0f;
            foreach (var interest in RimpsycheDatabase.InterestList)
            {
                var value = compPsyche.Interests.GetOrCreateInterestScore(interest);
                Rect rowRect = new Rect(0f, y, scrollRect.width, interestRowHeight);

                // Hover highlight + tooltip
                if (Mouse.IsOver(rowRect))
                {
                    Widgets.DrawHighlight(rowRect);
                    TooltipHandler.TipRegion(rowRect, $"{interest.label}: {Math.Round(value, 1)}");
                }
                float centerY = rowRect.y + rowRect.height / 2f;

                // Left label
                Rect leftRect = new Rect(rowRect.x + interestLabelPadding, centerY - Text.LineHeight / 2f, interestLabelWidth, Text.LineHeight);
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(leftRect, interest.label);

                // Bar background
                Rect barRect = new Rect(leftRect.x + interestLabelWidth, centerY - interestBarHeight / 2f, interestBarWidth, interestBarHeight);
                Widgets.DrawBoxSolid(barRect, barBackgroundColor);

                // Value bar
                float normalizedValue = value * 0.01f;
                float fillWidth = normalizedValue * interestBarWidth;
                Rect valueRect = new Rect(barRect.x, barRect.y, fillWidth, interestBarHeight);
                Color barColor = Color.Lerp(Color.yellow, Color.green, normalizedValue);
                Widgets.DrawBoxSolid(valueRect, barColor);

                y += interestRowHeight;
            }

            Widgets.EndScrollView();
            Text.Anchor = oldAnchor;
            Text.Font = oldFont;
        }


        public static void DrawPersonalityEditcard(Rect rect, Pawn pawn, CompPsyche compPsyche)
        {
            var scope = compPsyche.Personality.scopeCache;
            // Define internal padding/margins if desired
            TextAnchor oldAnchor = Text.Anchor;
            GameFont oldFont = Text.Font;
            Rect innerRect = rect.ContractedBy(innerPadding);

            // Title
            Rect titleRect = new Rect(innerRect.x, innerRect.y, innerRect.width, titleHeight);
            Text.Anchor = TextAnchor.MiddleCenter;
            Text.Font = GameFont.Medium;
            string titleString = "Personality".Translate();
            Widgets.Label(titleRect, titleString);
            Vector2 titleTextSize = Text.CalcSize(titleString);
            // Text.Anchor = oldAnchor;
            // Text.Font = oldFont;

            // Icon on the right
            float iconSize = 24f;
            float editIconX = titleRect.x + (titleRect.width / 2f) + (titleTextSize.x / 2f) + 8f;
            Rect editIconRect = new Rect(editIconX, titleRect.y + (titleHeight - iconSize) / 2f, iconSize, iconSize);

            // Draw & handle click
            if (Widgets.ButtonImage(editIconRect, Rimpsyche_UI_Utility.EditButton))
            {
                editPersonalityOn = !editPersonalityOn;
            }
            TooltipHandler.TipRegion(editIconRect, "RimpsycheEdit");


            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;

            // Scroll view
            Rect scrollRect = new Rect(innerRect.x, titleRect.yMax + titleContentSpacing, innerRect.width, innerRect.height - (titleRect.height + titleContentSpacing));
            Rect viewRect = new Rect(0f, 0f, scrollRect.width - scrollBarWidth, personalityViewHeight);

            Widgets.BeginScrollView(scrollRect, ref PersonalityNodeScrollPosition, viewRect);

            float y = 0f;
            float barCenterX = viewRect.width*0.5f;

            foreach (var def in personalityDefList)
            {
                float currentValue = compPsyche.Personality.GetPersonalityDirect(def);
                var (leftLabel, rightLabel, leftColor, rightColor) = (def.low, def.high, Color.red, Color.green);

                // rowRect and its sub-rects are correctly relative to 'y' which is inside viewRect
                Rect rowRect = new Rect(0f, y, viewRect.width, personalityRowHeight);

                if (Mouse.IsOver(rowRect))
                {
                    Widgets.DrawHighlight(rowRect);
                    TooltipHandler.TipRegion(rowRect, $"{def.label}: {Mathf.Round(currentValue * 100f) / 100f}");
                }
                float centerY = rowRect.y + rowRect.height / 2f;
                // Left label
                Rect leftRect = new Rect(rowRect.x + personalityLabelPadding, centerY - Text.LineHeight / 2f, interestLabelWidth, Text.LineHeight);
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(leftRect, leftLabel);

                // Right label
                Rect rightRect = new Rect(rowRect.xMax - interestLabelWidth - personalityLabelPadding, centerY - Text.LineHeight / 2f, interestLabelWidth, Text.LineHeight);
                Text.Anchor = TextAnchor.MiddleRight;
                Widgets.Label(rightRect, rightLabel);
                if (editPersonalityOn)
                {
                    float highend = 1f;
                    float lowend = -1f;
                    if (!scope.NullOrEmpty())
                    {
                        if (scope.TryGetValue(def.defName, out var range))
                        {
                            (lowend, highend) = range;
                        }
                    }
                    //Rect sliderRect = new Rect(barCenterX + barWidth / 2f * lowend , centerY - barHeight / 2f, barWidth*(highend-lowend)*0.5f, 24f);?
                    Rect sliderRect = new Rect(barCenterX - personalityBarWidth / 2f, centerY - personalityBarHeight / 2f, personalityBarWidth, 24f);
                    float newValue = Widgets.HorizontalSlider(sliderRect, currentValue, lowend, highend);
                    Mathf.Clamp(newValue, lowend, highend);
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
                    Color barColor = Color.Lerp(Color.yellow, Color.green, intensity);
                    Widgets.DrawBoxSolid(valueRect, barColor);
                }

                y += personalityRowHeight;
            }

            Widgets.EndScrollView();
            Text.Anchor = oldAnchor;
            Text.Font = oldFont;
        }

        public static void DrawFacetCard(Rect rect, Pawn pawn, CompPsyche compPsyche)
        {
            Text.Font = GameFont.Small;
            TextAnchor oldAnchor = Text.Anchor;
            float rowHeight = 28f;
            float labelPadding = 2f;
            float barWidth = 80f;
            float barHeight = 4f;

            // Define internal padding/margins if desired
            Rect innerRect = rect.ContractedBy(innerPadding);

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
            TooltipHandler.TipRegion(resetButtonRect, "ResetPsycheTooltip");

            float viewHeight = 15f * rowHeight + 3f;
            Rect viewRect = new Rect(0f, 0f, innerRect.width - 16f, viewHeight); // 16f for scrollbar width

            Rect scrollRect = new Rect(innerRect.x, titleRect.yMax + 5f, innerRect.width, innerRect.height - (titleRect.height + 5f));
            Widgets.BeginScrollView(scrollRect, ref FacetNodeScrollPosition, viewRect);

            float y = 0f; // This 'y' is correct as it's relative to the *inside* of the scroll view.

            foreach (Facet facet in RimpsycheDatabase.AllFacets)
            {
                var value = compPsyche.Personality.GetFacetValue(facet);
                var (leftLabel, rightLabel, lefColor, rightColor) = InterfaceComponents.FacetNotation[facet];

                // rowRect and its sub-rects are correctly relative to 'y' which is inside viewRect
                Rect rowRect = new Rect(0f, y, viewRect.width, personalityRowHeight);

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
                float halfBar = (Mathf.Abs(value) / 50f) * (barWidth / 2f);
                Rect valueRect;

                if (value >= 0)
                {
                    valueRect = new Rect(barCenterX, barRect.y, halfBar, barHeight);
                }
                else
                {
                    valueRect = new Rect(barCenterX - halfBar, barRect.y, halfBar, barHeight);
                }

                // Color gradient: red → green
                Color barColor = Color.Lerp(lefColor, rightColor, (value + 50f) / 100f);
                Widgets.DrawBoxSolid(valueRect, barColor);

                y += rowHeight * 1f;
            }

            Widgets.EndScrollView();
            Text.Anchor = oldAnchor;
        }

        public override void PostClose()
        {
            base.PostClose();
            PsycheInfoCard.CacheClean();
        }
    }
}
