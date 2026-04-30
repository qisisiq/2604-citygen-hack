using System.Text;
using CityGen.Agents;
using CityGen.Data;
using CityGen.Validation;
using UnityEngine;

namespace CityGen.Core
{
    public partial class CityGenerator : MonoBehaviour
    {
        [Header("Seed")]
        [SerializeField]
        private CitySeedProfile seedProfile;

        [SerializeField]
        private bool useDefaultSeedWhenMissing = true;

        [Header("Intent Space Trace")]
        [SerializeField]
        private string projectIntentId = "build-procedural-vertical-city-generator";

        [SerializeField]
        private string parentIntentId = "implement-semantic-graph-pipeline-v1";

        [SerializeField]
        private string currentWorkIntentId = "run-foundation-agents";

        [Header("Intent Space Bridge")]
        [SerializeField]
        private string intentSpaceWorkspaceRelativePath = "..";

        [SerializeField]
        private string intentSpacePublishParentId = string.Empty;

        [SerializeField]
        private string intentSpaceExportAssetFolder = "Assets/CityGen/Exports";

        [SerializeField]
        private string intentSpacePythonExecutable = "python3";

        [SerializeField]
        private string intentSpaceBridgeScriptProjectRelativePath = "Tools/citygen_publish_to_intentspace.py";

        [Header("Execution")]
        [SerializeField]
        private bool generateOnStart;

        [SerializeField]
        private bool logSummaryToConsole = true;

        [Header("Optional Agent Assets")]
        [SerializeField]
        private OriginAgent originAgent;

        [SerializeField]
        private TaxonomyMixingAgent taxonomyMixingAgent;

        [SerializeField]
        private MacroShapeAgent macroShapeAgent;

        [SerializeField]
        private VerticalZoningAgent verticalZoningAgent;

        [SerializeField]
        private ValidationAgent validationAgent;

        [Header("Generated State")]
        [SerializeField]
        private CityContext generatedContext;

        [SerializeField]
        [TextArea(6, 20)]
        private string lastGenerationSummary = string.Empty;

        [SerializeField]
        private Transform generatedBlockoutRoot;

        public CityContext GeneratedContext
        {
            get { return generatedContext; }
        }

        [ContextMenu("Generate City Context")]
        public void GenerateCityContext()
        {
            CityContext context = new CityContext();
            CitySeedData seed = ResolveSeed();
            context.InitializeFromSeed(seed);
            context.BindWorkIntent(projectIntentId, parentIntentId, currentWorkIntentId);

            ExecuteAgent(context, originAgent, () => ScriptableObject.CreateInstance<OriginAgent>());
            ExecuteAgent(context, taxonomyMixingAgent, () => ScriptableObject.CreateInstance<TaxonomyMixingAgent>());
            ExecuteAgent(context, macroShapeAgent, () => ScriptableObject.CreateInstance<MacroShapeAgent>());
            ExecuteAgent(context, verticalZoningAgent, () => ScriptableObject.CreateInstance<VerticalZoningAgent>());
            ExecuteAgent(context, validationAgent, () => ScriptableObject.CreateInstance<ValidationAgent>());

            generatedContext = context;
            lastGenerationSummary = BuildSummary(context);

            if (logSummaryToConsole)
            {
                Debug.Log(lastGenerationSummary, this);
            }
        }

        [ContextMenu("Clear Generated Context")]
        public void ClearGeneratedContext()
        {
            generatedContext = null;
            lastGenerationSummary = string.Empty;
        }

        private void Start()
        {
            if (generateOnStart)
            {
                GenerateCityContext();
            }
        }

        private CitySeedData ResolveSeed()
        {
            if (seedProfile != null && seedProfile.Data != null)
            {
                return seedProfile.Data;
            }

            if (useDefaultSeedWhenMissing)
            {
                return CitySeedFactory.CreateAscendingWard();
            }

            return new CitySeedData();
        }

        private void ExecuteAgent<T>(CityContext context, T configuredAgent, System.Func<T> factory)
            where T : CityAgentBase
        {
            bool createdTemporaryInstance = false;
            T agent = configuredAgent;

            if (agent == null)
            {
                agent = factory();
                createdTemporaryInstance = agent != null;
            }

            if (agent == null)
            {
                return;
            }

            agent.Execute(context);

            if (createdTemporaryInstance)
            {
                DestroyTemporary(agent);
            }
        }

        private string BuildSummary(CityContext context)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine($"CityGen: {context.CityName}");
            builder.AppendLine($"Seed: {context.Seed}");
            builder.AppendLine($"Work Intent: {context.IntentSpace.CurrentWorkIntentId}");
            builder.AppendLine($"Macro Shape: {context.MacroShape}");
            builder.AppendLine($"Bands: {context.VerticalBands.Count}");
            builder.AppendLine($"Claimed Identity: {context.Identity.ClaimedIdentity}");
            builder.AppendLine($"Actual Identity: {context.Identity.ActualIdentity}");

            if (context.VerticalBands.Count > 0)
            {
                builder.AppendLine($"Lowest Band: {context.VerticalBands[0].DisplayName}");
                builder.AppendLine($"Highest Band: {context.VerticalBands[context.VerticalBands.Count - 1].DisplayName}");
            }

            builder.AppendLine($"Validation Issues: {context.ValidationIssues.Count}");

            for (int i = 0; i < context.ValidationIssues.Count; i++)
            {
                builder.AppendLine($"- {context.ValidationIssues[i]}");
            }

            return builder.ToString().TrimEnd();
        }

        private static void DestroyTemporary(Object temporaryObject)
        {
            if (temporaryObject == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(temporaryObject);
            }
            else
            {
                DestroyImmediate(temporaryObject);
            }
        }
    }
}
