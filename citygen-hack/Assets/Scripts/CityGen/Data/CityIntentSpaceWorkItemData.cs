using System;
using System.Collections.Generic;

namespace CityGen.Data
{
    [Serializable]
    public class CityIntentSpaceWorkItemData
    {
        public string IntentId = string.Empty;
        public string ParentId = string.Empty;
        public string SenderId = string.Empty;
        public string Content = string.Empty;
        public string Kind = string.Empty;
        public string LatestState = string.Empty;
        public string Summary = string.Empty;
        public List<string> ArtifactPaths = new List<string>();
        public List<string> Tags = new List<string>();
    }
}
