using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Maux36.RimPsyche
{
    public class Pawn_SexualityTracker : IExposable
    {
        //Static
        private static readonly HashSet<TraitDef> _sexualityTraits = new()
        {
            TraitDefOf.Gay,
            TraitDefOf.Asexual,
            TraitDefOf.Bisexual
        };
        private static readonly float asexualCutoff = 0.05f;

        private readonly Pawn pawn;
        private readonly CompPsyche compPsyche;

        //Heterosexual: 0~0.2
        //Bisexual: 0.2~0.8
        //Homosexual: 0.8~1
        public bool shouldValidate = true;
        public SexualOrientation orientationCategory = SexualOrientation.None;
        public float kinsey = -1f;
        public float sexDrive = 0f;
        public float mAttraction = 0f;
        public float fAttraction = 0f;
        public HashSet<int> knownOrientation = new();
        //public Dictionary<int, float> acquaintanceship = new();
        //public Dictionary<int, float> relationship = new();


        //Cache
        private Dictionary<int, PawnRelationDef> _loversCache = new();
        private bool loversCacheDirty = true;
        public bool TryGetRomanticRelationDef(Pawn target, out PawnRelationDef def)
        {
            if (loversCacheDirty)
            {
                _loversCache.Clear();
                var relations = pawn.relations.DirectRelations;
                for (int i = 0; i < relations.Count; i++)
                {
                    if(SexualityHelper.LoverDefHash.Contains(relations[i].def) && relations[i].otherPawn != null)
                    {
                        _loversCache[relations[i].otherPawn.thingIDNumber] = relations[i].def;
                    }
                }
                loversCacheDirty = false;
            }
            return _loversCache.TryGetValue(target.thingIDNumber, out def);
        }

        public float GetLatestRebuffImpact(Pawn target)
        {
            Thought_Memory latestThought = null;
            int num = 999999;
            for (int i = 0; i < memories.Count; i++)
            {
                Thought_Memory thought_Memory = memories[i];
                if (thought_Memory.def == def && thought_Memory.otherPawn == target && thought_Memory.age < num)
                {
                    latestThought = thought_Memory;
                    num = thought_Memory.age;
                }
            }
            if (latestThought == null) return 0;
            return latestThought.OpinionOffset();
        }

        //Preference
        private Dictionary<string, List<PrefEntry>> _preference = new();
        public bool preferenceCacheDirty = true;

        public Dictionary<string, List<PrefEntry>> GetPreferenceRaw()
        {
            return _preference;
        }
        private Dictionary<int, List<PrefEntry>> Preference = new();
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
        public List<PrefEntry> GetPreference(PreferenceDef prefDef)
        {
            if (preferenceCacheDirty) RefreshPreferenceCache();
            if (Preference.TryGetValue(prefDef.shortHash, out var value)) return value;
            //Uninitialized Preference
            if (prefDef.worker.TryGenerate(pawn, out var prefEntries))
            {
                SetPreference(prefDef, prefEntries);
                return prefEntries;
            }
            Log.Error($"PreferenceDef {prefDef.defName} should not call GetPreference because it does not instantiate fixed preference.");
            return null;
        }

        public void SetPreference(PreferenceDef def, List<PrefEntry> value)
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

            float attraction; //Sexual and Romantic attraction are not distinguished for performance and implementation issue.
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
                if (attraction < asexualCutoff) { orientationCategory = SexualOrientation.Asexual; traits.allTraits.Add(new Trait(TraitDefOf.Asexual, TraitDefOf.Asexual.degreeDatas[0].degree)); Log.Message("Ace"); }
                else if (kinsey < 0.2f) { orientationCategory = SexualOrientation.Heterosexual; Log.Message("Het"); }
                else if (kinsey < 0.8f) { orientationCategory = SexualOrientation.Bisexual; traits.allTraits.Add(new Trait(TraitDefOf.Bisexual, TraitDefOf.Bisexual.degreeDatas[0].degree)); Log.Message("Bi"); }
                else { orientationCategory = SexualOrientation.Homosexual; traits.allTraits.Add(new Trait(TraitDefOf.Gay, TraitDefOf.Gay.degreeDatas[0].degree)); Log.Message("Gay"); }
                //Log.Message($"generating sexuality for {pawn.Name} | {kinsey} -> {orientationCategory} | A: {attraction}");
            }

            sexDrive = SexualityHelper.GenerateSexdrive();
            float forSame = kinsey;
            float forDiff = 1f - kinsey;
            float multiplier = attraction / Mathf.Max(forSame, forDiff);
            if (gender == Gender.Male)
            {
                mAttraction = multiplier * forSame;
                fAttraction = multiplier * forDiff;
            }
            else
            {
                mAttraction = multiplier * forDiff;
                fAttraction = multiplier * forSame;
            }
            return;
        }
        public void InjectData(PsycheData psyche)
        {
            shouldValidate = false;
            orientationCategory = psyche.orientationCategory;
            kinsey = psyche.kinsey;
            sexDrive = psyche.sexDrive;
            mAttraction = psyche.mAttraction;
            fAttraction = psyche.fAttraction;
            knownOrientation = new HashSet<int>(psyche.knownOrientation);
            //acquaintanceship = new Dictionary<int, float>(psyche.acquaintanceship);
            //relationship = new Dictionary<int, float>(psyche.relationship);
            _preference = new Dictionary<string, List<PrefEntry>>(psyche.preference);
            preferenceCacheDirty = true;

            //Clean sexuality trait. Pawn's traits null check already done with ShowOnUI()
            var traits = pawn.story.traits;
            for (int i = traits.allTraits.Count - 1; i >= 0; i--)
            {
                Trait trait = traits.allTraits[i];
                if (_sexualityTraits.Contains(trait.def)) traits.RemoveTrait(trait);
            }

            //Assign correct sexuality trait
            Trait traitToGive;
            switch (orientationCategory)
            {
                case (SexualOrientation.Asexual):
                    traitToGive = new Trait(TraitDefOf.Asexual, PawnGenerator.RandomTraitDegree(TraitDefOf.Asexual));
                    pawn.story.traits.GainTrait(traitToGive);
                    break;
                case (SexualOrientation.Heterosexual):
                    break;
                case (SexualOrientation.Bisexual):
                    traitToGive = new Trait(TraitDefOf.Bisexual, PawnGenerator.RandomTraitDegree(TraitDefOf.Bisexual));
                    pawn.story.traits.GainTrait(traitToGive);
                    break;
                case (SexualOrientation.Homosexual):
                    traitToGive = new Trait(TraitDefOf.Gay, PawnGenerator.RandomTraitDegree(TraitDefOf.Gay));
                    pawn.story.traits.GainTrait(traitToGive);
                    break;
            }
        }

        /// <summary>
        /// Validate if the current sexuality trait matches Psyche sexuality. If not, then override the Psyche Sexuality to match the trait 
        /// </summary>
        public void Validate()
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
                mAttraction = multiplier * forSame;
                fAttraction = multiplier * forDiff;
            }
            else
            {
                mAttraction = multiplier * forDiff;
                fAttraction = multiplier * forSame;
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
        public void SetMaleAttraction(float newValue)
        {
            mAttraction = newValue;
            if (fAttraction == 0f && mAttraction == 0f)
            {
                kinsey = Rand.Range(0, 1f);
                AdjustSexualityTrait(0f);
                return;
            }
            if (pawn.gender == Gender.Male) kinsey = mAttraction / (mAttraction + fAttraction);
            else kinsey = fAttraction / (mAttraction + fAttraction);
            float attraction = Mathf.Max(mAttraction, fAttraction);
            AdjustSexualityTrait(attraction);
        }
        public void SetFemaleAttraction(float newValue)
        {
            fAttraction = newValue;
            if (fAttraction == 0f && mAttraction == 0f)
            {
                kinsey = Rand.Range(0, 1f);
                AdjustSexualityTrait(0f);
                return;
            }
            if (pawn.gender == Gender.Male) kinsey = mAttraction / (mAttraction + fAttraction);
            else kinsey = fAttraction / (mAttraction + fAttraction);
            float attraction = Mathf.Max(mAttraction, fAttraction);
            AdjustSexualityTrait(attraction);
        }

        /// <summary>
        /// Adjust sexuality trait to match Psyche Sexuality
        /// </summary>
        /// <param name="attraction">max(mAttraction, fAttraction)</param>
        private void AdjustSexualityTrait(float attraction)
        {
            SexualOrientation orientationBasedOnAttraction;
            if (attraction < asexualCutoff) { orientationBasedOnAttraction = SexualOrientation.Asexual;}
            else if (kinsey < 0.2f) { orientationBasedOnAttraction = SexualOrientation.Heterosexual;}
            else if (kinsey < 0.8f) { orientationBasedOnAttraction = SexualOrientation.Bisexual;}
            else { orientationBasedOnAttraction = SexualOrientation.Homosexual; }
            
            //Adjustment not needed
            if (orientationBasedOnAttraction == orientationCategory) return;

            //Clean sexuality trait. Pawn's traits null check already done with ShowOnUI()
            var traits = pawn.story.traits;
            for (int i = traits.allTraits.Count - 1; i >= 0; i--)
            {
                Trait trait = traits.allTraits[i];
                if (_sexualityTraits.Contains(trait.def)) traits.RemoveTrait(trait);
            }

            //Assign correct sexuality trait
            Trait traitToGive;
            switch (orientationBasedOnAttraction)
            {
                case (SexualOrientation.Asexual):
                    orientationCategory = SexualOrientation.Asexual;
                    traitToGive = new Trait(TraitDefOf.Asexual, PawnGenerator.RandomTraitDegree(TraitDefOf.Asexual));
                    pawn.story.traits.GainTrait(traitToGive);
                    break;
                case (SexualOrientation.Heterosexual):
                    orientationCategory = SexualOrientation.Heterosexual;
                    break;
                case (SexualOrientation.Bisexual):
                    orientationCategory = SexualOrientation.Bisexual;
                    traitToGive = new Trait(TraitDefOf.Bisexual, PawnGenerator.RandomTraitDegree(TraitDefOf.Bisexual));
                    pawn.story.traits.GainTrait(traitToGive);
                    break;
                case (SexualOrientation.Homosexual):
                    orientationCategory = SexualOrientation.Homosexual;
                    traitToGive = new Trait(TraitDefOf.Gay, PawnGenerator.RandomTraitDegree(TraitDefOf.Gay));
                    pawn.story.traits.GainTrait(traitToGive);
                    break;
            }
        }


        public float GetAdjustedAttraction(Gender gender)
        {
            switch (gender)
            {
                case Gender.Male:
                    return SexualityHelper.AdjustAttraction(mAttraction);
                case Gender.Female:
                    return SexualityHelper.AdjustAttraction(fAttraction);
                default:
                    return 1f;
            }
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref orientationCategory, "category", SexualOrientation.None);
            Scribe_Values.Look(ref kinsey, "kinsey", -1f);
            Scribe_Values.Look(ref sexDrive, "sexDrive", 0f);
            Scribe_Values.Look(ref mAttraction, "mAttraction", 0f);
            Scribe_Values.Look(ref fAttraction, "fAttraction", 0f);
            Scribe_Collections.Look(ref knownOrientation, "knownOrientation", LookMode.Value);
            //Scribe_Collections.Look(ref acquaintanceship, "acquaintanceship", LookMode.Value, LookMode.Value);
            //Scribe_Collections.Look(ref relationship, "relationship", LookMode.Value, LookMode.Value);
            Scribe_Collections.Look(ref _preference, "preference", LookMode.Value, LookMode.Deep);
            //When loading: check sexuality is loaded. Check if the _preference is not null. Check it has PsychePreference inside.
            //If it does, iterate its content and fix intKey to become its short hash.
            if (Scribe.mode == LoadSaveMode.PostLoadInit && Rimpsyche.SexualityModuleLoaded)
            {
                if (_preference?.TryGetValue("Rimpsyche_PsychePreference", out var psychePreference) == true)
                {
                    for (int i = 0; i < psychePreference.Count; i++)
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
