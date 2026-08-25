using RimWorld;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;
using Verse;

namespace Maux36.RimPsyche
{
    public enum ShowMode : byte
    {
        Personality,
        Behavior,
        Facet
    }
    public enum SideMode : byte
    {
        Interest,
        Disposition,
        Sexuality
    }
    [StaticConstructorOnStartup]
    public class PsycheInfoCard
    {
        // Settings
        private static readonly bool usePreference = RimpsycheSexualitySettings.usePreferenceSystem && RimpsycheSexualitySettings.activePreferences.Count > 0;

        // Constants and style settings
        // width: 380 | 220
        public static readonly float PsycheRectWidth;
        public static readonly float PsycheRectHeight;


        public static GUIStyle style;
        public static Vector2 PersonalityScrollPosition = Vector2.zero;
        public static Vector2 InterestScrollPosition = Vector2.zero;
        public static Color barBackgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
        public static Color lightGreyColor = new Color(0.5f, 0.5f, 0.5f, 0.75f);
        public static Color barSurplusBackgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.3f);
        public static Color radarFillColor = new Color(0.5f, 1f, 0.5f, 0.6f);
        public static Color radarHighlightColor = new Color(0.6f, 0.6f, 0.6f, 0.5f);
        public static Color radarEdgeColor = new Color(0.5f, 0.5f, 0.5f, 0.6f);
        public static Color radarSpokeColor = new Color(0.5f, 0.5f, 0.5f, 0.3f);

        public static Color LowValueColor = Color.grey;
        public static Color HighValueColor = Color.green;

        public static Color LowInterestColor = new Color(0.6f, 0.55f, 0.65f, 0.5f);
        public static Color HighInterestColor = new Color(0.95f, 0.9f, 0.1f, 1f);

        public static Color LowSexualityBarColor = new Color(0.75f, 0.65f, 0.8f, 0.5f);
        public static Color HighSexualityBarColor = new Color(1f, 0.4f, 0.6f, 1f);
        public static Color HyperSexualityBarColor = new Color(0.9f, 0.15f, 0.25f, 1f);
        //public static Color maleHyperColor = new Color(0f, 0.15f, 0.6f, 1f);
        //public static Color maleHighColor = new Color(0.1f, 0.3f, 0.7f, 1f);
        //public static Color maleLowolor = new Color(0.4f, 0.7f, 0.9f, 1f);
        //public static Color femaleHyperColor = new Color(0.6f, 0f, 0.15f, 1f);
        //public static Color femaleHighColor = new Color(0.7f, 0.1f, 0.3f, 1f);
        //public static Color femaleLowolor = new Color(0.9f, 0.4f, 0.7f, 1f);
        public static Color maleLowolor = Color.grey;
        public static Color maleHighColor = Color.green;
        public static Color maleHyperColor = Color.cyan;
        public static Color femaleLowolor = Color.grey;
        public static Color femaleHighColor = Color.green;
        public static Color femaleHyperColor = Color.cyan;

        private const int SummaryRowCount = 3;
        private const int HalfSummaryRowCount = 2;
        private static readonly float dispositionIntensityWidth;

        public static readonly float headerHeight = 35f;
        public static readonly float labelPadding = 2f;
        public static readonly float innerPadding = 5f;
        public static readonly float scrollWidth = 20f;
        public static readonly float iconSize = 15f;
        public static readonly float psycheIconSize = 24f;
        public static readonly float iconSpacing = 6f;
        public static readonly float expandButtonSize = 8f;
        public static readonly float expandButtonAreaWidth = 12f;

        public static readonly string personalityTitle;
        public static readonly string behaviorTitle;
        public static readonly string facetTitle;
        public static readonly string effectHeaderString;
        public static readonly string shiftForFullString;

        public static readonly float LeftTitleTextWidth;
        public static readonly float lefPanelWidthActual;

        public static readonly float rightPanelWidthConstant = 220f;
        public static readonly float rightPanelWidthActual;
        public static readonly float rightTopPanelHeight = 150f;
        public static readonly Color LineColor = new Color(97f, 108f, 122f, 0.25f);

        private static readonly float RadarChartSize = 20f; // Diameter of the radar chart
        private static readonly float RadarChartPadding = 10f; // Padding from the header text
        private static readonly float TitlePadding = 10f; // Padding from the header text
        private static Material _lineMaterial;

        public static readonly float personalityLabelWidth;
        public static readonly float personalityIntensityWidth;
        public static readonly float personalityRowHeight = 28f;
        public static readonly float personalityBarWidth = 60f;
        public static readonly float personalityBarHeight = 4f;
        public static readonly float personalityIntensityGap = 10f;
        public static readonly float bioInfoLeftRectWidthOffset;

        public static readonly float interestLabelWidth;
        public static readonly float interestRowHeight = 28f;
        public static readonly float interestBarHeight = 4f;

        public static readonly float sexualityHeaderHeight = 35f;
        public static readonly float sexualityLineHeight = 25f;
        public static readonly float sexualityLabelWidth;
        public static readonly float sexualityBarMargin = 5f;
        public static readonly float sexualityRightMargin = 20f;
        public static readonly float sexualityBarHeight = 4f;

        //Options
        public static bool rightPanelVisible = false;
        public static bool showPreference = false;
        public static ShowMode showMode = ShowMode.Personality;
        public static SideMode sideMode = (Rimpsyche.DispositionModuleLoaded && RimpsycheSettings.ShowDispositionInUI) ? SideMode.Disposition : SideMode.Interest;

        public static bool shouldSort = false;
        public static SortMode sortOption = SortMode.Value; //0: value(high->low), 1: alphabet(a->z) 3: def
        public enum SortMode : byte
        {
            Value,
            Alphabet,
            Def
        }

        //Cache Management
        private static bool resetPreferenceHeights = true;

