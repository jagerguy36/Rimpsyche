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
        //DateHangout
        //To check activity cooldown.
        //Used by both Date and Hangout
        private int lastDateHangoutTick = -60000;
        public int TicksSinceLastDateHangout => Find.TickManager.TicksGame - lastDateHangoutTick;
        public void Notify_Hangout()
        {
            int tick = Find.TickManager.TicksGame;
            lastDateHangoutTick = tick;
        }
        //Used by ShowAffection Interaction to Calculate DatePull.
        private Dictionary<int, int> lastDateTickWith = new();
        private bool lastDateTickWithDirty = true;
        public IReadOnlyDictionary<int, int> LastDateTickWith
        {
            get
            {
                if (lastDateTickWithDirty) InitializeDateCache();
                return lastDateTickWith;
            }
        }
        private void InitializeDateCache()
        {
            lastDateTickWithDirty = false;
            lastDateTickWith.Clear();
            var memories = pawn.needs?.mood?.thoughts?.memories?.Memories;
            if (memories == null || RimpsycheDatabase.DateThoughtDef == null) return;
            int currentTick = Find.TickManager.TicksGame;
            for (int i = 0; i < memories.Count; i++)
            {
                Thought_Memory memory = memories[i];
                if (memory.def != RimpsycheDatabase.DateThoughtDef || memory.otherPawn == null) continue;
                int partnerId = memory.otherPawn.thingIDNumber;
                int dateTick = currentTick - memory.age;
                if (!lastDateTickWith.TryGetValue(partnerId, out int existingTick) || dateTick > existingTick)
                    lastDateTickWith[partnerId] = dateTick;
            }
        }
        public void Notify_DateWith(Pawn otherPawn)
        {
            int tick = Find.TickManager.TicksGame;
            lastDateHangoutTick = tick;
            if (lastDateTickWithDirty) InitializeDateCache();
            lastDateTickWith[otherPawn.thingIDNumber] = tick;
        }

        // Timestamp a romantic interaction for pairs actually in a romantic relationship.
        // Used to cull Non-exclusive Romantic Relations and adjust lovers activity chance.
        // Flirt and ShowAffection Both registers.
        // lastFlirtedTick and lastDateHangoutTick are pawn agnostic. This is Pawn specific.
        private Dictionary<int, int> lastRomanceInteractionTick = new();
        public void Notify_CasualRelationFormed(Pawn other)
        {
            lastRomanceInteractionTick[other.thingIDNumber] = Find.TickManager.TicksGame;
            compPsyche.shouldTick = true;
        }
        public void RegisterRomanceInteraction(Pawn other)
        {
            if (!compPsyche.TryGetRomanticRelationDef(other, out _)) return;
            lastRomanceInteractionTick[other.thingIDNumber] = Find.TickManager.TicksGame;
        }
        public int GetTickSinceLastRomanticInteractionWith(Pawn other)
        {
            if (lastRomanceInteractionTick.TryGetValue(other.thingIDNumber, out var tick))
            {
                return Find.TickManager.TicksGame - tick;
            }
            else
                return int.MaxValue;
        }
        public void Notify_BecameCommitted(Pawn other)
        {
            compPsyche.EvalShouldTick();
        }
        private static readonly List<(DirectPawnRelation rel, bool wentNowhere)> tmpToDissolve = [];
        public void CullStaleRelations()
        {
            int now = Find.TickManager.TicksGame;
            tmpToDissolve.Clear();
            // Casual relations past their window
            foreach (var kv in compPsyche.LoversCache)
            {
                if (!RimpsycheDatabase.RelationCullTick.TryGetValue(kv.Value, out int cullTick)) continue; //committed
                DirectPawnRelation rel = FindRelation(pawn, kv.Key, kv.Value);
                if (rel == null) continue;
                if (!lastRomanceInteractionTick.TryGetValue(kv.Key, out int last))
                {
                    last = rel.startTicks;
                    lastRomanceInteractionTick[kv.Key] = last;
                }

                if (now - last > cullTick) tmpToDissolve.Add((rel, false));
                else if (RimpsycheDatabase.RelationMaxAgeTick.TryGetValue(kv.Value, out int baseLifespan) && now - rel.startTicks > LifespanTicks(pawn, rel, baseLifespan))
                    tmpToDissolve.Add((rel, true));
            }

            // Dissolve. (LoverCache gets dirtied during Dissolve)
            for (int i = 0; i < tmpToDissolve.Count; i++)
                DissolveCasualRelation(tmpToDissolve[i].rel, tmpToDissolve[i].wentNowhere);
            tmpToDissolve.Clear();
        }
        private static int LifespanTicks(Pawn a, DirectPawnRelation rel, int baseTicks)
        {
            Pawn b = rel.otherPawn;
            // It ends when the less patient of the two stops waiting.
            float patience = PatienceOf(a, b);
            return (int)(baseTicks * patience);
        }
        // TODO: actually implement the logic
        private static float PatienceOf(Pawn pawn, Pawn other)
        {
            if (pawn.relations == null) return 1f;
            float opinion = pawn.relations.OpinionOf(other);
            float opinionFactor = 1f; // Mathf.Lerp(0.7f, 1.3f, Mathf.InverseLerp(0f, 80f, opinion));
            float inclinationFactor = 1f;
            return opinionFactor * inclinationFactor;
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
        public void DissolveCasualRelation(DirectPawnRelation rel, bool wentNowhere)
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

            if (wentNowhere && RimpsycheDatabase.WentNowhereThoughtDef != null)
            {
                //Add went nowhere thought that should prevent further romance until it expires.
                pawn.needs.mood?.thoughts.memories.TryGainMemory((Thought_Memory)ThoughtMaker.MakeThought(RimpsycheDatabase.WentNowhereThoughtDef), other);
                other.needs.mood?.thoughts.memories.TryGainMemory((Thought_Memory)ThoughtMaker.MakeThought(RimpsycheDatabase.WentNowhereThoughtDef), pawn);
            }

            if (PawnUtility.ShouldSendNotificationAbout(pawn) || PawnUtility.ShouldSendNotificationAbout(other))
            {
                string key = wentNowhere ? "RPR_WentNowhere" : "RPR_DriftedApart";
                Messages.Message(key.Translate(pawn.Named("PAWN1"), other.Named("PAWN2")), new LookTargets(pawn, other), MessageTypeDefOf.SilentInput, historical: false);
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
            Scribe_Values.Look(ref lastDateHangoutTick, "lastHangoutTick", -60000);
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
                    InitializeDateCache(); //Build LastHangoutTick cache
                    PruneRecordsOnLoad();
                }
            }
        }
    }
}
