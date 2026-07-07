using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Maux36.RimPsyche
{
    public readonly struct DescriptorResult
    {
        public readonly float strength;
        public readonly string key;
        public readonly int intensity;

        public DescriptorResult(float strength, string key, int intensity)
        {
            this.strength = strength;
            this.key = key;
            this.intensity = intensity;
        }
    }

    public abstract class DescriptorBase
    {
        public string positiveKey;
        public string negativeKey;
        public float threshold;
        public float strongThreshold;
        public float extremeThreshold;

        //●○○
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
    }
}