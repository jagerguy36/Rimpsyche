using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Maux36.RimPsyche
{
    public class SociabilityDescriptorWorker : PsycheDescriptorWorker
    {
        public override float Score(CompPsyche compPsyche)
        {
            return 0.5f * (1f + compPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Sociability));
        }
        public override string GetDescription(CompPsyche compPsyche)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(GetKey(compPsyche));
            sb.AppendLine("  " + "PsycheDescriptorBlame".Translate()); //From personalities:
            sb.AppendLine($"    - {GetBlame(compPsyche, PersonalityDefOf.Rimpsyche_Selfishness)}");
            return sb.ToString();
        }
    }

    public class EloquenceDescriptorWorker : PsycheDescriptorWorker
    {
        public override float Score(CompPsyche compPsyche)
        {
            var fervor = (0.2f * compPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Tact)) + compPsyche.Evaluate(RimpsycheDatabase.Fervor);  //-0.4~[0]~0.4
            return 1f + 2.5f * fervor;
        }
        public override string GetDescription(CompPsyche compPsyche)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(GetKey(compPsyche));
            sb.AppendLine("  " + "PsycheDescriptorBlame".Translate()); //From personalities:
            sb.AppendLine($"    - {GetBlame(compPsyche, PersonalityDefOf.Rimpsyche_Tact)}");
            sb.AppendLine($"    - {GetBlame(compPsyche, PersonalityDefOf.Rimpsyche_Passion)}");
            sb.AppendLine($"    - {GetBlame(compPsyche, PersonalityDefOf.Rimpsyche_Aggressiveness, false)}");
            return sb.ToString();
        }
    }
    //InitIntentFactor: How likely someone is to insult or slight others.
    //AssertBase + ReceiveBase: How likely are they to still have good convo when different opinion.
    //reciNegativeChanceMultiplier: How likely they are to take offense (slighted or insulted).
}