using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Maux36.RimPsyche
{
    public class SexualityHelper
    {
        //TODO: Look up distribution researches
        public static List<float> Distribution = [0.6f, 0.1f, 0.1f, 0.05f, 0.05f, 0.05f, 0.05f]; //TODO expose to settings
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
        public static readonly SimpleCurve StraightCurve = new SimpleCurve
        {
            new CurvePoint(Distribution[0]/StraightSum, steps[0]),
            new CurvePoint(1f, steps[1])
        };
        public static readonly SimpleCurve BiCurve = new SimpleCurve
        {
            new CurvePoint(0f, steps[1]),
            new CurvePoint(Distribution[2]/BiSum, steps[2]),
            new CurvePoint((Distribution[2]+Distribution[3])/BiSum, steps[3]),
            new CurvePoint(1f, steps[4])
        };
        public static readonly SimpleCurve NonGayCurve = new SimpleCurve
        {
            new CurvePoint(Distribution[0]/StraightBiSum, steps[0]),
            new CurvePoint((Distribution[0]+Distribution[1])/StraightBiSum, steps[1]),
            new CurvePoint((Distribution[0]+Distribution[1]+Distribution[2])/StraightBiSum, steps[2]),
            new CurvePoint((Distribution[0]+Distribution[1]+Distribution[2]+Distribution[3])/StraightBiSum, steps[3]),
            new CurvePoint(1f, steps[4])
        };
        public static readonly SimpleCurve GayCurve = new SimpleCurve
        {
            new CurvePoint(0f, steps[4]),
            new CurvePoint(Distribution[5]/GaySum, steps[5])
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

        //RNG
        public static float GenerateKinsey(bool allowGay)
        {
            if (allowGay)
            {
                return NonGayCurve.Evaluate(Rand.Value);
            }
            return SexualityCurve.Evaluate(Rand.Value);
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
            return GetNormalDistribution();
        }
        public static float GenerateAttractionFor(SexualOrientation orientation)
        {
            if (orientation == SexualOrientation.Asexual) return Rand.Range(0, 0.05f);
            else return GetNormalDistribution(0.05f, 1f);
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
                result = Rand.Gaussian(0.5f, 0.2f);
                attempts++;
            }
            while ((result < lowBracket || result > highBracket) && attempts < maxAttempts);

            if (result < lowBracket || result > highBracket) result = Mathf.Clamp(result, lowBracket, highBracket);
            return result;
        }
    }
}
