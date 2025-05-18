using RimWorld;
using System.Collections.Generic;
using Verse.AI.Group;
using Verse;

namespace Maux36.RimPsyche
{
    public class InteractionWorker_Conversation : InteractionWorker
    {
        public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
        {
            return 0;
        }


        public override void Interacted(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks, out string letterText, out string letterLabel, out LetterDef letterDef, out LookTargets lookTargets)
        {
            Log.Message($"Continue Interacted called!.");
            Log.Message($"by {initiator.Name} with {recipient.Name}.");
            letterText = null;
            letterLabel = null;
            letterDef = null;
            lookTargets = null;
            //string convoTopic = "RimPsycheTopic";
            var initiatorPsyche = initiator.compPsyche();
            var recipientPsyche = recipient.compPsyche();
            if (initiatorPsyche != null && recipientPsyche != null)
            {
                var convoLength = Rand.Range(250, 1250);
                Log.Message($"initiator {initiator.Name} continued a conversation with {recipient.Name}. new convolength: {convoLength}. Check again at :{Find.TickManager.TicksGame + convoLength}");
                initiatorPsyche.convoCheckTick = Find.TickManager.TicksGame + convoLength;
                initiatorPsyche.convoCheckTick = -1;
            }
        }
    }
}
