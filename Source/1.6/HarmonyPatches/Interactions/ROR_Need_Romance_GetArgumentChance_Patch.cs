using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using Verse;

namespace Maux36.RimPsyche
{
    //Romance on the Rim argument chance.
    [HarmonyPatch]
    public static class ROR_Need_Romance_GetArgumentChance_Patch
    {
        private const float ArgumentBaseChance = 1.6666667E-05f;
        private static SimpleCurve moodFactorCurve;
        public static bool Prepare()
        {
            return Rimpsyche_Utility.IsModActive("telardo.RomanceOnTheRim");
        }
        static MethodBase TargetMethod()
        {
            moodFactorCurve = (SimpleCurve)AccessTools.Field(AccessTools.TypeByName("RomanceOnTheRim.Tuning"), "ArgumentMoodFactorCurve").GetValue(null);
            return AccessTools.Method("RomanceOnTheRim.Need_Romance:GetArgumentChance");
        }
        //NeedInterval reads CachedLovers right before it calls this so ___cachedLovers is good
        //ROR's only caller is Rand.Value < GetArgumentChance(), so we roll here and return 1 or 0.
        public static bool Prefix(Pawn ___pawn, List<Pawn> ___cachedLovers, ref float __result)
        {
            if (___cachedLovers.Count == 0)
            {
                __result = 0f;
                return false;
            }
            float moodFactor = moodFactorCurve.Evaluate((___pawn.needs?.mood?.CurLevel).GetValueOrDefault());
            var psyche = ___pawn.compPsyche();
            if (psyche?.Enabled != true)
            {
                __result = ArgumentBaseChance * moodFactor;
                return false;
            }
            //The first pass uses the worst possible opinions (cached, cheap).
            float maxFactor = 0f;
            for (int i = 0; i < ___cachedLovers.Count; i++)
            {
                var loverPsyche = ___cachedLovers[i].compPsyche();
                if (loverPsyche?.Enabled != true)
                {
                    maxFactor += 1f;
                    continue;
                }
                maxFactor += MinOpinionCoupleArgumentCompatFactor(psyche, loverPsyche);
            }
            maxFactor /= ___cachedLovers.Count;
            float roll = Rand.Value;
            if (roll >= ArgumentBaseChance * maxFactor * moodFactor)
            {
                __result = 0f;
                return false;
            }
            //Fell under the bound
            float factor = 0f;
            for (int i = 0; i < ___cachedLovers.Count; i++)
            {
                Pawn lover = ___cachedLovers[i];
                var loverPsyche = lover.compPsyche();
                if (loverPsyche?.Enabled != true)
                {
                    factor += 1f;
                    continue;
                }
                factor += CoupleArgumentCompatFactor(___pawn, psyche, lover, loverPsyche);
            }
            factor /= ___cachedLovers.Count;
            __result = roll < ArgumentBaseChance * factor * moodFactor ? 1f : 0f;
            return false;
        }
        public static float MinOpinionCoupleArgumentCompatFactor(CompPsyche initPsyche, CompPsyche reciPsyche)
        {
            //Curve dictates 0.4f ~ 4f for -2.5f ~ 3f
            var intentFactor = initPsyche.Evaluate(RimpsycheDatabase.MinOpinionLoverArgumentFactor); //0.48875~3.45
            var result = intentFactor * reciPsyche.Evaluate(RimpsycheDatabase.reciNegativeChanceMultiplier) * 1.1f; //0.99~1.21
            //0.4838625 ~ 4.1745
            var initPlayfulness = initPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Playfulness);
            var reciPlayfulness = reciPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Playfulness);
            if (initPlayfulness > 0f && reciPlayfulness < 0f)
            {
                result *= (1f + 0.1f * (initPlayfulness * initPlayfulness * reciPlayfulness * reciPlayfulness)); // 1~1.1
            }
            return result;
        }
        public static float CoupleArgumentCompatFactor(Pawn initiator, CompPsyche initPsyche, Pawn recipient, CompPsyche reciPsyche)
        {
            //Curve dictates 0.4f ~ 4f for -2.5f ~ 3f
            var intentFactor = initPsyche.Evaluate(RimpsycheDatabase.LoverArgumentBase); // -8~7
            float initOpinion = initiator.relations.OpinionOf(recipient);
            intentFactor -= 0.03f * initOpinion; // -11~10
            intentFactor = intentFactor > 0f ? 1f + (intentFactor * 0.2f) : 1f + (intentFactor * 0.085f); //0.065~3
            float deliveryFactor = 1f - initPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Tact) * 0.15f; // 0.85~1.15
            intentFactor *= deliveryFactor; //0.05525~3.45
            float receiveFactor = reciPsyche.Evaluate(RimpsycheDatabase.reciNegativeChanceMultiplier); //0.9~1.1
            receiveFactor *= 1f - 0.001f * recipient.relations.OpinionOf(initiator); //0.81 ~ 1.21
            float result = intentFactor * receiveFactor; //0.0447525 ~ 4.1745
            var initPlayfulness = initPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Playfulness);
            var reciPlayfulness = reciPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Playfulness);
            if (initPlayfulness > 0f && reciPlayfulness < 0f)
            {
                result *= (1f + 0.1f * (initPlayfulness * initPlayfulness * reciPlayfulness * reciPlayfulness)); // 1~1.1
            }
            return result;
        }
    }
}
