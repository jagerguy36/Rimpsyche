using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Maux36.RimPsyche
{
    public class ITab_Pawn_Psyche : ITab
    {
        public ITab_Pawn_Psyche()
        {
            size = new Vector2(200f, 200f);
            labelKey = "TabPsyche";
            tutorTag = "Psyche";
        }

        public override bool IsVisible
        {
            get
            {
                //Log.Message("IsVisible");
                Pawn pawn = IsVisiblePawnHook(PawnToShowInfoAbout);
                return pawn.compPsyche() != null;
                //if (pawn == null)
                //{
                //  Log.Message("IsVisible, PawnToShowInfoAbout == null");
                //  return false;
                //}
                //CompPsychology comp = pawn.GetComp<CompPsychology>();
                //if (comp == null)
                //{
                //  Log.Message("IsVisible, comp == null");
                //  return false;
                //}
                //SpeciesSettings settings = SpeciesHelper.GetOrMakeSpeciesSettingsFromThingDef(pawn.def);
                //if (settings?.enablePsyche != true)
                //{
                //  Log.Message("IsVisible, enablePsyche != true");
                //  return false;
                //}
                //if (comp.IsPsychologyPawn != true)
                //{
                //  Log.Message("IsVisible, comp.IsPsychologyPawn != true");
                //  return false;
                //}
                //return true;
            }
        }

        public Pawn PawnToShowInfoAbout
        {
            get
            {
                //Log.Message("Inside PawnToShowInfoAbout");
                if (base.SelPawn != null)
                {
                    //Log.Message("Selected pawn label = " + base.SelPawn.Label);
                    return base.SelPawn;
                }
                if (base.SelThing is Corpse corpse)
                {
                    //Log.Message("Selected corpse label = " + corpse.InnerPawn.Label);
                    return corpse.InnerPawn;
                }
                throw new InvalidOperationException("Psyche tab found no selected pawn to display.");
            }
        }

        protected override void FillTab()
        {
            // Get pawn
            Pawn pawn = FillTabPawnHook(PawnToShowInfoAbout);
            // Modify pawn hook

            // Get total rectangle
            Rect psycheRect = PsycheCardUtility.PsycheRect;
            Rect totalRect = psycheRect;
            Rect editRect = new Rect(0f, 0f, 1f, 1f);
            //if (Prefs.DevMode)
            //{
            //    editRect = new Rect(psycheRect.xMax, psycheRect.y, EditPsycheUtility.CalculateEditWidth(pawn), psycheRect.height);
            //    totalRect.width += editRect.width;
            //}
            size = totalRect.size;
            GUI.BeginGroup(totalRect);
            PsycheCardUtility.DrawPsycheCard(psycheRect, pawn, OnWindow: false);
            //if (Prefs.DevMode)
            //{
            //    EditPsycheUtility.DrawEditPsyche(editRect, pawn);
            //    UIAssets.DrawLineVertical(editRect.x, editRect.y, editRect.height, UIAssets.LineColor);
            //}
            GUI.EndGroup();
        }

        public Pawn IsVisiblePawnHook(Pawn pawn) => pawn;


        public Pawn FillTabPawnHook(Pawn pawn) => pawn;

    }
}
