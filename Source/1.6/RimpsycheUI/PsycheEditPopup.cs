using RimWorld;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Verse;

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
        public static readonly float iconSpacing = 2f;
        public static readonly float resetButtonSize = 24f;
        public static readonly float resetButtonMargin = 5f;
        public static readonly Color barBackgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);

        //Facet
        public static readonly float facetRowHeight = 28f;
        public static readonly float facetViewHeight = 15f * facetRowHeight;
        public static readonly float facetLabelPadding = 2f;
        public static readonly float facetLabelWidth = 130f;
        public static readonly float facetBarWidth = 80f;
        public static readonly float facetBarHeight = 4f;

        //Personality
        public static readonly IEnumerable<PersonalityDef> personalityDefList = DefDatabase<PersonalityDef>.AllDefs;
        public static readonly float personalityRowHeight = 32f;
        public static readonly float personalityViewHeight = personalityDefList.Count() * personalityRowHeight;
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
        public static bool editPersonalityOn = false;
        public static bool editInterestOn = false;
        

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
            Widgets.Label(titleRect, "RPC_Sexuality".Translate());
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
            string titleString = "RPC_Interest".Translate();
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
            TooltipHandler.TipRegion(editIconRect, "RimpsycheEdit".Translate());


            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;

            Rect scrollRect = new Rect(innerRect.x, titleRect.yMax + titleContentSpacing, innerRect.width, innerRect.height - (titleRect.height + titleContentSpacing));
            Rect viewRect = new Rect(0f, 0f, scrollRect.width - scrollBarWidth, interestViewHeight);

            Widgets.BeginScrollView(scrollRect, ref InterestNodeScrollPosition, viewRect);
            float y = 0f;
            foreach (var interest in RimpsycheDatabase.InterestList)
            {
                float currentValue = compPsyche.Interests.GetOrCreateInterestScore(interest);
                Rect rowRect = new Rect(0f, y, scrollRect.width, interestRowHeight);

                // Hover highlight + tooltip
                if (Mouse.IsOver(rowRect))
                {
                    Widgets.DrawHighlight(rowRect);
                    TooltipHandler.TipRegion(rowRect, $"{interest.label}: {Math.Round(currentValue, 1)}\n{interest.description}");
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
                    newValue = Mathf.Clamp(newValue, minValue, maxValue);
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
                    Color barColor = Color.Lerp(Color.yellow, Color.green, normalizedValue);
                    Widgets.DrawBoxSolid(valueRect, barColor);
                }

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
            string titleString = "RPC_Personality".Translate();
            Widgets.Label(titleRect, titleString);
            Vector2 titleTextSize = Text.CalcSize(titleString);
            // Text.Anchor = oldAnchor;
            // Text.Font = oldFont;

            // Icon on the right
            float iconSize = 24f;
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
            TooltipHandler.TipRegion(infoIconRect, "RimpsychePersonalityInfo".Translate());


            Rect editIconRect = new Rect(infoIconRect.xMax + iconSpacing, titleRect.y + (titleHeight - iconSize) / 2f, iconSize, iconSize);

            // Draw & handle click
            if (Widgets.ButtonImage(editIconRect, Rimpsyche_UI_Utility.EditButton))
            {
                editPersonalityOn = !editPersonalityOn;
            }
            TooltipHandler.TipRegion(editIconRect, "RimpsycheEdit".Translate());

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
                var (leftLabel, rightLabel, leftColor, rightColor) = (def.low.CapitalizeFirst(), def.high.CapitalizeFirst(), Color.red, Color.green);

                // rowRect and its sub-rects are correctly relative to 'y' which is inside viewRect
                Rect rowRect = new Rect(0f, y, viewRect.width, personalityRowHeight);

                if (Mouse.IsOver(rowRect))
                {
                    Widgets.DrawHighlight(rowRect);
                    string tooltipString = $"{def.label.CapitalizeFirst()}: {(currentValue * 100f).ToString("F1")}\n{def.description}";
                    if (compPsyche.Personality.scopeInfoCache.TryGetValue(def.defName, out string explanation))
                    {
                        tooltipString += $"\n\n{explanation}";
                    }
                    TooltipHandler.TipRegion(rowRect, tooltipString);
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
                    Rect sliderRect = new Rect(barCenterX - personalityBarWidth / 2f, centerY - personalityBarHeight / 2f, personalityBarWidth, personalityRowHeight);
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


            // Icon on the right
            float iconSize = 24f;
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


            //Rect editIconRect = new Rect(infoIconRect.xMax + iconSpacing, titleRect.y + (titleHeight - iconSize) / 2f, iconSize, iconSize);

            //// Draw & handle click
            //if (Widgets.ButtonImage(editIconRect, Rimpsyche_UI_Utility.EditButton))
            //{
            //    editPersonalityOn = !editPersonalityOn;
            //}
            //TooltipHandler.TipRegion(editIconRect, "RimpsycheEdit".Translate());

            //Text.Anchor = TextAnchor.UpperLeft;
            //Text.Font = GameFont.Small;

            Rect resetButtonRect = new Rect(
                titleRect.xMax - resetButtonSize - resetButtonMargin,
                titleRect.y + (titleRect.height - resetButtonSize) / 2f,
                resetButtonSize,
                resetButtonSize
            );

            if (Widgets.ButtonImage(resetButtonRect, Rimpsyche_UI_Utility.resetIcon))
            {
                compPsyche.Personality.Initialize();
            }
            TooltipHandler.TipRegion(resetButtonRect, "ResetPsycheTooltip".Translate());

            Rect viewRect = new Rect(0f, 0f, innerRect.width - scrollBarWidth, facetViewHeight);
            Rect scrollRect = new Rect(innerRect.x, titleRect.yMax + 5f, innerRect.width, innerRect.height - (titleRect.height + 5f));
            Widgets.BeginScrollView(scrollRect, ref FacetNodeScrollPosition, viewRect);

            float y = 0f;
            foreach (Facet facet in RimpsycheDatabase.AllFacets)
            {
                var value = compPsyche.Personality.GetFacetValue(facet);
                var (leftLabel, rightLabel, lefColor, rightColor) = InterfaceComponents.FacetNotation[facet];
                Rect rowRect = new Rect(0f, y, viewRect.width, facetRowHeight);
                if (Mouse.IsOver(rowRect))
                {
                    Widgets.DrawHighlight(rowRect);
                    string tooltipString = $"{facet}: {(value*2f).ToString("F1")}\n\n{InterfaceComponents.FacetDescription[facet]}";
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

                y += facetRowHeight;
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
