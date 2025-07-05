using RimWorld;
using Verse;

namespace Maux36.RimPsyche
{
    public class Thought_MemoryPostDefined : Thought_MemorySocial
    {
        public string labelOverride;

        public Thought_MemoryPostDefined(){}
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref labelOverride, "labelOverride", "conversation");
        }
        public override void Init()
        {
            base.Init();
            labelOverride = "Conversed";
        }
        public override string LabelCap
        {
            get
            {
                return labelOverride.CapitalizeFirst();
            }
        }
        public override bool GroupsWith(Thought other)
        {
            if (!(other is Thought_MemoryPostDefined Thought_MemoryPostDefined))
            {
                return false;
            }

            if (base.GroupsWith(other))
            {
                if (otherPawn == Thought_MemoryPostDefined.otherPawn && labelOverride == Thought_MemoryPostDefined.labelOverride)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
