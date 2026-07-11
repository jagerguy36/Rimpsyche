using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace Maux36.RimPsyche
{
    public abstract class PsycheDescriptorWorker
    {
        public PsycheDescriptorDef descriptorDef;
        public bool positiveOnly = false;
        public static Color negBlameColor = new Color(0.8f, 0.2f, 0.4f);
        public static Color posBlameColor = new Color(0.2f, 0.8f, 0.6f);
        public abstract float Score(CompPsyche compPsyche);
        public virtual string GetTooltip(CompPsyche compPsyche)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(GetDescription(compPsyche));
            return stringBuilder.ToString();
        }
        public float GetTieredNormalizedAbsScore(float score)
        {
            score = Mathf.Abs(score);
            if (score >= descriptorDef.extremeThreshold)
            {
                // Avoid division by zero if extreme matches strong
                float range = descriptorDef.extremeThreshold - descriptorDef.strongThreshold;
                float progress = range > 0 ? (score - descriptorDef.extremeThreshold) / range : 0f;
                return 3.0f + progress;
            }

            // Tier 2: Strong to Extreme
            if (score >= descriptorDef.strongThreshold)
            {
                float range = descriptorDef.extremeThreshold - descriptorDef.strongThreshold;
                float progress = range > 0 ? (score - descriptorDef.strongThreshold) / range : 0f;
                return 2.0f + progress; // Returns 2.0 to 3.0
            }

            // Tier 1: Basic to Strong
            if (score >= descriptorDef.threshold)
            {
                float range = descriptorDef.strongThreshold - descriptorDef.threshold;
                float progress = range > 0 ? (score - descriptorDef.threshold) / range : 0f;
                return 1.0f + progress; // Returns 1.0 to 2.0
            }

            // Tier 0: Below threshold
            if (descriptorDef.threshold > 0)
            {
                return score / descriptorDef.threshold; // Returns 0.0 to 1.0
            }

            return 0f;
        }
        public string GetLabel(CompPsyche compPsyche)
        {
            return (Score(compPsyche) >= 0 ? descriptorDef.positiveLabel : descriptorDef.negativeLabel).CapitalizeFirst();
        }

        public string GetDescription(CompPsyche compPsyche)
        {
            return Score(compPsyche) >= 0 ? descriptorDef.positiveDescription : descriptorDef.negativeDescription;
        }

        public string GetIntensityString(CompPsyche compPsyche)
        {
            var strength = Mathf.Abs(Score(compPsyche));

            if (strength >= descriptorDef.extremeThreshold)
                return "●●●";

            if (strength >= descriptorDef.strongThreshold)
                return "●●○";

            if (strength >= descriptorDef.threshold)
                return "●○○";

            return "○○○";
        }
        public static string GetBlame(CompPsyche compPsyche, PersonalityDef personality, bool positive = true)
        {
            float value = compPsyche.Personality.GetPersonality(personality);
            var desc = Rimpsyche_Utility.GetPersonalityDesc(personality, value);
            string sign = ((value >= 0f) == positive) ? "+" : "−"; //U+2212
            Color targetColor = ((value >= 0f) == positive) ? posBlameColor : negBlameColor;
            Color blendedColor = Color.Lerp(Color.gray, targetColor, Mathf.Abs(value));
            return $"<color=#{ColorUtility.ToHtmlStringRGBA(blendedColor)}>{sign} {desc}</color>";
        }
    }
}