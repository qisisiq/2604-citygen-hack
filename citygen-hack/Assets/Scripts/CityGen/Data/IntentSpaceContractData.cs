using System;
using System.Collections.Generic;

namespace CityGen.Data
{
    [Serializable]
    public class IntentSpaceContractData
    {
        public string ProjectIntentId = string.Empty;
        public string CurrentWorkIntentId = string.Empty;
        public string ParentIntentId = string.Empty;
        public string OutputSummary = string.Empty;
        public List<string> ExpectedArtifactPaths = new List<string>();
    }
}
