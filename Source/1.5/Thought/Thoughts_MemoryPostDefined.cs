using RimWorld;
using Verse;

namespace Maux36.RimPsyche
{
    public class Thoughts_MemoryPostDefined : Thought_MemorySocial
    {
        public string defNameOverride;
        public string labelOverride;
        public float baseOpinionOffsetOverride;
        public Thoughts_MemoryPostDefined(){}
        public override void ExposeData()
        {
            if (def != null) def.defName = "Rimpsyche_ConversationOpinion";
            base.ExposeData();
            Scribe_Values.Look(ref defNameOverride, "defNameOverride", "Rimpsyche_ConversationOpinion");
            Scribe_Values.Look(ref labelOverride, "labelOverride", "conversation");
            Scribe_Values.Look(ref baseOpinionOffsetOverride, "baseOpinionOffsetOverride", 0);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                ThoughtDef newDef = Rimpsyche_Utility.CreateSocialThought(defNameOverride, labelOverride, baseOpinionOffsetOverride);
                def = newDef;
            }
            def.defName = defNameOverride;
        }
        public override void Init()
        {
            base.Init();
            defNameOverride = def.defName;
            labelOverride = def.stages[0].label;
            baseOpinionOffsetOverride = def.stages[0].baseOpinionOffset;
        }
        public override float OpinionOffset()
        {
            return baseOpinionOffsetOverride;
        }
        public override string LabelCap
        {
            get
            {
                return labelOverride.CapitalizeFirst(); // Per-instance label
            }
        }
    }
}
