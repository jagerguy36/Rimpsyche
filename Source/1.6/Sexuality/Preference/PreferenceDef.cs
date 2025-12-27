using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Maux36.RimPsyche
{
    public class PreferenceDef : Def
    {
        public bool isActive = false;
        public float baseStrength = 1f;
        public int priority = 0;
        public RimpsychePrefCategory category;
        public Type workerClass = typeof(PreferenceWorker);

        [Unsaved(false)]
        private PreferenceWorker workerInt;

        public PreferenceWorker worker
        {
            get
            {
                if (workerInt == null)
                {
                    workerInt = (PreferenceWorker)Activator.CreateInstance(workerClass);
                    workerInt.def = this;
                }
                return workerInt;
            }
        }
    }
    public enum RimpsychePrefCategory
    {
        Physical, //Called when SecondaryLovinFactor is calculated
        Romantic, //Called when SecondaryRomanceFactor is calculated
        Mix //Called on both
    }
    public abstract class PreferenceWorker
    {
        public float EditorHeight;
        public PreferenceDef def;

        public abstract bool TryGenerate(Pawn pawn, out List<PrefEntry> pref);

        public abstract string Report(Pawn pawn);

        public abstract float Evaluate(Pawn observer, Pawn target, float value, bool isRomantic);

        public abstract void DrawEditor(Rect rect, Pawn pawn, bool EditEnabled);

        public abstract void ClearEditorCache();
    }
}
