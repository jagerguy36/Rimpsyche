using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Maux36.RimPsyche
{
    [StaticConstructorOnStartup]
    public class PsycheInfoCard
    {
        // Settings
        private static readonly bool usePreference = RimpsycheSexualitySettings.usePreferenceSystem;

        // Constants and style settings
        // width: 380 | 220
        public static Rect PsycheRect = new Rect(0f, 0f, Mathf.Min(600f + personalityWidthDiff + rightsideWidthDiff,  UI.screenWidth * 0.8f), Mathf.Clamp(UI.screenHeight*0.5f,350f, 480f));
        public static GUIStyle style;
        public static Vector2 PersonalityScrollPosition = Vector2.zero;
        public static Vector2 InterestScrollPosition = Vector2.zero;
        public static Color barBackgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
        public static Color barSurplusBackgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.1f);
        public static Color radarFillColor = new Color(0.5f, 1f, 0.5f, 0.6f);
        public static Color radarHighlightColor = new Color(0.6f, 0.6f, 0.6f, 0.5f);
        public static Color radarEdgeColor = new Color(0.5f, 0.5f, 0.5f, 0.6f);
        public static Color radarSpokeColor = new Color(0.5f, 0.5f, 0.5f, 0.3f);

        public static readonly float expandButtonSize = 8f;
        public static readonly float rightPanelWidthConstant = 220f;
        public static float rightPanelWidthActual => rightPanelWidthConstant + rightsideWidthDiff;
        public static readonly Color LineColor = new Color(97f, 108f, 122f, 0.25f);
        public static readonly float headerHeight = 35f;
        public static readonly float labelPadding = 2f;
        public static readonly float innerPadding = 5f;
        public static readonly float scrollWidth = 20f;

        private static readonly float RadarChartSize = 20f; // Diameter of the radar chart
        private static readonly float RadarChartPadding = 10f; // Padding from the header text
        private static Material _lineMaterial;

        public static float personalityLabelWidth => RimpsycheDatabase.maxPersonalityLabelWidth;
        public static float personalityWidthDiff => 2f * (personalityLabelWidth - 130f);
        public static readonly float personalityRowHeight = 28f;
        public static readonly float personalityBarWidth = 80f;
        public static readonly float personalityBarHeight = 4f;
        
        public static float interestLabelWidth => RimpsycheDatabase.maxInterestLabelWidth;
        public static float rightsideWidthDiff => (RimpsycheDatabase.maxRightsideLabelWidth - 130f);
        public static readonly float interestRowHeight = 28f;
        public static readonly float interestBarHeight = 4f;

        public static readonly float sexualityHeaderHeight = 35f;
        public static readonly float sexualityLineHeight = 25f;
        public static float sexualityLabelWidth => RimpsycheDatabase.maxSexualityLabelWidth;
        public static readonly float sexualityBarMargin = 5f;
        public static readonly float sexualityRightMargin = 20f;
        public static readonly float sexualityBarHeight = 4f;

        //Options
        public static bool rightPanelVisible = false;
        public static bool showPreference = false;
        public static byte showMode = 0;

        //Cache Management
        private static bool resetPreferenceReport = true;

        static PsycheInfoCard()
        {
            EnsureMaterial();
        }
        private static void EnsureMaterial()
        {
            if (_lineMaterial == null)
            {
                Shader shader = Shader.Find("Hidden/Internal-Colored");
                _lineMaterial = new Material(shader)
                {
                    hideFlags = HideFlags.HideAndDontSave
                };
                _lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                _lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                _lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
                _lineMaterial.SetInt("_ZWrite", 0);
            }
        }

        //Cache
        private static List<PersonalityDisplayData> cachedPersonalityData = null;
        private static List<InterestDisplayData> cachedInterestData = null;
        private static List<Vector2> cachedValuePointData = null;
        private static List<Vector2> cachedMaxPointData = null;
        private static List<(string, float)> cachedPreferenceReport = null;
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
            public float AbsValue;
            public string CachedLabelText;
            public string CachedDescription;
            public Color CachedLabelColor;
        }

        public static void CacheClean()
        {
            cachedPersonalityData = null;
            cachedInterestData = null;
            cachedValuePointData = null;
            resetPreferenceReport = true;
        }

        public static void GenerateCacheData(CompPsyche compPsyche, Pawn currentPawn)
        {
            resetPreferenceReport = true;
            lastPawn = currentPawn;
            GenerateSortedPersonalityData(compPsyche, currentPawn);
            GenerateSortedInterestData(compPsyche, currentPawn);
        }

        public static void DrawPsycheCard(Rect totalRect, Pawn pawn, CompPsyche compPsyche)
        {
            var psycheEnabled = compPsyche.Enabled;
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
            float rightPanelWidth = rightPanelVisible ? rightPanelWidthActual : 0f;
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
            if (psycheEnabled && rightPanelVisible)
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
            if (psycheEnabled)
            {
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
            }


            if (showSexuality)
            {
                sexualityRect = sexualityRect.ContractedBy(innerPadding);
            }
            interestRect = interestRect.ContractedBy(innerPadding);
            personalityRect = personalityRect.ContractedBy(innerPadding); // Add padding

            // === Draw content ===
            DrawPersonalityBox(personalityRect, compPsyche, pawn);
            if (psycheEnabled && rightPanelVisible)
            {
                if (showSexuality)
                {
                    DrawSexaulityBox(sexualityRect, compPsyche, pawn);
                }
                DrawInterestBox(interestRect, compPsyche, pawn, showSexuality);
            }

            if (psycheEnabled != true)
            {
                Widgets.DrawHighlight(totalRect);
                Text.Anchor = TextAnchor.MiddleCenter;
                Text.Font = GameFont.Medium;
                GUI.color = new Color(1f, 0f, 0f, 0.80f);
                Widgets.Label(totalRect, "PsycheDisabled".Translate());
                GUI.color = Color.white;
                Text.Font = oldFont;
                Text.Anchor = oldAnchor;
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
            GenerateCacheData(compPsyche, currentPawn);
            return cachedPersonalityData;
        }

        private static List<InterestDisplayData> GetSortedInterestData(CompPsyche compPsyche, Pawn currentPawn)
        {
            if (currentPawn == lastPawn && cachedInterestData != null)
            {
                return cachedInterestData;
            }
            GenerateCacheData(compPsyche, currentPawn);
            return cachedInterestData;
        }

        private static List<(string, float)> GetPreferenceReport(Pawn currentPawn, float width)
        {
            //List<(string, float)> cachedPreferenceReport
            if (resetPreferenceReport==false && cachedPreferenceReport != null)
            {
                return cachedPreferenceReport;
            }
            GeneratePreferenceReport(currentPawn, width);
            return cachedPreferenceReport;
        }

        private static List<Vector2> GetValuePointData(Vector2 center, CompPsyche compPsyche, Pawn currentPawn)
        {
            if (currentPawn == lastPawn && cachedValuePointData != null)
            {
                return cachedValuePointData;
            }
            GenerateValuePointData(center, compPsyche);
            return cachedValuePointData;
        }
        private static List<Vector2> GetMaxPointData(Vector2 center)
        {
            if (cachedMaxPointData == null)
            {
                GenerateMaxPointData(center);
            }
            return cachedMaxPointData;
        }
        private static void GenerateSortedPersonalityData(CompPsyche compPsyche, Pawn currentPawn)
        {
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
                var personalityDesc = $"{personality.label.CapitalizeFirst()}: {(value * 100f).ToString("F1")}\n{personality.description}";
                if (compPsyche.Personality.scopeInfoCache.TryGetValue(personality.shortHash, out string explanation))
                {
                    personalityDesc += $"\n\n{explanation}";
                }
                sortedData.Add(new PersonalityDisplayData
                {
                    Personality = personality,
                    Value = value,
                    AbsValue = absValue,
                    CachedLabelText = cachedLabelText,
                    CachedLabelColor = cachedLabelColor,
                    CachedDescription = personalityDesc
                });
            }
            sortedData = sortedData.OrderByDescending(p => p.AbsValue).ToList();
            cachedPersonalityData = sortedData;
        }
        private static void GenerateSortedInterestData(CompPsyche compPsyche, Pawn currentPawn)
        {
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
                    CachedLabelColor = cachedLabelColor,
                    CachedDescription = $"{interest.label}: {Math.Round(value, 1)}\n{interest.description}\n\n{"RimpsycheTopicHeader".Translate()}\n{RimpsycheDatabase.InterstTopicStringDict[interest.id]}"});
            }
            sortedData = sortedData.OrderByDescending(p => p.AbsValue).ToList();
            cachedInterestData = sortedData;
        }
        private static void GeneratePreferenceReport(Pawn currentPawn, float width)
        {
            GameFont oldFont = Text.Font;
            Text.Font = GameFont.Small;
            cachedPreferenceReport = new();
            foreach (var prefDef in DefDatabase<PreferenceDef>.AllDefs)
            {
                string explanation = prefDef.worker.Report(currentPawn);
                float textHeight = Text.CalcHeight(explanation, width);
                cachedPreferenceReport.Add((explanation, textHeight));
            }
            Text.Font = oldFont;
            resetPreferenceReport = false;
        }

        private static void GenerateValuePointData(Vector2 center, CompPsyche compPsyche)
        {
            List<Vector2> valuePointData = new();
            float radius = RadarChartSize * 0.5f;


            for (int i = 0; i < RimpsycheSettings.facetCount; i++)
            {
                float angleRad = ((24f) * i - 90f) * Mathf.Deg2Rad;
                float cos = Mathf.Cos(angleRad);
                float sin = Mathf.Sin(angleRad);
                Facet facet = RimpsycheDatabase.AllFacets[i];
                float value = compPsyche.Personality.GetFacetValue(facet);
                float normalized = Mathf.InverseLerp(-50f, 50f, value);
                float valueRadius = normalized * radius;

                Vector2 valuePoint = new Vector2(center.x + valueRadius * cos, center.y + valueRadius * sin);
                valuePointData.Add(valuePoint);
            }
            cachedValuePointData= valuePointData;
        }
        private static void GenerateMaxPointData(Vector2 center)
        {
            List<Vector2> highlightPoints = new List<Vector2>();
            List<Vector2> maxPoints = new List<Vector2>();
            float radius = RadarChartSize * 0.5f;

            for (int i = 0; i < RimpsycheSettings.facetCount; i++)
            {
                float angleRad = ((24f) * i - 90f) * Mathf.Deg2Rad;
                float cos = Mathf.Cos(angleRad);
                float sin = Mathf.Sin(angleRad);

                Vector2 maxPoint = new Vector2(center.x + radius * cos, center.y + radius * sin);
                maxPoints.Add(maxPoint);
            }
            cachedMaxPointData = maxPoints;
        }

        public static void DrawRadarChart(Rect rect, CompPsyche compPsyche, Pawn pawn)
        {
            GUI.BeginGroup(rect);
            Rect chartArea = new Rect(0, 0, rect.width, rect.height);
            Vector2 center = new Vector2(chartArea.center.x, chartArea.center.y);
            var valuePoints = GetValuePointData(center, compPsyche, pawn);
            var maxPoints = GetMaxPointData(center);
            _lineMaterial.SetPass(0);

            GL.PushMatrix();

            GL.Begin(GL.LINES);
            //Spokes and Circle
            for (int i = 0; i < RimpsycheSettings.facetCount; i++)
            {
                GL.Color(radarSpokeColor);
                GL.Vertex(center);
                GL.Vertex(maxPoints[i]);
                GL.Color(radarEdgeColor);
                GL.Vertex(maxPoints[i]);
                GL.Vertex(maxPoints[(i + 1) % RimpsycheSettings.facetCount]);
            }
            GL.End();

            // Triangles
            GL.Begin(GL.TRIANGLES);
            for (int i = 0; i < RimpsycheSettings.facetCount; i++)
            {
                Vector2 a = valuePoints[i];
                Vector2 b = valuePoints[(i + 1) % RimpsycheSettings.facetCount];
                GL.Color(radarFillColor);
                GL.Vertex(center);
                GL.Vertex(a);
                GL.Vertex(b);
            }
            GL.End();

            GL.PopMatrix();
            GUI.EndGroup();
        }

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
            Widgets.Label(titleRect, "RPC_Personality".Translate());
            Vector2 titleTextSize = Text.CalcSize("RPC_Personality".Translate());

            if (RimpsycheSettings.showFacetGraph)
            {
                float radarChartX = (headerRect.width / 2f) - (titleTextSize.x / 2f) - RadarChartSize - RadarChartPadding;
                Rect radarChartRect = new Rect(radarChartX, titleRect.y + (titleRect.height - RadarChartSize) / 2f, RadarChartSize, RadarChartSize);
                DrawRadarChart(radarChartRect, compPsyche, pawn);
            }

            // Icon on the right
            float iconSize = 24f;
            float spacing = 2;
            float viewIconX = (headerRect.width / 2f) + (titleTextSize.x / 2f) + 8f;
            Rect viewIconRect = new Rect(viewIconX, titleRect.y + (titleRect.height - iconSize) / 2f, iconSize, iconSize);

            // Draw & handle click
            if (showMode == 0)
            {
                if (Widgets.ButtonImage(viewIconRect, Rimpsyche_UI_Utility.ViewBarButton))
                {
                    showMode = 1;
                }
                TooltipHandler.TipRegion(viewIconRect, "RimpsycheShowBar".Translate());
            }
            else if (showMode == 1)
            {
                if (RimpsycheSettings.showFacetInMenu)
                {
                    if (Widgets.ButtonImage(viewIconRect, Rimpsyche_UI_Utility.ViewFacetButton))
                    {
                        showMode = 2;
                        PersonalityScrollPosition = Vector2.zero;
                    }
                    TooltipHandler.TipRegion(viewIconRect, "RimpsycheShowFacet".Translate());
                }
                else
                {
                    if (Widgets.ButtonImage(viewIconRect, Rimpsyche_UI_Utility.ViewListButton))
                    {
                        showMode = 0;
                    }
                    TooltipHandler.TipRegion(viewIconRect, "RimpsycheShowList".Translate());
                }
            }
            else if (showMode == 2)
            {
                if (Widgets.ButtonImage(viewIconRect, Rimpsyche_UI_Utility.ViewListButton))
                {
                    showMode = 0;
                    PersonalityScrollPosition = Vector2.zero;
                }
                TooltipHandler.TipRegion(viewIconRect, "RimpsycheShowList".Translate());
            }
            else
            {
                showMode = 0;
            }
            
            Rect editIconRect = new Rect(viewIconRect.xMax + spacing, viewIconRect.y, iconSize, iconSize);
            if (Prefs.DevMode)
            {
                if (Widgets.ButtonImage(editIconRect, Rimpsyche_UI_Utility.EditButton))
                {
                    Find.WindowStack.Add(new PsycheEditPopup(pawn));
                }
                TooltipHandler.TipRegion(editIconRect, "RimpsycheEdit".Translate());
            }
            GUI.EndGroup();

            // Scroll View Setup
            Text.Font = GameFont.Small;
            var personalitiesToDisplay = GetSortedPersonalityData(compPsyche, pawn);

            float viewHeight = (showMode == 2)? 15 * personalityRowHeight + 3f : personalitiesToDisplay.Count() * personalityRowHeight + 3f;
            Rect scrollContentRect = new Rect(0f, 0f, personalityRect.width - scrollWidth, viewHeight);

            Rect scrollRect = new Rect(
                personalityRect.x,
                personalityRect.y + headerHeight,
                personalityRect.width,
                personalityRect.height - headerHeight
            );

            Widgets.BeginScrollView(scrollRect, ref PersonalityScrollPosition, scrollContentRect);

            float y = 0f;
            if (showMode == 2)
            {

                foreach (Facet facet in RimpsycheDatabase.AllFacets)
                {
                    var value = compPsyche.Personality.GetFacetValue(facet);
                    var (facetlabel, leftLabel, rightLabel, lefColor, rightColor) = InterfaceComponents.FacetNotation[facet];
                    Rect rowRect = new Rect(0f, y, scrollContentRect.width, personalityRowHeight);
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
                    float textY = centerY - Text.LineHeight / 2f;
                    float barY = centerY - personalityBarHeight / 2f;

                    // Left label
                    Rect leftRect = new Rect(rowRect.x + labelPadding, textY, personalityLabelWidth, Text.LineHeight);
                    Text.Anchor = TextAnchor.MiddleLeft;
                    Widgets.Label(leftRect, leftLabel);

                    // Right label
                    Rect rightRect = new Rect(rowRect.xMax - personalityLabelWidth - labelPadding, textY, personalityLabelWidth, Text.LineHeight);
                    Text.Anchor = TextAnchor.MiddleRight;
                    Widgets.Label(rightRect, rightLabel);

                    // Bar (centered vertically)
                    Rect barRect = new Rect(barCenterX - personalityBarWidth / 2f, barY, personalityBarWidth, personalityBarHeight);
                    Widgets.DrawBoxSolid(barRect, barBackgroundColor);

                    // Value bar
                    float halfBar = (Mathf.Abs(value) / 50f) * (personalityBarWidth / 2f);
                    Rect valueRect;

                    if (value >= 0)
                    {
                        valueRect = new Rect(barCenterX, barRect.y, halfBar, personalityBarHeight);
                    }
                    else
                    {
                        valueRect = new Rect(barCenterX - halfBar, barRect.y, halfBar, personalityBarHeight);
                    }

                    // Color gradient: red → green
                    Color barColor = Color.Lerp(lefColor, rightColor, (value + 50f) / 100f);
                    Widgets.DrawBoxSolid(valueRect, barColor);

                    y += personalityRowHeight;
                }
            }

            else if (showMode == 1)
            {
                foreach (var pData in personalitiesToDisplay)
                {
                    var personality = pData.Personality;
                    var value = pData.Value;
                    var (leftLabel, rightLabel, leftColor, rightColor) = (personality.low.CapitalizeFirst(), personality.high.CapitalizeFirst(), Color.red, Color.green);

                    Rect rowRect = new Rect(0f, y, scrollContentRect.width, personalityRowHeight);

                    // Hover highlight + tooltip
                    if (Mouse.IsOver(rowRect))
                    {
                        Widgets.DrawHighlight(rowRect);
                        TooltipHandler.TipRegion(rowRect, pData.CachedDescription);
                    }

                    float barCenterX = rowRect.x + rowRect.width / 2f;
                    float centerY = rowRect.y + rowRect.height / 2f;
                    float textY = centerY - Text.LineHeight / 2f;
                    float barY = centerY - personalityBarHeight / 2f;

                    // Left label
                    Rect leftRect = new Rect(rowRect.x + labelPadding, textY, personalityLabelWidth, Text.LineHeight);
                    Text.Anchor = TextAnchor.MiddleLeft;
                    Widgets.Label(leftRect, leftLabel);

                    // Right label
                    Rect rightRect = new Rect(rowRect.xMax - personalityLabelWidth - labelPadding, textY, personalityLabelWidth, Text.LineHeight);
                    Text.Anchor = TextAnchor.MiddleRight;
                    Widgets.Label(rightRect, rightLabel);

                    // Bar background
                    Rect barRect = new Rect(barCenterX - personalityBarWidth / 2f, barY, personalityBarWidth, personalityBarHeight);
                    Widgets.DrawBoxSolid(barRect, barBackgroundColor);

                    // Value bar
                    float halfBar = pData.AbsValue * (personalityBarWidth) / 2f;
                    Rect valueRect = value >= 0
                        ? new Rect(barCenterX, barRect.y, halfBar, personalityBarHeight)
                        : new Rect(barCenterX - halfBar, barRect.y, halfBar, personalityBarHeight);

                    // Color based on intensity (small = yellow, strong = green)
                    Widgets.DrawBoxSolid(valueRect, pData.CachedLabelColor);

                    y += personalityRowHeight;
                }
            }
            else
            {
                foreach (var pData in personalitiesToDisplay)
                {
                    Rect rowRect = new Rect(0f, y, scrollContentRect.width, personalityRowHeight);

                    // Hover highlight + tooltip
                    if (Mouse.IsOver(rowRect))
                    {
                        Widgets.DrawHighlight(rowRect);
                        TooltipHandler.TipRegion(rowRect, pData.CachedDescription);
                    }

                    // Draw label
                    Text.Anchor = TextAnchor.MiddleLeft;
                    GUI.color = pData.CachedLabelColor;
                    Rect labelRect = new Rect(rowRect.x + labelPadding, rowRect.y, scrollContentRect.width - (2 * labelPadding), personalityRowHeight);
                    Widgets.Label(labelRect, pData.CachedLabelText);
                    GUI.color = Color.white; // Reset color

                    y += personalityRowHeight;
                }
            }

            Widgets.EndScrollView();

            // Restore previous text settings
            Text.Anchor = oldAnchor;
            Text.Font = oldFont;
        }

        public static void DrawSexaulityBox(Rect sexualityRect, CompPsyche compPsyche, Pawn pawn)
        {
            TextAnchor oldAnchor = Text.Anchor;
            GameFont oldFont = Text.Font;

            // === Header Config ===
            float contentStartY = sexualityRect.y + sexualityHeaderHeight; // Starting Y for content below header

            // === Draw Header ===
            Rect headerRect = new Rect(sexualityRect.x, sexualityRect.y, sexualityRect.width, sexualityHeaderHeight);
            GUI.BeginGroup(headerRect);

            // Title: "Sexuality"
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleCenter;
            Rect titleRect = new Rect(0f, 0f, headerRect.width, headerRect.height);
            Widgets.Label(titleRect, "RPC_Sexuality".Translate());

            GUI.EndGroup();

            // === Draw Details ===
            GUI.BeginGroup(new Rect(sexualityRect.x, contentStartY, sexualityRect.width, sexualityRect.height - headerHeight));

            Text.Font = GameFont.Small; // Set font for the details
            Text.Anchor = TextAnchor.MiddleLeft; // Align text to the left

            // Name
            float y = 0f;
            // Sexuality
            Rect sexualityDetailRect = new Rect(0f, y, sexualityRect.width, sexualityLineHeight);
            sexualityDetailRect.SplitVertically(sexualityLabelWidth + sexualityBarMargin, out Rect sexualityDetailLeftRect, out Rect sexualityDetailRightRect);
            Widgets.Label(sexualityDetailLeftRect, "RPC_Orientation".Translate());
            sexualityDetailRightRect.SplitVertically(sexualityDetailRightRect.width / 1.5f, out Rect sexualityDetailNameRect, out Rect sexualityDetailKinseyRect);
            Widgets.Label(sexualityDetailNameRect, compPsyche.Sexuality.GetOrientationReport());
            if (Mouse.IsOver(sexualityDetailNameRect))
            {
                Widgets.DrawHighlight(sexualityDetailNameRect);
                TooltipHandler.TipRegion(sexualityDetailNameRect, compPsyche.Sexuality.GetOrientationDescription());
            }

            // Kinsey Rating
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(sexualityDetailKinseyRect, $"({compPsyche.Sexuality.GetKinseyReport()})");
            Text.Anchor = TextAnchor.MiddleLeft;
            if (Mouse.IsOver(sexualityDetailKinseyRect))
            {
                Widgets.DrawHighlight(sexualityDetailKinseyRect);
                var tooltipString = new StringBuilder()
                    .AppendTagged("RPS_KinseyTitleTooltip".Translate().Colorize(Color.yellow))
                    .Append("\n\n").Append("RPS_KinseyTooltip".Translate())
                    .ToString();
                TooltipHandler.TipRegion(sexualityDetailKinseyRect, tooltipString);
            }

            y += sexualityLineHeight;

            float barWidth = (sexualityRect.width - sexualityLabelWidth - sexualityBarMargin - sexualityRightMargin) * 2f / 3f;

            // Male Attraction
            Rect maleLabelRect = new Rect(0f, y, sexualityLabelWidth, sexualityLineHeight);
            Rect maleBarRect = new Rect(sexualityLabelWidth + sexualityBarMargin, y + (sexualityLineHeight - sexualityBarHeight) / 2f, barWidth, sexualityBarHeight); // Center bar vertically
            Rect maleBarSurplusRect = new Rect(maleBarRect.xMax, maleBarRect.y, barWidth * 0.5f, sexualityBarHeight);
            Rect maleAllRect = new Rect(0f, y, sexualityRect.width, sexualityLineHeight);
            Widgets.Label(maleLabelRect, "RPC_AttractionMale".Translate());
            Widgets.DrawBoxSolid(maleBarRect, barBackgroundColor);
            Widgets.DrawBoxSolid(maleBarSurplusRect, barSurplusBackgroundColor);
            float mAttraction = compPsyche.Sexuality.GetAdjustedAttractionToGender(Gender.Male);
            Rect mValueRect = new Rect(maleBarRect.x, maleBarRect.y, mAttraction * barWidth, sexualityBarHeight);
            Color mColor;
            if (mAttraction <= 1) mColor = Color.Lerp(Color.yellow, Color.green, mAttraction);
            else mColor = Color.Lerp(Color.green, Color.cyan, -1.25f + 1.5f * mAttraction);
            Widgets.DrawBoxSolid(mValueRect, mColor);
            Widgets.DrawLineVertical(maleBarRect.xMax, maleBarRect.y - 1, maleBarRect.height + 2);
            if (Mouse.IsOver(maleAllRect))
            {
                Widgets.DrawHighlight(maleAllRect);
                TooltipHandler.TipRegion(maleAllRect, "RPS_AttractionMaleTooltip".Translate() + ": " + mAttraction.ToStringPercent());
            }

            y += sexualityLineHeight;

            // Female Attraction
            Rect femaleLabelRect = new Rect(0f, y, sexualityLabelWidth, sexualityLineHeight);
            Rect femaleBarRect = new Rect(sexualityLabelWidth + sexualityBarMargin, y + (sexualityLineHeight - sexualityBarHeight) / 2f, barWidth, sexualityBarHeight);
            Rect femaleBarSurplusRect = new Rect(femaleBarRect.xMax, femaleBarRect.y, barWidth * 0.5f, sexualityBarHeight);
            Rect femaleAllRect = new Rect(0f, y, sexualityRect.width, sexualityLineHeight);
            Widgets.Label(femaleLabelRect, "RPC_AttractionFemale".Translate());
            Widgets.DrawBoxSolid(femaleBarRect, barBackgroundColor);
            Widgets.DrawBoxSolid(femaleBarSurplusRect, barSurplusBackgroundColor);
            float fAttraction = compPsyche.Sexuality.GetAdjustedAttractionToGender(Gender.Female);
            Rect fValueRect = new Rect(femaleBarRect.x, femaleBarRect.y, fAttraction * barWidth, sexualityBarHeight);
            Color fColor;
            if (fAttraction <= 1) fColor = Color.Lerp(Color.yellow, Color.green, fAttraction);
            else fColor = Color.Lerp(Color.green, Color.cyan, -1.25f + 1.5f * fAttraction);
            Widgets.DrawBoxSolid(fValueRect, fColor);
            Widgets.DrawLineVertical(femaleBarRect.xMax, femaleBarRect.y - 1, femaleBarRect.height + 2);
            if (Mouse.IsOver(femaleAllRect))
            {
                Widgets.DrawHighlight(femaleAllRect);
                TooltipHandler.TipRegion(femaleAllRect, "RPS_AttractionFemaleTooltip".Translate()+": "+fAttraction.ToStringPercent());
            }

            y += sexualityLineHeight;
            Rect sexDriveLabelRect = new Rect(0f, y, sexualityLabelWidth, sexualityLineHeight);
            Rect sexDriveRect = new Rect(sexualityLabelWidth + sexualityBarMargin, y + (sexualityLineHeight - sexualityBarHeight) / 2f, barWidth, sexualityBarHeight);
            Rect sexDriveBarSurplusRect = new Rect(sexDriveRect.xMax, sexDriveRect.y, barWidth * 0.5f, sexualityBarHeight);
            Rect sexDriveAllRect = new Rect(0f, y, sexualityRect.width, sexualityLineHeight);
            Widgets.Label(sexDriveLabelRect, "RPC_SexDrive".Translate());
            Widgets.DrawBoxSolid(sexDriveRect, barBackgroundColor);
            Widgets.DrawBoxSolid(sexDriveBarSurplusRect, barSurplusBackgroundColor);
            float sexDrive = compPsyche.Sexuality.GetAdjustedSexdrive();
            Rect sdValueRect = new Rect(sexDriveRect.x, sexDriveRect.y, sexDrive * barWidth, sexualityBarHeight);
            Color sdColor;
            if (sexDrive <= 1) sdColor = Color.Lerp(Color.yellow, Color.green, sexDrive);
            else sdColor = Color.Lerp(Color.green, Color.cyan, -1.25f + 1.5f * sexDrive);
            Widgets.DrawBoxSolid(sdValueRect, sdColor);
            Widgets.DrawLineVertical(sexDriveRect.xMax, sexDriveRect.y - 1, sexDriveRect.height + 2);
            if (Mouse.IsOver(sexDriveAllRect))
            {
                Widgets.DrawHighlight(sexDriveAllRect);
                TooltipHandler.TipRegion(sexDriveAllRect, "RPS_SexdriveTooltip".Translate() + ": " + sexDrive.ToStringPercent());
            }

            y += sexualityLineHeight;

            GUI.EndGroup();

            // Reset text settings
            Text.Anchor = oldAnchor;
            Text.Font = oldFont;

            Text.Anchor = oldAnchor;
            Text.Font = oldFont;
        }

        public static void DrawInterestBox(Rect interestRect, CompPsyche compPsyche, Pawn pawn, bool showSexuality)
        {
            TextAnchor oldAnchor = Text.Anchor;
            GameFont oldFont = Text.Font;

            // === Draw Header ===
            Rect headerRect = new Rect(interestRect.x, interestRect.y, interestRect.width, headerHeight);
            GUI.BeginGroup(headerRect);

            // Title: "Interest" || "Preference
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleCenter;
            Rect titleRect = new Rect(0f, 0f, headerRect.width, headerRect.height);
            Vector2 titleTextSize;
            if (showPreference)
            {
                Widgets.Label(titleRect, "RPC_Preference".Translate());
                titleTextSize = Text.CalcSize("RPC_Preference".Translate());
            }
            else
            {
                Widgets.Label(titleRect, "RPC_Interest".Translate());
                titleTextSize = Text.CalcSize("RPC_Interest".Translate());
            }
            if (Rimpsyche.SexualityModuleLoaded && usePreference && showSexuality)
            {
                // Icon on the right
                float iconSize = 24f;
                // float viewIconX = (headerRect.width / 2f) + (titleTextSize.x / 2f) + 8f;
                float viewIconX = (headerRect.width - 2f * innerPadding - iconSize);
                Rect viewIconRect = new Rect(viewIconX, titleRect.y + (titleRect.height - iconSize) / 2f, iconSize, iconSize);

                // Draw & handle click
                if (Widgets.ButtonImage(viewIconRect, showPreference?Rimpsyche_UI_Utility.InterestButton:Rimpsyche_UI_Utility.PreferenceButton))
                {
                    showPreference = !showPreference;
                }
                TooltipHandler.TipRegion(viewIconRect, showPreference?"RimpsycheShowInterest".Translate(): "RimpsycheShowPreference".Translate());
            }

            GUI.EndGroup();

            Text.Anchor = oldAnchor;
            Text.Font = oldFont;

            Rect scrollRect = new Rect(
                interestRect.x,
                interestRect.y + headerHeight,
                interestRect.width,
                interestRect.height - headerHeight
            );

            if (showPreference)
            {
                Text.Font = GameFont.Small;
                float y = scrollRect.y;
                var prefReport = GetPreferenceReport(pawn, scrollRect.width);
                foreach (var report in prefReport)
                {
                    float textHeight = report.Item2;
                    Rect prefExplanationRect = new Rect(scrollRect.x, y, scrollRect.width, textHeight);
                    Widgets.Label(prefExplanationRect, report.Item1);
                    y += textHeight + 5f;
                }
                Text.Font = oldFont;
            }
            else
            {
                // === Scroll View Setup ===
                Text.Font = GameFont.Small;
                var interestsToDisplay = GetSortedInterestData(compPsyche, pawn);
                float viewHeight = interestsToDisplay.Count() * interestRowHeight + 3f;
                Rect scrollContentRect = new Rect(0f, 0f, interestRect.width - scrollWidth, viewHeight);

                Widgets.BeginScrollView(scrollRect, ref InterestScrollPosition, scrollContentRect);
                float y = 0f;
                float barWidth = scrollContentRect.width - interestLabelWidth - labelPadding - 5f;

                Text.Font = GameFont.Small;
                Text.Anchor = TextAnchor.MiddleLeft;
                foreach (var interestData in interestsToDisplay)
                {
                    var value = interestData.Value;
                    Rect rowRect = new Rect(0f, y, scrollContentRect.width, interestRowHeight);

                    // Hover highlight + tooltip
                    if (Mouse.IsOver(rowRect))
                    {
                        Widgets.DrawHighlight(rowRect);
                        TooltipHandler.TipRegion(rowRect, interestData.CachedDescription);
                    }

                    float barCenterX = rowRect.x + rowRect.width / 2f;
                    float centerY = rowRect.y + rowRect.height / 2f;

                    // Left label
                    Rect leftRect = new Rect(rowRect.x + labelPadding, centerY - Text.LineHeight / 2f, interestLabelWidth, Text.LineHeight);
                    Widgets.Label(leftRect, interestData.CachedLabelText);

                    // Bar background
                    Rect barRect = new Rect(leftRect.x + interestLabelWidth, centerY - interestBarHeight / 2f, barWidth, interestBarHeight);
                    Widgets.DrawBoxSolid(barRect, new Color(0.2f, 0.2f, 0.2f, 0.5f));

                    // Value bar
                    float normalizedValue = value * 0.01f; // Normalize value to 0-1 range
                    float fillWidth = normalizedValue * barWidth; // Calculate the width of the filled part
                    Rect valueRect = new Rect(barRect.x, barRect.y, fillWidth, interestBarHeight); // Bar fills from the left

                    // Color based on intensity (small = yellow, strong = green)
                    Widgets.DrawBoxSolid(valueRect, interestData.CachedLabelColor);

                    y += interestRowHeight;
                }
                Widgets.EndScrollView();
            }


            // Restore previous text settings
            Text.Anchor = oldAnchor;
            Text.Font = oldFont;
        }
    }
}
