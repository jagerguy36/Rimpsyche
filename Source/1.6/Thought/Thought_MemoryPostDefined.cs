using RimWorld;
using Verse;

namespace Maux36.RimPsyche
{
    public class Thought_MemoryPostDefined : Thought_MemorySocial
    {
        public string topicName;
        public string topicLabel;
        private string cachedLabelCap;

        public Thought_MemoryPostDefined(){}
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref topicName, "topicName", "topicName");
            Scribe_Values.Look(ref topicLabel, "topicLabel", "something");
        }
        public override void Init()
        {
            base.Init();
            topicName = "topicName";
            topicLabel = "something";
        }
        public override string LabelCap
        {
            get
            {
                if (cachedLabelCap == null)
                {
                    cachedLabelCap = string.Format(RimpsycheDatabase.conversationMemoryString, topicLabel).CapitalizeFirst();
                }

                return cachedLabelCap;
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
                if (otherPawn == Thought_MemoryPostDefined.otherPawn && topicName == Thought_MemoryPostDefined.topicName)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
