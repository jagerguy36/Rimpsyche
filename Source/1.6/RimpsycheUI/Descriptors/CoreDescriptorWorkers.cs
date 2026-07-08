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
    }

    public class EloquenceDescriptorWorker : PsycheDescriptorWorker
    {
        public override float Score(CompPsyche compPsyche)
        {
            var fervor = (0.2f * compPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Tact)) + compPsyche.Evaluate(RimpsycheDatabase.Fervor);  //-0.4~[0]~0.4
            return 1f + 2.5f * fervor;
        }
    }
}