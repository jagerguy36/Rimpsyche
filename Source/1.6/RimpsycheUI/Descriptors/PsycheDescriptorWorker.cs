using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace Maux36.RimPsyche
{
    public enum PsycheDescDirection: byte
    {
        Positive,
        Neutral,
        Negative
    }
    public readonly struct Blamer(PersonalityDef personality, PsycheDescDirection direction, Func<CompPsyche, float, bool> validator)
    {
        public readonly PersonalityDef Personality = personality;
        public readonly PsycheDescDirection Direction = direction;
        public readonly Func<CompPsyche, float, bool> Validator = validator;
    }
    public abstract class PsycheDescriptorWorker
    {
        public PsycheDescriptorDef descriptorDef;
        public int maxLevel;
        public static Color negBlameColor = new Color(0.8f, 0.2f, 0.4f);
        public static Color neutBlameColor = new Color(0.2f, 0.4f, 0.8f);
        public static Color posBlameColor = new Color(0.2f, 0.8f, 0.6f);
        protected abstract float Score(CompPsyche compPsyche);
        protected abstract void Evaluate(StringBuilder ctx, CompPsyche compPsyche, float score);
        protected void Blame(StringBuilder ctx, CompPsyche compPsyche, PersonalityDef personality, PsycheDescDirection direction = PsycheDescDirection.Positive)
        {
            ctx.AppendLine($"  {GetBlameString(compPsyche, personality, direction)}");
        }

        //For built
        public Dictionary<ushort, string> bImpactRegistry = new();
        public float bScore = 0f;
        public float bNormalizedAbsValue = 0f;
        public string bIntensityString;
        public string bLabel;
        public string bDescription;
        public string bToolTip;
        public virtual void Build(CompPsyche compPsyche)
        {
            bImpactRegistry.Clear();
            bScore = Score(compPsyche);
            bNormalizedAbsValue = GetTieredNormalizedAbsScore(bScore);
            bLabel = GetLabel(bScore);
            bDescription = GetDescription(bScore);
            bIntensityString = GetIntensityString(bScore);
            if (bScore != 0f)
                bToolTip = GetTooltip(compPsyche, bScore);
            else
                bToolTip = string.Empty;
        }
        protected string GetTooltip(CompPsyche compPsyche, float score)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(bDescription);
            sb.AppendLine();
            sb.AppendLine("RPC_DescriptorBlame".Translate());
            Evaluate(sb, compPsyche, score);
            return sb.ToString();
        }
        private static float Progress(float value, float min, float max)
        {
            float range = max - min;
            return range > 0f ? (value - min) / range : 0f;
        }
        protected float GetTieredNormalizedAbsScore(float score)
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
        protected string GetLabel(float score)
        {
            return (score > 0 ? descriptorDef.positiveLabel : score == 0 ? "" : descriptorDef.negativeLabel).CapitalizeFirst();
        }

        protected string GetDescription(float score)
        {
            return score > 0 ? descriptorDef.positiveDescription : score == 0? "" : descriptorDef.negativeDescription;
        }

        protected string GetIntensityString(float score)
        {
            float strength = Mathf.Abs(score);
            int filled = 0;
            if (strength >= descriptorDef.threshold)
                filled++;
            if (maxLevel >= 2 && strength >= descriptorDef.strongThreshold)
                filled++;
            if (maxLevel >= 3 && strength >= descriptorDef.extremeThreshold)
                filled++;
            return new string('●', filled) + new string('○', maxLevel - filled);
        }

        protected string GetBlameString(CompPsyche compPsyche, PersonalityDef personality, PsycheDescDirection direction)
        {
            float value = compPsyche.Personality.GetPersonality(personality);
            bool aligned = false;
            if (bScore * value >= 0)
            {
                aligned = true;
            }
            var desc = Rimpsyche_Utility.GetPersonalityDesc(personality, value);
            Color targetColor = posBlameColor;
            string sign = "+";
            PsycheDescDirection impactDirection = PsycheDescDirection.Positive;
            if (direction == PsycheDescDirection.Positive)
            {
                if (!aligned)
                {
                    targetColor = negBlameColor;
                    sign = "−";//U+2212
                    impactDirection = PsycheDescDirection.Negative;
                }
            }
            else if (direction == PsycheDescDirection.Negative)
            {
                if (aligned)
                {
                    targetColor = negBlameColor;
                    sign = "−";//U+2212
                    impactDirection = PsycheDescDirection.Negative;
                }
            }
            else
            {
                sign = "±"; //U+2212
                targetColor = neutBlameColor;
                impactDirection = PsycheDescDirection.Neutral;
            }
            string descriptionColorCode = ColorUtility.ToHtmlStringRGBA(Color.Lerp(Color.gray, targetColor, Mathf.Abs(bNormalizedAbsValue * 0.4f)));
            if (!string.IsNullOrEmpty(bDescription))
            {
                var impactDesc = impactDirection switch
                {
                    PsycheDescDirection.Positive => $"  ▴ {bDescription}",
                    PsycheDescDirection.Neutral => $"  ♦ {bDescription}",//▴▾◆▵◊▿⬧♦
                    PsycheDescDirection.Negative => $"  ▾ {bDescription}",
                    _ => ""
                };
                impactDesc.CapitalizeFirst();
                bImpactRegistry.Add(personality.shortHash, $"<color=#{descriptionColorCode}>{impactDesc}</color>");
            }
            string behaviorColorCode = ColorUtility.ToHtmlStringRGBA(Color.Lerp(Color.gray, targetColor, Mathf.Abs(value)));
            return $"<color=#{behaviorColorCode}>{sign} {desc}</color>";
        }
    }
}