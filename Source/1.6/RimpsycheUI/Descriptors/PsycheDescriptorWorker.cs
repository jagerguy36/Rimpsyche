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
        public List<Blamer> blamers = new();
        public PsycheDescriptorWorker()
        {
            SetupBlamers();
        }
        protected abstract float Score(CompPsyche compPsyche);
        protected abstract void SetupBlamers();
        protected void Blame(PersonalityDef personality, PsycheDescDirection direction = PsycheDescDirection.Positive, Func<CompPsyche, float, bool> validator = null)
        {
            blamers.Add(new Blamer(personality, direction, validator));
        }

        //For built
        public Dictionary<ushort, PsycheDescDirection> bImpactRegistry = new();
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
            bToolTip = GetTooltip(compPsyche, bScore);
        }
        protected string GetTooltip(CompPsyche compPsyche, float score)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(bDescription);
            sb.AppendLine();
            sb.AppendLine("RPC_DescriptorBlame".Translate());
            foreach(var blamer in blamers)
            {
                var validator = blamer.Validator;
                if (validator == null)
                {
                    sb.AppendLine($"  {GetBlameString(compPsyche, blamer.Personality, blamer.Direction)}");
                }
                else if (validator(compPsyche, score))
                {
                    sb.AppendLine($"  {GetBlameString(compPsyche, blamer.Personality, blamer.Direction)}");
                }
            }
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
            return (score >= 0 ? descriptorDef.positiveLabel : descriptorDef.negativeLabel).CapitalizeFirst();
        }

        protected string GetDescription(float score)
        {
            return score >= 0 ? descriptorDef.positiveDescription : descriptorDef.negativeDescription;
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
                    sign = "-";
                    impactDirection = PsycheDescDirection.Negative;
                }
            }
            else if (direction == PsycheDescDirection.Negative)
            {
                if (aligned)
                {
                    targetColor = negBlameColor;
                    sign = "-";
                    impactDirection = PsycheDescDirection.Negative;
                }
            }
            else
            {
                sign = "±"; //U+2212
                targetColor = neutBlameColor;
                impactDirection = PsycheDescDirection.Neutral;
            }
            bImpactRegistry.Add(personality.shortHash, impactDirection);
            Color blendedColor = Color.Lerp(Color.gray, targetColor, Mathf.Abs(value));
            return $"<color=#{ColorUtility.ToHtmlStringRGBA(blendedColor)}>{sign} {desc}</color>";
        }
    }
}