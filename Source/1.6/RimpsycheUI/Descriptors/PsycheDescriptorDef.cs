using System;
using Verse;

namespace Maux36.RimPsyche
{
    public class PsycheDescriptorDef: Def
    {
        public string positiveLabel;
        public string negativeLabel = string.Empty;
        public string positiveDescription;
        public string negativeDescription = string.Empty;
        public float threshold;
        public float strongThreshold;
        public float extremeThreshold;
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
                    if (negativeLabel == string.Empty) workerInt.positiveOnly = true;
                }
                return workerInt;
            }
        }
    }
}