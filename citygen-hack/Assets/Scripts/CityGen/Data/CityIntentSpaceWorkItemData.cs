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
        public string Goal = string.Empty;
        public string RoleHint = string.Empty;
        public string Priority = string.Empty;
        public List<string> Inputs = new List<string>();
        public List<string> Outputs = new List<string>();
        public List<string> NonGoals = new List<string>();
        public List<string> DoneCondition = new List<string>();
        public List<string> ArtifactPaths = new List<string>();
        public List<string> Tags = new List<string>();
    }
}
