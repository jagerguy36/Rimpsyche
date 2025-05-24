using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Maux36.RimPsyche
{
    public class Thoughts_MemoryPostDefined : Thought_MemorySocial
    {
        public string defNameOverride;
        public string label;
        public float baseOpinionOffset;

        public Thoughts_MemoryPostDefined()
        {
        }

        public override void ExposeData()
        {
            if (def != null)
            {
                def.defName = "Rimpsyche_ConversationOpinion";
            }
            base.ExposeData();
            Scribe_Values.Look(ref defNameOverride, "defNameOverride", "Rimpsyche_ConversationOpinion");
            Scribe_Values.Look(ref label, "label", "conversation");
            Scribe_Values.Look(ref baseOpinionOffset, "realOpinionOffset", 5);
            ThoughtDef newDef = new ThoughtDef();
            newDef.defName = defNameOverride;
            newDef.label = "conversation";
            //def.durationDays = 60f; 
            newDef.durationDays = 5f; // ToDo: check if this change does anything
            newDef.thoughtClass = typeof(Thoughts_MemoryPostDefined);
            ThoughtStage stage = new ThoughtStage();
            stage.label = label;
            stage.baseOpinionOffset = baseOpinionOffset;
            newDef.stages.Add(stage);
            def = newDef;
        }

        public override void Init()
        {
            defNameOverride = def.defName;
            label = def.stages[0].label;
            baseOpinionOffset = def.stages[0].baseOpinionOffset;
            base.Init();
        }
    }
}
