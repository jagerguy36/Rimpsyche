using System;
using System.Collections.Generic;
using System.Text;
using Verse;

namespace Maux36.RimPsyche
{
    public static class PsycheDataUtil
    {
        public static PsycheData GetPsycheData(Pawn pawn, bool preserveMemory=false)
        {
            var compPsyche = pawn?.compPsyche();
            if (compPsyche == null) return null;

            //New PsycheData
            var psyche = new PsycheData();

            //Personality
            var personality = compPsyche.Personality;
            psyche.imagination = personality.GetFacetValueRaw(Facet.Imagination);
            psyche.intellect = personality.GetFacetValueRaw(Facet.Intellect);
            psyche.curiosity = personality.GetFacetValueRaw(Facet.Curiosity);

            psyche.industriousness = personality.GetFacetValueRaw(Facet.Industriousness);
            psyche.orderliness = personality.GetFacetValueRaw(Facet.Orderliness);
            psyche.integrity = personality.GetFacetValueRaw(Facet.Integrity);

            psyche.sociability = personality.GetFacetValueRaw(Facet.Sociability);
            psyche.assertiveness = personality.GetFacetValueRaw(Facet.Assertiveness);
            psyche.enthusiasm = personality.GetFacetValueRaw(Facet.Enthusiasm);

            psyche.compassion = personality.GetFacetValueRaw(Facet.Compassion);
            psyche.cooperation = personality.GetFacetValueRaw(Facet.Cooperation);
            psyche.humbleness = personality.GetFacetValueRaw(Facet.Humbleness);

            psyche.volatility = personality.GetFacetValueRaw(Facet.Volatility);
            psyche.pessimism = personality.GetFacetValueRaw(Facet.Pessimism);
            psyche.insecurity = personality.GetFacetValueRaw(Facet.Insecurity);

            //Interests
            var interests = compPsyche.Interests;
            psyche.interestScore = new Dictionary<string, float>(interests.interestScore);

            //Sexuality
            var sexuality = compPsyche.Sexuality;
            psyche.orientationCategory = sexuality.orientationCategory;
            psyche.mKinsey = sexuality.MKinsey;
            psyche.attraction = sexuality.RawAttraction;
            psyche.sexDrive = sexuality.RawSexdrive;
            //Initialize all preferences if not yet initialized.
            if (Rimpsyche.SexualityModuleLoaded)
            {
                var allPreference = DefDatabase<PreferenceDef>.AllDefsListForReading;
                foreach (var prefDef in allPreference)
                {
                    if (prefDef.isActive)
                    {
                        sexuality.GetPreference(prefDef);
                    }
                }
            }
            psyche.preference = new Dictionary<string, List<PrefEntry>>(sexuality.GetPreferenceRaw());
            psyche.preference ??= new();
            if (preserveMemory)
            {
                psyche.knownOrientation = [.. sexuality.knownOrientation];
                psyche.relationship = new Dictionary<int, float>(sexuality.relationship);
                psyche.knownOrientation ??= new();
                psyche.relationship ??= new();
            }
            else
            {
                psyche.knownOrientation = [];
                psyche.relationship = [];
            }

            return psyche;
        }

        public static void InjectPsycheData(Pawn pawn, PsycheData psyche, bool preserveMemory)
        {
            if(psyche == null) return;
            var compPsyche = pawn?.compPsyche();
            if (compPsyche == null) return;
            compPsyche.InjectPsycheData(psyche, preserveMemory);
        }

        // Used delims: | : = , ;
        public static string GetSerializedStringPsycheData(Pawn pawn, bool preserveMemory = false)
        {
            var psycheData = GetPsycheData(pawn, preserveMemory);
            StringBuilder sb = new StringBuilder();

            // 1. Personality
            sb.Append($"{psycheData.imagination};{psycheData.intellect};{psycheData.curiosity};");
            sb.Append($"{psycheData.industriousness};{psycheData.orderliness};{psycheData.integrity};");
            sb.Append($"{psycheData.sociability};{psycheData.assertiveness};{psycheData.enthusiasm};");
            sb.Append($"{psycheData.compassion};{psycheData.cooperation};{psycheData.humbleness};");
            sb.Append($"{psycheData.volatility};{psycheData.pessimism};{psycheData.insecurity}|");

            // 2. Interests
            List<string> interestPairs = new List<string>();
            foreach (var kvp in psycheData.interestScore)
            {
                interestPairs.Add($"{kvp.Key}:{kvp.Value}");
            }
            sb.Append(string.Join(",", interestPairs) + "|");

            // 3. Sexuality
            sb.Append($"{(int)psycheData.orientationCategory};{psycheData.mKinsey};{psycheData.attraction};{psycheData.sexDrive}|");

            // 4. Preference
            List<string> prefBlocks = new List<string>();
            foreach (var kvp in psycheData.preference)
            {
                if (kvp.Value == null) continue;

                List<string> entryPairs = new List<string>();
                foreach (var entry in kvp.Value)
                {
                    entryPairs.Add($"{entry.stringKey}:{entry.intKey}");
                }

                string entries = string.Join(",", entryPairs);
                prefBlocks.Add($"{kvp.Key}={entries}");
            }
            sb.Append(string.Join(";", prefBlocks));

            // Memory Area

            // 5. KnownOrientation
            sb.Append(string.Join(",", psycheData.knownOrientation) + "|");

            // 6. Relationship
            List<string> relationshipPairs = new List<string>();
            foreach (var kvp in psycheData.relationship)
            {
                relationshipPairs.Add($"{kvp.Key}:{kvp.Value}");
            }
            sb.Append(string.Join(",", relationshipPairs) + "|");

            return sb.ToString();
        }

