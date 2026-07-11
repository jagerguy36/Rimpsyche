using RimWorld;
using System.Text;
using Verse;

namespace Maux36.RimPsyche
{
    public class SociabilityDescriptorWorker : PsycheDescriptorWorker
    {
        public override float Score(CompPsyche compPsyche)
        {
            return compPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Sociability);
        }
        public override string GetTooltip(CompPsyche compPsyche)
        {
            bool direction = Score(compPsyche) > 0f;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(GetDescription(compPsyche));
            sb.AppendLine();
            sb.AppendLine("  " + "RPC_DescriptorBlame".Translate());
            sb.AppendLine($"    {GetBlame(compPsyche, PersonalityDefOf.Rimpsyche_Sociability, direction)}");
            return sb.ToString();
        }
    }

    public class AgreeableDescriptorWorker : PsycheDescriptorWorker
    {
        public override float Score(CompPsyche compPsyche)
        {
            var fervor = (0.2f * compPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Tact)) + compPsyche.Evaluate(RimpsycheDatabase.Fervor);  //-0.4~[0]~0.4
            return 2.5f * fervor;
        }
        public override string GetTooltip(CompPsyche compPsyche)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(GetDescription(compPsyche));
            sb.AppendLine();
            sb.AppendLine("  " + "RPC_DescriptorBlame".Translate());
            sb.AppendLine($"    {GetBlame(compPsyche, PersonalityDefOf.Rimpsyche_Tact)}");
            sb.AppendLine($"    {GetBlame(compPsyche, PersonalityDefOf.Rimpsyche_Passion)}");
            sb.AppendLine($"    {GetBlame(compPsyche, PersonalityDefOf.Rimpsyche_Aggressiveness, false)}");
            return sb.ToString();
        }
    }

    public class BelligerentDescriptorWorker : PsycheDescriptorWorker
    {
        public override float Score(CompPsyche compPsyche)
        {
            var intentFactor = compPsyche.Evaluate(RimpsycheDatabase.InitIntentFactor); // -4.5~3.5
            return intentFactor > 0f ? (intentFactor / 3.5f) : (intentFactor / 4.5f); // -1 ~ 1
        }
        public override string GetTooltip(CompPsyche compPsyche)
        {
            bool direction = Score(compPsyche) > 0f;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(GetDescription(compPsyche));
            sb.AppendLine();
            sb.AppendLine("  " + "RPC_DescriptorBlame".Translate());
            sb.AppendLine($"    {GetBlame(compPsyche, PersonalityDefOf.Rimpsyche_Aggressiveness, direction)}");
            sb.AppendLine($"    {GetBlame(compPsyche, PersonalityDefOf.Rimpsyche_Compassion, !direction)}");
            sb.AppendLine($"    {GetBlame(compPsyche, PersonalityDefOf.Rimpsyche_Tension, direction)}");
            if (compPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Competitiveness) < 0f)
                sb.AppendLine($"    {GetBlame(compPsyche, PersonalityDefOf.Rimpsyche_Competitiveness, direction)}");
            if (compPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Sociability) > 0f)
                sb.AppendLine($"    {GetBlame(compPsyche, PersonalityDefOf.Rimpsyche_Sociability, !direction)}");
            return sb.ToString();
        }
    }

    public class TouchyDescriptorWorker : PsycheDescriptorWorker
    {
        public override float Score(CompPsyche compPsyche)
        {
            var reciNegFactor = compPsyche.Evaluate(RimpsycheDatabase.reciNegativeChanceMultiplier); // -4.5~3.5
            return 10f * (reciNegFactor -1f);
        }
        public override string GetTooltip(CompPsyche compPsyche)
        {
            bool direction = Score(compPsyche) > 0f;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(GetDescription(compPsyche));
            sb.AppendLine();
            sb.AppendLine("  " + "RPC_DescriptorBlame".Translate());
            sb.AppendLine($"    {GetBlame(compPsyche, PersonalityDefOf.Rimpsyche_Tension, direction)}");
            sb.AppendLine($"    {GetBlame(compPsyche, PersonalityDefOf.Rimpsyche_Stability, !direction)}");
            sb.AppendLine($"    {GetBlame(compPsyche, PersonalityDefOf.Rimpsyche_Confidence, !direction)}");
            return sb.ToString();
        }
    }
    public class ReceptiveDescriptorWorker : PsycheDescriptorWorker
    {
        public override float Score(CompPsyche compPsyche)
        {
            var receiveBase = compPsyche.Evaluate(RimpsycheDatabase.ReceiveBase);
            var assertBase = compPsyche.Evaluate(RimpsycheDatabase.AssertBase);
            return 0.5f * (receiveBase + assertBase);
        }
        public override string GetTooltip(CompPsyche compPsyche)
        {
            bool direction = Score(compPsyche) > 0f;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(GetDescription(compPsyche));
            sb.AppendLine();
            sb.AppendLine("  " + "RPC_DescriptorBlame".Translate());
            sb.AppendLine($"    {GetBlame(compPsyche, PersonalityDefOf.Rimpsyche_Openness, direction)}");
            if (compPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Openness) > 0f)
                sb.AppendLine($"    {GetBlame(compPsyche, PersonalityDefOf.Rimpsyche_Trust, direction)}");
            else
                sb.AppendLine($"    {GetBlame(compPsyche, PersonalityDefOf.Rimpsyche_Trust, !direction)}");
            sb.AppendLine($"    {GetBlame(compPsyche, PersonalityDefOf.Rimpsyche_Tact, direction)}");
            if (compPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Tact) > 0f)
                sb.AppendLine($"    {GetBlame(compPsyche, PersonalityDefOf.Rimpsyche_Talkativeness, direction)}");
            else
                sb.AppendLine($"    {GetBlame(compPsyche, PersonalityDefOf.Rimpsyche_Talkativeness, !direction)}");
            return sb.ToString();
        }
    }
    public class TalkImpactDescriptorWorker : PsycheDescriptorWorker
    {
        public override float Score(CompPsyche compPsyche)
        {
            var talkfactor = compPsyche.Evaluate(RimpsycheDatabase.TalkFactor);
            return talkfactor - 1.5f; // -0.5 ~ 1
        }
        public override string GetTooltip(CompPsyche compPsyche)
        {
            bool direction = Score(compPsyche) > 0f;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(GetDescription(compPsyche));
            sb.AppendLine();
            sb.AppendLine("  " + "RPC_DescriptorBlame".Translate());
            sb.AppendLine($"    {GetBlame(compPsyche, PersonalityDefOf.Rimpsyche_Talkativeness, direction)}");
            if (compPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Playfulness) < 0f)
                sb.AppendLine($"    {GetBlame(compPsyche, PersonalityDefOf.Rimpsyche_Playfulness, !direction)}");
            return sb.ToString();
        }
    }
    public class InterestedTopicAttitudeDescriptorWorker : PsycheDescriptorWorker
    {
        public override float Score(CompPsyche compPsyche)
        {
            //High Passionate = higher highInterstTopic score | Low Passionate = highInterstTopic score is pretty much similar with low interest
            return compPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Passion);
        }
        public override string GetTooltip(CompPsyche compPsyche)
        {
            bool direction = Score(compPsyche) > 0f;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(GetDescription(compPsyche));
            sb.AppendLine();
            sb.AppendLine("  " + "RPC_DescriptorBlame".Translate());
            sb.AppendLine($"    {GetBlame(compPsyche, PersonalityDefOf.Rimpsyche_Passion, direction)}");
            return sb.ToString();
        }
    }
    public class UnInterestedTopicAttitudeDescriptorWorker : PsycheDescriptorWorker
    {
        public override float Score(CompPsyche compPsyche)
        {
            //High inquisitive = higher lowInterstTopic score | Low inquisitive = lowInterstTopic score is low.
            return compPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Inquisitiveness);
        }
        public override string GetTooltip(CompPsyche compPsyche)
        {
            bool direction = Score(compPsyche) > 0f;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(GetDescription(compPsyche));
            sb.AppendLine();
            sb.AppendLine("  " + "RPC_DescriptorBlame".Translate());
            sb.AppendLine($"    {GetBlame(compPsyche, PersonalityDefOf.Rimpsyche_Inquisitiveness, direction)}");
            return sb.ToString();
        }
    }
}