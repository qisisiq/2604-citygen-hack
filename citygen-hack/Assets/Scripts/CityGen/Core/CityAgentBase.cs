using CityGen.Data;
using UnityEngine;

namespace CityGen.Core
{
    public abstract class CityAgentBase : ScriptableObject
    {
        [SerializeField]
        private string agentId = "city-agent";

        [SerializeField]
        private string displayName = "City Agent";

        public string AgentId
        {
            get { return agentId; }
        }

        public string DisplayName
        {
            get { return displayName; }
        }

        public abstract void Execute(CityContext context);

        protected void LogDecision(CityContext context, string summary, string rationale)
        {
            if (context == null)
            {
                return;
            }

            context.RecordDecision(agentId, summary, rationale);
        }

        protected void RegisterArtifact(CityContext context, string path, string description)
        {
            if (context == null)
            {
                return;
            }

            context.RegisterArtifact(path, description, agentId);
        }
    }
}
