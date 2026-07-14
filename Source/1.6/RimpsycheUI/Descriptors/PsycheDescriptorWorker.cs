using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace Maux36.RimPsyche
{
    public abstract class PsycheDescriptorWorker
    {
        public PsycheDescriptorDef descriptorDef;
        public int maxLevel;
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
        private static float Progress(float value, float min, float max)
        {
            float range = max - min;
            return range > 0f ? (value - min) / range : 0f;
        }
        public float GetTieredNormalizedAbsScore(float score)
        {
            if (maxLevel == 0)
                return 0f;

            score = Mathf.Abs(score);

            if (score < descriptorDef.threshold)
                return Progress(score, 0f, descriptorDef.threshold);

            if (maxLevel == 1)
                return 1f;

            if (score < descriptorDef.strongThreshold)
                return 1f + Progress(score, descriptorDef.threshold, descriptorDef.strongThreshold);

            if (maxLevel == 2)
                return 2f;

            if (score < descriptorDef.extremeThreshold)
                return 2f + Progress(score, descriptorDef.strongThreshold, descriptorDef.extremeThreshold);

            return 3f + Progress(score, descriptorDef.extremeThreshold, descriptorDef.strongThreshold);
        }
        public string GetLabel(CompPsyche compPsyche)
        {
            return (Score(compPsyche) >= 0 ? descriptorDef.positiveLabel : descriptorDef.negativeLabel).CapitalizeFirst();
        }

        public string GetDescription(CompPsyche compPsyche)
        {
            var score = Score(compPsyche);
            return score >= 0 ? $"{score}\n{GetTieredNormalizedAbsScore(score)}\n\n" + descriptorDef.positiveDescription : $"{score}\n{GetTieredNormalizedAbsScore(score)}\n\n" + descriptorDef.negativeDescription;
        }

        public string GetIntensityString(CompPsyche compPsyche)
        {
            float strength = Mathf.Abs(Score(compPsyche));
            int filled = 0;
            if (strength >= descriptorDef.threshold)
                filled++;
            if (maxLevel >= 2 && strength >= descriptorDef.strongThreshold)
                filled++;
            if (maxLevel >= 3 && strength >= descriptorDef.extremeThreshold)
                filled++;
            return new string('●', filled) + new string('○', maxLevel - filled);
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