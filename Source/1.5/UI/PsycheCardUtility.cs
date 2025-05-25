using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Maux36.RimPsyche
{
    public class PsycheCardUtility
    {
        public static TextAnchor OldAnchor;
        public static int OldSmallFontSize = Text.fontStyles[1].fontSize;

        public static Rect PsycheRect = new Rect(0f, 0f, 550f, 450f);
        public static bool ShowNumbersBool = false;
        public static GUIStyle style;
        public const int OptionFontSize = 16;
        public static readonly Color LineColor = new Color(1f, 1f, 1f, 0.5f);
        public static Vector2 NodeScrollPosition = Vector2.zero;
        public static float BoundaryPadding = 5f;

        public static void DrawPsycheCard(Rect totalRect, Pawn pawn, bool OnWindow = true, bool ShowNumbers = false)
        {
            OldAnchor = Text.Anchor;

            GUI.BeginGroup(totalRect);
            totalRect.position = Vector2.zero;
            Text.Font = GameFont.Small;
            GUI.color = Color.white;
            style = Text.fontStyles[1];
            

            //CalculateNodeWidth(pawn);
            //float totalRectX = CategoryWidth + NodeWidth + SexualityWidth + 4f * BoundaryPadding + 3f * TraitListPadding + 3f * TraitListPadding;
            //Rect totalRect = new Rect(0f, 0f, totalRectWidth, totalRectHeight);
            float SexualityWidth = 150f;
            float SexualityHeight = 200f;
            float KinseyWidth = 100f;
            Vector2 KinseyTextSize = Text.CalcSize("KinseyRating".Translate() + " 0");// * calcSizeScalingVector;
            float HighlightPadding = 5f;
            float kWidth = OnWindow || Prefs.DevMode ? SexualityWidth : KinseyWidth;
            Rect kinseyRect = new Rect(totalRect.xMax - SexualityWidth - BoundaryPadding, totalRect.y + BoundaryPadding, kWidth, KinseyTextSize.y);
            //Calculate personality rectangle
            Rect personalityRect = totalRect;
            personalityRect.xMax = kinseyRect.x - HighlightPadding;
            personalityRect = personalityRect.ContractedBy(BoundaryPadding);
            Rect forbiddenRect = new Rect(personalityRect.xMax, 0f, totalRect.width - personalityRect.xMax, 10f + BoundaryPadding);
            //Rect forbiddenRect = new Rect(personalityRect.xMax, 0f, forbiddenRectWidth, forbiddenRectHeight);
            Rect fiveFactorRect = new Rect(forbiddenRect.center.x - 0.5f * 10f, 10f + 2f * BoundaryPadding, 10f, 10f);

            personalityRect.xMax = totalRect.xMax - SexualityWidth - HighlightPadding - 2f * BoundaryPadding;

            GUI.color = LineColor;
            Widgets.DrawLineVertical(forbiddenRect.x, totalRect.y, totalRect.height);
            Widgets.DrawLineHorizontal(forbiddenRect.x, SexualityHeight + BoundaryPadding, forbiddenRect.width);
            GUI.color = Color.white;

            /* Draw Big Five */
            //DrawBigFive(pawn, fiveFactorRect, forbiddenRect);

            /* Draw personality node list */
            PersonalityTraitList(personalityRect, pawn);

            ////Log.Message("Checking PsychologyEnabled for pawn = " + pawn.Label);
            //if (pawn.compPsyche()==null)
            //{
            //    Widgets.DrawHighlight(totalRect);
            //    Widgets.DrawHighlight(totalRect);
            //    Text.Anchor = TextAnchor.MiddleCenter;
            //    style.fontSize = 30;
            //    GUI.color = new Color(1f, 0f, 0f, 0.85f);
            //    Widgets.Label(totalRect, "PsycheCurrentlyDisabled".Translate());
            //    GUI.color = Color.white;
            //    style.fontSize = OldSmallFontSize;
            //    Text.Anchor = OldAnchor;
            //}
            GUI.EndGroup();
        }

        public static void PersonalityTraitList(Rect personalityRect, Pawn pawn)
        {
            var compPsyche = pawn.compPsyche();
            List<Tuple<Facet, float>> FacetList = new();
            foreach (Facet facet in Enum.GetValues(typeof(Facet)))
            {
                var notation = InterfaceComponents.FacetNotation[facet];
                FacetList.Add(new Tuple<Facet, float>(facet, compPsyche.Personality.GetFacetValue(facet)));
            }
            DrawTraitList(personalityRect, FacetList);
        }
        public static void DrawTraitList(Rect personalityRect, List<Tuple<Facet, float>> FacetList)
        {
            Text.Font = GameFont.Small;
            TextAnchor oldAnchor = Text.Anchor;
            float rowHeight = 28f;
            float labelPadding = 2f;
            float barWidth = 80f;
            float barHeight = 4f;

            float viewHeight = FacetList.Count * rowHeight + 3f;
            Rect viewRect = new Rect(0f, 0f, personalityRect.width - 20f, viewHeight);

            Widgets.BeginScrollView(personalityRect, ref NodeScrollPosition, viewRect);

            float y = 0f;

            for (int i = 0; i < FacetList.Count; i++)
            {
                var (facet, value) = FacetList[i];
                var (leftLabel, rightLabel, lefColor, rightColor) = InterfaceComponents.FacetNotation[facet];

                Rect rowRect = new Rect(0f, y, viewRect.width, rowHeight);

                // Hover & tooltip
                if (Mouse.IsOver(rowRect))
                {
                    Widgets.DrawHighlight(rowRect);
                    TooltipHandler.TipRegion(rowRect, $"{facet}: {Math.Round(value, 1)} \n\n"+InterfaceComponents.FacetDescription[facet]);
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
                //float intensity = Mathf.Abs(clamped) / 50f;
                //Color barColor = Color.Lerp(Color.grey, Color.green, intensity);
                Widgets.DrawBoxSolid(valueRect, barColor);

                y += rowHeight * 1f;
            }

            Widgets.EndScrollView();
            Text.Anchor = oldAnchor;
        }


    }
}