        public static PsycheData DeserializeStringPsycheData(string dataString)
        {
            PsycheData data = new PsycheData();
            if (string.IsNullOrEmpty(dataString)) return null;

            try
            {
                string[] sections = dataString.Split('|');
                if (sections.Length < 6)
                {
                    Log.Error("[RimPsyche] Cannot deserialize string. Missing primary data blocks.");
                    return data;
                }

                // 1. Personality
                string[] p = sections[0].Split(';');
                if (p.Length >= 15)
                {
                    data.imagination = float.Parse(p[0]); data.intellect = float.Parse(p[1]); data.curiosity = float.Parse(p[2]);
                    data.industriousness = float.Parse(p[3]); data.orderliness = float.Parse(p[4]); data.integrity = float.Parse(p[5]);
                    data.sociability = float.Parse(p[6]); data.assertiveness = float.Parse(p[7]); data.enthusiasm = float.Parse(p[8]);
                    data.compassion = float.Parse(p[9]); data.cooperation = float.Parse(p[10]); data.humbleness = float.Parse(p[11]);
                    data.volatility = float.Parse(p[12]); data.pessimism = float.Parse(p[13]); data.insecurity = float.Parse(p[14]);
                }

                // 2. Interests
                if (!string.IsNullOrEmpty(sections[1]))
                {
                    foreach (var kvp in sections[1].Split(','))
                    {
                        string[] tokens = kvp.Split(':');
                        if (tokens.Length == 2) data.interestScore[tokens[0]] = float.Parse(tokens[1]);
                    }
                }

                // 3. Sexuality
                string[] sex = sections[2].Split(';');
                if (sex.Length >= 4)
                {
                    data.orientationCategory = (SexualOrientation)int.Parse(sex[0]);
                    data.mKinsey = float.Parse(sex[1]);
                    data.attraction = float.Parse(sex[2]);
                    data.sexDrive = float.Parse(sex[3]);
                }

                // 4. Preference
                if (!string.IsNullOrEmpty(sections[3]))
                {
                    foreach (var block in sections[3].Split(';'))
                    {
                        string[] kvp = block.Split('=');
                        if (kvp.Length != 2) continue;

                        string dictKey = kvp[0];
                        data.preference[dictKey] = new List<PrefEntry>();

                        if (string.IsNullOrEmpty(kvp[1])) continue;
                        foreach (var entryStr in kvp[1].Split(','))
                        {
                            string[] tokens = entryStr.Split(':');
                            if (tokens.Length == 2)
                            {
                                PrefEntry entry = new PrefEntry
                                {
                                    stringKey = tokens[0],
                                    intKey = int.Parse(tokens[1])
                                };
                                data.preference[dictKey].Add(entry);
                            }
                        }
                    }
                }

                // 5. KnownOrientation
                if (!string.IsNullOrEmpty(sections[4]))
                {
                    foreach (var id in sections[3].Split(','))
                    {
                        if (int.TryParse(id, out int parsedId)) data.knownOrientation.Add(parsedId);
                    }
                }

                // 6. Relationship
                if (!string.IsNullOrEmpty(sections[5]))
                {
                    foreach (var kvp in sections[5].Split(','))
                    {
                        string[] tokens = kvp.Split(':');
                        if (tokens.Length == 2) data.relationship[int.Parse(tokens[0])] = float.Parse(tokens[1]);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[RimPsyche] Handled exception while deserializing custom string: {ex}");
            }

            // Execute automated post-parse safety check
            data.interestScore ??= new();
            data.knownOrientation ??= new();
            data.relationship ??= new();
            data.preference ??= new();

            if (Rimpsyche.SexualityModuleLoaded)
            {
                //Serialized data does not go through ExposeData() so it needs to be adjusted on deserialization.
                var allPreference = DefDatabase<PreferenceDef>.AllDefsListForReading;
                foreach (var prefDef in allPreference)
                {
                    if (prefDef.isActive)
                    {
                        prefDef.worker.PostLoadAdjustment(data.preference);
                    }
                }
            }
            return data;
        }


        public static void InjectSerializedStringPsycheData(Pawn pawn, string dataString, bool preserveMemory)
        {
            var psycheData = DeserializeStringPsycheData(dataString);
            if (psycheData == null) return;
            var compPsyche = pawn?.compPsyche();
            if (compPsyche == null) return;
            compPsyche.InjectPsycheData(psycheData, preserveMemory);
        }
    }
}
