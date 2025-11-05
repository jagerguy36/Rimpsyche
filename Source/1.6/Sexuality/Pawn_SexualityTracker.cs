using RimWorld;
using UnityEngine;
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
    public struct PrefEntry
    {
        public string stringKey;
        public int intKey;
        public float target;
        public float importance;

        public PrefEntry(string stringKey, int intKey, float target, float importance)
        {
            this.stringKey = stringKey;
            this.intKey = intKey;
            this.target = target;
            this.importance = importance;
        }
    }
    public class Pawn_SexualityTracker : IExposable
    {
        private Pawn pawn;
        private CompPsyche compPsyche;
        public SexualOrientation orientationCategory = SexualOrientation.None;
        //Heterosexual: 0~0.2
        //Bisexual: 0.2~0.8
        //Homosexual: 0.8~1
        public bool shouldValidate = true;
        public float kinsey = -1f;
        public float sexDrive = 0f;
        public float mAattraction = 0f;
        public float fAattraction = 0f;

        private Dictionary<string, PrefEntry[]> _preference = new();
        private Dictionary<int, PrefEntry[]> Preference = new();
        public bool preferenceCacheDirty = true;
        private void RefreshPreferenceCache()
        {
            preferenceCacheDirty = false;
            Preference.Clear();

            List<string> invalidKeys = new();
            foreach (var kvp in _preference)
            {
                PreferenceDef def = DefDatabase<PreferenceDef>.GetNamedSilentFail(kvp.Key);
                if (def != null)
                {
                    Preference[def.shortHash] = kvp.Value;
                }
                else invalidKeys.Add(kvp.Key);
            }
            foreach (var k in invalidKeys)
            {
                _preference.Remove(k);
            }
        }

        //Only prefDef that Generates pref should call this.
        public PrefEntry[] GetPreference(PreferenceDef prefDef)
        {
            if (preferenceCacheDirty) RefreshPreferenceCache();
            if (Preference.TryGetValue(prefDef.shortHash, out var value)) return value;
            //Uninitialized Preference
            if (prefDef.TryGenerate(parentPawn, out var prefEntries))
            {
                SetPreference(prefDef, prefEntries);
                return prefEntries
            }
            Log.Error($"PreferenceDef {prefDef.defName} should not call GetPreference because it does not instantiate fixed preference.");
            return null;
        }

        public void SetPreference(PreferenceDef def, PrefEntry[] value)
        {
            _preference[def.defName] = value;
            if (preferenceCacheDirty) RefreshPreferenceCache();
            else Preference[def.shortHash]= value;
        }

        public Pawn_SexualityTracker(Pawn p)
        {
            pawn = p;
            compPsyche = p.compPsyche();
        }
        public void Initialize(bool generate = false, bool allowGay = true)
        {
            shouldValidate = false;
            //Sexuality Module not loaded
            if (!Rimpsyche.SexualityModuleLoaded) return;
            //Already initialized before
            if (orientationCategory != SexualOrientation.None && orientationCategory != SexualOrientation.Developing) return;
            //Only Develop sexuality via generation sexuality request, not via initialization
            if (orientationCategory == SexualOrientation.Developing && !generate) return;
            var traits = pawn.story?.traits;
            var gender = pawn.gender;
            //Not Applicable
            if (traits == null || gender == Gender.None) return;

            //From here on: SexualOrientation.None || Sexuality generation request

            //Assign Developing to non-adults
            if (pawn.ageTracker.AgeBiologicalYears < Rimpsyche_Utility.GetMinAdultAge(pawn))
            {
                orientationCategory = SexualOrientation.Developing;
                return;
            }

            //Assign Sexuality to adults

            float attraction; //Sexual and Romantic attraction is not distinguished for performance and implementation issue.
            //If the pawn already has sexuality trait, either from mid-save addition or generation restriction:
            if (traits.HasTrait(TraitDefOf.Gay))
            {
                orientationCategory = SexualOrientation.Homosexual;
                kinsey = SexualityHelper.GenerateKinseyFor(orientationCategory);
                attraction = SexualityHelper.GenerateAttractionFor(orientationCategory);
                //Log.Warning($"interpreting sexuality for {pawn.Name} | {kinsey} -> {orientationCategory} | A: {attraction}");
            }
            else if (traits.HasTrait(TraitDefOf.Bisexual))
            {
                orientationCategory = SexualOrientation.Bisexual;
                kinsey = SexualityHelper.GenerateKinseyFor(orientationCategory);
                attraction = SexualityHelper.GenerateAttractionFor(orientationCategory);
                //Log.Warning($"interpreting sexuality for {pawn.Name} | {kinsey} -> {orientationCategory} | A: {attraction}");
            }
            else if (traits.HasTrait(TraitDefOf.Asexual))
            {
                orientationCategory = SexualOrientation.Asexual;
                kinsey = SexualityHelper.GenerateKinseyFor(orientationCategory);
                attraction = SexualityHelper.GenerateAttractionFor(orientationCategory);
                //Log.Warning($"interpreting sexuality for {pawn.Name} | {kinsey} -> {orientationCategory} | A: {attraction}");
            }
            //Pawn without sexuality trait who is already generated == heterosexual pawns
            else if (!generate)
            {
                orientationCategory = SexualOrientation.Heterosexual;
                kinsey = SexualityHelper.GenerateKinseyFor(orientationCategory);
                attraction = SexualityHelper.GenerateAttractionFor(orientationCategory);
                //Log.Warning($"interpreting sexuality for {pawn.Name} | {kinsey} -> {orientationCategory} | A: {attraction}");
            }
            //Pawn without sexuality trait who is being generated == undecided 
            else
            {
                kinsey = SexualityHelper.GenerateKinsey(allowGay);
                attraction = SexualityHelper.GenerateAttraction();
                if (attraction < 0.05f) { orientationCategory = SexualOrientation.Asexual; traits.allTraits.Add(new Trait(TraitDefOf.Asexual, TraitDefOf.Asexual.degreeDatas[0].degree)); Log.Message("Ace"); }
                else if (kinsey < 0.2f) { orientationCategory = SexualOrientation.Heterosexual; Log.Message("Het"); }
                else if (kinsey < 0.8f) { orientationCategory = SexualOrientation.Bisexual; traits.allTraits.Add(new Trait(TraitDefOf.Bisexual, TraitDefOf.Bisexual.degreeDatas[0].degree)); Log.Message("Bi"); }
                else { orientationCategory = SexualOrientation.Homosexual; traits.allTraits.Add(new Trait(TraitDefOf.Gay, TraitDefOf.Gay.degreeDatas[0].degree)); Log.Message("Gay"); }
                //Log.Message($"generating sexuality for {pawn.Name} | {kinsey} -> {orientationCategory} | A: {attraction}");
            }

            sexDrive = SexualityHelper.GetNormalDistribution();
            float forSame = kinsey;
            float forDiff = 1f - kinsey;
            float multiplier = attraction / Mathf.Max(forSame, forDiff);
            if (gender == Gender.Male)
            {
                mAattraction = multiplier * forSame;
                fAattraction = multiplier * forDiff;
            }
            else
            {
                mAattraction = multiplier * forDiff;
                fAattraction = multiplier * forSame;
            }
            return;
        }
        public void Validate() //Validate if the current sexuality trait matches Psyche sexuality.
        {
            shouldValidate = false;
            if (orientationCategory == SexualOrientation.Developing || orientationCategory == SexualOrientation.None)
            {
                //Log.Message($"Validate: {pawn.Name}'s sexuality {orientationCategory} skip trait check.");
                return;
            }
            var traitCategory = SexualityHelper.EvaluateSexuality(pawn);
            if (traitCategory == orientationCategory)
            {
                //Log.Message($"Validate: {pawn.Name}'s sexuality {orientationCategory} unchanged.");
                return;
            }
            else
            {
                //Log.Warning($"Validate: {pawn.Name}'s sexuality {orientationCategory} changed to {traitCategory}.");
                AssignSexuality(traitCategory);
                return;
            }
        }
        private void AssignSexuality(SexualOrientation newCategory)
        {
            orientationCategory = newCategory;
            if (newCategory == SexualOrientation.None || newCategory == SexualOrientation.Developing)
            {
                return;
            }
            kinsey = SexualityHelper.GenerateKinseyFor(newCategory);
            var attraction = SexualityHelper.GenerateAttractionFor(newCategory);
            float forSame = kinsey;
            float forDiff = 1f - forSame;
            float multiplier = attraction / Mathf.Max(forSame, forDiff);
            if (pawn.gender == Gender.Male)
            {
                mAattraction = multiplier * forSame;
                fAattraction = multiplier * forDiff;
            }
            else
            {
                mAattraction = multiplier * forDiff;
                fAattraction = multiplier * forSame;
            }
            return;
        }
        public void DirtyTraitCache()
        {
            //Log.Message($"{pawn.Name}'s Validation Dirtied.");
            shouldValidate = true;
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
        public float GetMaleAttraction()
        {
            return (float)((int)(mAattraction * 100f)) * 0.01f;
        }
        public float GetFemaleAttraction()
        {
            return (float)((int)(fAattraction * 100f)) * 0.01f;
        }


        public float GetAdjustedAttraction(Gender gender)
        {
            switch (gender)
            {
                case Gender.Male:
                    return SexualityHelper.AdjustAttraction(GetMaleAttraction());
                case Gender.Female:
                    return SexualityHelper.AdjustAttraction(GetFemaleAttraction());
                default:
                    return 1f;
            }
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref orientationCategory, "category", SexualOrientation.None);
            Scribe_Values.Look(ref kinsey, "kinsey", -1f);
            Scribe_Values.Look(ref sexDrive, "sexDrive", 0f);
            Scribe_Values.Look(ref mAattraction, "mAattraction", 0f);
            Scribe_Values.Look(ref fAattraction, "fAattraction", 0f);
            Scribe_Collections.Look(ref _preference, "preference", LookMode.Value, LookMode.Value);
            //When loading: check sexuality is loaded. Check if the _preference is not null. Check it has PsychePreference inside.
            //If it does, iterate its content and fix intKey to become its short hash.
            if (Scribe.mode == LoadSaveMode.PostLoadInit && Rimpsyche.SexualityModuleLoaded)
            {
                if (_preference?.TryGetValue("PsychePreference", out var psychePreference))
                {
                    for (int i = 0; i < psychePreference.Length; i++)
                    {
                        PersonalityDef p = DefDatabase<PersonalityDef>.GetNamed(psychePreference[i].stringKey, false);
                        if (p == null)
                        {
                            Log.Warning($"Psyche Preference unable to load Personality def {psychePreference[i].stringKey}");
                            //Logic to fix it.
                        }
                        else psychePreference[i].intKey = p.shortHash;
                    }
                }
            }
        }
    }
}
