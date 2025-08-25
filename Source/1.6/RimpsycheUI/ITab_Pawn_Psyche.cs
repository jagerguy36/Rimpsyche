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
            Rect psycheRect = PsycheInfoCard.PsycheRect;
            psycheRect.width -= (compPsyche.Enabled && PsycheInfoCard.rightPanelVisible ? 0f : PsycheInfoCard.rightPanelWidthActual);
            if (PsycheInfoCard.showMode !=1 )
            {
                psycheRect.width -= PsycheInfoCard.personalityWidthDiff;
            }
            size = psycheRect.size;
            GUI.BeginGroup(psycheRect);
            PsycheInfoCard.DrawPsycheCard(psycheRect, pawn, compPsyche);
            GUI.EndGroup();
        }

        public override void OnOpen()
        {
            base.OnOpen();
            PsycheInfoCard.CacheClean();
            PsycheInfoCard.PersonalityScrollPosition = Vector2.zero;
            PsycheInfoCard.InterestScrollPosition = Vector2.zero;
        }

        public Pawn IsVisiblePawnHook(Pawn pawn) => pawn;


        public Pawn FillTabPawnHook(Pawn pawn) => pawn;

    }
}
