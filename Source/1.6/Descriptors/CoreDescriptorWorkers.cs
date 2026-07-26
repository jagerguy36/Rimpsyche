using RimWorld;
using System.Text;
using Verse;

namespace Maux36.RimPsyche
{
    public class SociabilityDescriptorWorker : PsycheDescriptorWorker
    {
        protected override float Score(CompPsyche compPsyche)
        {
            return compPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Sociability);
        }
        protected override void Evaluate(StringBuilder ctx, CompPsyche compPsyche, float score)
        {
            Blame(ctx, compPsyche, PersonalityDefOf.Rimpsyche_Sociability);
        }
    }

    public class EloquenceDescriptorWorker : PsycheDescriptorWorker
    {
        protected override float Score(CompPsyche compPsyche)
        {
            var fervor = (0.2f * compPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Tact)) + compPsyche.Evaluate(RimpsycheDatabase.Fervor);  //-0.4~[0]~0.4
            return 2.5f * fervor;
        }
        protected override void Evaluate(StringBuilder ctx, CompPsyche compPsyche, float score)
        {
            Blame(ctx, compPsyche, PersonalityDefOf.Rimpsyche_Tact);
            Blame(ctx, compPsyche, PersonalityDefOf.Rimpsyche_Passion);
            Blame(ctx, compPsyche, PersonalityDefOf.Rimpsyche_Aggressiveness, PsycheDescDirection.Negative);
        }
    }

    public class InitIntentFactorDescriptorWorker : PsycheDescriptorWorker
    {
        protected override float Score(CompPsyche compPsyche)
        {
            var intentFactor = compPsyche.Evaluate(RimpsycheDatabase.InitIntentFactor); // -4.5~3.5
            float deliveryFactor = -compPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Tact) * 0.5f;
            var score = intentFactor + deliveryFactor; // -5~4
            return score > 0f ? (intentFactor / 4f) : (intentFactor / 5f); // -1 ~ 1
        }
        protected override void Evaluate(StringBuilder ctx, CompPsyche compPsyche, float score)
        {
            Blame(ctx, compPsyche, PersonalityDefOf.Rimpsyche_Aggressiveness);
            Blame(ctx, compPsyche, PersonalityDefOf.Rimpsyche_Compassion, PsycheDescDirection.Negative);
            Blame(ctx, compPsyche, PersonalityDefOf.Rimpsyche_Tension);
            if (compPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Competitiveness) < 0f)
                Blame(ctx, compPsyche, PersonalityDefOf.Rimpsyche_Competitiveness, PsycheDescDirection.Positive);
            if (compPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Sociability) > 0f)
                Blame(ctx, compPsyche, PersonalityDefOf.Rimpsyche_Sociability, PsycheDescDirection.Negative);
            Blame(ctx, compPsyche, PersonalityDefOf.Rimpsyche_Tact, PsycheDescDirection.Negative);
            Blame(ctx, compPsyche, PersonalityDefOf.Rimpsyche_Playfulness, PsycheDescDirection.Neutral);
        }
    }

    public class ReciNegativeChanceFactorDescriptorWorker : PsycheDescriptorWorker
    {
        protected override float Score(CompPsyche compPsyche)
        {
            var reciNegFactor = compPsyche.Evaluate(RimpsycheDatabase.reciNegativeChanceMultiplier); // -4.5~3.5
            return 10f * (reciNegFactor -1f);
        }
        protected override void Evaluate(StringBuilder ctx, CompPsyche compPsyche, float score)
        {
            Blame(ctx, compPsyche, PersonalityDefOf.Rimpsyche_Tension);
            Blame(ctx, compPsyche, PersonalityDefOf.Rimpsyche_Stability, PsycheDescDirection.Negative);
            Blame(ctx, compPsyche, PersonalityDefOf.Rimpsyche_Confidence, PsycheDescDirection.Negative);
        }
    }
    public class ReceptiveBaseDescriptorWorker : PsycheDescriptorWorker
    {
        protected override float Score(CompPsyche compPsyche)
        {
            var receiveBase = compPsyche.Evaluate(RimpsycheDatabase.ReceiveBase);
            var assertBase = compPsyche.Evaluate(RimpsycheDatabase.AssertBase);
            return 0.5f * (receiveBase + assertBase);
        }
        protected override void Evaluate(StringBuilder ctx, CompPsyche compPsyche, float score)
        {
            Blame(ctx, compPsyche, PersonalityDefOf.Rimpsyche_Openness);
            if (compPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Openness) > 0f)
                Blame(ctx, compPsyche, PersonalityDefOf.Rimpsyche_Trust, PsycheDescDirection.Positive);
            else
                Blame(ctx, compPsyche, PersonalityDefOf.Rimpsyche_Trust, PsycheDescDirection.Negative);
            if (compPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Tact) > 0f)
                Blame(ctx, compPsyche, PersonalityDefOf.Rimpsyche_Talkativeness, PsycheDescDirection.Positive);
            else
                Blame(ctx, compPsyche, PersonalityDefOf.Rimpsyche_Talkativeness, PsycheDescDirection.Negative);


        }
    }
    public class TalkFactorDescriptorWorker : PsycheDescriptorWorker
    {
        protected override float Score(CompPsyche compPsyche)
        {
            var talkfactor = compPsyche.Evaluate(RimpsycheDatabase.TalkFactor);
            return talkfactor - 1.5f; // -0.5 ~ 1
        }
        protected override void Evaluate(StringBuilder ctx, CompPsyche compPsyche, float score)
        {
            Blame(ctx, compPsyche, PersonalityDefOf.Rimpsyche_Talkativeness);
            if (compPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Playfulness) < 0f)
                Blame(ctx, compPsyche, PersonalityDefOf.Rimpsyche_Playfulness, PsycheDescDirection.Negative);
        }
    }
    public class InterestedTopicAttitudeDescriptorWorker : PsycheDescriptorWorker
    {
        protected override float Score(CompPsyche compPsyche)
        {
            //High Passionate = higher highInterstTopic score | Low Passionate = highInterstTopic score is pretty much similar with low interest
            return compPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Passion);
        }
        protected override void Evaluate(StringBuilder ctx, CompPsyche compPsyche, float score)
        {
            Blame(ctx, compPsyche, PersonalityDefOf.Rimpsyche_Passion);
        }
    }
    public class UnInterestedTopicAttitudeDescriptorWorker : PsycheDescriptorWorker
    {
        protected override float Score(CompPsyche compPsyche)
        {
            //High inquisitive = higher lowInterstTopic score | Low inquisitive = lowInterstTopic score is low.
            return compPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Inquisitiveness);
        }
        protected override void Evaluate(StringBuilder ctx, CompPsyche compPsyche, float score)
        {
            Blame(ctx, compPsyche, PersonalityDefOf.Rimpsyche_Inquisitiveness);
        }
    }
    public class SpontaneityDescriptorWorker : PsycheDescriptorWorker
    {
        protected override float Score(CompPsyche compPsyche)
        {
            return compPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Spontaneity);
        }
        protected override void Evaluate(StringBuilder ctx, CompPsyche compPsyche, float score)
        {
            Blame(ctx, compPsyche, PersonalityDefOf.Rimpsyche_Spontaneity);
        }
    }
}