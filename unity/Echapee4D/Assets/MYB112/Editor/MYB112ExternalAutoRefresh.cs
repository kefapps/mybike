using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace MYB112.Editor
{
    [InitializeOnLoad]
    public static class MYB112ExternalAutoRefresh
    {
        private const string EnabledPrefKey = "MYB.ExternalAutoRefresh.Enabled";
        private const string MenuPath = "Tools/MYB/Workflow/External Auto Refresh/Enabled";
        private const double DebounceSeconds = 0.85d;
        private const double PollIntervalSeconds = 3.0d;

        private static readonly object Gate = new object();
        private static readonly List<FileSystemWatcher> Watchers = new List<FileSystemWatcher>();
        private static readonly string[] WatchedRelativePaths = { "Assets", "Packages", "ProjectSettings" };
        private static DateTime lastChangeUtc;
        private static long latestKnownWriteTicks;
        private static double nextPollAt;
        private static bool pendingRefresh;
        private static bool initialized;
        private static string pendingReason = "external file change";

        static MYB112ExternalAutoRefresh()
        {
            EditorApplication.delayCall += Initialize;
            EditorApplication.update += Update;
            AssemblyReloadEvents.beforeAssemblyReload += StopWatching;
            EditorApplication.quitting += StopWatching;
        }

        [MenuItem(MenuPath)]
        private static void ToggleEnabled()
        {
            SetEnabled(!IsEnabled);
        }

        [MenuItem(MenuPath, true)]
        private static bool ValidateToggleEnabled()
        {
            Menu.SetChecked(MenuPath, IsEnabled);
            return true;
        }

        [MenuItem("Tools/MYB/Workflow/External Auto Refresh/Refresh Now")]
        private static void RefreshNow()
        {
            RequestRefresh("manual refresh");
        }

        private static bool IsEnabled => EditorPrefs.GetBool(EnabledPrefKey, true);

        private static void Initialize()
        {
            if (initialized)
            {
                return;
            }

            initialized = true;
            RebuildWatchers();
        }

        private static void SetEnabled(bool enabled)
        {
            EditorPrefs.SetBool(EnabledPrefKey, enabled);
            RebuildWatchers();
            Debug.Log("MYB external auto refresh " + (enabled ? "enabled" : "disabled") + ".");
        }

        private static void RebuildWatchers()
        {
            StopWatching();
            if (!IsEnabled)
            {
                return;
            }

            latestKnownWriteTicks = FindLatestWriteTicks(out _);
            nextPollAt = EditorApplication.timeSinceStartup + PollIntervalSeconds;

            foreach (var relativePath in WatchedRelativePaths)
            {
                var absolutePath = Path.Combine(ProjectRoot, relativePath);
                if (!Directory.Exists(absolutePath))
                {
                    continue;
                }

                var watcher = new FileSystemWatcher(absolutePath)
                {
                    IncludeSubdirectories = true,
                    NotifyFilter = NotifyFilters.FileName
                        | NotifyFilters.DirectoryName
                        | NotifyFilters.LastWrite
                        | NotifyFilters.Size
                };

                watcher.Changed += OnFileChanged;
                watcher.Created += OnFileChanged;
                watcher.Deleted += OnFileChanged;
                watcher.Renamed += OnFileRenamed;
                watcher.Error += OnWatcherError;
                watcher.EnableRaisingEvents = true;
                Watchers.Add(watcher);
            }
        }

        private static void StopWatching()
        {
            foreach (var watcher in Watchers)
            {
                watcher.EnableRaisingEvents = false;
                watcher.Changed -= OnFileChanged;
                watcher.Created -= OnFileChanged;
                watcher.Deleted -= OnFileChanged;
                watcher.Renamed -= OnFileRenamed;
                watcher.Error -= OnWatcherError;
                watcher.Dispose();
            }

            Watchers.Clear();
        }

        private static void OnFileChanged(object sender, FileSystemEventArgs args)
        {
            if (ShouldIgnore(args.FullPath))
            {
                return;
            }

            RequestRefresh(Path.GetFileName(args.FullPath));
        }

        private static void OnFileRenamed(object sender, RenamedEventArgs args)
        {
            if (ShouldIgnore(args.FullPath))
            {
                return;
            }

            RequestRefresh(Path.GetFileName(args.FullPath));
        }

        private static void OnWatcherError(object sender, ErrorEventArgs args)
        {
            lock (Gate)
            {
                pendingRefresh = true;
                lastChangeUtc = DateTime.UtcNow;
                pendingReason = "watcher recovery";
            }
        }

        private static void RequestRefresh(string reason)
        {
            lock (Gate)
            {
                pendingRefresh = true;
                lastChangeUtc = DateTime.UtcNow;
                pendingReason = string.IsNullOrWhiteSpace(reason) ? "external file change" : reason;
            }
        }

        private static void Update()
        {
            if (!IsEnabled)
            {
                return;
            }

            PollForExternalChanges();

            string reason;
            lock (Gate)
            {
                if (!pendingRefresh || (DateTime.UtcNow - lastChangeUtc).TotalSeconds < DebounceSeconds)
                {
                    return;
                }

                reason = pendingReason;
                pendingRefresh = false;
                pendingReason = "external file change";
            }

            if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                RequestRefresh(reason);
                return;
            }

            Debug.Log("MYB external auto refresh: " + reason);
            AssetDatabase.Refresh(ImportAssetOptions.Default);
            latestKnownWriteTicks = FindLatestWriteTicks(out _);
            nextPollAt = EditorApplication.timeSinceStartup + PollIntervalSeconds;
        }

        private static void PollForExternalChanges()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            if (EditorApplication.timeSinceStartup < nextPollAt)
            {
                return;
            }

            nextPollAt = EditorApplication.timeSinceStartup + PollIntervalSeconds;
            var latestTicks = FindLatestWriteTicks(out var latestPath);
            if (latestTicks <= latestKnownWriteTicks)
            {
                return;
            }

            latestKnownWriteTicks = latestTicks;
            RequestRefresh(Path.GetFileName(latestPath));
        }

        private static long FindLatestWriteTicks(out string latestPath)
        {
            latestPath = string.Empty;
            var latestTicks = 0L;
            foreach (var relativePath in WatchedRelativePaths)
            {
                var absolutePath = Path.Combine(ProjectRoot, relativePath);
                if (!Directory.Exists(absolutePath))
                {
                    continue;
                }

                try
                {
                    foreach (var path in Directory.EnumerateFiles(absolutePath, "*", SearchOption.AllDirectories))
                    {
                        if (ShouldIgnore(path))
                        {
                            continue;
                        }

                        long ticks;
                        try
                        {
                            ticks = File.GetLastWriteTimeUtc(path).Ticks;
                        }
                        catch (IOException)
                        {
                            continue;
                        }
                        catch (UnauthorizedAccessException)
                        {
                            continue;
                        }

                        if (ticks <= latestTicks)
                        {
                            continue;
                        }

                        latestTicks = ticks;
                        latestPath = path;
                    }
                }
                catch (IOException)
                {
                    continue;
                }
                catch (UnauthorizedAccessException)
                {
                    continue;
                }
            }

            return latestTicks;
        }

        private static bool ShouldIgnore(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return true;
            }

            var normalizedPath = path.Replace('\\', '/');
            var fileName = Path.GetFileName(normalizedPath);
            if (string.IsNullOrEmpty(fileName)
                || fileName == ".DS_Store"
                || fileName.EndsWith(".tmp", StringComparison.OrdinalIgnoreCase)
                || fileName.EndsWith(".swp", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return ContainsIgnoreCase(normalizedPath, "/Library/")
                || ContainsIgnoreCase(normalizedPath, "/Temp/")
                || ContainsIgnoreCase(normalizedPath, "/Obj/")
                || ContainsIgnoreCase(normalizedPath, "/Logs/");
        }

        private static bool ContainsIgnoreCase(string value, string search)
        {
            return value.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string ProjectRoot => Directory.GetParent(Application.dataPath)?.FullName ?? Application.dataPath;
    }
}
