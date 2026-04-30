using System;
using System.Collections.Generic;

namespace CityGen.Data
{
    [Serializable]
    public class CityGenerationLogEntry
    {
        public string AgentId = string.Empty;
        public string Summary = string.Empty;
        public string Rationale = string.Empty;
        public string TimestampUtc = string.Empty;
        public string WorkIntentId = string.Empty;
        public List<string> ArtifactPaths = new List<string>();
    }
}
