using System;

namespace CityGen.Data
{
    [Serializable]
    public class ArtifactReference
    {
        public string Path = string.Empty;
        public string Description = string.Empty;
        public string ProducingAgentId = string.Empty;
        public string WorkIntentId = string.Empty;
    }
}
