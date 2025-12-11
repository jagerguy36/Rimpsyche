using System.Collections.Generic;
using System.IO;
using System.Linq;
using Verse;

namespace Maux36.RimPsyche
{
    public class PsycheSlot : IExposable
    {
        public string name;
        public PsycheData data;

        // REQUIRED by Scribe
        public PsycheSlot() { }

        public PsycheSlot(string name, PsycheData data)
        {
            this.name = name;
            this.data = data;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref name, "name");
            Scribe_Deep.Look(ref data, "data");
        }
    }

    [StaticConstructorOnStartup]
    public static class PsycheSaveManager
    {
        private const int numSlots = 15;
        public static List<PsycheSlot> Slots;
        private const string FileName = "Rimpsyche_PsycheDataSlots.xml";
        private static string FilePath =>  Path.Combine(GenFilePaths.ConfigFolderPath, FileName);


        static PsycheSaveManager()
        {
            Slots = LoadSlots();

            if (Slots == null)
            {
                Slots = Enumerable.Repeat<PsycheSlot>(null, numSlots).ToList();
            }
            else
            {
                if (Slots.Count > numSlots)
                {
                    Slots = [.. Slots.Take(numSlots)];
                }
                else if (Slots.Count < numSlots)
                {
                    Slots.AddRange(Enumerable.Repeat<PsycheSlot>(null, numSlots - Slots.Count));
                }
            }
            SaveSlots(Slots);
        }
        public static void Save()
        {
            SaveSlots(Slots);
        }


        private static void SaveSlots(List<PsycheSlot> slots)
        {
            SafeSaver.Save(FilePath, "PsycheSlots", () =>
            {
                Scribe_Collections.Look(ref slots, "Slots", LookMode.Deep);
            });
        }
        private static List<PsycheSlot> LoadSlots()
        {
            List<PsycheSlot> slots = null;

            if (!File.Exists(FilePath))
                return null;


            Scribe.loader.InitLoading(FilePath);
            Scribe_Collections.Look(ref slots, "Slots", LookMode.Deep);
            Scribe.loader.FinalizeLoading();
            return slots;
        }
    }
}
