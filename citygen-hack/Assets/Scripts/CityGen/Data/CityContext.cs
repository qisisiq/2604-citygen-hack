using System;
using System.Collections.Generic;

namespace CityGen.Data
{
    [Serializable]
    public class CityContext
    {
        public string ContextVersion = "0.1.0";
        public int Seed = 2604;
        public string CityName = "The Ascending Ward";
        public MacroShapeKind MacroShape = MacroShapeKind.HollowCylinder;
        public CitySeedData SeedDefinition = new CitySeedData();
        public IntentSpaceContractData IntentSpace = new IntentSpaceContractData();
        public CityOriginData Origin = new CityOriginData();
        public CityIdentitySummary Identity = new CityIdentitySummary();
        public CityGridDefinition Grid = new CityGridDefinition();
        public VerticalLogicProfile VerticalLogic = new VerticalLogicProfile();
        public NavigationPrinciples Navigation = new NavigationPrinciples();
        public List<TaxonomyWeight> TaxonomyWeights = new List<TaxonomyWeight>();
        public List<VerticalBandData> VerticalBands = new List<VerticalBandData>();
        public List<DistrictNode> Districts = new List<DistrictNode>();
        public List<RouteEdge> Routes = new List<RouteEdge>();
        public List<InfrastructureFlow> InfrastructureFlows = new List<InfrastructureFlow>();
        public List<LandmarkNode> Landmarks = new List<LandmarkNode>();
        public List<ArtifactReference> Artifacts = new List<ArtifactReference>();
        public List<string> NarrativeHooks = new List<string>();
        public List<string> ValidationIssues = new List<string>();
        public List<CityGenerationLogEntry> GenerationLog = new List<CityGenerationLogEntry>();

        public void InitializeFromSeed(CitySeedData seedData)
        {
            if (seedData == null)
            {
                return;
            }

            SeedDefinition = seedData;
            Seed = seedData.Seed;
            CityName = seedData.CityName;
            MacroShape = seedData.MacroShape;
            Origin = seedData.Origin;
            Identity = seedData.Identity;
            Grid = seedData.Grid;
            VerticalLogic = seedData.VerticalLogic;
            Navigation = seedData.Navigation;
            TaxonomyWeights = new List<TaxonomyWeight>(seedData.TaxonomyWeights);
        }

        public void RecordDecision(string agentId, string summary, string rationale)
        {
            CityGenerationLogEntry entry = new CityGenerationLogEntry();
            entry.AgentId = agentId;
            entry.Summary = summary;
            entry.Rationale = rationale;
            entry.TimestampUtc = DateTime.UtcNow.ToString("O");
            entry.WorkIntentId = IntentSpace != null ? IntentSpace.CurrentWorkIntentId : string.Empty;
            GenerationLog.Add(entry);
        }

        public void AddValidationIssue(string issue)
        {
            if (!string.IsNullOrWhiteSpace(issue))
            {
                ValidationIssues.Add(issue);
            }
        }

        public void BindWorkIntent(string projectIntentId, string parentIntentId, string currentWorkIntentId)
        {
            if (IntentSpace == null)
            {
                IntentSpace = new IntentSpaceContractData();
            }

            IntentSpace.ProjectIntentId = projectIntentId ?? string.Empty;
            IntentSpace.ParentIntentId = parentIntentId ?? string.Empty;
            IntentSpace.CurrentWorkIntentId = currentWorkIntentId ?? string.Empty;
        }

        public void RegisterArtifact(string path, string description, string producingAgentId)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            ArtifactReference artifact = new ArtifactReference();
            artifact.Path = path;
            artifact.Description = description ?? string.Empty;
            artifact.ProducingAgentId = producingAgentId ?? string.Empty;
            artifact.WorkIntentId = IntentSpace != null ? IntentSpace.CurrentWorkIntentId : string.Empty;
            Artifacts.Add(artifact);

            if (GenerationLog.Count > 0)
            {
                GenerationLog[GenerationLog.Count - 1].ArtifactPaths.Add(path);
            }
        }
    }
}
