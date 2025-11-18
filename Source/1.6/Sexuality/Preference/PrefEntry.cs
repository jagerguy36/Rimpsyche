using Verse;

namespace Maux36.RimPsyche
{
    public class PrefEntry : IExposable
    {
        public string stringKey;
        public int intKey;
        public float target;
        public float importance;
        public PrefEntry() { }
        public PrefEntry(string stringKey, int intKey, float target, float importance)
        {
            this.stringKey = stringKey;
            this.intKey = intKey;
            this.target = target;
            this.importance = importance;
        }
        public void ExposeData()
        {
            Scribe_Values.Look(ref stringKey, "stringKey");
            Scribe_Values.Look(ref intKey, "intKey");
            Scribe_Values.Look(ref target, "target");
            Scribe_Values.Look(ref importance, "importance");
        }
    }
}
