using System.Collections.Generic;
using Verse;

namespace Maux36.RimPsyche
{
    public class PsycheData : IExposable
    {
        //Personality
        public float imagination = 0f;
        public float intellect = 0f;
        public float curiosity = 0f;

        public float industriousness = 0f;
        public float orderliness = 0f;
        public float integrity = 0f;

        public float sociability = 0f;
        public float assertiveness = 0f;
        public float enthusiasm = 0f;

        public float compassion = 0f;
        public float cooperation = 0f;
        public float humbleness = 0f;

        public float volatility = 0f;
        public float pessimism = 0f;
        public float insecurity = 0f;

        //Interests
        public Dictionary<string, float> interestScore = new Dictionary<string, float>(); // -35~35

        //Sexuality
        public SexualOrientation orientationCategory = SexualOrientation.None;
        public float mKinsey = -1f;
        public float attraction = 0f;
        public float sexDrive = 0f;

        public HashSet<int> knownOrientation = new();
        //public Dictionary<int, float> acquaintanceship = new();
        //public Dictionary<int, float> relationship = new();
        public Dictionary<string, List<PrefEntry>> preference = new();

        public void ExposeData()
        {
            Scribe_Values.Look(ref imagination, "imagination", 0, false);
            Scribe_Values.Look(ref intellect, "intellect", 0, false);
            Scribe_Values.Look(ref curiosity, "curiosity", 0, false);

            Scribe_Values.Look(ref industriousness, "industriousness", 0, false);
            Scribe_Values.Look(ref orderliness, "orderliness", 0, false);
            Scribe_Values.Look(ref integrity, "integrity", 0, false);

            Scribe_Values.Look(ref sociability, "sociability", 0, false);
            Scribe_Values.Look(ref assertiveness, "assertiveness", 0, false);
            Scribe_Values.Look(ref enthusiasm, "enthusiasm", 0, false);

            Scribe_Values.Look(ref compassion, "compassion", 0, false);
            Scribe_Values.Look(ref cooperation, "cooperation", 0, false);
            Scribe_Values.Look(ref humbleness, "humbleness", 0, false);

            Scribe_Values.Look(ref volatility, "volatility", 0, false);
            Scribe_Values.Look(ref pessimism, "pessimism", 0, false);
            Scribe_Values.Look(ref insecurity, "insecurity", 0, false);

            Scribe_Collections.Look(ref interestScore, "interestScore", LookMode.Value, LookMode.Value);

            Scribe_Values.Look(ref orientationCategory, "category", SexualOrientation.None);
            Scribe_Values.Look(ref mKinsey, "mKinsey", -1f);
            Scribe_Values.Look(ref attraction, "attraction", 0f);
            Scribe_Values.Look(ref sexDrive, "sexDrive", 0f);
            Scribe_Collections.Look(ref knownOrientation, "knownOrientation", LookMode.Value);
            //Scribe_Collections.Look(ref acquaintanceship, "acquaintanceship", LookMode.Value, LookMode.Value);
            //Scribe_Collections.Look(ref relationship, "relationship", LookMode.Value, LookMode.Value);
            Scribe_Collections.Look(ref preference, "preference", LookMode.Value, LookMode.Deep);
        }
    }
}
