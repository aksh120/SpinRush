using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace SpinRush.Editor
{
    /// <summary>
    /// Headless batch-mode build automation script for compiling SpinRush to WebGL.
    /// Configures optimized player settings, uncompressed artifacts for universal local server compatibility,
    /// and exports the playable build to Build/WebGL/.
    /// </summary>
    public static class BuildScript
    {
        [MenuItem("SpinRush/Build WebGL Player")]
        public static void BuildWebGL()
        {
            Debug.Log("=================================================");
            Debug.Log("  STARTING SPINRUSH WEBGL HEADLESS BATCH BUILD   ");
            Debug.Log("=================================================");

            string buildPath = "Build/WebGL";
            if (!Directory.Exists(buildPath))
            {
                Directory.CreateDirectory(buildPath);
            }

            // WebGL Player Configuration
            PlayerSettings.productName = "SpinRush";
            PlayerSettings.companyName = "SpinRush Royal VIP";
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;

            // Use Uncompressed WebGL output for guaranteed instant preview on all local & remote HTTP servers
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
            PlayerSettings.WebGL.decompressionFallback = false;
            PlayerSettings.WebGL.memorySize = 256; // 256 MB heap
            PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.None; // Maximum execution speed
            PlayerSettings.WebGL.threadsSupport = false;

            string scenePath = "Assets/Scenes/MainGameScene.unity";
            if (!File.Exists(scenePath))
            {
                Debug.LogWarning($"Main scene not found at {scenePath}, generating scene first...");
                SceneSetupEditor.BuildSceneAndPrefabs();
            }

            string[] scenes = new string[] { scenePath };

            BuildPlayerOptions buildOptions = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = buildPath,
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            };

            Debug.Log($"Building WebGL player to destination: {Path.GetFullPath(buildPath)}");
            DateTime startTime = DateTime.Now;
            BuildReport report = BuildPipeline.BuildPlayer(buildOptions);
            BuildSummary summary = report.summary;

            TimeSpan duration = DateTime.Now - startTime;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log("=================================================");
                Debug.Log($"  WEBGL BUILD SUCCEEDED! [Duration: {duration.TotalSeconds:F1}s]");
                Debug.Log($"  Total Size: {summary.totalSize / (1024f * 1024f):F2} MB");
                Debug.Log($"  Output: {Path.GetFullPath(buildPath)}");
                Debug.Log("=================================================");
            }
            else
            {
                Debug.LogError($"[WEBGL BUILD FAILED] Result: {summary.result}, Errors: {summary.totalErrors}");
                throw new Exception($"WebGL Build Failed with {summary.totalErrors} errors.");
            }
        }
    }
}
