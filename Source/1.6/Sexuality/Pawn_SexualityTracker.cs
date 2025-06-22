using Verse;

namespace Maux36.RimPsyche
{
    public enum SexualOrientation : byte
    {
        None,
        Heterosexual,
        Bisexual,
        Homosexual,
        Asexual,
        Developing
    }
    public class Pawn_SexualityTracker : IExposable
    {
        private Pawn pawn;
        public SexualOrientation orientationCategory = SexualOrientation.None;
        public float kinsey = -1f;
        public float sexDrive = 0f;
        public float attractionM = 0f;
        public float attractionF = 0f;
        public Pawn_SexualityTracker(Pawn p)
        {
            pawn = p;
        }
        public void Initialize(Pawn pawn, int inputSeed = 0, bool rewrite = false)
        {
        }
        public bool ShowOnUI()
        {
            if (Rimpsyche.SexualityModuleLoaded)
            {
                if (orientationCategory != SexualOrientation.None && orientationCategory != SexualOrientation.Developing)
                {
                    return true;
                }
            }
            return false;
        }
        public SexualOrientation GetOrientationCategory()
        {
            return orientationCategory;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref orientationCategory, "category", SexualOrientation.None);
            Scribe_Values.Look(ref kinsey, "kinsey", -1f);
            Scribe_Values.Look(ref sexDrive, "sexDrive", 0f);
            Scribe_Values.Look(ref attractionM, "attractionM", 0f);
            Scribe_Values.Look(ref attractionF, "attractionF", 0f);
        }
    }
}
