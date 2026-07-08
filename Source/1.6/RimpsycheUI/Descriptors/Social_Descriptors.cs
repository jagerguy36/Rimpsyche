using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Maux36.RimPsyche
{
    public class SocialDescriptor : PsycheDescriptorWorker
    {
        public override float Score(CompPsyche compPsyche)
        {
            return compPsyche.Personality.Sociality;
        }
    }
}