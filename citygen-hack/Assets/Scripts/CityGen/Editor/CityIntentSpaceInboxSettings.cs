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
