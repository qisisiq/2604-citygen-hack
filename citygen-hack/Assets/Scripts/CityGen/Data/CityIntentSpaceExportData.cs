using System;
using System.Collections.Generic;

namespace CityGen.Data
{
    [Serializable]
    public class CityIntentSpaceExportData
    {
        public string ExportVersion = "0.1.0";
        public string GeneratedAtUtc = string.Empty;
        public string Title = string.Empty;
        public string Summary = string.Empty;
        public string CityName = string.Empty;
        public int Seed;
        public string MacroShape = string.Empty;
        public string ClaimedIdentity = string.Empty;
        public string ActualIdentity = string.Empty;
        public IntentSpaceContractData IntentSpace = new IntentSpaceContractData();
        public List<string> ArtifactPaths = new List<string>();
        public List<string> ValidationIssues = new List<string>();
        public List<string> Landmarks = new List<string>();
        public List<CityIntentSpaceBandExportData> Bands = new List<CityIntentSpaceBandExportData>();
    }
}
