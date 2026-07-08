using System;
using System.Collections.Generic;
using Verse;

namespace Maux36.RimPsyche
{
    public class PsycheDescriptorDef: Def
    {
        public string positiveKey;
        public string negativeKey;
        public float threshold;
        public float strongThreshold;
        public float extremeThreshold;
        public List<PersonalityDef> contributors;
        public Type workerClass = typeof(PsycheDescriptorWorker);
        [Unsaved(false)]
        private PsycheDescriptorWorker workerInt;

        public PsycheDescriptorWorker Worker
        {
            get
            {
                if (workerInt == null)
                {
                    workerInt = (PsycheDescriptorWorker)Activator.CreateInstance(workerClass);
                    workerInt.descriptorDef = this;
                }
                return workerInt;
            }
        }
    }
}