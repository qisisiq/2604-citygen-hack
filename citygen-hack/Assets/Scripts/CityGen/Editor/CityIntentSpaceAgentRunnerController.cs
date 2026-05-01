#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using CityGen.Core;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace CityGen.Editor
{
    [InitializeOnLoad]
    internal static class CityIntentSpaceAgentRunnerController
    {
        private const string RunnerScriptProjectRelativePath = "Tools/citygen_agent_runner.py";
        private const string PythonExecutable = "python3";
        private static double nextStatusPollTime;
        private static readonly Queue<RunnerLogMessage> PendingLogMessages = new Queue<RunnerLogMessage>();
        private static readonly object PendingLogLock = new object();

        static CityIntentSpaceAgentRunnerController()
        {
            nextStatusPollTime = EditorApplication.timeSinceStartup + 2.0d;
            EditorApplication.update += Update;
        }

        public static bool IsRunnerRunning
        {
            get
            {
                return TryGetRunnerProcess(CityIntentSpaceInboxSettings.instance.RunnerProcessId, out _);
            }
        }

        public static void StartRunner(CityGenerator generator, bool once)
        {
            CityIntentSpaceInboxSettings settings = CityIntentSpaceInboxSettings.instance;
            if (IsRunnerRunning)
            {
                settings.RunnerLastStatusMessage = "Runner start skipped: already running.";
                CityIntentSpaceInboxWindow.RepaintOpenWindows();
                return;
            }

            string workspaceRoot = ResolveWorkspaceRoot();
            string projectRoot = ResolveProjectRoot();
            string runnerScriptPath = Path.Combine(projectRoot, RunnerScriptProjectRelativePath);

            if (!File.Exists(runnerScriptPath))
            {
                settings.RunnerLastStatusMessage = $"Runner script missing: {runnerScriptPath}";
                CityIntentSpaceInboxWindow.RepaintOpenWindows();
                return;
            }

            string parentIntentId = settings.RunnerParentIntentId;
            if (string.IsNullOrWhiteSpace(parentIntentId) && generator != null)
            {
                parentIntentId = generator.IntentSpaceImportParentId;
            }

            if (string.IsNullOrWhiteSpace(parentIntentId))
            {
                settings.RunnerLastStatusMessage = "Runner start failed: parent intent id is required.";
                CityIntentSpaceInboxWindow.RepaintOpenWindows();
                return;
            }

            string agentName = string.IsNullOrWhiteSpace(settings.RunnerAgentName) ? "citygen-worker-01" : settings.RunnerAgentName;
            string roleFilter = settings.RunnerRoleFilter ?? string.Empty;
            string backend = string.IsNullOrWhiteSpace(settings.RunnerBackend) ? "codex" : settings.RunnerBackend;
            string backendModel = settings.RunnerBackendModel ?? string.Empty;

            string logsFolder = Path.Combine(projectRoot, "Docs", "Intentspace", "Runs");
            Directory.CreateDirectory(logsFolder);
            string timestamp = DateTime.UtcNow.ToString("yyyyMMddTHHmmssZ");
            string safeAgentName = SanitizeFileName(agentName);
            string stdoutLogPath = Path.Combine(logsFolder, $"{timestamp}-{safeAgentName}-runner-stdout.log");
            string stderrLogPath = Path.Combine(logsFolder, $"{timestamp}-{safeAgentName}-runner-stderr.log");

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = PythonExecutable,
                Arguments = BuildArguments(
                    "citygen-hack/" + RunnerScriptProjectRelativePath,
                    "--workspace-root", workspaceRoot,
                    "--parent-id", parentIntentId,
                    "--agent-name", agentName,
                    "--backend", backend,
                    "--poll-seconds", settings.RunnerPollIntervalSeconds.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture)),
                WorkingDirectory = workspaceRoot,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            if (!string.IsNullOrWhiteSpace(roleFilter))
            {
                startInfo.Arguments += " " + Quote("--role-filter") + " " + Quote(roleFilter);
            }

            if (!string.IsNullOrWhiteSpace(backendModel))
            {
                startInfo.Arguments += " " + Quote("--backend-model") + " " + Quote(backendModel);
            }

            if (once)
            {
                startInfo.Arguments += " " + Quote("--once");
            }

            if (settings.RunnerMaxTasks > 0)
            {
                startInfo.Arguments += " " + Quote("--max-tasks") + " " + Quote(settings.RunnerMaxTasks.ToString());
            }

            try
            {
                Process process = new Process();
                process.StartInfo = startInfo;
                process.EnableRaisingEvents = true;
                process.OutputDataReceived += (_, eventArgs) => HandleProcessOutput(stdoutLogPath, eventArgs.Data, false);
                process.ErrorDataReceived += (_, eventArgs) => HandleProcessOutput(stderrLogPath, eventArgs.Data, true);
                process.Exited += (_, _) => OnRunnerExited(process.ExitCode);

                if (!process.Start())
                {
                    settings.RunnerLastStatusMessage = "Runner failed to start.";
                    CityIntentSpaceInboxWindow.RepaintOpenWindows();
                    return;
                }

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                settings.RunnerProcessId = process.Id;
                settings.RunnerLastStartedAtUtc = DateTime.UtcNow.ToString("O");
                settings.RunnerLastStatusMessage = once ? "Runner started in one-shot mode." : "Runner started.";
                settings.RunnerLastLogPath = stdoutLogPath;
                CityIntentSpaceInboxWindow.RepaintOpenWindows();
            }
            catch (Exception exception)
            {
                settings.RunnerProcessId = 0;
                settings.RunnerLastStatusMessage = $"Runner start failed: {exception.Message}";
                Debug.LogError(settings.RunnerLastStatusMessage);
                CityIntentSpaceInboxWindow.RepaintOpenWindows();
            }
        }

        public static void StopRunner()
        {
            CityIntentSpaceInboxSettings settings = CityIntentSpaceInboxSettings.instance;
            if (!TryGetRunnerProcess(settings.RunnerProcessId, out Process process))
            {
                settings.RunnerProcessId = 0;
                settings.RunnerLastStatusMessage = "Runner stop skipped: no active process.";
                CityIntentSpaceInboxWindow.RepaintOpenWindows();
                return;
            }

            try
            {
                process.Kill();
                settings.RunnerProcessId = 0;
                settings.RunnerLastStatusMessage = "Runner stopped.";
            }
            catch (Exception exception)
            {
                settings.RunnerLastStatusMessage = $"Runner stop failed: {exception.Message}";
                Debug.LogError(settings.RunnerLastStatusMessage);
            }

            CityIntentSpaceInboxWindow.RepaintOpenWindows();
        }

        private static void Update()
        {
            if (EditorApplication.timeSinceStartup < nextStatusPollTime)
            {
                FlushPendingLogMessages();
                return;
            }

            nextStatusPollTime = EditorApplication.timeSinceStartup + 2.0d;
            FlushPendingLogMessages();
            CityIntentSpaceInboxSettings settings = CityIntentSpaceInboxSettings.instance;
            if (settings.RunnerProcessId <= 0)
            {
                return;
            }

            if (!TryGetRunnerProcess(settings.RunnerProcessId, out _))
            {
                settings.RunnerProcessId = 0;
                if (string.IsNullOrWhiteSpace(settings.RunnerLastStatusMessage) || settings.RunnerLastStatusMessage == "Runner started.")
                {
                    settings.RunnerLastStatusMessage = "Runner exited.";
                }
                CityIntentSpaceInboxWindow.RepaintOpenWindows();
            }
        }

        private static void OnRunnerExited(int exitCode)
        {
            EditorApplication.delayCall += () =>
            {
                CityIntentSpaceInboxSettings settings = CityIntentSpaceInboxSettings.instance;
                settings.RunnerProcessId = 0;
                settings.RunnerLastStatusMessage = exitCode == 0 ? "Runner exited normally." : $"Runner exited with code {exitCode}.";
                CityIntentSpaceInboxWindow.RepaintOpenWindows();
            };
        }

        private static string ResolveProjectRoot()
        {
            string assetsPath = Application.dataPath;
            return Directory.GetParent(assetsPath).FullName;
        }

        private static string ResolveWorkspaceRoot()
        {
            string projectRoot = ResolveProjectRoot();
            return Directory.GetParent(projectRoot).FullName;
        }

        private static bool TryGetRunnerProcess(int processId, out Process process)
        {
            process = null;
            if (processId <= 0)
            {
                return false;
            }

            try
            {
                Process candidate = Process.GetProcessById(processId);
                if (candidate.HasExited)
                {
                    return false;
                }

                process = candidate;
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static string BuildArguments(params string[] values)
        {
            string[] quoted = new string[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                quoted[i] = Quote(values[i]);
            }

            return string.Join(" ", quoted);
        }

        private static string Quote(string value)
        {
            return "\"" + (value ?? string.Empty).Replace("\"", "\\\"") + "\"";
        }

        private static string SanitizeFileName(string value)
        {
            string sanitized = string.IsNullOrWhiteSpace(value) ? "runner" : value;
            foreach (char invalid in Path.GetInvalidFileNameChars())
            {
                sanitized = sanitized.Replace(invalid, '-');
            }

            return sanitized.Replace(' ', '-');
        }

        private static void AppendLogLine(string path, string line)
        {
            if (line == null)
            {
                return;
            }

            try
            {
                File.AppendAllText(path, line + Environment.NewLine);
            }
            catch
            {
            }
        }

        private static void HandleProcessOutput(string path, string line, bool isError)
        {
            AppendLogLine(path, line);
            if (string.IsNullOrWhiteSpace(line))
            {
                return;
            }

            lock (PendingLogLock)
            {
                PendingLogMessages.Enqueue(new RunnerLogMessage(line, isError));
            }
        }

        private static void FlushPendingLogMessages()
        {
            while (true)
            {
                RunnerLogMessage? message = null;
                lock (PendingLogLock)
                {
                    if (PendingLogMessages.Count > 0)
                    {
                        message = PendingLogMessages.Dequeue();
                    }
                }

                if (!message.HasValue)
                {
                    break;
                }

                LogRunnerMessage(message.Value);
            }
        }

        private static void LogRunnerMessage(RunnerLogMessage message)
        {
            CityIntentSpaceInboxSettings settings = CityIntentSpaceInboxSettings.instance;
            string rawText = message.Text.Trim();
            string displayText = rawText;

            try
            {
                RunnerEventPayload payload = JsonUtility.FromJson<RunnerEventPayload>(rawText);
                if (!string.IsNullOrWhiteSpace(payload.message))
                {
                    displayText = payload.message;
                    settings.RunnerLastStatusMessage = payload.message;
                }
            }
            catch
            {
            }

            if (message.IsError)
            {
                Debug.LogError($"[CityGen Runner] {displayText}");
            }
            else
            {
                Debug.Log($"[CityGen Runner] {displayText}");
            }
        }

        private readonly struct RunnerLogMessage
        {
            public RunnerLogMessage(string text, bool isError)
            {
                Text = text;
                IsError = isError;
            }

            public string Text { get; }

            public bool IsError { get; }
        }

        [Serializable]
        private sealed class RunnerEventPayload
        {
            public string eventName = string.Empty;
            public string message = string.Empty;
        }
    }
}
#endif
