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
        public static float Boost(float A, float boostFactor = 0.5f)
        {
            float res = A * (1f + boostFactor * (1f - A * A));
            return res;

        }
        public static float Boost2(float A)
        {
            float adjA;
            float res;
            if (A >= 0)
            {
                adjA = (A - 1f);
                res = 1f - (adjA * adjA);
                return res;
            }
            else
            {
                adjA = (A + 1f);
                res = (adjA * adjA) - 1f;
                return res;
            }

        }
        public static float Boost3(float A)
        {
            float adjA;
            float res;
            if (A >= 0)
            {
                adjA = (1f - A);
                res = 1f - (adjA * adjA * adjA);
                return res;
            }
            else
            {
                adjA = (1f + A);
                res = (adjA * adjA * adjA) - 1f;
                return res;
            }

        }
        public static float SaddleShapeFunction(float x, float y, float controversiality = 1)
        {
            float C = 2 * controversiality * controversiality;
            float diff = (x - y);
            float diff2 = diff * diff;
            float sum = (x + y);
            float sum2 = sum * sum;
            float val = (-(1f + C) * diff2 + sum2) / (4f + C * diff2);
            val = Boost3(val);
            //Log.Message($"SaddleShapeFunction: {x}, {y} | controversiality: {controversiality}. val: {val}");
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

        public static bool IsGoodPositionForInteraction(IntVec3 cell, IntVec3 recipientCell, Map map)
        {
            if (cell.InHorDistOf(recipientCell, 12f)) return GenSight.LineOfSight(cell, recipientCell, map, skipFirstCell: true);
            return false;
        }
        private static int maxConvoOpinions = 10;
        public static void GainCoversationMemoryFast(string topicName, string topicLabel, float opinionOffset, Pawn parentPawn, Pawn otherPawn)
        {
            Thought_MemoryPostDefined newThought = (Thought_MemoryPostDefined)Activator.CreateInstance(DefOfRimpsyche.Rimpsyche_ConversationOpinion.ThoughtClass);
            newThought.def = DefOfRimpsyche.Rimpsyche_ConversationOpinion;
            newThought.sourcePrecept = null;
            newThought.Init();
            newThought.topicName = topicName;
            newThought.topicLabel = topicLabel;
            newThought.opinionOffset = opinionOffset;
            //Log.Message($"adding thought about {topicLabel} with opinionOffset {opinionOffset}");
            if (newThought.otherPawn == null && otherPawn == null)
            {
                Log.Error(string.Concat("Can't gain social thought ", newThought.def, " because its otherPawn is null and otherPawn passed to this method is also null. Social thoughts must have otherPawn."));
                return;
            }
            otherPawn = otherPawn ?? newThought.otherPawn;
            if (!newThought.def.socialTargetDevelopmentalStageFilter.Has(otherPawn.DevelopmentalStage))
            {
                return;
            }
            newThought.pawn = parentPawn;
            newThought.otherPawn = otherPawn;
            List<Thought_MemoryPostDefined> currentConvoMemories = parentPawn.needs.mood.thoughts.memories.Memories
                .OfType<Thought_MemoryPostDefined>()
                .Where(m => m.otherPawn == otherPawn)
                .ToList();

            if (currentConvoMemories.Count < maxConvoOpinions)
            {
                parentPawn.needs?.mood?.thoughts?.memories?.Memories.Add(newThought);
            }
            else
            {
                currentConvoMemories.Sort((m1, m2) => Mathf.Abs(m2.OpinionOffset()).CompareTo(Mathf.Abs(m1.OpinionOffset())));
                Thought_MemoryPostDefined memoryToCompareWith = currentConvoMemories[maxConvoOpinions - 1];
                if (Mathf.Abs(opinionOffset) < Mathf.Abs(memoryToCompareWith.OpinionOffset()))
                {
                    return;
                }
                for (int i = maxConvoOpinions - 1; i < currentConvoMemories.Count; i++)
                {
                    Thought_MemoryPostDefined m = currentConvoMemories[i];
                    m.age = m.DurationTicks + 300;
                }
                parentPawn.needs?.mood?.thoughts?.memories?.Memories.Add(newThought);
            }
        }
        public static float ConvoSocialFightChance(Pawn startCand, Pawn other, float startCandBaseChance, float startCandOpinio)
        {
            if (!startCand.interactions.SocialFightPossible(other))
            {
                return 0f;
            }
            float socialFightBaseChance = startCandBaseChance;
            socialFightBaseChance *= Mathf.InverseLerp(0.3f, 1f, startCand.health.capacities.GetLevel(PawnCapacityDefOf.Manipulation));
            socialFightBaseChance *= Mathf.InverseLerp(0.3f, 1f, startCand.health.capacities.GetLevel(PawnCapacityDefOf.Moving));
            List<Hediff> hediffs = startCand.health.hediffSet.hediffs;
            for (int i = 0; i < hediffs.Count; i++)
            {
                if (hediffs[i].CurStage != null)
                {
                    socialFightBaseChance *= hediffs[i].CurStage.socialFightChanceFactor;
                }
            }
            float num = startCandOpinio;
            socialFightBaseChance = ((!(num < 0f)) ? (socialFightBaseChance * GenMath.LerpDouble(0f, 1f, 1f, 0.6f, num)) : (socialFightBaseChance * GenMath.LerpDouble(-1f, 0f, 4f, 1f, num)));
            if (startCand.RaceProps.Humanlike)
            {
                List<Trait> allTraits = startCand.story.traits.allTraits;
                for (int j = 0; j < allTraits.Count; j++)
                {
                    if (!allTraits[j].Suppressed)
                    {
                        socialFightBaseChance *= allTraits[j].CurrentData.socialFightChanceFactor;
                    }
                }
            }
            int num2 = Mathf.Abs(startCand.ageTracker.AgeBiologicalYears - other.ageTracker.AgeBiologicalYears);
            if (num2 > 10)
            {
                if (num2 > 50)
                {
                    num2 = 50;
                }
                socialFightBaseChance *= GenMath.LerpDouble(10f, 50f, 1f, 0.25f, num2);
            }
            if (startCand.IsSlave)
            {
                socialFightBaseChance *= 0.5f;
            }
            if (startCand.genes != null)
            {
                socialFightBaseChance *= startCand.genes.SocialFightChanceFactor;
            }
            if (other.genes != null)
            {
                socialFightBaseChance *= other.genes.SocialFightChanceFactor;
            }
            //Log.Message($"{startCand.Name}'s startCandBaseChance: {startCandBaseChance} --> socialFightBaseChance: {socialFightBaseChance}");
            return Mathf.Clamp01(socialFightBaseChance);
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
            return Mathf.Max(1f, pawn.ageTracker.AdultMinAge);
        }

    }
}
