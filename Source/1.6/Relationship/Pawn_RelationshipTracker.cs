using RimWorld;
using System.Collections.Generic;
using System.Linq;
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
        
        //Flirt
        private int lastFlirtedTick = -60000;
        public int TicksSinceLastFlirt => Find.TickManager.TicksGame - lastFlirtedTick;
        public void Notify_Flirted()
        {
            lastFlirtedTick = Find.TickManager.TicksGame;
        }
        //Hangout
        private int lastHangoutTick = -60000;
        public int TicksSinceLastHangout => Find.TickManager.TicksGame - lastHangoutTick;
        public void Notify_Hangout()
        {
            lastHangoutTick = Find.TickManager.TicksGame;
        }

        //1. Used to cull Non-exclusive Romantic Relations that has not been used for long.
        //2. Affects lovers activity calm.
        public Dictionary<int, int> lastRomanceInteractionTick = new();
        public void Notify_CasualRelationFormed(Pawn other)
        {
            lastRomanceInteractionTick[other.thingIDNumber] = Find.TickManager.TicksGame;
            compPsyche.shouldTick = true;
        }
        // Timestamp a romantic interaction for pairs actually in a romantic relationship.
        public void RegisterRomanceInteraction(Pawn other)
        {
            if (!compPsyche.TryGetRomanticRelationDef(other, out _)) return;
            lastRomanceInteractionTick[other.thingIDNumber] = Find.TickManager.TicksGame;
        }
        public void Notify_BecameCommitted(Pawn other)
        {
            compPsyche.EvalShouldTick();
        }
        private static readonly List<(int id, PawnRelationDef def)> tmpToDissolve = [];
        public void CullStaleRelations()
        {
            int now = Find.TickManager.TicksGame;
            tmpToDissolve.Clear();
            // Casual relations past their window
            foreach (var kv in compPsyche.LoversCache)
            {
                if (!RimpsycheDatabase.RelationCullTick.TryGetValue(kv.Value, out int cullTick)) continue; //committed
                if (!lastRomanceInteractionTick.TryGetValue(kv.Key, out int last)) // Somehow lost its record. Formation was itself a romantic contact.
                {
                    DirectPawnRelation rel = FindRelation(pawn, kv.Key, kv.Value);
                    if (rel == null) continue;
                    last = rel.startTicks;
                    lastRomanceInteractionTick[kv.Key] = last;
                }
                if (now - last > cullTick) tmpToDissolve.Add((kv.Key, kv.Value));
            }

            // Dissolve. (LoverCache gets dirtied during Dissolve)
            for (int i = 0; i < tmpToDissolve.Count; i++)
            {
                DirectPawnRelation rel = FindRelation(pawn, tmpToDissolve[i].id, tmpToDissolve[i].def);
                if (rel != null) DissolveCasualRelation(rel);
            }
        }
        private static readonly List<int> tmpOrphans = [];
        public void PruneRecordsOnLoad()
        {
            tmpOrphans.Clear();
            foreach (int id in lastRomanceInteractionTick.Keys)
                if (!compPsyche.LoversCache.ContainsKey(id)) tmpOrphans.Add(id);
            for (int i = 0; i < tmpOrphans.Count; i++)
                lastRomanceInteractionTick.Remove(tmpOrphans[i]);
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
        public void DissolveCasualRelation(DirectPawnRelation rel)
        {
            Pawn other = rel.otherPawn;
            pawn.relations.RemoveDirectRelation(rel);   // reflexive: removes both sides
            lastRomanceInteractionTick.Remove(other.thingIDNumber);
            ConsumeRainCheck(other);
            compPsyche.EvalShouldTick();

            var otherRelationship = other.compPsyche()?.Relationship;
            if (otherRelationship != null)
            {
                otherRelationship.lastRomanceInteractionTick.Remove(pawn.thingIDNumber);
                otherRelationship.ConsumeRainCheck(pawn);
                otherRelationship.compPsyche.EvalShouldTick();
            }

            if (PawnUtility.ShouldSendNotificationAbout(pawn) || PawnUtility.ShouldSendNotificationAbout(other))
            {
                Messages.Message("RPR_DriftedApart".Translate(pawn.Named("PAWN1"), other.Named("PAWN2")), new LookTargets(pawn, other), MessageTypeDefOf.SilentInput, historical: false);
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
                foreach (int id in lastRomanceInteractionTick.Keys.ToList())
                {
                    if (VersionManager.DiscardedPawnThingIDnumber.Contains(id)) lastRomanceInteractionTick.Remove(id);
                }
                foreach (int id in rainCheckMemory.Keys.ToList())
                {
                    if (VersionManager.DiscardedPawnThingIDnumber.Contains(id)) rainCheckMemory.Remove(id);
                }
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
                if (Rimpsyche.RelationshipModuleLoaded)
                {
                    compPsyche.EvalShouldTick(); //Builds Lover cache
                    PruneRecordsOnLoad();
                }
            }
        }
    }
}
