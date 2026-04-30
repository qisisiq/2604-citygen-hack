using System;
using System.Collections.Generic;
using UnityEngine;

namespace CityGen.Data
{
    [Serializable]
    public class CityIdentitySummary
    {
        [TextArea(2, 5)]
        public string ClaimedIdentity = "A sacred hospital city for healing and pilgrimage.";

        [TextArea(2, 5)]
        public string ActualIdentity = "A medical bureaucracy that converts residents into managed labor and extractable biological output.";

        [TextArea(2, 5)]
        public string LegacyIdentity = "An older temple city built around ritual healing.";

        [TextArea(2, 5)]
        public string EmergentIdentity = "A living megastructure that grows new wards in response to suffering, prayer, and waste.";

        public List<string> ConflictingInstitutions = new List<string>();
        public List<string> VisualMotifs = new List<string>();
        public List<string> SocialTensions = new List<string>();
    }
}
