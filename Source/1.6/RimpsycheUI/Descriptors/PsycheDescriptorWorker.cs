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
        public static Color negBlameColor = new Color(0.8f, 0.2f, 0.2f);
        public static Color posBlameColor = new Color(0.2f, 0.8f, 0.2f);
        public abstract float Score(CompPsyche compPsyche);
        public virtual string GetTooltip(CompPsyche compPsyche)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(GetDescription(compPsyche));
            return stringBuilder.ToString();
        }
        public string GetLabel(CompPsyche compPsyche)
        {
            return Score(compPsyche) >= 0 ? descriptorDef.positiveLabel : descriptorDef.negativeLabel;
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

            return "●○○";
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