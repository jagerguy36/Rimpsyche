using RimWorld;
using System.Collections.Generic;
using Verse;

namespace Maux36.RimPsyche
{
    public class Pawn_RelationshipTracker : IExposable
    {
        public readonly Pawn pawn;
        public readonly CompPsyche compPsyche;

        public Pawn_RelationshipTracker(Pawn p)
        {
            pawn = p;
            compPsyche = p.compPsyche();
        }
        public void Initialize(PsycheData psycheData = null)
        {

        }
        public int crushPawnIdNumber = -1;
        public int crushEndTick = -1;
        private int lastFlirtedTick = -60000;
        public int TicksSinceLastFlirt => Find.TickManager.TicksGame - lastFlirtedTick;
        public void Notify_Flirted()
        {
            lastFlirtedTick = Find.TickManager.TicksGame;
        }
        private int lastHangoutTick = -60000;
        public void Notify_Hangout()
        {
            lastHangoutTick = Find.TickManager.TicksGame;
        }
        public int TicksSinceLastHangout => Find.TickManager.TicksGame - lastHangoutTick;


        //1. Used to cull Non-exclusive Romantic Relations that has not been used for long.
        //2. Used to decide exclusive lovers Date chance.
        public Dictionary<int, int> lastRomanceInteractionTick = new();
        public void Notify_CasualRelationFormed(Pawn other)
        {
            lastRomanceInteractionTick[other.thingIDNumber] = Find.TickManager.TicksGame;
        }
        // Timestamp a romantic interaction for pairs actually in a romantic relationship.
        public void RegisterRomanceInteraction(Pawn other)
        {
            if (!compPsyche.TryGetRomanticRelationDef(other, out _)) return;
            lastRomanceInteractionTick[other.thingIDNumber] = Find.TickManager.TicksGame;
        }
        public void Notify_BecameCommitted(Pawn other)
        {
            EvalShouldTick();
        }
        public bool HasCullableRelations()
        {
            if (lastRomanceInteractionTick.Count == 0) return false;
            List<DirectPawnRelation> rels = pawn.relations?.DirectRelations;
            if (rels == null) return false;
            for (int i = 0; i < rels.Count; i++)
                if (RimpsycheDatabase.RelationCullTick.ContainsKey(rels[i].def)) return true;
            return false;
        }
        public void EvalShouldTick()
        {
            compPsyche.shouldTick = HasCullableRelations();
        }
        private const int MinCullAfterTicks = 8 * 60000; // shortest tier
        private static readonly List<int> tmpStale = [];
        public void CullStaleRelations(Pawn pawn)
        {
            int now = Find.TickManager.TicksGame;
            tmpStale.Clear();
            foreach (var kv in lastRomanceInteractionTick)
                if (now - kv.Value > MinCullAfterTicks) tmpStale.Add(kv.Key);
            if (tmpStale.Count == 0) return;

            for (int i = 0; i < tmpStale.Count; i++)
            {
                int id = tmpStale[i];
                if (!compPsyche.LoversCache.TryGetValue(id, out PawnRelationDef def)
                    || !RimpsycheDatabase.RelationCullTick.TryGetValue(def, out int cullTick))
                {
                    lastRomanceInteractionTick.Remove(id);
                    continue;
                }

                if (now - lastRomanceInteractionTick[id] <= cullTick) continue;

                DirectPawnRelation rel = FindRelation(pawn, id, def);
                if (rel != null) DissolveCasualRelation(pawn, rel);
                else lastRomanceInteractionTick.Remove(id);   // cache was stale; drop the record
            }
        }
        private static DirectPawnRelation FindRelation(Pawn pawn, int otherId, PawnRelationDef def)
        {
            List<DirectPawnRelation> rels = pawn.relations?.DirectRelations;
            if (rels == null) return null;
            for (int i = 0; i < rels.Count; i++)
                if (rels[i].def == def && rels[i].otherPawn?.thingIDNumber == otherId)
                    return rels[i];
            return null;
        }
        public void DissolveCasualRelation(Pawn pawn, DirectPawnRelation rel, string messageKey = "RPR_DriftedApart")
        {
            Pawn other = rel.otherPawn;
            pawn.relations.RemoveDirectRelation(rel.def, other);   // reflexive: removes both sides

            lastRomanceInteractionTick.Remove(other.thingIDNumber);
            ConsumeRainCheck(other);

            var otherRelationship = other.compPsyche()?.Relationship;
            if (otherRelationship != null)
            {
                otherRelationship.lastRomanceInteractionTick.Remove(pawn.thingIDNumber);
                otherRelationship.ConsumeRainCheck(pawn);
            }

            if (messageKey != null
                && (PawnUtility.ShouldSendNotificationAbout(pawn) || PawnUtility.ShouldSendNotificationAbout(other)))
            {
                Messages.Message(messageKey.Translate(pawn.Named("PAWN1"), other.Named("PAWN2")),
                    new LookTargets(pawn, other), MessageTypeDefOf.SilentInput, historical: false);
            }
        }
        //RainCheck
        public const int RainCheckValidTick = 5 * 60000; //A raincheck holds for 5 days
        private Dictionary<int, int> rainCheckMemory = new();
        public void GiveRainCheck(Pawn otherPawn)
        {
            rainCheckMemory[otherPawn.thingIDNumber] = Find.TickManager.TicksGame + RainCheckValidTick;
        }
        public bool HasValidRainCheck(Pawn otherPawn)
        {
            if (rainCheckMemory.TryGetValue(otherPawn.thingIDNumber, out var checkTick))
            {
                if (Find.TickManager.TicksGame < checkTick) return true;
                rainCheckMemory.Remove(otherPawn.thingIDNumber);
            }
            return false;
        }
        public int TryGetRainCheck(Pawn otherPawn)
        {
            if (rainCheckMemory.TryGetValue(otherPawn.thingIDNumber, out var checkTick))
            {
                return checkTick;
            }
            return -1;
        }
        public bool ConsumeRainCheck(Pawn otherPawn)
        {
            return rainCheckMemory.Remove(otherPawn.thingIDNumber);
        }
        public void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.Saving)
            {
            }
            Scribe_Values.Look(ref lastFlirtedTick, "lastFlirtedTick", -60000);
            Scribe_Values.Look(ref lastHangoutTick, "lastHangoutTick", -60000);
            Scribe_Collections.Look(ref lastRomanceInteractionTick, "lastRomanceInteractionTick", LookMode.Value, LookMode.Value);
            Scribe_Collections.Look(ref rainCheckMemory, "rainCheckMemory", LookMode.Value, LookMode.Value);
            //Post load operations
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                //Fix null memories
                lastRomanceInteractionTick ??= new();
                rainCheckMemory ??= new();
            }
        }
    }
}
