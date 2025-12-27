using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Maux36.RimPsyche
{
    public enum SexualOrientation : byte
    {
        None, //Not initialized
        Developing, //Pawn age less than adult age
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
            SexualityTraitHashSet = GetSexualityTraitHashSet();
            Distribution = CalculateNormalizedDistribution();
            StraightSum = Distribution[0] + Distribution[1];
            BiSum = Distribution[2] + Distribution[3] + Distribution[4];
            StraightBiSum = Distribution[0] + Distribution[1] + Distribution[2] + Distribution[3] + Distribution[4];
            GaySum = Distribution[5] + Distribution[6];
            SexualityCurve = CreateSexualityCurve();

            StraightCurve = (StraightSum > 0f)
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

            BiCurve = (BiSum > 0f)
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

            NonGayCurve = (StraightBiSum > 0f)
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

            GayCurve = (GaySum > 0f)
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
        }

        public static HashSet<int> NonSexualDefShorthashSet = new();
        public static HashSet<PawnRelationDef> LoverDefHash = new();
        public static HashSet<int> SexualityTraitHashSet = new();
        private static HashSet<PawnRelationDef> GetLoverDefs()
        {
            HashSet<PawnRelationDef> loverDefs = new();
            loverDefs.Add(PawnRelationDefOf.Lover);
            loverDefs.Add(PawnRelationDefOf.Fiance);
            loverDefs.Add(PawnRelationDefOf.Spouse);
            return loverDefs;
        }
        private static HashSet<int> GetSexualityTraitHashSet()
        {
            HashSet<int> loverDefs = new();
            loverDefs.Add(TraitDefOf.Gay.shortHash);
            loverDefs.Add(TraitDefOf.Bisexual.shortHash);
            loverDefs.Add(TraitDefOf.Asexual.shortHash);
            return loverDefs;
        }

        public static List<float> Distribution;
        private static List<float> CalculateNormalizedDistribution()
        {
            int total = 0;
            foreach (var value in RimpsycheSexualitySettings.KinseyDistributionSetting)
            {
                total += value;
            }
            if (total == 0)
            {
                return [1f / 7f, 1f / 7f, 1f / 7f, 1f / 7f, 1f / 7f, 1f / 7f, 1f / 7f];
            }
            List<float> normalizedDistribution = new List<float>();
            foreach (float value in RimpsycheSexualitySettings.KinseyDistributionSetting)
            {
                normalizedDistribution.Add(value / total);
            }
            return normalizedDistribution;
        }
        private static readonly List<float> steps = [0f, 0.2f, 0.4f, 0.6f, 0.8f, 1f];
        // private static readonly List<float> steps = [0f, 0.08f, 0.36f, 0.64f, 0.92f, 1f];
        private static readonly float StraightSum;
        private static readonly float BiSum;
        private static readonly float StraightBiSum;
        private static readonly float GaySum;
        private static readonly SimpleCurve SexualityCurve;
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
        private static readonly SimpleCurve StraightCurve;
        private static readonly SimpleCurve BiCurve;
        private static readonly SimpleCurve NonGayCurve;
        private static readonly SimpleCurve GayCurve;
        public static SexualOrientation EvaluateSexuality(Pawn pawn)
        {
            var traits = pawn.story?.traits;
            var gender = pawn.gender;
            //Not Applicable
            if (traits == null || gender == Gender.None) return SexualOrientation.None;
            //Non-adults
            if (Rimpsyche_Utility.GetPawnAge(pawn) < Rimpsyche_Utility.GetMinAdultAge(pawn))
            {
                return SexualOrientation.Developing;
            }
            if (traits.HasTrait(TraitDefOf.Gay)) return SexualOrientation.Homosexual;
            else if (traits.HasTrait(TraitDefOf.Bisexual)) return SexualOrientation.Bisexual;
            else if (traits.HasTrait(TraitDefOf.Asexual)) return SexualOrientation.Asexual;
            else return SexualOrientation.Heterosexual;
        }

        public static float AdjustRawValues(float rawValue)
        {
            return 3f - 3f / (rawValue + 1f);
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
                default:
                    kinsey = -1;
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
            if (orientation == SexualOrientation.None) return 0f;
            if (orientation == SexualOrientation.Developing) return 0f;
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

        public static float EvaluateRomPreference(Pawn pawn, Pawn otherPawn, float value)
        {
            //Only active preferences are stored in OrderedPrefDefs
            var prefDefs = RimpsycheDatabase.OrderedRomPreferenceDefs;
            for (int i = 0; i < prefDefs.Count; i++)
            {
                var def = prefDefs[i];
                value = def.worker.Evaluate(pawn, otherPawn, value, true);
            }
            return value;
        }
        public static float EvaluateSexPreference(Pawn pawn, Pawn otherPawn, float value)
        {
            //Only active preferences are stored in OrderedPrefDefs
            var prefDefs = RimpsycheDatabase.OrderedSexPreferenceDefs;
            for (int i = 0; i < prefDefs.Count; i++)
            {
                var def = prefDefs[i];
                value = def.worker.Evaluate(pawn, otherPawn, value, false);
            }
            return value;
        }
    }
}
