#if UNITY_EDITOR
using System.IO;
using CityGen.Core;
using CityGen.Data;
using UnityEditor;
using UnityEngine;

namespace CityGen.Editor
{
    internal sealed class CityIntentSpaceInboxWindow : EditorWindow
    {
        private Vector2 scrollPosition;

        [MenuItem("Window/CityGen/IntentSpace Inbox")]
        private static void OpenWindow()
        {
            CityIntentSpaceInboxWindow window = GetWindow<CityIntentSpaceInboxWindow>();
            window.titleContent = new GUIContent("IntentSpace Inbox");
            window.minSize = new Vector2(480f, 320f);
            window.Show();
        }

        internal static void RepaintOpenWindows()
        {
            CityIntentSpaceInboxWindow[] windows = Resources.FindObjectsOfTypeAll<CityIntentSpaceInboxWindow>();
            for (int i = 0; i < windows.Length; i++)
            {
                windows[i].Repaint();
            }
        }

        private void OnGUI()
        {
            CityIntentSpaceInboxSettings settings = CityIntentSpaceInboxSettings.instance;
            CityGenerator generator = settings.ResolveGenerator();

            DrawTargetSection(settings, generator);
            EditorGUILayout.Space(8f);
            DrawImportSection(settings, generator);
            EditorGUILayout.Space(8f);
            DrawStatusSection(settings, generator);
            EditorGUILayout.Space(8f);
            DrawBacklogSection(generator);
        }