        static PsycheInfoCard()
        {
            //Strings
            personalityTitle = "RPC_Personality".Translate();
            behaviorTitle = "RPC_Disposition".Translate();
            facetTitle = "RPC_Facets".Translate();
            effectHeaderString = $"\n\n{"RP_PsycheEffects".Translate()}:\n";
            shiftForFullString = $"\n\n<i><color=#808080BF>{"RP_ShiftForFull".Translate()}</color></i>";

            //Psyche Tab
            //Left Panel: Title, Facet, Personality, Disposition
            //LeftPanel Titles
            GameFont oldFont = Text.Font;
            Text.Font = GameFont.Medium;
            float personalityTitleSize = Text.CalcSize(personalityTitle).x;
            float behaviorTitleSize = Text.CalcSize(behaviorTitle).x;
            float facetTitleSize = Text.CalcSize(facetTitle).x;
            Text.Font = oldFont;
            LeftTitleTextWidth = Mathf.Max(personalityTitleSize, behaviorTitleSize, facetTitleSize);
            //LeftPanel Title Max Width Calculation
            var LeftTitleWidth = RadarChartSize + RadarChartPadding + LeftTitleTextWidth + 3f * iconSize + 3f * iconSpacing + 2f * TitlePadding;

            //LeftPanel Contents
            //Personality
            personalityLabelWidth = RimpsycheDatabase.maxPersonalityLabelWidth;
            personalityIntensityWidth = RimpsycheDatabase.maxPersonalityIntensityWidth;
            var BarMinSize = 60f;
            var barModePersonalityWidth = labelPadding + personalityLabelWidth + labelPadding + BarMinSize + labelPadding + personalityLabelWidth + labelPadding;
            var labelModePersonalityWidth = labelPadding + personalityIntensityWidth + personalityIntensityGap + personalityLabelWidth + labelPadding;
            //Facets
            var facetContentWidth = labelPadding + RimpsycheDatabase.maxFacetLabelWidth + labelPadding + BarMinSize + labelPadding + RimpsycheDatabase.maxFacetLabelWidth + labelPadding;
            //Disposition
            var dispositionContentWidth = labelPadding + RimpsycheDatabase.maxDescriptorLabelWidth + labelPadding + RimpsycheDatabase.dispositionIntensityWidth + labelPadding ;
            lefPanelWidthActual = Mathf.Max(LeftTitleWidth, labelModePersonalityWidth, barModePersonalityWidth, facetContentWidth, dispositionContentWidth) + scrollWidth;
            lefPanelWidthActual = Mathf.Max(lefPanelWidthActual, 360f);
            personalityBarWidth = lefPanelWidthActual - 4 * labelPadding - 2 * personalityLabelWidth - scrollWidth;

            //RightPanel: Sexuality, Interests, Preference
            interestLabelWidth = RimpsycheDatabase.maxInterestLabelWidth;
            sexualityLabelWidth = RimpsycheDatabase.maxSexualityTabLabelWidth;
            var interestLabelDiff = Mathf.Max(interestLabelWidth - 130f, 0f);
            var sexualityLabelDiff = Mathf.Max(sexualityLabelWidth - 70f, 0f);
            rightPanelWidthActual = rightPanelWidthConstant + Mathf.Max(interestLabelDiff, sexualityLabelDiff);

            PsycheRectWidth = Mathf.Min(lefPanelWidthActual + expandButtonAreaWidth + rightPanelWidthActual, UI.screenWidth * 0.8f);
            PsycheRectHeight = Mathf.Clamp(UI.screenHeight * 0.5f, 350f, 480f);
            EnsureMaterial();

            //Character Tab
            //Character Card width 480f. 0.5f - 10f | 230f
            //LeftSide, Personality.
            bioInfoLeftRectWidthOffset = Mathf.Max(0f, labelModePersonalityWidth - 230f);

            //for disposition
            dispositionIntensityWidth = RimpsycheDatabase.dispositionIntensityWidth;

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
        private static List<PersonalityDisplayData> cachedPersonalitySummaryData = null;
        private static Dictionary<ushort, List<string>> cachedPersonalityEffects = new();
        private static List<BehaviorData> cachedBehaviorData = null;
        private static List<InterestDisplayData> cachedInterestData = null;
        private static List<Vector2> cachedValuePointData = null;
        private static List<Vector2> cachedMaxPointData = null;
        private static string cachedSexualityDescription = string.Empty;
        private static List<float> cachedViewerHeights = null;
        private static int lastPawnId = -1;
        public struct BehaviorData
        {
            public string Label;
            public string Intensity;
            public string Description;
            public string Tooltip;
            public float NormalizedAbsValue;
            public bool IsSignificant;
        }
        private struct PersonalityDisplayData
        {
            public PersonalityDef Personality;
            public float Value;
            public float AbsValue;
            public string CachedLabelText;
            public string CachedIntensityKeyText;
            public string CachedDescription;
            public string CachedShortDescription;
            public string CachedFullDescription;
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
            lastPawnId = -1;
            cachedPersonalityData = null;
            cachedPersonalitySummaryData = null;
            cachedBehaviorData = null;
            cachedInterestData = null;
            cachedValuePointData = null;
            cachedSexualityDescription = string.Empty;
            resetPreferenceHeights = true;
            var allPrefDefs = DefDatabase<PreferenceDef>.AllDefsListForReading;
            for (int i = 0; i < allPrefDefs.Count; i++)
            {
                var pref = allPrefDefs[i];
                if (pref.isActive)
                    pref.worker.ClearViewerCache();
            }
        }

        public static void GenerateCacheData(CompPsyche compPsyche, Pawn currentPawn)
        {
            resetPreferenceHeights = true;
            cachedSexualityDescription = string.Empty;
            lastPawnId = currentPawn.thingIDNumber;
            GenerateSortedPersonalityData(compPsyche, currentPawn);
            GenerateSortedInterestData(compPsyche, currentPawn);
            var allPrefDefs = DefDatabase<PreferenceDef>.AllDefsListForReading;
            for (int i = 0; i < allPrefDefs.Count; i++)
            {
                var pref = allPrefDefs[i];
                if (pref.isActive)
                    pref.worker.ClearViewerCache();
            }
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
            Rect personalityRect = new Rect(
                totalRect.x,
                totalRect.y,
                lefPanelWidthActual,
                totalRect.height
            );
            Rect MiddleRect = new Rect(
                personalityRect.xMax,
                totalRect.y,
                expandButtonAreaWidth,
                totalRect.height
            );
            Rect RightRect = new Rect(
                MiddleRect.xMax,
                totalRect.y,
                rightPanelWidthActual,
                totalRect.height
            );
            float rightTopPanelHeight = 0f;
            if (showSexuality)
            {
                rightTopPanelHeight = 150f;
            }

            // Define the sexuality panel rect
            Rect sexualityRect = new Rect(
                RightRect.x,
                RightRect.y,
                rightPanelWidthActual,
                rightTopPanelHeight
            );

            // Define the interest panel rect
            Rect interestRect = new Rect(
                RightRect.x,
                sexualityRect.yMax,
                rightPanelWidthActual,
                RightRect.height - rightTopPanelHeight
            );
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

            Rect openButtonRect = new Rect(
                0.5f * (MiddleRect.x + MiddleRect.xMax) - expandButtonSize / 2, // Center the button in the buttonAreaWidth
                totalRect.y + (totalRect.height / 2) - (expandButtonSize / 2), // Vertically center the button
                expandButtonSize,
                expandButtonSize
            );
            if (psycheEnabled)
            {
                var sideButton = rightPanelVisible switch
                {
                    true  => Rimpsyche_UI_Utility.HideButton,
                    false => Rimpsyche_UI_Utility.RevealButton
                };
                if (Widgets.ButtonImage(openButtonRect, sideButton))
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
            if (psycheEnabled && rightPanelVisible)
            {
                if (showSexuality)
                {
                    DrawSexualityBox(sexualityRect, compPsyche, pawn);
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
            if (currentPawn.thingIDNumber != lastPawnId || cachedPersonalityData == null)
            {
                GenerateCacheData(compPsyche, currentPawn);
                return cachedPersonalityData;
            }
            if (shouldSort)
            {
                cachedPersonalityData = (sortOption switch
                {
                    SortMode.Value => cachedPersonalityData.OrderByDescending(p => p.AbsValue),
                    SortMode.Alphabet => cachedPersonalityData.OrderBy(p => p.Personality.label),
                    SortMode.Def => cachedPersonalityData.OrderBy(p => RimpsycheDatabase.PersonalityOrder[p.Personality.shortHash]),
                    _ => cachedPersonalityData.OrderByDescending(p => p.AbsValue)
                }).ToList();
                shouldSort = false;
            }
            return cachedPersonalityData;
        }
        private static List<PersonalityDisplayData> GetSortedPersonalitySummaryData(CompPsyche compPsyche, Pawn currentPawn)
        {
            if (currentPawn.thingIDNumber != lastPawnId || cachedPersonalitySummaryData == null)
            {
                GenerateCacheData(compPsyche, currentPawn);
            }
            return cachedPersonalitySummaryData;
        }
        private static List<BehaviorData> GetBehaviorData(CompPsyche compPsyche, Pawn currentPawn)
        {
            if (currentPawn.thingIDNumber != lastPawnId || cachedBehaviorData == null)
            {
                GenerateCacheData(compPsyche, currentPawn);
            }
            return cachedBehaviorData;
        }
        private static List<InterestDisplayData> GetSortedInterestData(CompPsyche compPsyche, Pawn currentPawn)
        {
            if (currentPawn.thingIDNumber != lastPawnId || cachedInterestData == null)
            {
                GenerateCacheData(compPsyche, currentPawn);
            }
            return cachedInterestData;
        }
        private static List<float> GetViewerHeights(Pawn currentPawn)
        {
            //List<(string, float)> cachedPreferenceReport
            if (resetPreferenceHeights == true || cachedViewerHeights == null)
            {
                GenerateViewerHeights(currentPawn);
            }
            return cachedViewerHeights;
        }
        private static string GetSexualityTooltip(CompPsyche compPsyche)
        {
            if (cachedSexualityDescription == string.Empty)
            {
                cachedSexualityDescription = compPsyche.Sexuality.GetOrientationDescription();
            }
            return cachedSexualityDescription;
        }
        private static List<Vector2> GetValuePointData(Vector2 center, CompPsyche compPsyche, Pawn currentPawn)
        {
            if (currentPawn.thingIDNumber != lastPawnId || cachedInterestData == null)
            {
                GenerateValuePointData(center, compPsyche);
                return cachedValuePointData;
            }
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
            GenerateBehaviorData(compPsyche);
            var personalityDefList = DefDatabase<PersonalityDef>.AllDefsListForReading;
            var rawData = new List<PersonalityDisplayData>();
            foreach (var personality in personalityDefList)
            {
                float value = compPsyche.Personality.GetPersonality(personality);
                float absValue = Mathf.Abs(value);
                string cachedLabelText = ((value >= 0) ? personality.high : personality.low).CapitalizeFirst();
                string cachedIntensityKeyText = Rimpsyche_Utility.GetPersonalityIntensity(value);
                Color cachedLabelColor = Color.Lerp(LowValueColor, HighValueColor, absValue);
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
                rawData.Add(new PersonalityDisplayData
                {
                    Personality = personality,
                    Value = value,
                    AbsValue = absValue,
                    CachedLabelText = cachedLabelText,
                    CachedIntensityKeyText = cachedIntensityKeyText,
                    CachedLabelColor = cachedLabelColor,
                    CachedShortDescription = personalityShortDesc,
                    CachedFullDescription = personalityFullDesc,
                    CachedDescription = personalityDesc
                });
            }
            var orderedData = rawData.OrderByDescending(p => p.AbsValue);
            cachedPersonalitySummaryData = orderedData.Take(SummaryRowCount * 2).ToList();
            if (sortOption == SortMode.Value)
            {
                cachedPersonalityData = orderedData.ToList();
            }
            else if (sortOption == SortMode.Alphabet)
            {
                cachedPersonalityData = rawData.OrderBy(p => p.Personality.label).ToList();
            }
            else
            {
                cachedPersonalityData = rawData;
            }
            shouldSort = false;
        }
        private static void GenerateBehaviorData(CompPsyche compPsyche)
        {
            cachedPersonalityEffects.Clear();
            var sortedData = new List<BehaviorData>();

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
                var result = new BehaviorData
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
        private static void GenerateSortedInterestData(CompPsyche compPsyche, Pawn currentPawn)
        {
            var interestList = RimpsycheDatabase.InterestList;
            var sortedData = new List<InterestDisplayData>();

            foreach (var interest in interestList)
            {
                float value = compPsyche.Interests.GetOrGenerateAdjustedInterestScore(interest);
                float lerpValue = value * 0.01f;
                string cachedLabelText = interest.label;
                Color cachedLabelColor = Color.Lerp(LowInterestColor, HighInterestColor, lerpValue);
                sortedData.Add(new InterestDisplayData
                {
                    Interest = interest,
                    Value = value,
                    CachedLabelText = cachedLabelText,
                    CachedLabelColor = cachedLabelColor,
                    CachedDescription = $"{interest.label}: {Math.Round(value, 1)}\n{interest.description}\n\n{"RimpsycheTopicHeader".Translate()}\n{RimpsycheDatabase.InterstTopicStringDict[interest.id]}"});
                }
            sortedData = sortedData.OrderByDescending(p => p.Value).ToList();
            cachedInterestData = sortedData;
        }
        private static void GenerateViewerHeights(Pawn currentPawn)
        {
            cachedViewerHeights = new();
            var allPrefDefs = DefDatabase<PreferenceDef>.AllDefsListForReading;
            for (int i = 0; i < allPrefDefs.Count; i++)
            {
                var prefDef = allPrefDefs[i];
                if (!prefDef.isActive)
                    continue;
                float viewerHeight = prefDef.worker.GetViewerHeight(currentPawn);
                cachedViewerHeights.Add(viewerHeight);
            }
            resetPreferenceHeights = false;
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
            if (!RimpsycheSettings.ShowDispositionInUI && showMode == ShowMode.Behavior)
            {
                showMode = ShowMode.Personality;
            }
            else if (!RimpsycheSettings.showFacetInUI && showMode == ShowMode.Facet)
            {
                showMode = ShowMode.Personality;
            }

            // Draw Header
            Rect headerRect = new Rect(personalityRect.x, personalityRect.y, personalityRect.width, headerHeight);
            GUI.BeginGroup(headerRect);
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleCenter;
            Rect titleRect = new Rect(0f, 0f, headerRect.width, headerRect.height);
            var headerString = showMode switch
            {
                ShowMode.Personality => personalityTitle,
                ShowMode.Behavior => behaviorTitle,
                ShowMode.Facet => facetTitle,
                _ => personalityTitle
            };
            Widgets.Label(titleRect, headerString);
            if (RimpsycheSettings.showFacetGraph)
            {
                float radarChartX = (headerRect.width / 2f) - (LeftTitleTextWidth / 2f) - RadarChartSize - RadarChartPadding;
                Rect radarChartRect = new Rect(radarChartX, titleRect.y + (titleRect.height - RadarChartSize) / 2f, RadarChartSize, RadarChartSize);
                DrawRadarChart(radarChartRect, compPsyche, pawn);
            }

            // Icon on the right
            float spacing = 6;

            float actionIconX = (headerRect.width / 2f) + (LeftTitleTextWidth / 2f) + 8f;
            float actionIconY = titleRect.y + (titleRect.height - iconSize) / 2f;

            // Sort Mode Toggle
            Rect sortIconRect = new Rect(actionIconX, actionIconY, iconSize, iconSize);
            bool isSortDisabled = (showMode == ShowMode.Behavior || showMode == ShowMode.Facet);
            if (isSortDisabled)
            {
                Widgets.ButtonImage(sortIconRect, Rimpsyche_UI_Utility.SortButton, barBackgroundColor, barBackgroundColor, false, "RimpsycheSortDisabled".Translate());
            }
            else
            {
                var (optionTooltipKey, nextOption) = sortOption switch
                {
                    SortMode.Value => ("RimpsycheSortAlphabet", SortMode.Alphabet),
                    SortMode.Alphabet => ("RimpsycheSortDef", SortMode.Def),
                    _ => ("RimpsycheSortValue", SortMode.Value) // Handles 2 and fallback
                };
                if (Widgets.ButtonImage(sortIconRect, Rimpsyche_UI_Utility.SortButton, true, optionTooltipKey.Translate()))
                {
                    sortOption = nextOption;
                    shouldSort = true;
                }
            }
            actionIconX += iconSize + spacing;

            // View Mode Toggle
            if (RimpsycheSettings.ShowDispositionInUI || RimpsycheSettings.showFacetInUI)
            {
                Rect viewIconRect = new Rect(actionIconX, actionIconY, iconSize, iconSize);
                var (icon, modeTooltipKey, nextMode, resetScroll) = showMode switch
                {
                    ShowMode.Personality => RimpsycheSettings.ShowDispositionInUI
                        ? (Rimpsyche_UI_Utility.ViewBehaviorButton, "RimpsycheShowDisposition", ShowMode.Behavior, true)
                        : RimpsycheSettings.showFacetInUI
                        ? (Rimpsyche_UI_Utility.ViewFacetButton, "RimpsycheShowFacet", ShowMode.Facet, true)
                        : (RimpsycheSettings.personalityAsBar ? Rimpsyche_UI_Utility.ViewBarButton : Rimpsyche_UI_Utility.ViewListButton, "RimpsycheShowPersonality", ShowMode.Personality, false),
                    ShowMode.Behavior => RimpsycheSettings.showFacetInUI
                        ? (Rimpsyche_UI_Utility.ViewFacetButton, "RimpsycheShowFacet", ShowMode.Facet, true)
                        : (RimpsycheSettings.personalityAsBar ? Rimpsyche_UI_Utility.ViewBarButton : Rimpsyche_UI_Utility.ViewListButton, "RimpsycheShowPersonality", ShowMode.Personality, false),
                    _ => (RimpsycheSettings.personalityAsBar ? Rimpsyche_UI_Utility.ViewBarButton : Rimpsyche_UI_Utility.ViewListButton, "RimpsycheShowPersonality", ShowMode.Personality, true)
                };
                if (Widgets.ButtonImage(viewIconRect, icon))
                {
                    showMode = nextMode;
                    if (resetScroll) PersonalityScrollPosition = Vector2.zero;
                }
                TooltipHandler.TipRegion(viewIconRect, modeTooltipKey.Translate());
                actionIconX += iconSize + spacing;
            }

            Rect editIconRect = new Rect(actionIconX, actionIconY, iconSize, iconSize);
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
            var behaviorsToDisplay = GetBehaviorData(compPsyche, pawn);
            var viewHeight = showMode switch
            {
                ShowMode.Personality => personalitiesToDisplay.Count() * personalityRowHeight + 3f,
                ShowMode.Behavior => behaviorsToDisplay.Count() * personalityRowHeight + 3f,
                ShowMode.Facet => 15 * personalityRowHeight + 3f,
                _ => 15 * personalityRowHeight + 3f,
            };
            Rect scrollContentRect = new Rect(0f, 0f, personalityRect.width - scrollWidth, viewHeight);

            Rect scrollRect = new Rect(
                personalityRect.x,
                personalityRect.y + headerHeight,
                personalityRect.width,
                personalityRect.height - headerHeight
            );

            Widgets.BeginScrollView(scrollRect, ref PersonalityScrollPosition, scrollContentRect);

            float y = 0f;


            if (showMode == ShowMode.Personality && !RimpsycheSettings.personalityAsBar)
            {
                Text.Anchor = TextAnchor.MiddleLeft;
                int firstIndex = Mathf.FloorToInt(PersonalityScrollPosition.y / personalityRowHeight);
                int lastIndex = Mathf.FloorToInt((PersonalityScrollPosition.y + scrollRect.height) / personalityRowHeight);
                firstIndex = Mathf.Clamp(firstIndex, 0, personalitiesToDisplay.Count - 1);
                lastIndex = Mathf.Clamp(lastIndex, 0, personalitiesToDisplay.Count - 1);
                for (int i = firstIndex; i <= lastIndex; i++)
                {
                    var pData = personalitiesToDisplay[i];
                    y = i * personalityRowHeight;
                    Rect rowRect = new Rect(0f, y, scrollContentRect.width, personalityRowHeight);

                    // Hover highlight + tooltip
                    if (Mouse.IsOver(rowRect))
                    {
                        Widgets.DrawHighlight(rowRect);
                        int uniqueId = i;
                        TipSignal tip = new TipSignal(() => Event.current.shift ? pData.CachedFullDescription : pData.CachedDescription, uniqueId);
                        TooltipHandler.TipRegion(rowRect, tip);
                    }

                    GUI.color = pData.CachedLabelColor;
                    Rect intensityRect = new Rect(rowRect.x + labelPadding, rowRect.y, personalityIntensityWidth, personalityRowHeight);
                    Widgets.Label(intensityRect, pData.CachedIntensityKeyText);

                    float mainLabelX = intensityRect.xMax + personalityIntensityGap;
                    float mainLabelWidth = scrollContentRect.width - mainLabelX - labelPadding;

                    Rect labelRect = new Rect(mainLabelX, rowRect.y, mainLabelWidth, personalityRowHeight);

                    Widgets.Label(labelRect, pData.CachedLabelText);
                    GUI.color = Color.white; // Reset color
                }
                Text.Anchor = TextAnchor.MiddleCenter;
            }
            else if (showMode == ShowMode.Personality && RimpsycheSettings.personalityAsBar)
            {
                int firstIndex = Mathf.FloorToInt(PersonalityScrollPosition.y / personalityRowHeight);
                int lastIndex = Mathf.FloorToInt((PersonalityScrollPosition.y + scrollRect.height) / personalityRowHeight);
                firstIndex = Mathf.Clamp(firstIndex, 0, personalitiesToDisplay.Count - 1);
                lastIndex = Mathf.Clamp(lastIndex, 0, personalitiesToDisplay.Count - 1);

                for (int i = firstIndex; i <= lastIndex; i++)
                {
                    var pData = personalitiesToDisplay[i];
                    var personality = pData.Personality;
                    var value = pData.Value;
                    var (leftLabel, rightLabel, leftColor, rightColor) = (personality.low.CapitalizeFirst(), personality.high.CapitalizeFirst(), Color.red, Color.green);

                    y = i * personalityRowHeight;
                    Rect rowRect = new Rect(0f, y, scrollContentRect.width, personalityRowHeight);

                    // Hover highlight + tooltip
                    if (Mouse.IsOver(rowRect))
                    {
                        Widgets.DrawHighlight(rowRect);
                        int uniqueId = i;
                        TipSignal tip = new TipSignal(() => Event.current.shift ? pData.CachedFullDescription : pData.CachedDescription, uniqueId);
                        TooltipHandler.TipRegion(rowRect, tip);
                    }

                    float barCenterX = rowRect.x + rowRect.width / 2f;
                    float centerY = rowRect.y + rowRect.height / 2f;
                    float textY = centerY - Text.LineHeight / 2f;
                    float barY = centerY - personalityBarHeight / 2f;

                    Color originalColor = GUI.color;
                    var labelColor = Color.Lerp(lightGreyColor, originalColor, pData.AbsValue);
                    GUI.color = labelColor;
                    // Left label
                    Rect leftRect = new Rect(rowRect.x + labelPadding, textY, personalityLabelWidth, Text.LineHeight);
                    Text.Anchor = TextAnchor.MiddleLeft;
                    Widgets.Label(leftRect, leftLabel);

                    // Right label
                    Rect rightRect = new Rect(rowRect.xMax - personalityLabelWidth - labelPadding, textY, personalityLabelWidth, Text.LineHeight);
                    Text.Anchor = TextAnchor.MiddleRight;
                    Widgets.Label(rightRect, rightLabel);
                    GUI.color = originalColor;

                    // Bar background
                    Rect barRect = new Rect(barCenterX - personalityBarWidth / 2f, barY, personalityBarWidth, personalityBarHeight);
                    Widgets.DrawBoxSolid(barRect, barBackgroundColor);

                    // Value bar
                    float halfBar = pData.AbsValue * (personalityBarWidth) / 2f;
                    Rect valueRect = value >= 0
                        ? new Rect(barCenterX, barRect.y, halfBar, personalityBarHeight)
                        : new Rect(barCenterX - halfBar, barRect.y, halfBar, personalityBarHeight);

                    // Color based on intensity
                    Widgets.DrawBoxSolid(valueRect, pData.CachedLabelColor);
                }
            }
            else if (showMode == ShowMode.Behavior)
            {
                Color originalColor = GUI.color;
                TextAnchor originalAnchor = Text.Anchor;
                if (behaviorsToDisplay.Count == 0)
                {
                    Rect emptyRect = new Rect(10f, y, scrollContentRect.width - 10f, personalityRowHeight);
                    GUI.color = Color.gray;
                    Widgets.Label(emptyRect, "RPC_NoBehavior".Translate());
                    GUI.color = originalColor;
                    y += personalityRowHeight;
                }
                else
                {
                    int firstIndex = Mathf.FloorToInt(PersonalityScrollPosition.y / personalityRowHeight);
                    int lastIndex = Mathf.FloorToInt((PersonalityScrollPosition.y + scrollRect.height) / personalityRowHeight);
                    firstIndex = Mathf.Clamp(firstIndex, 0, behaviorsToDisplay.Count - 1);
                    lastIndex = Mathf.Clamp(lastIndex, 0, behaviorsToDisplay.Count - 1);
                    for (int i = firstIndex; i <= lastIndex; i++)
                    {
                        var entry = behaviorsToDisplay[i];
                        y = i * personalityRowHeight;
                        Rect outerRowRect = new Rect(0f, y, scrollContentRect.width, personalityRowHeight);
                        Rect rowRect = new Rect(labelPadding, y, outerRowRect.width - 2 * labelPadding, personalityRowHeight);
                        Rect intensityRect = new Rect(rowRect.xMax - dispositionIntensityWidth, rowRect.y, dispositionIntensityWidth, rowRect.height);
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
            else if (showMode == ShowMode.Facet)
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

            Widgets.EndScrollView();

            // Restore previous text settings
            Text.Anchor = oldAnchor;
            Text.Font = oldFont;
        }

        public static void DrawBioPersonalitySummary(Rect fedRect, Pawn pawn)
        {
            if (!RimpsycheSettings.ShowSummaryInBio)
                return;
            CompPsyche comp = pawn?.compPsyche();
            if (comp == null) return;
            var rect = new Rect(fedRect.x, fedRect.yMax, fedRect.width, (float)RimpsycheSettings.ExtraBioHeight);

            var originalFont = Text.Font;
            var originalAnchor = Text.Anchor;
            var originalColor = GUI.color;

            Text.Anchor = TextAnchor.MiddleLeft;

            var personalities = GetSortedPersonalitySummaryData(comp, pawn).ToList();

            // Split the rect
            var LeftProp = 0.5f;
            var LeftWidthOffset = 0f;
            if (RimpsycheSettings.showSideInfoInSummary)
                LeftWidthOffset = bioInfoLeftRectWidthOffset;
            Rect leftRect = new Rect(rect.x, rect.y, rect.width * LeftProp -10f + LeftWidthOffset, rect.height);
            Rect rightRect = new Rect(leftRect.xMax+20f, rect.y, rect.width * (1- LeftProp) - 10f - LeftWidthOffset, rect.height);

            Text.Font = GameFont.Small;
            float leftY = leftRect.y;
            float rightY = rightRect.y;

            if (!RimpsycheSettings.showSideInfoInSummary)
            {
                // Unified
                Widgets.Label(new Rect(rect.x, rect.y, rect.width, 22f), "RPC_Personality".Translate());
                if (!RimpsycheSettings.usePsycheTab)
                {
                    float psycheIconX = rect.xMax - psycheIconSize;
                    Rect psycheIconRect = new Rect(psycheIconX, rect.y + 11f - psycheIconSize * 0.5f, psycheIconSize, psycheIconSize);
                    Rimpsyche_UI_Utility.DrawEditButton(psycheIconRect, pawn);
                }
                leftY += 24f;
                GUI.color = LineColor;
                Widgets.DrawLineHorizontal(rect.x, leftY, rect.width);
                GUI.color = originalColor;
                leftY += 5f;
                rightY = leftY;
            }
            else
            {
                // left
                Widgets.Label(new Rect(leftRect.x, leftY, leftRect.width, 22f), "RPC_Personality".Translate());
                if (!RimpsycheSettings.usePsycheTab)
                {
                    float psycheIconX = leftRect.xMax - psycheIconSize;
                    Rect psycheIconRect = new Rect(psycheIconX, leftRect.y + 11f - psycheIconSize * 0.5f, psycheIconSize, psycheIconSize);
                    Rimpsyche_UI_Utility.DrawEditButton(psycheIconRect, pawn);
                }
                leftY += 24f;

                GUI.color = LineColor;
                Widgets.DrawLineHorizontal(leftRect.x, leftY, leftRect.width);
                GUI.color = originalColor;
                leftY += 5f;
            }

            var showCount = SummaryRowCount;
            if (!RimpsycheSettings.showSideInfoInSummary)
            {
                showCount = HalfSummaryRowCount;
            }

            // left
            for (int i = 0; i < showCount; i++)
            {
                var personality = personalities[i];
                Rect rowRect = new Rect(leftRect.x, leftY, leftRect.width, 22f);
                Widgets.DrawHighlightIfMouseover(rowRect);
                TooltipHandler.TipRegion(rowRect, personality.CachedShortDescription);
                if (RimpsycheSettings.personalityAsBar)
                {
                    // Left Side: Personality Label
                    Rect labelRect = new Rect(rowRect.x + labelPadding, rowRect.y, personalityLabelWidth, rowRect.height);
                    var personalityColor = Color.Lerp(lightGreyColor, Color.white, personality.AbsValue);
                    GUI.color = personalityColor;
                    Widgets.Label(labelRect, personality.CachedLabelText);

                    // Right Side: Small Widget Bar
                    Rect barRect = new Rect(labelRect.xMax + labelPadding, rowRect.y + 0.5f * (rowRect.height - personalityBarHeight), rowRect.xMax - labelRect.xMax - (2 * labelPadding), personalityBarHeight);

                    // Draw Bar Background (Dark Gray)
                    Widgets.DrawBoxSolid(barRect, barBackgroundColor);
                    float fillPercent = Mathf.Clamp01(personality.AbsValue);
                    Rect fillRect = new Rect(barRect.x, barRect.y, barRect.width * fillPercent, barRect.height);
                    Widgets.DrawBoxSolid(fillRect, personality.CachedLabelColor);
                }
                else
                {
                    GUI.color = personality.CachedLabelColor;
                    Rect intensityRect = new Rect(rowRect.x + labelPadding, rowRect.y, personalityIntensityWidth, personalityRowHeight);
                    Widgets.Label(intensityRect, personality.CachedIntensityKeyText);

                    float mainLabelX = intensityRect.xMax + personalityIntensityGap;
                    float mainLabelWidth = rowRect.width - mainLabelX - labelPadding;

                    Rect labelRect = new Rect(mainLabelX, rowRect.y, personalityLabelWidth, personalityRowHeight);

                    Widgets.Label(labelRect, personality.CachedLabelText);
                }
                GUI.color = originalColor;
                leftY += 22f;
            }

            // right
            if (!RimpsycheSettings.showSideInfoInSummary)
            {
                for (int i = HalfSummaryRowCount; i < 2 * HalfSummaryRowCount; i++)
                {
                    var personality = personalities[i];
                    Rect rowRect = new Rect(rightRect.x, rightY, rightRect.width, 22f);
                    Widgets.DrawHighlightIfMouseover(rowRect);
                    TooltipHandler.TipRegion(rowRect, personality.CachedShortDescription);
                    if (RimpsycheSettings.personalityAsBar)
                    {
                        // Left Side: Personality Label
                        Rect labelRect = new Rect(rowRect.x + labelPadding, rowRect.y, personalityLabelWidth, rowRect.height);
                        var personalityColor = Color.Lerp(lightGreyColor, Color.white, personality.AbsValue);
                        GUI.color = personalityColor;
                        Widgets.Label(labelRect, personality.CachedLabelText);

                        // Right Side: Small Widget Bar
                        Rect barRect = new Rect(labelRect.xMax + labelPadding, rowRect.y + 0.5f * (rowRect.height - personalityBarHeight), rowRect.xMax - labelRect.xMax - (2 * labelPadding), personalityBarHeight);

                        // Draw Bar Background (Dark Gray)
                        Widgets.DrawBoxSolid(barRect, barBackgroundColor);
                        float fillPercent = Mathf.Clamp01(personality.AbsValue);
                        Rect fillRect = new Rect(barRect.x, barRect.y, barRect.width * fillPercent, barRect.height);
                        Widgets.DrawBoxSolid(fillRect, personality.CachedLabelColor);
                    }
                    else
                    {
                        GUI.color = personality.CachedLabelColor;
                        Rect intensityRect = new Rect(rowRect.x + labelPadding, rowRect.y, personalityIntensityWidth, personalityRowHeight);
                        Widgets.Label(intensityRect, personality.CachedIntensityKeyText);

                        float mainLabelX = intensityRect.xMax + personalityIntensityGap;
                        float mainLabelWidth = rowRect.width - mainLabelX - labelPadding;

                        Rect labelRect = new Rect(mainLabelX, rowRect.y, personalityLabelWidth, personalityRowHeight);

                        Widgets.Label(labelRect, personality.CachedLabelText);
                    }

                    GUI.color = originalColor;
                    rightY += 22f;
                }
            }
            else
            {
                //Sexuality Guard
                bool canUseSexuality = Rimpsyche.SexualityModuleLoaded && comp.Sexuality.SexualityExpressed();
                bool canUseDisposition = Rimpsyche.DispositionModuleLoaded && RimpsycheSettings.ShowDispositionInUI;
                if (sideMode == SideMode.Sexuality && !canUseSexuality)
                {
                    sideMode = GetNextSideMode(sideMode, canUseDisposition, canUseSexuality);
                }
                if (sideMode == SideMode.Disposition && !canUseDisposition)
                {
                    sideMode = GetNextSideMode(sideMode, canUseDisposition, canUseSexuality);
                }
                var rightHeaderRect = new Rect(rightRect.x, rightY, rightRect.width, 22f);
                string rightSideLabel = sideMode switch
                {
                    SideMode.Interest => "RPC_Interest".Translate(),
                    SideMode.Disposition => "RPC_Disposition".Translate(),
                    SideMode.Sexuality => "RPC_Sexuality".Translate(),
                    _ => ""
                };
                Widgets.Label(rightHeaderRect, rightSideLabel);
                bool showToggleButton = canUseDisposition || canUseSexuality;
                if (showToggleButton)
                {
                    float toggleIconX = rightHeaderRect.xMax - iconSize - iconSpacing;
                    Rect toggleIconRect = new Rect(toggleIconX, rightHeaderRect.y + (rightHeaderRect.height - iconSize) * 0.5f, iconSize, iconSize);
                    var nextSideMode = GetNextSideMode(sideMode, canUseDisposition, canUseSexuality);
                    Texture2D icon = nextSideMode switch
                    {
                        SideMode.Interest => Rimpsyche_UI_Utility.InterestButton,
                        SideMode.Disposition => Rimpsyche_UI_Utility.DispositionButton,
                        SideMode.Sexuality => Rimpsyche_UI_Utility.PreferenceButton,
                        _ => Rimpsyche_UI_Utility.InterestButton
                    };
                    if (Widgets.ButtonImage(toggleIconRect, icon))
                    {
                        sideMode = nextSideMode;
                    }

                    TooltipHandler.TipRegion(toggleIconRect, nextSideMode switch
                    {
                        SideMode.Interest => "RimpsycheShowInterest".Translate(),
                        SideMode.Disposition => "RimpsycheShowDisposition".Translate(),
                        SideMode.Sexuality => "RimpsycheShowSexuality".Translate(),
                        _ => ""
                    });
                }
                rightY += 24f;

                GUI.color = LineColor;
                Widgets.DrawLineHorizontal(rightRect.x, rightY, rightRect.width);
                GUI.color = originalColor;
                rightY += 5f;

                if (sideMode == SideMode.Interest)
                {
                    float barWidth = rightRect.width - interestLabelWidth - labelPadding - 5f;
                    int shownCount = 0;
                    var interests = GetSortedInterestData(comp, pawn);
                    foreach (var interestData in interests)
                    {
                        var value = interestData.Value;
                        Rect rowRect = new Rect(rightRect.x, rightY, rightRect.width, 22f);

                        // Hover highlight + tooltip
                        if (Mouse.IsOver(rowRect))
                        {
                            Widgets.DrawHighlight(rowRect);
                            TooltipHandler.TipRegion(rowRect, interestData.CachedDescription);
                        }

                        float barCenterX = rowRect.x + rowRect.width / 2f;
                        float centerY = rowRect.y + rowRect.height / 2f;

                        // Left label
                        Rect interestLeftRect = new Rect(rowRect.x + labelPadding, centerY - rowRect.height / 2f, interestLabelWidth, rowRect.height);
                        Widgets.Label(interestLeftRect, interestData.CachedLabelText);
                        // Bar background
                        Rect interestBarRect = new Rect(interestLeftRect.x + interestLabelWidth, centerY - interestBarHeight / 2f, barWidth, interestBarHeight);
                        Widgets.DrawBoxSolid(interestBarRect, new Color(0.2f, 0.2f, 0.2f, 0.5f));

                        // Value bar
                        float normalizedValue = value * 0.01f; // Normalize value to 0-1 range
                        float fillWidth = normalizedValue * barWidth; // Calculate the width of the filled part
                        Rect valueRect = new Rect(interestBarRect.x, interestBarRect.y, fillWidth, interestBarHeight); // Bar fills from the left

                        // Color based on intensity (small = yellow, strong = green)
                        Widgets.DrawBoxSolid(valueRect, interestData.CachedLabelColor);

                        rightY += 22F;
                        shownCount++;
                        if (shownCount >= SummaryRowCount)
                            break;
                    }
                }
                else if (sideMode == SideMode.Disposition)
                {
                    int shownCount = 0;
                    var behaviors = GetBehaviorData(comp, pawn);
                    foreach (var behavior in behaviors)
                    {
                        if (!behavior.IsSignificant)
                            continue;
                        Rect outerRowRect = new Rect(rightRect.x, rightY, rightRect.width, 22f);
                        Rect rowRect = new Rect(outerRowRect.x + labelPadding, rightY, rightRect.width - 2 * labelPadding, 22f);
                        Rect intensityRect = new Rect(rowRect.xMax - dispositionIntensityWidth - labelPadding, rowRect.y, dispositionIntensityWidth, rowRect.height);
                        Widgets.DrawHighlightIfMouseover(outerRowRect);
                        TooltipHandler.TipRegion(outerRowRect, behavior.Tooltip);
                        Widgets.Label(rowRect, behavior.Label);
                        Widgets.Label(intensityRect, behavior.Intensity);
                        rightY += 22f;
                        shownCount++;
                        if (shownCount >= SummaryRowCount)
                            break;
                    }
                    if (shownCount == 0)
                    {
                        Rect rowRect = new Rect(rightRect.x, rightY, rightRect.width, 22f);
                        GUI.color = Color.gray;
                        Text.Anchor = TextAnchor.MiddleCenter;
                        Widgets.Label(rowRect, "RPC_NoBehavior".Translate());
                        Text.Anchor = TextAnchor.MiddleLeft;
                        GUI.color = originalColor;
                    }
                }
                else if (sideMode == SideMode.Sexuality)
                {
                    float sexualityRectX = rightRect.x + labelPadding;
                    float sexualityRectWidth = rightRect.width - labelPadding - sexualityBarMargin;
                    float barWidth = (sexualityRectWidth - sexualityLabelWidth - 2f * sexualityBarMargin) * 2f / 3f;
                    // Male Attraction
                    Rect maleLabelRect = new Rect(sexualityRectX, rightY, sexualityLabelWidth, 22f);
                    Rect maleBarRect = new Rect(sexualityRectX + sexualityLabelWidth + sexualityBarMargin, rightY + (22f - sexualityBarHeight) / 2f, barWidth, sexualityBarHeight); // Center bar vertically
                    Rect maleBarSurplusRect = new Rect(maleBarRect.xMax, maleBarRect.y, barWidth * 0.5f, sexualityBarHeight);
                    Rect maleAllRect = new Rect(sexualityRectX, rightY, sexualityRectWidth, 22f);
                    Widgets.Label(maleLabelRect, "RPC_AttractionMale".Translate());
                    Widgets.DrawBoxSolid(maleBarRect, barBackgroundColor);
                    Widgets.DrawBoxSolid(maleBarSurplusRect, barSurplusBackgroundColor);
                    float mAttraction = comp.Sexuality.GetAdjustedAttractionToGender(Gender.Male);
                    Rect mValueRect = new Rect(maleBarRect.x, maleBarRect.y, mAttraction * barWidth, sexualityBarHeight);
                    Color mColor;
                    if (mAttraction <= 1) mColor = Color.Lerp(maleLowolor, maleHighColor, mAttraction);
                    else mColor = Color.Lerp(maleHighColor, maleHyperColor, -1.25f + 1.5f * mAttraction);
                    Widgets.DrawBoxSolid(mValueRect, mColor);
                    Widgets.DrawLineVertical(maleBarRect.xMax, maleBarRect.y - 1, maleBarRect.height + 2);
                    if (Mouse.IsOver(maleAllRect))
                    {
                        Widgets.DrawHighlight(maleAllRect);
                        TooltipHandler.TipRegion(maleAllRect, "RPS_AttractionMaleTooltip".Translate() + ": " + mAttraction.ToStringPercent());
                    }

                    rightY += 22f;

                    // Female Attraction
                    Rect femaleLabelRect = new Rect(sexualityRectX, rightY, sexualityLabelWidth, 22f);
                    Rect femaleBarRect = new Rect(sexualityRectX + sexualityLabelWidth + sexualityBarMargin, rightY + (22f - sexualityBarHeight) / 2f, barWidth, sexualityBarHeight);
                    Rect femaleBarSurplusRect = new Rect(femaleBarRect.xMax, femaleBarRect.y, barWidth * 0.5f, sexualityBarHeight);
                    Rect femaleAllRect = new Rect(sexualityRectX, rightY, sexualityRectWidth, 22f);
                    Widgets.Label(femaleLabelRect, "RPC_AttractionFemale".Translate());
                    Widgets.DrawBoxSolid(femaleBarRect, barBackgroundColor);
                    Widgets.DrawBoxSolid(femaleBarSurplusRect, barSurplusBackgroundColor);
                    float fAttraction = comp.Sexuality.GetAdjustedAttractionToGender(Gender.Female);
                    Rect fValueRect = new Rect(femaleBarRect.x, femaleBarRect.y, fAttraction * barWidth, sexualityBarHeight);
                    Color fColor;
                    if (fAttraction <= 1) fColor = Color.Lerp(femaleLowolor, femaleHighColor, fAttraction);
                    else fColor = Color.Lerp(femaleHighColor, femaleHyperColor, -1.25f + 1.5f * fAttraction);
                    Widgets.DrawBoxSolid(fValueRect, fColor);
                    Widgets.DrawLineVertical(femaleBarRect.xMax, femaleBarRect.y - 1, femaleBarRect.height + 2);
                    if (Mouse.IsOver(femaleAllRect))
                    {
                        Widgets.DrawHighlight(femaleAllRect);
                        TooltipHandler.TipRegion(femaleAllRect, "RPS_AttractionFemaleTooltip".Translate() + ": " + fAttraction.ToStringPercent());
                    }

                    rightY += 22f;
                    Rect sexDriveLabelRect = new Rect(sexualityRectX, rightY, sexualityLabelWidth, 22f);
                    Rect sexDriveRect = new Rect(sexualityRectX + sexualityLabelWidth + sexualityBarMargin, rightY + (22f - sexualityBarHeight) / 2f, barWidth, sexualityBarHeight);
                    Rect sexDriveBarSurplusRect = new Rect(sexDriveRect.xMax, sexDriveRect.y, barWidth * 0.5f, sexualityBarHeight);
                    Rect sexDriveAllRect = new Rect(sexualityRectX, rightY, sexualityRectWidth, 22f);
                    Widgets.Label(sexDriveLabelRect, "RPC_SexDrive".Translate());
                    Widgets.DrawBoxSolid(sexDriveRect, barBackgroundColor);
                    Widgets.DrawBoxSolid(sexDriveBarSurplusRect, barSurplusBackgroundColor);
                    float sexDrive = comp.Sexuality.GetAdjustedSexdrive();
                    Rect sdValueRect = new Rect(sexDriveRect.x, sexDriveRect.y, sexDrive * barWidth, sexualityBarHeight);
                    Color sdColor;
                    if (sexDrive <= 1) sdColor = Color.Lerp(LowSexualityBarColor, HighSexualityBarColor, sexDrive);
                    else sdColor = Color.Lerp(HighSexualityBarColor, HyperSexualityBarColor, -1.25f + 1.5f * sexDrive);
                    Widgets.DrawBoxSolid(sdValueRect, sdColor);
                    Widgets.DrawLineVertical(sexDriveRect.xMax, sexDriveRect.y - 1, sexDriveRect.height + 2);
                    if (Mouse.IsOver(sexDriveAllRect))
                    {
                        Widgets.DrawHighlight(sexDriveAllRect);
                        TooltipHandler.TipRegion(sexDriveAllRect, "RPS_SexdriveTooltip".Translate() + ": " + sexDrive.ToStringPercent());
                    }
                }
            }

            Text.Font = originalFont;
            Text.Anchor = originalAnchor;
        }
        private static SideMode GetNextSideMode(SideMode current,bool canUseDisposition, bool canUseSexuality)
        {
            switch (current)
            {
                case SideMode.Interest:
                    if (canUseDisposition) return SideMode.Disposition;
                    if (canUseSexuality) return SideMode.Sexuality;
                    return SideMode.Interest;

                case SideMode.Disposition:
                    if (canUseSexuality) return SideMode.Sexuality;
                    return SideMode.Interest;

                case SideMode.Sexuality:
                    return SideMode.Interest;

                default:
                    return SideMode.Interest;
            }
        }
        public static void DrawSexualityBox(Rect sexualityRect, CompPsyche compPsyche, Pawn pawn)
        {
            TextAnchor oldAnchor = Text.Anchor;
            GameFont oldFont = Text.Font;

            float contentStartY = sexualityRect.y + sexualityHeaderHeight; // Starting Y for content below header
            Rect headerRect = new Rect(sexualityRect.x, sexualityRect.y, sexualityRect.width, sexualityHeaderHeight);
            GUI.BeginGroup(headerRect);

            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleCenter;
            Rect titleRect = new Rect(0f, 0f, headerRect.width, headerRect.height);
            Widgets.Label(titleRect, "RPC_Sexuality".Translate());

            GUI.EndGroup();
            GUI.BeginGroup(new Rect(sexualityRect.x, contentStartY, sexualityRect.width - scrollWidth, sexualityRect.height - headerHeight));

            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleLeft;

            float y = 0f;
            Rect sexualityLabelRect = new Rect(0f, y, sexualityLabelWidth, sexualityLineHeight);
            Rect sexualityDescRect = new Rect(sexualityLabelWidth + sexualityBarMargin, y, sexualityRect.width - (sexualityLabelWidth + sexualityBarMargin), sexualityLineHeight);
            Rect sexualityAllRect = new Rect(0f, y, sexualityRect.width, sexualityLineHeight);
            Widgets.Label(sexualityLabelRect, "RPC_Orientation".Translate());
            Widgets.Label(sexualityDescRect, compPsyche.Sexuality.GetOrientationReport() + $" ({compPsyche.Sexuality.GetKinseyReport()})");
            if (Mouse.IsOver(sexualityAllRect))
            {
                Widgets.DrawHighlight(sexualityAllRect);
                TooltipHandler.TipRegion(sexualityAllRect, GetSexualityTooltip(compPsyche));
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
            if (mAttraction <= 1) mColor = Color.Lerp(maleLowolor, maleHighColor, mAttraction);
            else mColor = Color.Lerp(maleHighColor, maleHyperColor, -1.25f + 1.5f * mAttraction);
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
            if (fAttraction <= 1) fColor = Color.Lerp(femaleLowolor, femaleHighColor, fAttraction);
            else fColor = Color.Lerp(femaleHighColor, femaleHyperColor, -1.25f + 1.5f * fAttraction);
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
            if (sexDrive <= 1) sdColor = Color.Lerp(LowSexualityBarColor, HighSexualityBarColor, sexDrive);
            else sdColor = Color.Lerp(HighSexualityBarColor, HyperSexualityBarColor, -1.25f + 1.5f * sexDrive);
            Widgets.DrawBoxSolid(sdValueRect, sdColor);
            Widgets.DrawLineVertical(sexDriveRect.xMax, sexDriveRect.y - 1, sexDriveRect.height + 2);
            if (Mouse.IsOver(sexDriveAllRect))
            {
                Widgets.DrawHighlight(sexDriveAllRect);
                TooltipHandler.TipRegion(sexDriveAllRect, "RPS_SexdriveTooltip".Translate() + ": " + sexDrive.ToStringPercent());
            }
            GUI.EndGroup();
            Text.Anchor = oldAnchor;
            Text.Font = oldFont;
        }

        public static void DrawInterestBox(Rect interestRect, CompPsyche compPsyche, Pawn pawn, bool showSexuality)
        {
            TextAnchor oldAnchor = Text.Anchor;
            GameFont oldFont = Text.Font;
            bool preferenceView = showPreference && showSexuality;

            Rect headerRect = new Rect(interestRect.x, interestRect.y, interestRect.width, headerHeight);
            GUI.BeginGroup(headerRect);

            // Title: "Interest" || "Preference
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleCenter;
            Rect titleRect = new Rect(0f, 0f, headerRect.width, headerRect.height);
            Vector2 titleTextSize;
            if (preferenceView)
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

            if (preferenceView)
            {
                Text.Font = GameFont.Small;
                //var prefReport = GetPreferenceReport(pawn, scrollRect.width - scrollWidth);
                float totalContentHeight = 0f;
                var viewerHeights = GetViewerHeights(pawn);
                foreach (var height in viewerHeights)
                {
                    totalContentHeight += height + 5f;
                }
                Rect scrollContentRect = new Rect(0f, 0f, scrollRect.width - scrollWidth, totalContentHeight);
                Widgets.BeginScrollView(scrollRect, ref InterestScrollPosition, scrollContentRect);

                float y = 0f;
                var allPrefDefs = DefDatabase<PreferenceDef>.AllDefsListForReading;
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
                Widgets.EndScrollView();
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
