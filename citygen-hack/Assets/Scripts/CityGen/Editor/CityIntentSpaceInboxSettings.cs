#if UNITY_EDITOR
using CityGen.Core;
using UnityEditor;
using UnityEngine;

namespace CityGen.Editor
{
    [FilePath("UserSettings/CityIntentSpaceInboxSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    internal sealed class CityIntentSpaceInboxSettings : ScriptableSingleton<CityIntentSpaceInboxSettings>
    {
        [SerializeField]
        private bool autoImportEnabled;

        [SerializeField]
        private float autoImportIntervalSeconds = 60f;

        [SerializeField]
        private string generatorGlobalObjectId = string.Empty;

        [SerializeField]
        private string lastAttemptTimestampUtc = string.Empty;

        [SerializeField]
        private string lastStatusMessage = string.Empty;

        [SerializeField]
        private int lastImportedItemCount;

        [SerializeField]
        private string lastImportedSummary = string.Empty;

        [SerializeField]
        private string runnerParentIntentId = string.Empty;

        [SerializeField]
        private string runnerRoleFilter = "graph-systems agent";

        [SerializeField]
        private string runnerAgentName = "infra-agent-01";

        [SerializeField]
        private string runnerBackend = "codex";

        [SerializeField]
        private string runnerBackendModel = string.Empty;

        [SerializeField]
        private float runnerPollIntervalSeconds = 90f;

        [SerializeField]
        private int runnerMaxTasks;

        [SerializeField]
        private int runnerProcessId;

        [SerializeField]
        private string runnerLastStartedAtUtc = string.Empty;

        [SerializeField]
        private string runnerLastStatusMessage = string.Empty;

        [SerializeField]
        private string runnerLastLogPath = string.Empty;

        public bool AutoImportEnabled
        {
            get { return autoImportEnabled; }
            set
            {
                autoImportEnabled = value;
                Save(true);
            }
        }

        public float AutoImportIntervalSeconds
        {
            get { return autoImportIntervalSeconds; }
            set
            {
                autoImportIntervalSeconds = Mathf.Max(5f, value);
                Save(true);
            }
        }

        public string LastAttemptTimestampUtc
        {
            get { return lastAttemptTimestampUtc; }
            set
            {
                lastAttemptTimestampUtc = value ?? string.Empty;
                Save(true);
            }
        }

        public string LastStatusMessage
        {
            get { return lastStatusMessage; }
            set
            {
                lastStatusMessage = value ?? string.Empty;
                Save(true);
            }
        }

        public int LastImportedItemCount
        {
            get { return lastImportedItemCount; }
            set
            {
                lastImportedItemCount = Mathf.Max(0, value);
                Save(true);
            }
        }

        public string LastImportedSummary
        {
            get { return lastImportedSummary; }
            set
            {
                lastImportedSummary = value ?? string.Empty;
                Save(true);
            }
        }

        public string RunnerParentIntentId
        {
            get { return runnerParentIntentId; }
            set
            {
                runnerParentIntentId = value ?? string.Empty;
                Save(true);
            }
        }

        public string RunnerRoleFilter
        {
            get { return runnerRoleFilter; }
            set
            {
                runnerRoleFilter = value ?? string.Empty;
                Save(true);
            }
        }

        public string RunnerAgentName
        {
            get { return runnerAgentName; }
            set
            {
                runnerAgentName = value ?? string.Empty;
                Save(true);
            }
        }

        public string RunnerBackend
        {
            get { return runnerBackend; }
            set
            {
                runnerBackend = value ?? string.Empty;
                Save(true);
            }
        }

        public string RunnerBackendModel
        {
            get { return runnerBackendModel; }
            set
            {
                runnerBackendModel = value ?? string.Empty;
                Save(true);
            }
        }

        public float RunnerPollIntervalSeconds
        {
            get { return runnerPollIntervalSeconds; }
            set
            {
                runnerPollIntervalSeconds = Mathf.Max(5f, value);
                Save(true);
            }
        }

        public int RunnerMaxTasks
        {
            get { return runnerMaxTasks; }
            set
            {
                runnerMaxTasks = Mathf.Max(0, value);
                Save(true);
            }
        }

        public int RunnerProcessId
        {
            get { return runnerProcessId; }
            set
            {
                runnerProcessId = Mathf.Max(0, value);
                Save(true);
            }
        }

        public string RunnerLastStartedAtUtc
        {
            get { return runnerLastStartedAtUtc; }
            set
            {
                runnerLastStartedAtUtc = value ?? string.Empty;
                Save(true);
            }
        }

        public string RunnerLastStatusMessage
        {
            get { return runnerLastStatusMessage; }
            set
            {
                runnerLastStatusMessage = value ?? string.Empty;
                Save(true);
            }
        }

        public string RunnerLastLogPath
        {
            get { return runnerLastLogPath; }
            set
            {
                runnerLastLogPath = value ?? string.Empty;
                Save(true);
            }
        }

        public void SetGenerator(CityGenerator generator)
        {
            if (generator == null)
            {
                generatorGlobalObjectId = string.Empty;
                Save(true);
                return;
            }

            GlobalObjectId id = GlobalObjectId.GetGlobalObjectIdSlow(generator);
            generatorGlobalObjectId = id.ToString();
            Save(true);
        }

        public CityGenerator ResolveGenerator()
        {
            if (!string.IsNullOrWhiteSpace(generatorGlobalObjectId) &&
                GlobalObjectId.TryParse(generatorGlobalObjectId, out GlobalObjectId globalObjectId))
            {
                Object resolved = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(globalObjectId);
                if (resolved is CityGenerator generator)
                {
                    return generator;
                }
            }

            return Object.FindFirstObjectByType<CityGenerator>();
        }
    }
}
#endif
