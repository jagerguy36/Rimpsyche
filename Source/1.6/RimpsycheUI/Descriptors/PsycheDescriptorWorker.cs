using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace Maux36.RimPsyche
{
    public abstract class PsycheDescriptorWorker
    {
        public PsycheDescriptorDef descriptorDef;
        public static negBlameColor = new Color(0.8f, 0.2f, 0.2f);
        public static posBlameColor = new Color(0.2f, 0.8f, 0.2f);
        public abstract float Score(CompPsyche compPsyche);
        public virtual string GetDescription(CompPsyche compPsyche)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(GetKey(compPsyche));
            return stringBuilder.ToString();
        }

        public string GetKey(CompPsyche compPsyche)
        {
            return Score(compPsyche) >= 0 ? descriptorDef.positiveKey : descriptorDef.negativeKey;
        }

        public float GetStrength(CompPsyche compPsyche)
        {
            return Mathf.Abs(Score(compPsyche));
        }

        public int GetIntensity(float strength)
        {
            if (strength >= descriptorDef.extremeThreshold)
                return 3;

            if (strength >= descriptorDef.strongThreshold)
                return 2;

            return 1;
        }

        public static string GetIntensityString(int intensity)
        {
            return intensity switch
            {
                1 => "●○○",
                2 => "●●○",
                3 => "●●●",
                _ => "○○○",
            };
        }
        public static string GetBlame(CompPsyche compPsyche, PersonalityDef personality, bool positive = true)
        {
            float value = compPsyche.Personality.GetPersonality(personality);
            var desc = Rimpsyche_Utility.GetPersonalityDesc(personality, value);
            Color targetColor = ((value >= 0f) == positive) ? posBlameColor : negBlameColor;
            Color blendedColor = Color.Lerp(Color.gray, targetColor, Mathf.Abs(value));
            return $"<color=#{ColorUtility.ToHtmlStringRGBA(blendedColor)}>{desc}</color>";
        }
    }
}