using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Maux36.RimPsyche
{
    public readonly struct DescriptorResult
    {
        public readonly float strength;
        public readonly string key;
        public readonly int intensity;
        public DescriptorResult(float strength, string key, int intensity)
        {
            this.strength = strength;
            this.key = key;
            this.intensity = intensity;
        }
    }

    public class PsycheDescriptorDef: Def
    {
        public string positiveKey;
        public string negativeKey;
        public float threshold;
        public float strongThreshold;
        public float extremeThreshold;
        public List<PersonalityDef> contributors;
        public Type workerClass = typeof(PsycheDescriptorWorker);
    }
}