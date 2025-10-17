using Verse;

namespace Maux36.RimPsyche
{
    public enum SexualOrientation : byte
    {
        None,
        Developing,
        Heterosexual,
        Bisexual,
        Homosexual,
        Asexual
    }
    public class Pawn_SexualityTracker : IExposable
    {
        private Pawn pawn;
        private CompPsyche compPsyche;
        public SexualOrientation orientationCategory = SexualOrientation.None;
        //Heterosexual: 0~0.2
        //Bisexual: 0.2~0.8
        //Homosexual: 0.8~1
        public float kinsey = -1f;
        public float sexDrive = 0f;
        public float mAattraction = 0f;
        public float fAattraction = 0f;
        public Pawn_SexualityTracker(Pawn p)
        {
            pawn = p;
            compPsyche = p.compPsyche();
        }
        public void Initialize(Pawn pawn, bool generate = true)
        {
        }
        public int GetKinseyReport()
        {
            if (kinsey == 0f) return 0;
            else if (kinsey < 0.2f) return 1;
            else if (kinsey < 0.4f) return 2;
            else if (kinsey < 0.6f) return 3;
            else if (kinsey < 0.8f) return 4;
            else if (kinsey < 1f) return 5;
            else if (kinsey == 1f) return 6;
            else return -1;
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
        public int GetMaleAttractionNorm()
        {
            return (int)(mAattraction * 100f);
        }
        public int GetFemaleAttractionNorm()
        {
            return (int)(fAattraction * 100f);
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref orientationCategory, "category", SexualOrientation.None);
            Scribe_Values.Look(ref kinsey, "kinsey", -1f);
            Scribe_Values.Look(ref sexDrive, "sexDrive", 0f);
            Scribe_Values.Look(ref mAattraction, "mAattraction", 0f);
            Scribe_Values.Look(ref fAattraction, "fAattraction", 0f);
        }
    }
}
