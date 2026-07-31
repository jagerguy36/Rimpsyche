using System;
using Verse;

namespace Maux36.RimPsyche
{
    public enum DescriptorType: byte
    {
        Conversation,
        ThoughtTag,
        Gameplay,
        Romance
    }

    public class PsycheDescriptorDef: Def
    {
        public DescriptorType type = DescriptorType.Gameplay;

        //For Displosition UI.
        //If the label is left as empty, it will not be shown on the Disposition at all
        public string positiveLabel;
        public string negativeLabel;

        //Score 0 does not appear at all.
        //Under threshold 0~1 | lvl 0
        public float threshold; // 1->2 | lvl 1
        public float strongThreshold; // 2->3 | lvl 2
        public float extremeThreshold; // 3->~ | lvl 3

        //For Personality Node UI.
        //If the description is left as empty, it will not be shown on the Personality node at all.
        public string positiveDescription;
        public string neutralDescription;
        public string negativeDescription;

        [Unsaved(false)]
        public bool showEffect = false;

        //
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