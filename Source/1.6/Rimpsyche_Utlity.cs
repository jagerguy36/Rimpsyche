using LudeonTK;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Maux36.RimPsyche
{
    public class Rimpsyche_Utility
    {

        public static ThoughtDef CreateSocialThought(string defName, string label, float offset)
        {
            ThoughtDef newDef = new ThoughtDef();
            newDef.defName = defName;
            newDef.durationDays = 5f;
            newDef.nullifyingTraits = new List<TraitDef> { TraitDefOf.Psychopath };
            newDef.thoughtClass = typeof(Thought_MemoryPostDefined);
            newDef.stackedEffectMultiplier = 0.9f;
            newDef.stackLimitForSameOtherPawn = 10;
            newDef.stackLimit = 300;
            newDef.developmentalStageFilter = DevelopmentalStage.Baby | DevelopmentalStage.Child | DevelopmentalStage.Adult;
            newDef.socialTargetDevelopmentalStageFilter = DevelopmentalStage.Baby | DevelopmentalStage.Child | DevelopmentalStage.Adult;
            ThoughtStage stage = new ThoughtStage
            {
                label = label,
                baseOpinionOffset = offset
            };
            newDef.stages.Add(stage);
            return newDef;
        }
        public static float SaddleShapeFunction(float x, float y, float controversiality = 1)
        {
            float C = 2 * controversiality * controversiality;
            float diff = (x - y);
            float diff2 = diff * diff;
            float sum = (x + y);
            float sum2 = sum * sum;
            float val = (-(1f + C) * diff2 + sum2) / (4f + C * diff2);
            Log.Message($"SaddleShapeFunction: {x}, {y} | controversiality: {controversiality}. val: {val}");
            return val;
        }
        public static float MapRange(float value, float sourceMin, float sourceMax, float targetMin, float targetMax)
        {
            return Mathf.Lerp(targetMin, targetMax, Mathf.InverseLerp(sourceMin, sourceMax, value));
        }
        public static float ApplyGate(float value, float targetMin, float targetMax)
        {
            return Mathf.Lerp(targetMin, targetMax, Mathf.InverseLerp(-50, 50, value));
        }
        public static float RestoreGatedValue(float value, float sourceMin, float sourceMax)
        {
            return Mathf.Lerp(-50, 50, Mathf.InverseLerp(sourceMin, sourceMax, value));
        }

        public static float ApplyScope(float value, float targetMin, float targetMax)
        {
            return Mathf.Lerp(targetMin, targetMax, Mathf.InverseLerp(-1, 1, value));
        }
        public static float RestoreScopedValue(float value, float sourceMin, float sourceMax)
        {
            return Mathf.Lerp(-1, 1, Mathf.InverseLerp(sourceMin, sourceMax, value));
        }

        //Debug Actions

        [DebugAction("Pawns", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = 1000)]
        public static void LogPawnPsyche(Pawn pawn)
        {
            var compPsyche = pawn.compPsyche();
            if (compPsyche != null)
            {
                string message = string.Join(", ", Enum.GetValues(typeof(Facet)).Cast<Facet>().Select(f => $"{f}: {compPsyche.Personality.GetFacetValue(f)}<< {compPsyche.Personality.GetFacetValueRaw(f)}"));
                Log.Message($"RimPsyche info for pawn {pawn.Name}\n\n{message}");
            }
        }

        [DebugAction("Pawns", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = 1000)]
        public static void LogPawnInterest(Pawn pawn)
        {
            var compPsyche = pawn.compPsyche();
            if (compPsyche != null)
            {
                string offsetMessage = string.Join(", ", compPsyche.Interests.interestOffset.Select(kvp => $"{kvp.Key}: {kvp.Value:F2}"));
                string message = string.Join(", ", compPsyche.Interests.interestScore.Select(kvp => $"{kvp.Key}: {kvp.Value:F2}"));
                Log.Message($"Interest info for pawn {pawn.Name}\n\nOffsets: {offsetMessage}\n\nScores: {message}");
            }
        }

        public static float GetMinAdultAge(Pawn pawn)
        {
            return pawn.ageTracker.AdultMinAge; ;
        }

    }
}
