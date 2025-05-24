using LudeonTK;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
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

        [DebugAction("Pawns", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = 1000)]
        public static void LogPawnPsyche(Pawn pawn)
        {
            var compPsyche = pawn.compPsyche();
            if (compPsyche != null)
            {
                string message = string.Join(", ", Enum.GetValues(typeof(Facet)).Cast<Facet>().Select(f => $"{f}: {compPsyche.Personality.GetFacetValue(f)}"));
                Log.Message($"RimPsyche info for pawn {pawn.Name}\n\n{message}");
            }
        }

        [DebugAction("Pawns", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = 1000)]
        public static void LogPawnInterest(Pawn pawn)
        {
            var compPsyche = pawn.compPsyche();
            if (compPsyche != null)
            {
                string message = string.Join(", ", compPsyche.Interests.interestScore.Select(kvp => $"{kvp.Key}: {kvp.Value:F2}"));
                Log.Message($"Interest info for pawn {pawn.Name}\n\n{message}");
            }
        }

    }
}
