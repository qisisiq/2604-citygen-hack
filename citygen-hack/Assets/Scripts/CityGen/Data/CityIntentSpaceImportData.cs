using System;
using System.Collections.Generic;

namespace CityGen.Data
{
    [Serializable]
    public class CityIntentSpaceImportData
    {
        public string ImportVersion = "0.1.0";
        public string ImportedAtUtc = string.Empty;
        public string SpaceId = string.Empty;
        public string ObservatoryUrl = string.Empty;
        public string ParentFilter = string.Empty;
        public List<CityIntentSpaceWorkItemData> Items = new List<CityIntentSpaceWorkItemData>();
    }
}
