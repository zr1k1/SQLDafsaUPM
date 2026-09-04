#if UNITY_EDITOR

using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class DafsaMenu {
    private const string PackageName = "com.r.sqldafsa";

    private const string SampleDisplayName = "SQL → DAFSA Example";

    private const string SceneRelativePath =
        "Scenes/Databases_To_DAFSA_Example_Scene.unity";

    [MenuItem("DAFSA/Open Example Scene")]
    public static void OpenExampleScene() {
        string packageRoot = FindPackageRoot();

        if (string.IsNullOrEmpty(packageRoot)) {
            Debug.LogError("DAFSA: Could not find package root.");
            return;
        }

        string version = GetPackageVersion(packageRoot);

        if (string.IsNullOrEmpty(version)) {
            Debug.LogError("DAFSA: Could not read package version.");
            return;
        }

        string scenePath = Path.Combine(
            "Assets",
            "Samples",
            "SQL → DAFSA",
            version,
            SampleDisplayName,
            SceneRelativePath
        ).Replace("\\", "/");

        if (!File.Exists(scenePath)) {
            EditorUtility.DisplayDialog(
                "DAFSA Converter",
                "The DAFSA example is not imported.\n\n" +
                "Open Package Manager and import:\n" +
                $"'{SampleDisplayName}'",
                "OK"
            );

            return;
        }

        EditorSceneManager.OpenScene(scenePath);
    }

    private static string GetPackageVersion(string packageRoot) {
        string packageJsonPath = Path.Combine(
            packageRoot,
            "package.json"
        );

        if (!File.Exists(packageJsonPath))
            return null;

        string json = File.ReadAllText(packageJsonPath);

        PackageInfo packageInfo =
            JsonUtility.FromJson<PackageInfo>(json);

        return packageInfo?.version;
    }

    private static string FindPackageRoot() {
        string[] guids =
            AssetDatabase.FindAssets("DafsaMenu t:MonoScript");

        foreach (string guid in guids) {
            string scriptPath =
                AssetDatabase.GUIDToAssetPath(guid);

            if (!scriptPath.EndsWith(
                    "DafsaMenu.cs",
                    StringComparison.OrdinalIgnoreCase)) {
                continue;
            }

            string directory =
                Path.GetDirectoryName(scriptPath);

            while (!string.IsNullOrEmpty(directory)) {
                string packageJsonPath =
                    Path.Combine(directory, "package.json");

                if (File.Exists(packageJsonPath))
                    return directory;

                directory = Path.GetDirectoryName(directory);
            }
        }

        return null;
    }

    [Serializable]
    private class PackageInfo {
        public string version;
    }
}

#endif