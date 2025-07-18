using RimWorld;
using System.Collections.Generic;
using Verse;

namespace Maux36.RimPsyche
{
    public class PlayLogEntry_InteractionConversation : PlayLogEntry_Interaction
    {
        public string topicName;
        public string topicLabel;
        private string RecipientName
        {
            get
            {
                if (recipient == null)
                {
                    return "null";
                }

                return recipient.LabelShort;
            }
        }
        public PlayLogEntry_InteractionConversation()
        {
        }
        public PlayLogEntry_InteractionConversation(InteractionDef intDef, Pawn initiator, Pawn recipient, string tName, string tLabel, List<RulePackDef> extraSentencePacks)
        {
            this.intDef = intDef;
            this.initiator = initiator;
            this.recipient = recipient;
            this.extraSentencePacks = extraSentencePacks;
            initiatorFaction = initiator.Faction;
            initiatorIdeo = initiator.Ideo;
            topicName = tName;
            topicLabel = tLabel;
        }

        protected override string ToGameStringFromPOV_Worker(Thing pov, bool forceLog)
        {
            var original = base.ToGameStringFromPOV_Worker(pov, forceLog);
            return string.Format(original, topicLabel);
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref topicName, "topicName", "topicName");
            Scribe_Values.Look(ref topicLabel, "topicLabel", "Something");
        }
        public override string ToString()
        {
            return intDef.label + ": " + InitiatorName + "->" + RecipientName;
        }
    }
}
