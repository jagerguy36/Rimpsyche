using System;
using Verse;

namespace Maux36.RimPsyche
{
    public enum DescriptorType: byte
    {
        Undefined,
        Social,
        Mental,
        Work,
    }

    public class PsycheDescriptorDef: Def
    {
        public string positiveLabel;
        public string negativeLabel = string.Empty;
        public string positiveDescription;
        public string negativeDescription = string.Empty;
        //Under threshold 0~1 | lvl 0
        public float threshold; // 1->2 | lvl 1
        public float strongThreshold; // 2->3 | lvl 2
        public float extremeThreshold; // 3->~ | lvl 3
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
                    if (threshold == 0f)
                        workerInt.maxLevel = 0;
                    if (strongThreshold == 0f)
                        workerInt.maxLevel = 1;
                    else if (extremeThreshold == 0f)
                        workerInt.maxLevel = 2;
                    else
                        workerInt.maxLevel = 3;
                }
                return workerInt;
            }
        }
    }
}