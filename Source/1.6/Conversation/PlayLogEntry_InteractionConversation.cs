using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace Maux36.RimPsyche
{
    public class PlayLogEntry_InteractionConversation : PlayLogEntry_Interaction
    {
        public string topic;
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
        public PlayLogEntry_InteractionConversation(InteractionDef intDef, Pawn initiator, Pawn recipient, string topicName, List<RulePackDef> extraSentencePacks)
        {
            this.intDef = intDef;
            this.initiator = initiator;
            this.recipient = recipient;
            this.extraSentencePacks = extraSentencePacks;
            initiatorFaction = initiator.Faction;
            initiatorIdeo = initiator.Ideo;
            topic = topicName;
        }

        protected override string ToGameStringFromPOV_Worker(Thing pov, bool forceLog)
        {
            var original = base.ToGameStringFromPOV_Worker(pov, forceLog);
            return string.Format(original, topic);
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref topic, "topic", "Something");
        }
        public override string ToString()
        {
            return intDef.label + ": " + InitiatorName + "->" + RecipientName;
        }
    }
}
