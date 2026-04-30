using System;
using System.Collections.Generic;

namespace CityGen.Data
{
    [Serializable]
    public class CityIntentSpaceBandExportData
    {
        public string Id = string.Empty;
        public string DisplayName = string.Empty;
        public string AccessLevel = string.Empty;
        public string HistoricalLayer = string.Empty;
        public string Description = string.Empty;
        public List<string> OfficialFunctions = new List<string>();
        public List<string> HiddenFunctions = new List<string>();
        public List<string> InheritedFunctions = new List<string>();
    }
}
