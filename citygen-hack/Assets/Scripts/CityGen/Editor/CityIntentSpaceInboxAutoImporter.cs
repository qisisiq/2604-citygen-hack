#if UNITY_EDITOR
using System;
using CityGen.Core;
using UnityEditor;

namespace CityGen.Editor
{
    [InitializeOnLoad]
    internal static class CityIntentSpaceInboxAutoImporter
    {
        private static double nextPollTime;

        static CityIntentSpaceInboxAutoImporter()
        {
            nextPollTime = EditorApplication.timeSinceStartup + 5.0d;
            EditorApplication.update += Update;
        }

        private static void Update()
        {
            CityIntentSpaceInboxSettings settings = CityIntentSpaceInboxSettings.instance;
            if (!settings.AutoImportEnabled)
            {
                return;
            }

            if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            if (EditorApplication.timeSinceStartup < nextPollTime)
            {
                return;
            }

            nextPollTime = EditorApplication.timeSinceStartup + Math.Max(5.0d, settings.AutoImportIntervalSeconds);

            CityGenerator generator = settings.ResolveGenerator();
            if (generator == null)
            {
                settings.LastAttemptTimestampUtc = DateTime.UtcNow.ToString("O");
                settings.LastStatusMessage = "Auto-import skipped: no CityGenerator target found.";
                CityIntentSpaceInboxWindow.RepaintOpenWindows();
                return;
            }

            bool success = generator.RunIntentSpaceImport(false);
            settings.LastAttemptTimestampUtc = DateTime.UtcNow.ToString("O");
            settings.LastImportedItemCount = generator.LastImportedIntentSpaceData != null ? generator.LastImportedIntentSpaceData.Items.Count : 0;
            settings.LastImportedSummary = generator.LastImportedIntentSpaceSummary;
            settings.LastStatusMessage = success ? "Auto-import succeeded." : "Auto-import failed.";
            CityIntentSpaceInboxWindow.RepaintOpenWindows();
        }
    }
}
#endif
