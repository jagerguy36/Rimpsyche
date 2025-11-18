using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Maux36.RimPsyche
{
    public enum SexualOrientation : byte
    {
        None,
        Developing,
        Heterosexual,
        Bisexual,
        Homosexual,
        Asexual
    }

    public static class SexualityHelper
    {
        static SexualityHelper()
        {
            LoverDefHash = GetLoverDefs();
            Distribution = CalculateNormalizedDistribution();
        }

        public static HashSet<PawnRelationDef> LoverDefHash = new();
        private static HashSet<PawnRelationDef> GetLoverDefs()
        {
            HashSet<PawnRelationDef> loverDefs = new();
            loverDefs.Add(PawnRelationDefOf.Lover);
            loverDefs.Add(PawnRelationDefOf.Fiance);
            loverDefs.Add(PawnRelationDefOf.Spouse);
            return loverDefs;
        }

        public static List<float> Distribution = [1f / 7f, 1f / 7f, 1f / 7f, 1f / 7f, 1f / 7f, 1f / 7f, 1f / 7f];
        private static List<float> CalculateNormalizedDistribution()
        {
            int total = 0;
            foreach (var value in RimpsycheSettings.KinseyDistributionSetting)
            {
                total += value;
            }
            if (total == 0)
            {
                return [1f / 7f, 1f / 7f, 1f / 7f, 1f / 7f, 1f / 7f, 1f / 7f, 1f / 7f];
            }
            List<float> normalizedDistribution = new List<float>();
            foreach (float value in RimpsycheSettings.KinseyDistributionSetting)
            {
                normalizedDistribution.Add(value / total);
            }
            return normalizedDistribution;
        }
        public static readonly List<float> steps = [0f, 0.2f, 0.4f, 0.6f, 0.8f, 1f];
        public static readonly float StraightSum = Distribution[0] + Distribution[1];
        public static readonly float BiSum = Distribution[2] + Distribution[3] + Distribution[4];
        public static readonly float StraightBiSum = Distribution[0] + Distribution[1] + Distribution[2] + Distribution[3] + Distribution[4];
        public static readonly float GaySum = Distribution[5] + Distribution[6];
        public static readonly SimpleCurve SexualityCurve = CreateSexualityCurve();
        private static SimpleCurve CreateSexualityCurve()
        {
            List<CurvePoint> curvePoints = new List<CurvePoint>();
            float cumulativeX = 0f;
            for (int i = 0; i < Distribution.Count - 1; i++)
            {
                cumulativeX += Distribution[i];
                curvePoints.Add(new CurvePoint(cumulativeX, steps[i]));
            }
            return new SimpleCurve(curvePoints.ToArray());
        }
        public static readonly SimpleCurve StraightCurve = (StraightSum > 0f)
        ? new SimpleCurve
        {
            new CurvePoint(Distribution[0] / StraightSum, steps[0]),
            new CurvePoint(1f, steps[1])
        }
        : new SimpleCurve
        {
            new CurvePoint(0.5f, steps[0]),
            new CurvePoint(1f, steps[1])
        };
        public static readonly SimpleCurve BiCurve = (BiSum > 0f)
        ? new SimpleCurve
        {
            new CurvePoint(0f, steps[1]),
            new CurvePoint(Distribution[2] / BiSum, steps[2]),
            new CurvePoint((Distribution[2] + Distribution[3]) / BiSum, steps[3]),
            new CurvePoint(1f, steps[4])
        }
        : new SimpleCurve
        {
            new CurvePoint(0f, steps[1]),
            new CurvePoint(1f, steps[4])
        };
        public static readonly SimpleCurve NonGayCurve = (StraightBiSum > 0f)
        ? new SimpleCurve
        {
            new CurvePoint(Distribution[0] / StraightBiSum, steps[0]),
            new CurvePoint((Distribution[0] + Distribution[1]) / StraightBiSum, steps[1]),
            new CurvePoint((Distribution[0] + Distribution[1] + Distribution[2]) / StraightBiSum, steps[2]),
            new CurvePoint((Distribution[0] + Distribution[1] + Distribution[2] + Distribution[3]) / StraightBiSum, steps[3]),
            new CurvePoint(1f, steps[4])
        }
        : new SimpleCurve
        {
            //If everyone should be gay, but generation is forcing no gay, then give them kinsey(4)
            new CurvePoint(0f, steps[3]),
            new CurvePoint(1f, steps[4])
        };
        public static readonly SimpleCurve GayCurve = (GaySum > 0f)
        ? new SimpleCurve
        {
            new CurvePoint(0f, steps[4]),
            new CurvePoint(Distribution[5] / GaySum, steps[5])
        }
        : new SimpleCurve
        {
            new CurvePoint(0f, steps[4]),
            new CurvePoint(0.5f, steps[5])
        };
        public static SexualOrientation EvaluateSexuality(Pawn pawn)
        {
            var traits = pawn.story?.traits;
            var gender = pawn.gender;
            //Not Applicable
            if (traits == null || gender == Gender.None) return SexualOrientation.None;
            //Non-adults
            if (pawn.ageTracker.AgeBiologicalYears < Rimpsyche_Utility.GetMinAdultAge(pawn))
            {
                return SexualOrientation.Developing;
            }
            if (traits.HasTrait(TraitDefOf.Gay)) return SexualOrientation.Homosexual;
            else if (traits.HasTrait(TraitDefOf.Bisexual)) return SexualOrientation.Bisexual;
            else if (traits.HasTrait(TraitDefOf.Asexual)) return SexualOrientation.Asexual;
            else return SexualOrientation.Heterosexual;
        }

        public static float AdjustAttraction(float rawAttraction)
        {
            return 3f - 3f / (rawAttraction + 1f);
        }

        //RNG
        public static float GenerateKinsey(bool allowGay)
        {
            if (allowGay)
            {
                return SexualityCurve.Evaluate(Rand.Value);
            }
            return NonGayCurve.Evaluate(Rand.Value);
        }
        public static float GenerateKinseyFor(SexualOrientation orientation)
        {
            float kinsey = 0f;
            float flatRatio = Rand.Value;
            switch (orientation)
            {
                case SexualOrientation.Heterosexual:
                    kinsey = StraightCurve.Evaluate(flatRatio);
                    break;
                case SexualOrientation.Bisexual:
                    kinsey = BiCurve.Evaluate(flatRatio);
                    break;
                case SexualOrientation.Homosexual:
                    kinsey = GayCurve.Evaluate(flatRatio);
                    break;
                case SexualOrientation.Asexual:
                    kinsey = SexualityCurve.Evaluate(flatRatio);
                    break;
            }
            return kinsey;
        }
        public static float GenerateAttraction()
        {
            return GetNormalDistribution(lowBracket:0f, highBracket:1f);
        }
        public static float GenerateSexdrive()
        {
            return GetNormalDistribution(lowBracket: 0f, highBracket: 1f);
        }
        public static float GenerateAttractionFor(SexualOrientation orientation)
        {
            if (orientation == SexualOrientation.Asexual) return Rand.Range(0, 0.05f);
            else return GetNormalDistribution(lowBracket: 0.05f, highBracket: 1f);
        }
        public static float ReEvaluateKinsey(float sameAttraction, float diffAttraction)
        {
            return sameAttraction / (sameAttraction + diffAttraction);
        }
        public static float GetNormalDistribution(float lowBracket = 0f, float highBracket = 1f, int maxAttempts = 4)
        {
            float result;
            int attempts = 0;
            do
            {
                result = Rand.Gaussian(0.5f, 1f/6f);
                attempts++;
            }
            while ((result < lowBracket || result > highBracket) && attempts < maxAttempts);

            if (result < lowBracket || result > highBracket) result = Mathf.Clamp(result, lowBracket, highBracket);
            return result;
        }
        public static float GetSkewedPreference(float center = 0f, int maxAttempts = 4)
        {
            float result;
            int attempts = 0;
            float lowerWidthFactor = (center + 1f) / 3f;
            float upperWidthFactor = (1f - center) / 3f;
            do
            {
                result = Rand.GaussianAsymmetric(center, lowerWidthFactor, upperWidthFactor);
                attempts++;
            }
            while ((result < -1f || result > 1f) && attempts < maxAttempts);

            if (result < -1f || result > 1f) result = Mathf.Clamp(result, -1f, 1f);
            return result;
        }
    }
}
