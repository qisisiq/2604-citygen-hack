using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using CityGen.Data;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CityGen.Core
{
    public partial class CityGenerator
    {
#if UNITY_EDITOR
        [ContextMenu("Export IntentSpace Summary")]
        public void ExportIntentSpaceSummary()
        {
            if (generatedContext == null)
            {
                GenerateCityContext();
            }

            if (generatedContext == null)
            {
                UnityEngine.Debug.LogWarning("CityGenerator could not create a context to export.", this);
                return;
            }

            string assetPath = WriteIntentSpaceExportAsset(generatedContext);
            UnityEngine.Debug.Log($"IntentSpace export written to {assetPath}", this);
        }

        [ContextMenu("Publish IntentSpace Summary")]
        public void PublishIntentSpaceSummary()
        {
            if (generatedContext == null)
            {
                GenerateCityContext();
            }

            if (generatedContext == null)
            {
                UnityEngine.Debug.LogWarning("CityGenerator could not create a context to publish.", this);
                return;
            }

            string exportAssetPath = WriteIntentSpaceExportAsset(generatedContext);
            string projectRoot = GetProjectRootPath();
            string exportAbsolutePath = Path.Combine(projectRoot, exportAssetPath);
            string workspaceRoot = ResolveIntentSpaceWorkspaceRoot(projectRoot);
            string bridgeScriptPath = Path.Combine(projectRoot, intentSpaceBridgeScriptProjectRelativePath);

            if (!File.Exists(bridgeScriptPath))
            {
                UnityEngine.Debug.LogError($"IntentSpace bridge script not found: {bridgeScriptPath}", this);
                return;
            }

            List<string> commandArguments = new List<string>
            {
                bridgeScriptPath,
                "--workspace-root", workspaceRoot,
                "--export-json", exportAbsolutePath,
                "--title", $"Unity export: {generatedContext.CityName} foundation pass"
            };

            if (!string.IsNullOrWhiteSpace(intentSpacePublishParentId))
            {
                commandArguments.Add("--parent-id");
                commandArguments.Add(intentSpacePublishParentId);
            }

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = intentSpacePythonExecutable,
                Arguments = BuildQuotedArguments(commandArguments.ToArray()),
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = workspaceRoot
            };

            try
            {
                using (Process process = Process.Start(startInfo))
                {
                    if (process == null)
                    {
                        UnityEngine.Debug.LogError("Failed to start the IntentSpace bridge process.", this);
                        return;
                    }

                    string stdout = process.StandardOutput.ReadToEnd();
                    string stderr = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    if (!string.IsNullOrWhiteSpace(stdout))
                    {
                        UnityEngine.Debug.Log(stdout.Trim(), this);
                    }

                    if (!string.IsNullOrWhiteSpace(stderr))
                    {
                        UnityEngine.Debug.LogWarning(stderr.Trim(), this);
                    }

                    if (process.ExitCode != 0)
                    {
                        UnityEngine.Debug.LogError($"IntentSpace publish failed with exit code {process.ExitCode}.", this);
                    }
                }
            }
            catch (Exception exception)
            {
                UnityEngine.Debug.LogError($"IntentSpace publish failed: {exception.Message}", this);
            }
        }

        [Header("Intent Space Import")]
        [SerializeField]
        private string intentSpaceImportAssetFolder = "Assets/CityGen/Imports";

        [SerializeField]
        private string intentSpaceImportParentId = string.Empty;

        [SerializeField]
        private string intentSpaceImportKindFilter = string.Empty;

        [SerializeField]
        private string intentSpaceImportScriptProjectRelativePath = "Tools/citygen_import_from_intentspace.py";

        [SerializeField]
        private CityIntentSpaceImportData lastImportedIntentSpaceData;

        [SerializeField]
        [TextArea(6, 20)]
        private string lastImportedIntentSpaceSummary = string.Empty;

        public CityIntentSpaceImportData LastImportedIntentSpaceData
        {
            get { return lastImportedIntentSpaceData; }
        }

        public string LastImportedIntentSpaceSummary
        {
            get { return lastImportedIntentSpaceSummary; }
        }

        public string LastImportedObservatoryUrl
        {
            get { return lastImportedIntentSpaceData != null ? lastImportedIntentSpaceData.ObservatoryUrl : string.Empty; }
        }

        public string IntentSpaceImportParentId
        {
            get { return intentSpaceImportParentId; }
        }

        public string IntentSpaceImportKindFilter
        {
            get { return intentSpaceImportKindFilter; }
        }

        [ContextMenu("Import IntentSpace Work Items")]
        public void ImportIntentSpaceWorkItems()
        {
            RunIntentSpaceImport(true);
        }

        public bool RunIntentSpaceImport(bool logOutput)
        {
            string projectRoot = GetProjectRootPath();
            string workspaceRoot = ResolveIntentSpaceWorkspaceRoot(projectRoot);
            string bridgeScriptPath = Path.Combine(projectRoot, intentSpaceImportScriptProjectRelativePath);

            if (!File.Exists(bridgeScriptPath))
            {
                UnityEngine.Debug.LogError($"IntentSpace import script not found: {bridgeScriptPath}", this);
                return false;
            }

            string importFolderAbsolute = Path.Combine(projectRoot, intentSpaceImportAssetFolder);
            Directory.CreateDirectory(importFolderAbsolute);
            string importAbsolutePath = Path.Combine(importFolderAbsolute, "intentspace-work-items.json");

            List<string> commandArguments = new List<string>
            {
                bridgeScriptPath,
                "--workspace-root", workspaceRoot,
                "--output-json", importAbsolutePath
            };

            if (!string.IsNullOrWhiteSpace(intentSpaceImportParentId))
            {
                commandArguments.Add("--parent-id");
                commandArguments.Add(intentSpaceImportParentId);
            }

            if (!string.IsNullOrWhiteSpace(intentSpaceImportKindFilter))
            {
                commandArguments.Add("--kind-filter");
                commandArguments.Add(intentSpaceImportKindFilter);
            }

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = intentSpacePythonExecutable,
                Arguments = BuildQuotedArguments(commandArguments.ToArray()),
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = workspaceRoot
            };

            try
            {
                using (Process process = Process.Start(startInfo))
                {
                    if (process == null)
                    {
                        UnityEngine.Debug.LogError("Failed to start the IntentSpace import process.", this);
                        return false;
                    }

                    string stdout = process.StandardOutput.ReadToEnd();
                    string stderr = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    if (logOutput && !string.IsNullOrWhiteSpace(stdout))
                    {
                        UnityEngine.Debug.Log(stdout.Trim(), this);
                    }

                    if (logOutput && !string.IsNullOrWhiteSpace(stderr))
                    {
                        UnityEngine.Debug.LogWarning(stderr.Trim(), this);
                    }

                    if (process.ExitCode != 0)
                    {
                        UnityEngine.Debug.LogError($"IntentSpace import failed with exit code {process.ExitCode}.", this);
                        return false;
                    }
                }

                AssetDatabase.Refresh();

                if (File.Exists(importAbsolutePath))
                {
                    string json = File.ReadAllText(importAbsolutePath, Encoding.UTF8);
                    lastImportedIntentSpaceData = JsonUtility.FromJson<CityIntentSpaceImportData>(json);
                    lastImportedIntentSpaceSummary = BuildImportedWorkSummary(lastImportedIntentSpaceData);
                    if (logOutput)
                    {
                        UnityEngine.Debug.Log(lastImportedIntentSpaceSummary, this);
                    }

                    EditorUtility.SetDirty(this);
                    return true;
                }
            }
            catch (Exception exception)
            {
                UnityEngine.Debug.LogError($"IntentSpace import failed: {exception.Message}", this);
            }

            return false;
        }

        public void ConfigureIntentSpaceImport(string parentId, string kindFilter)
        {
            intentSpaceImportParentId = parentId ?? string.Empty;
            intentSpaceImportKindFilter = kindFilter ?? string.Empty;
            EditorUtility.SetDirty(this);
        }

        public void AdoptImportedWorkItem(CityIntentSpaceWorkItemData item)
        {
            if (item == null)
            {
                return;
            }

            parentIntentId = item.ParentId ?? string.Empty;
            currentWorkIntentId = item.IntentId ?? string.Empty;
            intentSpacePublishParentId = item.IntentId ?? string.Empty;
            intentSpaceImportParentId = item.ParentId ?? string.Empty;
            EditorUtility.SetDirty(this);
        }

        private string WriteIntentSpaceExportAsset(CityContext context)
        {
            string projectRoot = GetProjectRootPath();
            string exportFolderAbsolute = Path.Combine(projectRoot, intentSpaceExportAssetFolder);
            Directory.CreateDirectory(exportFolderAbsolute);

            CityIntentSpaceExportData exportData = BuildIntentSpaceExportData(context, projectRoot);
            string fileName = $"{SanitizeFileName(context.CityName).ToLowerInvariant()}-foundation-export.json";
            string absolutePath = Path.Combine(exportFolderAbsolute, fileName);
            File.WriteAllText(absolutePath, JsonUtility.ToJson(exportData, true), Encoding.UTF8);

            AssetDatabase.Refresh();

            string relativePath = absolutePath.Replace(projectRoot + Path.DirectorySeparatorChar, string.Empty);
            context.RegisterArtifact(relativePath, "Unity Intentspace export summary", "city-generator");
            return relativePath;
        }

        private CityIntentSpaceExportData BuildIntentSpaceExportData(CityContext context, string projectRoot)
        {
            CityIntentSpaceExportData exportData = new CityIntentSpaceExportData();
            exportData.GeneratedAtUtc = DateTime.UtcNow.ToString("O");
            exportData.CityName = context.CityName;
            exportData.Seed = context.Seed;
            exportData.MacroShape = context.MacroShape.ToString();
            exportData.ClaimedIdentity = context.Identity != null ? context.Identity.ClaimedIdentity : string.Empty;
            exportData.ActualIdentity = context.Identity != null ? context.Identity.ActualIdentity : string.Empty;
            exportData.IntentSpace = context.IntentSpace != null ? DeepCopyIntentSpace(context.IntentSpace) : new IntentSpaceContractData();
            exportData.Title = $"Unity export: {context.CityName} foundation pass";
            exportData.Summary = BuildExportSummary(context);

            if (context.ValidationIssues != null)
            {
                exportData.ValidationIssues = new List<string>(context.ValidationIssues);
            }

            if (context.Landmarks != null)
            {
                for (int i = 0; i < context.Landmarks.Count; i++)
                {
                    exportData.Landmarks.Add(context.Landmarks[i].DisplayName);
                }
            }

            if (context.Artifacts != null)
            {
                for (int i = 0; i < context.Artifacts.Count; i++)
                {
                    if (!string.IsNullOrWhiteSpace(context.Artifacts[i].Path))
                    {
                        exportData.ArtifactPaths.Add(context.Artifacts[i].Path);
                    }
                }
            }

            if (context.VerticalBands != null)
            {
                for (int i = 0; i < context.VerticalBands.Count; i++)
                {
                    VerticalBandData band = context.VerticalBands[i];
                    CityIntentSpaceBandExportData bandExport = new CityIntentSpaceBandExportData();
                    bandExport.Id = band.Id;
                    bandExport.DisplayName = band.DisplayName;
                    bandExport.AccessLevel = band.DefaultAccessLevel.ToString();
                    bandExport.HistoricalLayer = band.DominantHistoricalLayer.ToString();
                    bandExport.Description = band.Description;
                    bandExport.OfficialFunctions = ToStringList(band.OfficialFunctions);
                    bandExport.HiddenFunctions = ToStringList(band.HiddenFunctions);
                    bandExport.InheritedFunctions = ToStringList(band.InheritedFunctions);
                    exportData.Bands.Add(bandExport);
                }
            }

            return exportData;
        }

        private string BuildExportSummary(CityContext context)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append($"{context.CityName} generated with {context.VerticalBands.Count} vertical bands");
            builder.Append($" and macro shape {context.MacroShape}.");

            if (context.ValidationIssues.Count == 0)
            {
                builder.Append(" Validation passed.");
            }
            else
            {
                builder.Append($" Validation reported {context.ValidationIssues.Count} issue(s).");
            }

            return builder.ToString();
        }

        private static IntentSpaceContractData DeepCopyIntentSpace(IntentSpaceContractData source)
        {
            IntentSpaceContractData copy = new IntentSpaceContractData();
            copy.ProjectIntentId = source.ProjectIntentId;
            copy.ParentIntentId = source.ParentIntentId;
            copy.CurrentWorkIntentId = source.CurrentWorkIntentId;
            copy.OutputSummary = source.OutputSummary;
            copy.ExpectedArtifactPaths = source.ExpectedArtifactPaths != null
                ? new List<string>(source.ExpectedArtifactPaths)
                : new List<string>();
            return copy;
        }

        private static List<string> ToStringList<T>(List<T> values)
        {
            List<string> result = new List<string>();
            if (values == null)
            {
                return result;
            }

            for (int i = 0; i < values.Count; i++)
            {
                result.Add(values[i] != null ? values[i].ToString() : string.Empty);
            }

            return result;
        }

        private string ResolveIntentSpaceWorkspaceRoot(string projectRoot)
        {
            if (Path.IsPathRooted(intentSpaceWorkspaceRelativePath))
            {
                return intentSpaceWorkspaceRelativePath;
            }

            string combined = Path.GetFullPath(Path.Combine(projectRoot, intentSpaceWorkspaceRelativePath));
            return combined;
        }

        private static string GetProjectRootPath()
        {
            return Directory.GetParent(Application.dataPath).FullName;
        }

        private static string SanitizeFileName(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return "city";
            }

            StringBuilder builder = new StringBuilder(raw.Length);
            for (int i = 0; i < raw.Length; i++)
            {
                char character = raw[i];
                if (char.IsLetterOrDigit(character) || character == '-' || character == '_')
                {
                    builder.Append(character);
                }
                else if (char.IsWhiteSpace(character))
                {
                    builder.Append('-');
                }
            }

            return builder.Length == 0 ? "city" : builder.ToString();
        }

        private static string BuildQuotedArguments(params string[] args)
        {
            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < args.Length; i++)
            {
                if (string.IsNullOrEmpty(args[i]))
                {
                    continue;
                }

                if (builder.Length > 0)
                {
                    builder.Append(' ');
                }

                builder.Append(QuoteArgument(args[i]));
            }

            return builder.ToString();
        }

        private static string QuoteArgument(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return "\"\"";
            }

            return "\"" + value.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
        }

        private static string BuildImportedWorkSummary(CityIntentSpaceImportData data)
        {
            if (data == null)
            {
                return "IntentSpace import returned no data.";
            }

            StringBuilder builder = new StringBuilder();
            builder.AppendLine($"Imported {data.Items.Count} Intentspace work item(s)");
            builder.AppendLine($"Space: {data.SpaceId}");

            for (int i = 0; i < data.Items.Count; i++)
            {
                CityIntentSpaceWorkItemData item = data.Items[i];
                builder.AppendLine($"- [{item.LatestState}] {item.Content} ({item.IntentId})");
            }

            return builder.ToString().TrimEnd();
        }
#endif
    }
}
