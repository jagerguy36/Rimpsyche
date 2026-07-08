using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Maux36.RimPsyche
{
    public abstract class PsycheDescriptorWorker
    {
        public string positiveKey;
        public string negativeKey;
        public float threshold;
        public float strongThreshold;
        public float extremeThreshold;
        public List<PersonalityDef> contributors;

        public abstract float Score(CompPsyche compPsyche);

        public string GetKey(CompPsyche compPsyche)
        {
            return Score(compPsyche) >= 0 ? positiveKey : negativeKey;
        }

        public float GetStrength(CompPsyche compPsyche)
        {
            return Mathf.Abs(Score(compPsyche));
        }

        public int GetIntensity(float strength)
        {
            if (strength >= extremeThreshold)
                return 3;

            if (strength >= strongThreshold)
                return 2;

            return 1;
        }

        public DescriptorResult Evaluate(CompPsyche compPsyche)
        {
            float score = Score(compPsyche);
            float strength = Mathf.Abs(score);
            int intensity = GetIntensity(strength);

            return new DescriptorResult(
                strength,
                score >= 0 ? positiveKey : negativeKey,
                intensity
            );
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
        public static string GetPersonalityDescription(CompPsyche compPsyche, PersonalityDef personality)
        {
            float value = compPsyche.Personality.GetPersonality(personality);
            float absValue = Mathf.Abs(value);

            string intensityKey = absValue switch
            {
                >= 0.75f => "RimPsycheIntensityExtremely",
                >= 0.50f => "RimPsycheIntensityVery",
                >= 0.25f => "RimPsycheIntensitySomewhat",
                > 0f     => "RimPsycheIntensityMarginally",
                _         => "RimPsycheIntensityNeutral"
            };

            string personalityName = value >= 0 ? personality.high : personality.low;

            string label = LanguageDatabase.activeLanguage.HaveTextForKey(intensityKey)
                ? intensityKey.Translate(personalityName)
                : RimpsycheDatabase.IntensityKeysDefault[intensityKey] + " " + personalityName;

            return $"{label} ({Mathf.RoundToInt(absValue * 100f)}%)";
        }
        public static string BuildContributorsTooltip(CompPsyche compPsyche, List<PersonalityDef> contributors)
        {
            if (contributors == null || contributors.Count == 0)
                return "Influenced by\nNone";

            StringBuilder sb = new StringBuilder("Influenced by\n");

            foreach (PersonalityDef personality in contributors)
            {
                sb.Append("• ")
                .AppendLine(GetPersonalityDescription(compPsyche, personality));
            }

            return sb.ToString().TrimEnd();
        }
    }
}