using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        private static readonly float asexualCutoff = 0.05f; //Adjusted = 0.142...

        public readonly Pawn pawn;
        public readonly CompPsyche compPsyche;
        private float minAdultAge;
        private float fullAdultAge;

        //Semi constant
        private static readonly bool usePreference = RimpsycheSexualitySettings.usePreferenceSystem;
        private static readonly float minRelAttraction = RimpsycheSexualitySettings.minRelAttraction;

        //kinsey
        //0 [0] 1 [0.2] 2 [0.4] 3 [0.6] 4 [0.8] 5 [1] 6
        public bool shouldValidate = true;
        public SexualOrientation orientationCategory = SexualOrientation.None;
        private float mKinsey = -1f;
        public float MKinsey => mKinsey;
        private float attraction = 0f;
        public float Attraction
        {
            get
            {
                if (orientationCategory == SexualOrientation.None || orientationCategory == SexualOrientation.Developing)
                {
                    return 0f;
                }
                return attraction;
            }
        }
        private float sexDrive = 0f;
        public float SexDrive
        {
            get
            {
                if (orientationCategory == SexualOrientation.None || orientationCategory == SexualOrientation.Developing)
                {
                    return 0f;
                }
                return sexDrive;
            }
        }
        private bool adjustmentDirty = true;
        private float mAttraction = 0f;
        private float fAttraction = 0f;
        public bool driveDirty = true;
        private float adjustedDrive = 0f;

        //For Androids or Other non-sexual beings
        private bool shouldCheckSuppressed = true;
        private bool SuppressedInternal = false;
        public bool Suppressed
        {
            get
            {
                if (shouldCheckSuppressed)
                {
                    SuppressedInternal = CheckSexualitySuppressed();
                    adjustmentDirty = true;
                    driveDirty = true;
                    shouldCheckSuppressed = false;
                }
                return SuppressedInternal;
            }
        }
        private bool CheckSexualitySuppressed()
        {
            return false;
        }

        //General check
        public bool SexualityExpressed()
        {
            if (!Suppressed && Rimpsyche_Utility.GetPawnAge(pawn) >= minAdultAge)
                return true;
            return false;
        }

        //Memory
        public HashSet<int> knownOrientation = new();
        /// <summary>
        /// The romantic feeling this pawn has towards the other.
        /// Increases everytime the pawns do something romantic.
        /// The keys can be used to determine whether this pawn has ever found the other pawn attractive enough to consider for a relationship.
        /// </summary>
        public Dictionary<int, float> relationship = new();

        //Cache
        private readonly Dictionary<int, PawnRelationDef> _loversCache = new();
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
        public void DirtyLoversCache()
        {
            loversCacheDirty = true;
        }
        public float GetLatestRebuffImpact(Pawn target)
        {
            Thought_MemorySocial latestThought = null;
            int num = 999999;
            var memories = pawn.needs.mood.thoughts.memories.Memories;
            for (int i = 0; i < memories.Count; i++)
            {
                Thought_Memory thought_Memory = memories[i];
                if (thought_Memory.def == ThoughtDefOf.RebuffedMyRomanceAttempt && thought_Memory.otherPawn == target && thought_Memory.age < num)
                {
                    latestThought = thought_Memory as Thought_MemorySocial;
                    num = thought_Memory.age;
                }
            }
            if (latestThought == null) return 0;
            return latestThought.OpinionOffset();
        }
        public int crushPawnIdNumber = -1;
        public int crushEndTick = -1;

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
            if (!usePreference) return null;
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

        public bool KnowsOrientationOf(Pawn target)
        {
            return knownOrientation.Contains(target.thingIDNumber);
        }
        public void LearnOrientationOf(Pawn target)
        {
            knownOrientation.Add(target.thingIDNumber);
        }

        public void IncrementRelationshipWith(Pawn target, float amount)
        {
            float max = GetRelationshipCeliing(target);
            relationship.TryGetValue(target.thingIDNumber, out var current);
            relationship[target.thingIDNumber] = Mathf.Clamp01(Mathf.Min(current + amount, max));
            LearnOrientationOf(target);
        }
        public float GetRelationshipCeliing(Pawn target)
        {
            float max = 0.1f;
            if (TryGetRomanticRelationDef(target, out _))
                max = 1f;
            //0.1 [No/LB] | 0.35 [SH] | 0.6 [D] | 0.85 [LF] | 1 [L+]
            return max;
        }
        public float GetRelationshipWith(Pawn target)
        {
            relationship.TryGetValue(target.thingIDNumber, out var current);
            return current;
        }
        public void ClampRelationshipWith(Pawn target, float ceiling)
        {
            if(relationship.TryGetValue(target.thingIDNumber, out var current))
            {
                relationship[target.thingIDNumber] = Mathf.Clamp01(Mathf.Min(current + ceiling));
            }
        }

        public Pawn_SexualityTracker(Pawn p)
        {
            pawn = p;
            compPsyche = p.compPsyche();
        }
        //generate = false is coming from loading saves.
        //Newly generate pawns initialize with generate = true
        //Growth moment sexuality generation calls with generate = true
        //Pawn lifestage change from child to adult also calls with generate = true
        public void Initialize(bool generate = false, bool allowGay = true)
        {
            //Log.Message($" {pawn.LabelShort} |initialize called");
            float kinsey;
            shouldValidate = false;
            adjustmentDirty = true;
            driveDirty = true;
            loversCacheDirty = true;
            //Set up near-constant variable to be used frequently
            minAdultAge = compPsyche.MinAdultAge;
            fullAdultAge = compPsyche.FullAdultAge;
            //Sexuality Module not loaded
            if (!Rimpsyche.SexualityModuleLoaded) return;
            //Already initialized before
            if (mKinsey >= 0f)
            {
                //Growth moment for pawn who's already assigned their sexuality
                //Since lifestageChange assigns sexuality, growth moment calls shuld end up here.
                if (generate)
                {
                    AdjustSexualityCategory(attraction);
                }
                return;
            }
            //Only Develop sexuality via generation sexuality request, not via initialization
            if (orientationCategory == SexualOrientation.Developing && !generate) return;

            //Not Applicable
            if (SexualityHelper.NonSexualDefShorthashSet.Contains(pawn.def.shortHash)) return;
            var traits = pawn.story?.traits;
            var gender = pawn.gender;
            if (traits == null || gender == Gender.None) return;

            //From here on: SexualOrientation.None || Sexuality generation request

            //Assign Developing to non-adults
            if (Rimpsyche_Utility.GetPawnAge(pawn) < minAdultAge)
            {
                orientationCategory = SexualOrientation.Developing;
                return;
            }

            //Assign Sexuality to adults

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
                if (attraction < asexualCutoff) { orientationCategory = SexualOrientation.Asexual; traits.allTraits.Add(new Trait(TraitDefOf.Asexual, TraitDefOf.Asexual.degreeDatas[0].degree)); }
                else if (kinsey < 0.2f) { orientationCategory = SexualOrientation.Heterosexual;}
                else if (kinsey < 0.8f) { orientationCategory = SexualOrientation.Bisexual; traits.allTraits.Add(new Trait(TraitDefOf.Bisexual, TraitDefOf.Bisexual.degreeDatas[0].degree));}
                else { orientationCategory = SexualOrientation.Homosexual; traits.allTraits.Add(new Trait(TraitDefOf.Gay, TraitDefOf.Gay.degreeDatas[0].degree));}
                //Log.Message($" {pawn.LabelShort} | generating sexuality | {kinsey} -> {orientationCategory} | A: {attraction}");
            }

            sexDrive = SexualityHelper.GenerateSexdrive();
            if (gender == Gender.Male) mKinsey = kinsey;
            else mKinsey = 1 - kinsey;
            return;
        }
        public void InjectData(PsycheData psyche, bool preserveMemory)
        {
            //Not Applicable
            if (SexualityHelper.NonSexualDefShorthashSet.Contains(pawn.def.shortHash)) return;
            var traits = pawn.story?.traits;
            var gender = pawn.gender;
            if (traits == null || gender == Gender.None) return;

            shouldValidate = false;
            adjustmentDirty = true;
            driveDirty = true;
            loversCacheDirty = true;
            shouldCheckSuppressed = true;
            preferenceCacheDirty = true;

            //Clean sexuality trait.
            traits.allTraits = traits.allTraits
                .Where(trait => !_sexualityTraits.Contains(trait.def))
                .ToList();

            //Randomize Sexuality if loaded sexuality is undefined
            if (psyche.orientationCategory == SexualOrientation.None || psyche.orientationCategory == SexualOrientation.Developing)
            {
                if (Rimpsyche_Utility.GetPawnAge(pawn) < minAdultAge)
                {
                    orientationCategory = SexualOrientation.Developing;
                    return;
                }
                float genkinsey;
                genkinsey = SexualityHelper.GenerateKinsey(true);
                attraction = SexualityHelper.GenerateAttraction();
                if (attraction < asexualCutoff) { orientationCategory = SexualOrientation.Asexual; traits.allTraits.Add(new Trait(TraitDefOf.Asexual, TraitDefOf.Asexual.degreeDatas[0].degree)); }
                else if (genkinsey < 0.2f) { orientationCategory = SexualOrientation.Heterosexual; }
                else if (genkinsey < 0.8f) { orientationCategory = SexualOrientation.Bisexual; traits.allTraits.Add(new Trait(TraitDefOf.Bisexual, TraitDefOf.Bisexual.degreeDatas[0].degree)); }
                else { orientationCategory = SexualOrientation.Homosexual; traits.allTraits.Add(new Trait(TraitDefOf.Gay, TraitDefOf.Gay.degreeDatas[0].degree)); }
                sexDrive = SexualityHelper.GenerateSexdrive();
                if (gender == Gender.Male) mKinsey = genkinsey;
                else mKinsey = 1 - genkinsey;
                _preference.Clear();
                return;
            }

            //Inject Sexuality from the psyche
            //If the injected pawns are too young, they are assigned Developing Orientation and will be reassigned their proper orientation based on the mKinsey when they reach the growth moment with Initialize()
            mKinsey = psyche.mKinsey;
            attraction = psyche.attraction;
            sexDrive = psyche.sexDrive;
            _preference = new Dictionary<string, List<PrefEntry>>(psyche.preference);
            _preference ??= new();
            if (preserveMemory)
            {
                knownOrientation = [.. psyche.knownOrientation];
                relationship = new Dictionary<int, float>(psyche.relationship);
                knownOrientation ??= new();
                relationship ??= new();
            }

            if (Rimpsyche_Utility.GetPawnAge(pawn) < minAdultAge)
            {
                orientationCategory = SexualOrientation.Developing;
                return;
            }
            float kinsey;
            if (pawn.gender == Gender.Male) kinsey = mKinsey;
            else kinsey = 1 - mKinsey;
            if (attraction < asexualCutoff) { orientationCategory = SexualOrientation.Asexual; traits.allTraits.Add(new Trait(TraitDefOf.Asexual, TraitDefOf.Asexual.degreeDatas[0].degree)); }
            else if (kinsey < 0.2f) { orientationCategory = SexualOrientation.Heterosexual; }
            else if (kinsey < 0.8f) { orientationCategory = SexualOrientation.Bisexual; traits.allTraits.Add(new Trait(TraitDefOf.Bisexual, TraitDefOf.Bisexual.degreeDatas[0].degree)); }
            else { orientationCategory = SexualOrientation.Homosexual; traits.allTraits.Add(new Trait(TraitDefOf.Gay, TraitDefOf.Gay.degreeDatas[0].degree)); }
        }

        /// <summary>
        /// Validate if the current sexuality trait matches Psyche sexuality. If not, then override the Psyche Sexuality to match the trait 
        /// </summary>
        public void Validate()
        {
            shouldValidate = false;
            if (orientationCategory == SexualOrientation.Developing || orientationCategory == SexualOrientation.None) return;
            var traitCategory = SexualityHelper.EvaluateSexuality(pawn);
            if (traitCategory == orientationCategory) return;
            else
            {
                AssignSexuality(traitCategory);
                return;
            }
        }
        /// <summary>
        /// Force new Psyche sexuality onto the pawn.
        /// This is only called when Validate checked after dirtied validateion (trait changed) and new traits is mismatched with Psyche sexuality.
        /// </summary>
        /// <param name="newCategory"></param>
        private void AssignSexuality(SexualOrientation newCategory)
        {
            orientationCategory = newCategory;
            if (newCategory == SexualOrientation.None || newCategory == SexualOrientation.Developing)
            {
                return;
            }
            float kinsey = SexualityHelper.GenerateKinseyFor(newCategory);
            attraction = SexualityHelper.GenerateAttractionFor(newCategory);
            if (pawn.gender == Gender.Male)
            {
                mKinsey = kinsey;
            }
            else
            {
                mKinsey = 1 - kinsey;
            }
            adjustmentDirty = true;
            return;
        }
        //When Trait is manipulated, Rimpsyche Sexuality will check the traits and follow it rather than enforcing RPS on Vanilla
        public bool DirtyTraitCache(TraitDef def)
        {
            if (SexualityHelper.SexualityTraitHashSet.Contains(def.shortHash))
            {
                shouldValidate = true;
                return true;
            }
            return false;
        }
        public void DirtySuppressedCheck()
        {
            shouldCheckSuppressed = true;
        }
        public int GetKinseyReport()
        {
            float kinsey;
            if (pawn.gender == Gender.Male) kinsey = mKinsey;
            else kinsey = 1 - mKinsey;
            if (kinsey == 0f) return 0;
            else if (kinsey < 0.2f) return 1;
            else if (kinsey < 0.4f) return 2;
            else if (kinsey < 0.6f) return 3;
            else if (kinsey < 0.8f) return 4;
            else if (kinsey < 1f) return 5;
            else if (kinsey == 1f) return 6;
            else return -1;
        }
        public string GetOrientationReport()
        {
            switch (orientationCategory)
            {
                case SexualOrientation.Heterosexual:
                    return "RPC_Heterosexual".Translate();
                case SexualOrientation.Bisexual:
                    return "RPC_Bisexual".Translate();
                case SexualOrientation.Homosexual:
                    return "RPC_Homosexual".Translate();
                case SexualOrientation.Asexual:
                    return "RPC_Asexual".Translate();
                default:
                    return "RPC_SexualityUndefined".Translate();
            }
        }
        public string GetOrientationDescription()
        {
            var pawnName = pawn.Name?.ToStringShort ?? "";
            var descriptionString = new StringBuilder();
            switch (orientationCategory)
            {
                case SexualOrientation.Heterosexual:
                    descriptionString.Append("RPC_HeterosexualDescription".Translate(pawnName));
                    break;
                case SexualOrientation.Bisexual:
                    descriptionString.Append("RPC_BisexualDescription".Translate(pawnName));
                    break;
                case SexualOrientation.Homosexual:
                    descriptionString.Append("RPC_HomosexualDescription".Translate(pawnName));
                    break;
                case SexualOrientation.Asexual:
                    descriptionString.Append("RPC_AsexualDescription".Translate(pawnName));
                    break;
                default:
                    descriptionString.Append("RPC_SexualityUndefinedDescription".Translate(pawnName));
                    break;
            }

            descriptionString
                .Append("\n\n")
                .AppendTagged("RPS_KinseyTitleTooltip".Translate().Colorize(Color.yellow))
                .Append("\n\n")
                .Append("RPS_KinseyTooltip".Translate());
            return descriptionString.ToString();
        }
        public bool ShowOnUI()
        {
            if (Rimpsyche.SexualityModuleLoaded)
            {
                if (Suppressed || orientationCategory == SexualOrientation.None || orientationCategory == SexualOrientation.Developing)
                {
                    return false;
                }
                return true;
            }
            return false;

        }
        public SexualOrientation GetOrientationCategory()
        {
            return orientationCategory;
        }
        public void SetmKinsey(float value)
        {
            mKinsey = value;
            adjustmentDirty = true;
            AdjustSexualityCategory(attraction);
        }
        public void SetAttraction(float value)
        {
            attraction = value;
            adjustmentDirty = true;
            AdjustSexualityCategory(attraction);
        }
        public void SetSexdrive(float value)
        {
            driveDirty = true;
            sexDrive = value;
        }

        /// <summary>
        /// Adjust sexuality trait and orienation category to match mKinsey and attraction
        /// </summary>
        /// <param name="attraction">max(mAttraction, fAttraction)</param>
        private void AdjustSexualityCategory(float attraction)
        {
            float kinsey;
            if (pawn.gender == Gender.Male) kinsey = mKinsey;
            else kinsey = 1 - mKinsey;
            SexualOrientation orientationBasedOnAttraction;
            if (attraction < asexualCutoff) { orientationBasedOnAttraction = SexualOrientation.Asexual;}
            else if (kinsey < 0.2f) { orientationBasedOnAttraction = SexualOrientation.Heterosexual;}
            else if (kinsey < 0.8f) { orientationBasedOnAttraction = SexualOrientation.Bisexual;}
            else { orientationBasedOnAttraction = SexualOrientation.Homosexual; }
            
            //Adjustment not needed
            if (orientationBasedOnAttraction == orientationCategory) return;

            //Assign correct sexuality trait
            switch (orientationBasedOnAttraction)
            {
                case (SexualOrientation.Asexual):
                    orientationCategory = SexualOrientation.Asexual;
                    AdjustTraitTo(TraitDefOf.Asexual);
                    break;
                case (SexualOrientation.Heterosexual):
                    orientationCategory = SexualOrientation.Heterosexual;
                    AdjustTraitTo(null);
                    break;
                case (SexualOrientation.Bisexual):
                    orientationCategory = SexualOrientation.Bisexual;
                    AdjustTraitTo(TraitDefOf.Bisexual);
                    break;
                case (SexualOrientation.Homosexual):
                    orientationCategory = SexualOrientation.Homosexual;
                    AdjustTraitTo(TraitDefOf.Gay);
                    break;
            }
        }
        private void AdjustTraitTo(TraitDef TargetDef)
        {
            var traits = pawn.story.traits;
            bool shouldGiveTrait = true;
            Trait traitToGive;
            for (int i = traits.allTraits.Count - 1; i >= 0; i--)
            {
                Trait trait = traits.allTraits[i];
                if (trait.def == TargetDef) shouldGiveTrait = false;
            }
            traits.allTraits = traits.allTraits
                .Where(trait => !_sexualityTraits.Contains(trait.def) || trait.def == TargetDef)
                .ToList();
            if (TargetDef != null && shouldGiveTrait)
            {
                traitToGive = new Trait(TargetDef, PawnGenerator.RandomTraitDegree(TargetDef));
                pawn.story.traits.allTraits.Add(traitToGive);
            }
        }
        public void Notify_Sexchange()
        {
            AdjustSexualityCategory(attraction);
        }
        public void Notify_LifestageChanged()
        {
            if (Rimpsyche_Utility.GetPawnAge(pawn) < minAdultAge)
            {
                if (orientationCategory != SexualOrientation.Developing)
                {
                    orientationCategory = SexualOrientation.Developing;
                    adjustmentDirty = true;
                    driveDirty = true;
                }
            }
            else
            {
                if (orientationCategory == SexualOrientation.Developing)
                {
                    initialize(generate = true);
                }
            }
        }

        /// <summary>
        /// Get adjusted attraction that takes into account the target pawn gender and whether the observer has ever found the pawn attractive.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public float GetAdjustedAttraction(Pawn target)
        {
            var genderAttraction = GetAdjustedAttractionToGender(target.gender);
            if (relationship.TryGetValue(target.thingIDNumber, out float rel))
            {
                float minAtt = Mathf.Max(genderAttraction, minRelAttraction);
                return minAtt * (1f + 0.25f * rel);
            }
            return genderAttraction;
        }

        public float GetAdjustedAttractionToGender(Gender gender)
        {
            if (adjustmentDirty)
            {
                adjustmentDirty = false;
                if (!Suppressed && Rimpsyche_Utility.GetPawnAge(pawn) >= minAdultAge)
                {
                    float multiplier = SexualityHelper.AdjustRawValues(Attraction) / Mathf.Max(mKinsey, 1f - mKinsey);
                    mAttraction = multiplier * mKinsey;
                    fAttraction = multiplier * (1f - mKinsey);
                }
                else
                {
                    mAttraction = 0f;
                    fAttraction = 0f;
                }
            }
            return gender switch
            {
                Gender.Male => mAttraction,
                Gender.Female => fAttraction,
                Gender.None => 0f,
                _ => 0f,
            };
        }

        public bool CanFeelAttractionToGender(Gender gender)
        {
            if (attraction < asexualCutoff) return false;
            if (GetAdjustedAttractionToGender(gender) > 0f) return true;
            return false;
        }

        public float GetAdjustedSexdrive()
        {
            if (driveDirty)
            {
                if (!Suppressed && Rimpsyche_Utility.GetPawnAge(pawn) >= minAdultAge)
                {
                    adjustedDrive = SexualityHelper.AdjustRawValues(SexDrive);
                }
                else
                {
                    adjustedDrive = 0f;
                }
            }
            return adjustedDrive;
        }

        public void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                knownOrientation.RemoveWhere(id => VersionManager.DiscardedPawnThingIDnumber.Contains(id));
                foreach (int id in relationship.Keys.ToList())
                {
                    if (VersionManager.DiscardedPawnThingIDnumber.Contains(id)) relationship.Remove(id);
                }
            }
            Scribe_Values.Look(ref orientationCategory, "category", SexualOrientation.None);
            Scribe_Values.Look(ref mKinsey, "mKinsey", -1f);
            Scribe_Values.Look(ref attraction, "attraction", 0f);
            Scribe_Values.Look(ref sexDrive, "sexDrive", 0f);
            Scribe_Collections.Look(ref knownOrientation, "knownOrientation", LookMode.Value);
            Scribe_Collections.Look(ref relationship, "relationship", LookMode.Value, LookMode.Value);
            Scribe_Collections.Look(ref _preference, "preference", LookMode.Value, LookMode.Deep);
            //Post load operations
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                //Fix null memories
                knownOrientation ??= new();
                relationship ??= new();
                _preference ??= new();
                //Reset intKey for psychePreference
                if (Rimpsyche.SexualityModuleLoaded)
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
}
