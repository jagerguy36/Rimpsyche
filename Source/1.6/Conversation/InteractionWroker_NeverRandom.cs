using RimWorld;
using Verse;

namespace Maux36.RimPsyche
{
    public class InteractionWroker_NeverRandom : InteractionWorker
    {
        public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
        {
            return 0f;
        }
    }
}
