using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Maux36.RimPsyche
{
    [StaticConstructorOnStartup]
    public class Rimpsyche_UI_Utility
    {
        public static readonly Color ButtonDarkColor = new Color(0.623529f, 0.623529f, 0.623529f);
        public static readonly Color ButtonLightColor = new Color(0.97647f, 0.97647f, 0.97647f);
        public static readonly Texture2D PsycheButton = ContentFinder<Texture2D>.Get("Buttons/RimpsycheIcon", true);

        //Psyche Info Card
        public static Texture2D ViewFacetButton = ContentFinder<Texture2D>.Get("Buttons/FacetViewButton", true);
        public static Texture2D ViewListButton = ContentFinder<Texture2D>.Get("Buttons/RimpsycheViewList", true);
        public static Texture2D ViewBarButton = ContentFinder<Texture2D>.Get("Buttons/RimpsycheViewBar", true);
        public static Texture2D EditButton = ContentFinder<Texture2D>.Get("Buttons/RimpsycheEdit", true);
        public static Texture2D InfoButton = ContentFinder<Texture2D>.Get("Buttons/RimpsycheInfo", true);
        public static Texture2D InfoHLButton = ContentFinder<Texture2D>.Get("Buttons/RimpsycheInfoHL", true);
        public static Texture2D resetIcon = ContentFinder<Texture2D>.Get("Buttons/RimpsycheRefresh", true);
        public static Texture2D SaveLoadButton = ContentFinder<Texture2D>.Get("Buttons/SaveLoadButton", true);
        public static Texture2D RevealButton = ContentFinder<Texture2D>.Get("Buttons/RimpsycheReveal", true);
        public static Texture2D HideButton = ContentFinder<Texture2D>.Get("Buttons/RimpsycheHide", true);
        public static Texture2D InterestButton = ContentFinder<Texture2D>.Get("Buttons/RimpsycheInterest", true);
        public static Texture2D PreferenceButton = ContentFinder<Texture2D>.Get("Buttons/RimpsychePreference", true);

        public static void DrawEditButton(Rect rect, Pawn pawn)
        {
            Color oldColor = GUI.color;
            GUI.color = rect.Contains(Event.current.mousePosition) ? ButtonLightColor : ButtonDarkColor;
            GUI.DrawTexture(rect, PsycheButton);
            if (Widgets.ButtonInvisible(rect, false))
            {
                SoundDefOf.Tick_Low.PlayOneShotOnCamera(null);
                Find.WindowStack.Add(new PsycheEditPopup(pawn));
            }
            GUI.color = oldColor;
        }
    }
}
