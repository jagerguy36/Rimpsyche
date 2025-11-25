using RimWorld;
using Verse;

namespace Maux36.RimPsyche
{
    public class Thought_Situational_Shame : Thought_Situational
    {
        protected override void Notify_BecameActive()
        {
            base.Notify_BecameActive();
            if (pawn.IsColonist)
            {
                var compPsyche = pawn.compPsyche();
                if (compPsyche != null)
                {
                    if (compPsyche.Enabled != true)
                    {
                        compPsyche.CleanShame();
                        return;
                    }
                    compPsyche.Notify_ShameThoughtBecameActive(def);
                }
            }
        }

        protected override void Notify_BecameInactive()
        {
            base.Notify_BecameInactive();
            if (pawn.IsColonist)
            {
                var compPsyche = pawn.compPsyche();
                if (compPsyche != null)
                {
                    if (compPsyche.Enabled != true)
                    {
                        compPsyche.CleanShame();
                        return;
                    }
                    compPsyche.Notify_ShameThoughtBecameInactive(def);
                }
            }
        }
    }
}
