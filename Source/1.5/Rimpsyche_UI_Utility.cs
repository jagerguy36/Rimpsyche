using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Maux36.RimPsyche
{
    [StaticConstructorOnStartup]
    public class Rimpsyche_UI_Utility
    {
        public static readonly Color ButtonDarkColor = new Color(0.623529f, 0.623529f, 0.623529f);
        public static readonly Color ButtonLightColor = new Color(0.97647f, 0.97647f, 0.97647f);
        public static readonly Texture2D PsycheButton = ContentFinder<Texture2D>.Get("Buttons/RimpsycheIcon", true);
    }
}
