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
                    var shame = compPsyche.ShameThoughts;
                    if (shame.TryGetValue(def, out int count))
                    {
                        shame[def] = count + 1;
                    }
                    else
                    {
                        shame[def] = 1;
                    }
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
                    var shame = compPsyche.ShameThoughts;
                    if (shame.TryGetValue(def, out int count))
                    {
                        if (count > 1)
                        {
                            shame[def] = count - 1;
                        }
                        else
                        {
                            shame.Remove(def);
                        }
                    }
                    else
                    {
                        compPsyche.RefreshShameThoughts();
                    }
                }
            }
        }


    }
}
