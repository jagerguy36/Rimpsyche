using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Maux36.RimPsyche
{
    public class SocialDescriptor : PsycheDescriptorWorker
    {
        public SocialDescriptor()
        {
            positiveKey = "Personality_Social_Outgoing";
            negativeKey = "Personality_Social_Reserved";
            threshold = 0.35f;
            strongThreshold = 0.65f;
            extremeThreshold = 0.9f;
        }

        public override float Score(CompPsyche compPsyche)
        {
            return compPsyche.Personality.Sociality;
        }
    }
}