using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Maux36.RimPsyche
{
    public class Pawn_PersonalityTracker : IExposable
    {
        private Pawn pawn;
        public float creativity = 1f;
        public float curiosity = 1f;
        public float aestheticSensitivity = 1f;

        public float dicipline = 1f;
        public float organization = 1f;
        public float acheivementDriven = 1f;

        public float enthusiasm = 1f;
        public float sociability = 1f;
        public float assertiveness = 1f;

        public float compasstion = 1f;
        public float altruism = 1f;
        public float cooperation = 1f;

        public float anxiety = 1f;
        public float irritability = 1f;
        public float volatility = 1f;



        public void ExposeData()
        {
            Scribe_Values.Look(ref creativity, "creativity", 0, false);
            Scribe_Values.Look(ref curiosity, "curiosity", 0, false);
            Scribe_Values.Look(ref aestheticSensitivity, "aestheticSensitivity", 0, false);

            Scribe_Values.Look(ref dicipline, "dicipline", 0, false);
            Scribe_Values.Look(ref organization, "organization", 0, false);
            Scribe_Values.Look(ref acheivementDriven, "acheivementDriven", 0, false);

            Scribe_Values.Look(ref enthusiasm, "enthusiasm", 0, false);
            Scribe_Values.Look(ref sociability, "sociability", 0, false);
            Scribe_Values.Look(ref assertiveness, "assertiveness", 0, false);

            Scribe_Values.Look(ref compasstion, "compasstion", 0, false);
            Scribe_Values.Look(ref altruism, "altruism", 0, false);
            Scribe_Values.Look(ref cooperation, "cooperation", 0, false);

            Scribe_Values.Look(ref anxiety, "anxiety", 0, false);
            Scribe_Values.Look(ref irritability, "irritability", 0, false);
            Scribe_Values.Look(ref volatility, "volatility", 0, false);
        }
        public Pawn_PersonalityTracker(Pawn p)
        {
            pawn = p;
        }
        public void Initialize(int inputSeed = 0)
        {
            float baseOCEANvalue = Rand.Gaussian(0.5f, 0.2f);
            creativity = Mathf.Clamp(baseOCEANvalue + Rand.Range(-0.2f, 0.2f), 0f ,1f);
            curiosity = Mathf.Clamp(baseOCEANvalue + Rand.Range(-0.2f, 0.2f), 0f, 1f);
            aestheticSensitivity = Mathf.Clamp(baseOCEANvalue + Rand.Range(-0.2f, 0.2f), 0f, 1f);

            baseOCEANvalue = Rand.Gaussian(0.5f, 0.2f);
            dicipline = Mathf.Clamp(baseOCEANvalue + Rand.Range(-0.2f, 0.2f), 0f, 1f);
            organization = Mathf.Clamp(baseOCEANvalue + Rand.Range(-0.2f, 0.2f), 0f, 1f);
            acheivementDriven = Mathf.Clamp(baseOCEANvalue + Rand.Range(-0.2f, 0.2f), 0f, 1f);

            baseOCEANvalue = Rand.Gaussian(0.5f, 0.2f);
            enthusiasm = Mathf.Clamp(baseOCEANvalue + Rand.Range(-0.2f, 0.2f), 0f, 1f);
            sociability = Mathf.Clamp(baseOCEANvalue + Rand.Range(-0.2f, 0.2f), 0f, 1f);
            assertiveness = Mathf.Clamp(baseOCEANvalue + Rand.Range(-0.2f, 0.2f), 0f, 1f);

            baseOCEANvalue = Rand.Gaussian(0.5f, 0.2f);
            compasstion = Mathf.Clamp(baseOCEANvalue + Rand.Range(-0.2f, 0.2f), 0f, 1f);
            altruism = Mathf.Clamp(baseOCEANvalue + Rand.Range(-0.2f, 0.2f), 0f, 1f);
            cooperation = Mathf.Clamp(baseOCEANvalue + Rand.Range(-0.2f, 0.2f), 0f, 1f);

            baseOCEANvalue = Rand.Gaussian(0.5f, 0.2f);
            anxiety = Mathf.Clamp(baseOCEANvalue + Rand.Range(-0.2f, 0.2f), 0f, 1f);
            irritability = Mathf.Clamp(baseOCEANvalue + Rand.Range(-0.2f, 0.2f), 0f, 1f);
            volatility = Mathf.Clamp(baseOCEANvalue + Rand.Range(-0.2f, 0.2f), 0f, 1f);
        }
    }
}
