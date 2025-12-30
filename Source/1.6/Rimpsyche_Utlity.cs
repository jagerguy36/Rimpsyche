using LudeonTK;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Maux36.RimPsyche
{
    public static class Rimpsyche_Utility
    {
        public static float Boost(float A, float boostFactor = 0.5f) //Unused
        {
            float res = A * (1f + boostFactor * (1f - A * A));
            return res;

        }
        public static float Boost2(float A) //For opinion. 0.2:0.36 | 0.5:0.75 | 0.7:0.91
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
        public static float Boost3(float A) //For Saddle shape function. 0.2:0.5 | 0.5:0.875 | 0.7:0.973
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
        public static float SaddleShapeFunction(float x, float y, float controversiality = 1f)
        {
            float C = 2f * controversiality * controversiality;
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

        public static float GetMinAdultAge(Pawn pawn)
        {
            return Mathf.Max(1f, pawn.ageTracker.AdultMinAge);
        }
        public static float GetFullAdultAge(Pawn pawn)
        {
            return Mathf.Max(1f, pawn.RaceProps.lifeStageAges[pawn.RaceProps.lifeStageAges.Count - 1].minAge);
        }
        public static float GetPawnAge(Pawn pawn)
        {
            return pawn.ageTracker.AgeBiologicalYearsFloat;
        }

        public static int GetParticipantIndex(bool isInitAdult, bool isReciAdult, bool limitNSFW)
        {
            int bits = (isInitAdult? 2 : 0) | (isReciAdult ? 1 : 0);
            int participantIndex = bits switch
            {
                0b11 => limitNSFW ? 1 : 0, // 1 = AAs, 0 = AA
                0b10 => 2, // 2 = AC
                0b01 => 3, // 3 = CA
                0b00 => 4, // 4 = CC
                _ => -1
            };
            return participantIndex;
        }

        public static bool IsGoodPositionForInteraction(IntVec3 cell, IntVec3 recipientCell, Map map)
        {
            if (cell.InHorDistOf(recipientCell, 12f)) return GenSight.LineOfSight(cell, recipientCell, map, skipFirstCell: true);
            return false;
        }
        private static int maxConvoOpinions = 10;
        public static void GainCoversationMemoryFast_Old(string topicName, string topicLabel, float opinionOffset, Pawn parentPawn, Pawn otherPawn)
        {
            Thought_MemoryPostDefined newThought = (Thought_MemoryPostDefined)Activator.CreateInstance(DefOfRimpsyche.Rimpsyche_ConversationOpinion.ThoughtClass);
            newThought.def = DefOfRimpsyche.Rimpsyche_ConversationOpinion;
            newThought.sourcePrecept = null;
            newThought.Init();
            newThought.topicName = topicName;
            newThought.topicLabel = topicLabel;
            newThought.opinionOffset = opinionOffset;
            newThought.durationTicksOverride = (int)((40f + Mathf.Abs(opinionOffset)) * 60000f);
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
            var memories = parentPawn.needs.mood.thoughts.memories.Memories;
            List<Thought_MemoryPostDefined> currentConvoMemories = new();
            for (int i = 0; i < memories.Count; i++)
            {
                var mem = memories[i];
                if (mem.otherPawn == otherPawn && mem is Thought_MemoryPostDefined mpd)
                    currentConvoMemories.Add(mpd);
            }

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
        public static void GainCoversationMemoryFast(string topicName, string topicLabel, float opinionOffset, Pawn parentPawn, Pawn otherPawn)
        {
            //Log.Message($"adding thought about {topicLabel} with opinionOffset {opinionOffset}");
            if (!DefOfRimpsyche.Rimpsyche_ConversationOpinion.socialTargetDevelopmentalStageFilter.Has(otherPawn.DevelopmentalStage))
                return;

            bool shouldAdd = false;
            var memories = parentPawn.needs.mood.thoughts.memories.Memories;
            List<Thought_MemoryPostDefined> currentConvoMemories = new();
            for (int i = 0; i < memories.Count; i++)
            {
                var mem = memories[i];
                if (mem.otherPawn == otherPawn && mem is Thought_MemoryPostDefined mpd)
                    currentConvoMemories.Add(mpd);
            }
            if (currentConvoMemories.Count < maxConvoOpinions)
            {
                shouldAdd = true;
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
                shouldAdd = true;
            }
            if (!shouldAdd)
                return;

            Thought_MemoryPostDefined newThought = (Thought_MemoryPostDefined)Activator.CreateInstance(DefOfRimpsyche.Rimpsyche_ConversationOpinion.ThoughtClass);
            newThought.def = DefOfRimpsyche.Rimpsyche_ConversationOpinion;
            newThought.sourcePrecept = null;
            newThought.Init();
            newThought.topicName = topicName;
            newThought.topicLabel = topicLabel;
            newThought.opinionOffset = opinionOffset;
            newThought.durationTicksOverride = (int)((40f + Mathf.Abs(opinionOffset)) * 60000f);
            newThought.pawn = parentPawn;
            newThought.otherPawn = otherPawn;

            parentPawn.needs?.mood?.thoughts?.memories?.Memories.Add(newThought);
        }
        public static float ConvoSocialFightChance(Pawn startCand, Pawn other, float startCandBaseChance, float startCandOpinio) //Same as vanilla, just using this to avoid calculating opinion again for performance.
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
            int num2 = Mathf.Abs((int)GetPawnAge(startCand) - (int)GetPawnAge(other));
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
        public static float GetRandomCompatibility(CompPsyche initiatorPsyche, CompPsyche recipientPsyche)
        {
            float initPassion = initiatorPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Passion);
            float initInquisitiveness = initiatorPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Inquisitiveness);

            float reciPassion = recipientPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Passion);
            float reciInquisitiveness = recipientPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Inquisitiveness);

            Interest convoInterest = initiatorPsyche.Interests.ChooseInterest();
            float topicAlignment = convoInterest.GetAverageAlignment(initiatorPsyche, recipientPsyche); // -1~1
            if (topicAlignment > 0)
            {
                float initInterestScore = initiatorPsyche.Interests.GetOrCreateInterestScore(convoInterest) * 0.01f;
                float reciInterestScore = recipientPsyche.Interests.GetOrCreateInterestScore(convoInterest) * 0.01f;
                float tAbs = Mathf.Abs(topicAlignment);
                //Assume mutual opinion of 1f
                float initInterestF = 1.5f + (initInterestScore * (1f + (0.5f * initPassion))) + 0.25f * ((1f - initInterestScore) * (1f + initInquisitiveness)); //1.5 + 0~1.5 => 1.5~3
                float reciInterestF = 1.5f + (reciInterestScore * (1f + (0.5f * reciPassion))) + 0.25f * ((1f - reciInterestScore) * (1f + reciInquisitiveness)); //1.5 + 0~1.5 => 1.5~3
                float initTalkF = initiatorPsyche.Evaluate(RimpsycheDatabase.TalkFactor) * initInterestF; // 0.5~7.5 [2.625]
                float reciTalkF = recipientPsyche.Evaluate(RimpsycheDatabase.TalkFactor) * reciInterestF; // 0.5~7.5 [2.625]
                float aligntmentLengthFactor = -1f * tAbs * (tAbs - 2f) + 1f; //1~2
                float lengthMult = 0.1f * (5f + initTalkF + reciTalkF) * aligntmentLengthFactor; // 0.8~2 * 1~2 || 0.8~4
                float scoreBase = 1.5f + (4f * topicAlignment); //1.5~5.5
                float lengthOpinionMult = (6f * lengthMult) / (lengthMult + 2f); //1.71 ~ 4
                float averageScore = scoreBase * lengthOpinionMult; //2.57~22
                //Log.Message($"======{convoInterest.name} Alignment Between {initiatorPsyche.parentPawn.Name} and {recipientPsyche.parentPawn.Name} is {averageScore / 8f}");
                return averageScore/8f; //This should give 1 when 8, so that it can compare to SexDrive 1.
            }
            else
            {
                //Log.Message($"======{convoInterest.name} Alignment Between {initiatorPsyche.parentPawn.Name} and {recipientPsyche.parentPawn.Name} is {0f}");
                return 0f;
            }
        }

        //For General report
        public static string GetPersonalityDesc(PersonalityDef personality, float value)
        {
            float absValue = Mathf.Abs(value);
            string intensityKey = "RimPsycheIntensityNeutral";
            if (absValue >= 0.75f)
            {
                intensityKey = "RimPsycheIntensityExtremely";
            }
            else if (absValue >= 0.5f)
            {
                intensityKey = "RimPsycheIntensityVery";
            }
            else if (absValue >= 0.25f)
            {
                intensityKey = "RimPsycheIntensitySomewhat";
            }
            else if (absValue > 0f)
            {
                intensityKey = "RimPsycheIntensityMarginally";
            }

            string personalityName = (value >= 0) ? personality.high : personality.low;

            if (LanguageDatabase.activeLanguage.HaveTextForKey(intensityKey))
            {
                return intensityKey.Translate(personalityName);
            }
            else
            {
                return RimpsycheDatabase.IntensityKeysDefault[intensityKey] + " " + personalityName;
            }
        }

        //For Use of LLM
        public static string GetPersonalityDescriptionNumber(Pawn pawn, int count = 0)
        {
            var compPsyche = pawn.compPsyche();
            if (compPsyche?.Enabled == true)
            {
                var sortedPersonality = DefDatabase<PersonalityDef>.AllDefs.OrderByDescending(f => Math.Abs(compPsyche.Personality.GetPersonality(f))).Select(f => $"{f.label.CapitalizeFirst()}: {compPsyche.Personality.GetPersonality(f).ToString("F2")}"); // Format each trait
                count = Mathf.Min(count, sortedPersonality.Count());
                if (count == 0) count = sortedPersonality.Count();
                return string.Join(", ", [.. sortedPersonality.Take(count)]);
            }
            return string.Empty;
        }

        private static string PersonalityInWords(float value, string personalityLow, string personalityHigh)
        {
            float absValue = Mathf.Abs(value);
            string intensityKey = "RimPsycheIntensityNeutral";
            if (absValue >= 0.75f)
            {
                intensityKey = "RimPsycheIntensityExtremely";
            }
            else if (absValue >= 0.5f)
            {
                intensityKey = "RimPsycheIntensityVery";
            }
            else if (absValue >= 0.25f)
            {
                intensityKey = "RimPsycheIntensitySomewhat";
            }
            else if (absValue > 0f)
            {
                intensityKey = "RimPsycheIntensityMarginally";
            }

            string personalityName = (value >= 0) ? personalityHigh : personalityLow;

            if (LanguageDatabase.activeLanguage.HaveTextForKey(intensityKey))
            {
                return intensityKey.Translate(personalityName) + $" ({(absValue * 100f).ToString("F0")}/100)";
            }
            else
            {
                return RimpsycheDatabase.IntensityKeysDefault[intensityKey] + $" {personalityName} ({(absValue * 100f).ToString("F0")}/100)";
            }
        }
        public static string GetPersonalityDescriptionWord(Pawn pawn, int count = 0)
        {
            var compPsyche = pawn.compPsyche();
            if (compPsyche?.Enabled == true)
            {
                var sortedPersonality = DefDatabase<PersonalityDef>.AllDefs.OrderByDescending(p => Math.Abs(compPsyche.Personality.GetPersonality(p))).Select(p => PersonalityInWords(compPsyche.Personality.GetPersonality(p), p.low, p.high)); // Format each trait
                count = Mathf.Min(count, sortedPersonality.Count());
                if (count == 0) count = sortedPersonality.Count();
                return string.Join(", ", [.. sortedPersonality.Take(count)]);
            }
            return string.Empty;
        }
    }
}
