using RimWorld;
using System;
using UnityEngine;
using Verse;

namespace Maux36.RimPsyche
{
    public class ITab_Pawn_Psyche : ITab
    {
        public ITab_Pawn_Psyche()
        {
            size = new Vector2(500, 350f);
            labelKey = "TabPsyche";
            tutorTag = "Psyche";
        }

        public override bool IsVisible
        {
            get
            {
                if (!RimpsycheSettings.usePsycheTab) return false;
                Pawn pawn = IsVisiblePawnHook(PawnToShowInfoAbout);
                return pawn.compPsyche() != null;
            }
        }

        public Pawn PawnToShowInfoAbout
        {
            get
            {
                if (base.SelPawn != null)
                {
                    return base.SelPawn;
                }
                if (base.SelThing is Corpse corpse)
                {
                    return corpse.InnerPawn;
                }
                throw new InvalidOperationException("Psyche tab found no selected pawn to display.");
            }
        }

        protected override void FillTab()
        {
            // Get pawn
            Pawn pawn = FillTabPawnHook(PawnToShowInfoAbout);
            CompPsyche compPsyche = pawn.compPsyche();
            Rect psycheRect = new Rect(0f, 0f, PsycheInfoCard.PsycheRectWidth, PsycheInfoCard.PsycheRectHeight);
            psycheRect.width -= (compPsyche.Enabled && PsycheInfoCard.rightPanelVisible ? 0f : PsycheInfoCard.rightPanelWidthActual);
            size = psycheRect.size;
            GUI.BeginGroup(psycheRect);
            PsycheInfoCard.DrawPsycheCard(psycheRect, pawn, compPsyche);
            GUI.EndGroup();
        }

        public override void OnOpen()
        {
            base.OnOpen();
            //PsycheInfoCard.CacheClean();
            PsycheInfoCard.PersonalityScrollPosition = Vector2.zero;
            PsycheInfoCard.InterestScrollPosition = Vector2.zero;
        }

        public Pawn IsVisiblePawnHook(Pawn pawn) => pawn;


        public Pawn FillTabPawnHook(Pawn pawn) => pawn;

    }
}
