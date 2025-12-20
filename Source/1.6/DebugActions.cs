using LudeonTK;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Maux36.RimPsyche
{
    public static class DebugActions
    {
        [DebugAction("Pawns", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = 1000)]
        public static void PersonalityFull_LLM(Pawn pawn)
        {
            var message = Rimpsyche_Utility.GetPersonalityDescriptionNumber(pawn, 0) + " (Range: -1 ~ 1)";
            Log.Message($"RimPsyche info for pawn {pawn.Name}\n\n{message}\n\n");
        }

        [DebugAction("Pawns", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = 1000)]
        public static void PersonalityShort_LLM(Pawn pawn)
        {
            var message = Rimpsyche_Utility.GetPersonalityDescriptionNumber(pawn, 5) + " (Range: -1 ~ 1)";
            Log.Message($"RimPsyche info for pawn {pawn.Name}\n\n{message}\n\n");
        }

        [DebugAction("Pawns", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = 1000)]
        public static void PersonalityWordsFull_LLM(Pawn pawn)
        {
            var message = Rimpsyche_Utility.GetPersonalityDescriptionWord(pawn);
            Log.Message($"RimPsyche info for pawn {pawn.Name}\n\n{message}\n\n");
        }

        [DebugAction("Pawns", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = 1000)]
        public static void PersonalityWordsShort_LLM(Pawn pawn)
        {
            var message = Rimpsyche_Utility.GetPersonalityDescriptionWord(pawn, 5);
            Log.Message($"RimPsyche info for pawn {pawn.Name}\n\n{message}\n\n");
        }

        [DebugAction("Pawns", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = 1000)]
        public static void CopyPawnPsyche(Pawn pawn)
        {
            RimPsycheWorldComp.tempData = PsycheDataUtil.GetPsycheData(pawn);
            Log.Message($"RimPsyche copied {pawn.Name}'s Psyche.");
        }

        [DebugAction("Pawns", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = 1000)]
        public static void PastePawnPsyche(Pawn pawn)
        {
            var newPsyche = RimPsycheWorldComp.tempData;
            PsycheDataUtil.InjectPsycheData(pawn, newPsyche, true);
            Log.Message($"RimPsyche injected copied psyche to {pawn.Name}");
        }


        [DebugAction("Pawns", "Get Random Alignment", false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void GetRandomAlignment(Pawn p)
        {
            var compPsyche = p.compPsyche();
            if (compPsyche?.Enabled!=true)
            {
                return;
            }
            List<DebugMenuOption> list = new List<DebugMenuOption>();
            foreach (Pawn item in from x in PawnsFinder.AllMapsWorldAndTemporary_Alive
                                  where x.RaceProps.Humanlike && x.Faction == Faction.OfPlayer
                                  orderby x.def == p.def descending, x.IsWorldPawn()
                                  select x)
            {
                if (p != item)
                {
                    Pawn otherLocal = item;
                    list.Add(new DebugMenuOption(otherLocal.LabelShort + " (" + otherLocal.KindLabel + ")", DebugMenuOptionMode.Action, delegate
                    {
                        var otherPsyche = otherLocal.compPsyche();
                        if (otherPsyche?.Enabled != true)
                            return;
                        var randAlignment = Rimpsyche_Utility.GetRandomCompatibility(compPsyche, otherPsyche);
                        Log.Message($"======{p.Name} <-> {otherLocal.Name} | {randAlignment}");
                    }));
                }
            }
            Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
        }

        //[DebugAction("Pawns", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = 1000)]
        //public static void LogPawnPsyche(Pawn pawn)
        //{
        //    var compPsyche = pawn.compPsyche();
        //    if (compPsyche != null)
        //    {
        //        string message = string.Join(", ", Enum.GetValues(typeof(Facet)).Cast<Facet>().Select(f => $"{f}: {compPsyche.Personality.GetFacetValue(f)}<< {compPsyche.Personality.GetFacetValueRaw(f)}"));
        //        Log.Message($"RimPsyche info for pawn {pawn.Name}\n\n{message}");
        //    }
        //}

        //[DebugAction("Pawns", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = 1000)]
        //public static void LogPawnPersonality(Pawn pawn)
        //{
        //    var compPsyche = pawn.compPsyche();
        //    if (compPsyche != null)
        //    {
        //        string message = string.Join("\n", DefDatabase<PersonalityDef>.AllDefs.Select(f => $"{f.label}: {compPsyche.Personality.GetPersonality(f)}"));
        //        Log.Message($"Personality of {pawn.Name}\n\n{message}");
        //    }
        //}

        //[DebugAction("Pawns", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = 1000)]
        //public static void LogPawnInterest(Pawn pawn)
        //{
        //    var compPsyche = pawn.compPsyche();
        //    if (compPsyche != null)
        //    {
        //        string offsetMessage = string.Join(", ", compPsyche.Interests.interestOffset.Select(kvp => $"{kvp.Key}: {kvp.Value:F2}"));
        //        string message = string.Join(", ", compPsyche.Interests.interestScore.Select(kvp => $"{kvp.Key}: {kvp.Value:F2}"));
        //        Log.Message($"Interest info for pawn {pawn.Name}\n\nOffsets: {offsetMessage}\n\nScores: {message}");
        //    }
        //}

        //[DebugAction("Pawns", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = 1000)]
        //public static void ShowAllInteractionChances(Pawn pawn)
        //{
        //    DebugTools.curTool = new DebugTool("Select target...", delegate
        //    {
        //        Pawn target = PawnAt(UI.MouseCell());
        //        if (target == null)
        //        {
        //            Log.Message($"target is null");
        //            return;
        //        }
        //        List<InteractionDef> allDefsListForReading = DefDatabase<InteractionDef>.AllDefsListForReading;
        //        Log.Message($"{pawn.Name} -> {target.Name}");
        //        foreach (InteractionDef def in allDefsListForReading)
        //        {
        //            var name = def.defName;
        //            var weight = def.Worker.RandomSelectionWeight(pawn, target);
        //            Log.Message($"{name} | {weight}");
        //        }
        //    });
        //    static Pawn PawnAt(IntVec3 c)
        //    {
        //        foreach (Thing item in Find.CurrentMap.thingGrid.ThingsAt(c))
        //        {
        //            if (item is Pawn result)
        //            {
        //                return result;
        //            }
        //        }
        //        return null;
        //    }
        //}

        //[DebugAction("Pawns", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = 1000)]
        //public static void ReportSexualityMemory(Pawn pawn)
        //{
        //    var compPsyche = pawn.compPsyche();
        //    if (compPsyche.Enabled != true)
        //    {
        //        Log.Message("Psyche is not enabled");
        //        return;
        //    }
        //    var sexuality = compPsyche.Sexuality;
        //    if (sexuality.relationship == null || sexuality.GetPreferenceRaw() == null || sexuality.knownOrientation == null) Log.Message($"{pawn.Name} has null memory");
        //    Log.Message($"{pawn.Name}| relationship count: {sexuality.relationship.Count} | preference count: {sexuality.GetPreferenceRaw().Count} | knownOrientation count: {sexuality.knownOrientation.Count}");

        //}
    }
}
