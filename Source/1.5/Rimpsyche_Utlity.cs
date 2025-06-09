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
            newDef.thoughtClass = typeof(Thoughts_MemoryPostDefined);
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
            float f0 = 1f - 0.5f * controversiality;
            float gamma = 4f * controversiality * controversiality;
            float a = 1f + f0 + gamma;
            float b = 1f - f0;
            float diff = 0.5f * (x - y);
            float diff2 = diff * diff;
            float sum = 0.5f * (x + y);
            float sum2 = sum * sum;
            Log.Message($"SaddleShapeFunction: {x}, {y} | controversiality: {controversiality}");
            return (f0 - a * diff2 + b * sum2) / (1f + gamma * diff2);
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

        public static Dictionary<Pair<string, int>, List<(Facet, float, float)>> TraitGateDatabase = new()
        {
            [new Pair<string, int>("NaturalMood", 2)] = new List<(Facet, float, float)>
            {
                (Facet.Pessimism, -50f, -25f)
            },
            [new Pair<string, int>("NaturalMood", 1)] = new List<(Facet, float, float)>
            {
                (Facet.Pessimism, -50f, 0f)
            },
            [new Pair<string, int>("NaturalMood", -1)] = new List<(Facet, float, float)>
            {
                (Facet.Pessimism, 0f, 50f)
            },
            [new Pair<string, int>("NaturalMood", -2)] = new List<(Facet, float, float)>
            {
                (Facet.Pessimism, 25f, 50f)
            },
            [new Pair<string, int>("Kind", 0)] = new List<(Facet, float, float)>
            {
                (Facet.Cooperation, 0f, 50f)
            },
        };

    }
}