        private void DrawTargetSection(CityIntentSpaceInboxSettings settings, CityGenerator generator)
        {
            EditorGUILayout.LabelField("Target", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            CityGenerator selectedGenerator = (CityGenerator)EditorGUILayout.ObjectField(
                "City Generator",
                generator,
                typeof(CityGenerator),
                true);

            if (EditorGUI.EndChangeCheck())
            {
                settings.SetGenerator(selectedGenerator);
                generator = selectedGenerator;
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Use Selected Object") && Selection.activeGameObject != null)
                {
                    CityGenerator selected = Selection.activeGameObject.GetComponent<CityGenerator>();
                    if (selected != null)
                    {
                        settings.SetGenerator(selected);
                    }
                }

                if (GUILayout.Button("Select Generator") && generator != null)
                {
                    Selection.activeObject = generator.gameObject;
                    EditorGUIUtility.PingObject(generator.gameObject);
                }
            }
        }

        private void DrawImportSection(CityIntentSpaceInboxSettings settings, CityGenerator generator)
        {
            EditorGUILayout.LabelField("Import", EditorStyles.boldLabel);

            bool autoImport = EditorGUILayout.Toggle("Auto Import", settings.AutoImportEnabled);
            if (autoImport != settings.AutoImportEnabled)
            {
                settings.AutoImportEnabled = autoImport;
            }

            float interval = EditorGUILayout.FloatField("Poll Interval (sec)", settings.AutoImportIntervalSeconds);
            if (!Mathf.Approximately(interval, settings.AutoImportIntervalSeconds))
            {
                settings.AutoImportIntervalSeconds = interval;
            }

            if (generator != null)
            {
                EditorGUI.BeginChangeCheck();
                string parentId = EditorGUILayout.TextField("Parent Filter", generator.IntentSpaceImportParentId);
                string kindFilter = EditorGUILayout.TextField("Kind Filter", generator.IntentSpaceImportKindFilter);
                if (EditorGUI.EndChangeCheck())
                {
                    generator.ConfigureIntentSpaceImport(parentId, kindFilter);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Assign a CityGenerator to enable import controls.", MessageType.Info);
            }

            using (new EditorGUI.DisabledScope(generator == null))
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Import Now") && generator != null)
                {
                    generator.RunIntentSpaceImport(true);
                    settings.LastAttemptTimestampUtc = System.DateTime.UtcNow.ToString("O");
                    settings.LastImportedItemCount = generator.LastImportedIntentSpaceData != null ? generator.LastImportedIntentSpaceData.Items.Count : 0;
                    settings.LastImportedSummary = generator.LastImportedIntentSpaceSummary;
                    settings.LastStatusMessage = "Manual import completed.";
                }

                if (GUILayout.Button("Generate Context") && generator != null)
                {
                    generator.GenerateCityContext();
                }

                if (GUILayout.Button("Generate Blockout") && generator != null)
                {
                    generator.GenerateBlockout();
                }
            }
        }

        private void DrawStatusSection(CityIntentSpaceInboxSettings settings, CityGenerator generator)
        {
            EditorGUILayout.LabelField("Status", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Last Attempt", string.IsNullOrWhiteSpace(settings.LastAttemptTimestampUtc) ? "Never" : settings.LastAttemptTimestampUtc);
            EditorGUILayout.LabelField("Last Result", string.IsNullOrWhiteSpace(settings.LastStatusMessage) ? "None" : settings.LastStatusMessage);
            EditorGUILayout.LabelField("Imported Item Count", settings.LastImportedItemCount.ToString());

            if (generator != null && !string.IsNullOrWhiteSpace(generator.LastImportedObservatoryUrl))
            {
                if (GUILayout.Button("Open Observatory"))
                {
                    Application.OpenURL(generator.LastImportedObservatoryUrl);
                }
            }

            if (!string.IsNullOrWhiteSpace(settings.LastImportedSummary))
            {
                EditorGUILayout.TextArea(settings.LastImportedSummary, GUILayout.MinHeight(72f));
            }
        }

        private void DrawBacklogSection(CityGenerator generator)
        {
            EditorGUILayout.LabelField("Backlog", EditorStyles.boldLabel);

            if (generator == null || generator.LastImportedIntentSpaceData == null)
            {
                EditorGUILayout.HelpBox("No imported work items yet.", MessageType.Info);
                return;
            }

            CityIntentSpaceImportData data = generator.LastImportedIntentSpaceData;
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            for (int i = 0; i < data.Items.Count; i++)
            {
                DrawWorkItem(generator, data.Items[i]);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawWorkItem(CityGenerator generator, CityIntentSpaceWorkItemData item)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField(item.Content, EditorStyles.boldLabel);
                EditorGUILayout.LabelField("State", item.LatestState);
                EditorGUILayout.LabelField("Kind", string.IsNullOrWhiteSpace(item.Kind) ? "(none)" : item.Kind);
                EditorGUILayout.LabelField("Intent ID", item.IntentId);
                EditorGUILayout.LabelField("Parent", item.ParentId);
                if (!string.IsNullOrWhiteSpace(item.Summary))
                {
                    EditorGUILayout.LabelField("Summary", item.Summary);
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Adopt Work Item"))
                    {
                        generator.AdoptImportedWorkItem(item);
                    }

                    if (GUILayout.Button("Copy Intent ID"))
                    {
                        EditorGUIUtility.systemCopyBuffer = item.IntentId;
                    }
                }

                if (item.ArtifactPaths != null && item.ArtifactPaths.Count > 0)
                {
                    for (int i = 0; i < item.ArtifactPaths.Count; i++)
                    {
                        string normalizedPath = NormalizeAssetPath(item.ArtifactPaths[i]);
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            EditorGUILayout.LabelField(normalizedPath);
                            if (GUILayout.Button("Ping", GUILayout.Width(64f)))
                            {
                                Object asset = AssetDatabase.LoadMainAssetAtPath(normalizedPath);
                                if (asset != null)
                                {
                                    EditorGUIUtility.PingObject(asset);
                                    Selection.activeObject = asset;
                                }
                            }
                        }
                    }
                }
            }
        }

        private static string NormalizeAssetPath(string rawPath)
        {
            if (string.IsNullOrWhiteSpace(rawPath))
            {
                return string.Empty;
            }

            string normalized = rawPath.Replace('\\', '/');
            int assetsIndex = normalized.IndexOf("Assets/", System.StringComparison.Ordinal);
            if (assetsIndex >= 0)
            {
                normalized = normalized.Substring(assetsIndex);
            }

            return normalized;
        }
    }
}
#endif
