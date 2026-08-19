#if UNITY_EDITOR

using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class DafsaMenu {
    // Path relative to the package root.
    private const string SceneRelativePath = "Scenes/Databases_To_DAFSA_Converter_Scene.unity";

    [MenuItem("DAFSA/Open Converter")]
    public static void OpenConverterScene() {
        string packageRoot = FindPackageRoot();

        if (string.IsNullOrEmpty(packageRoot)) {
            Debug.LogError("DAFSA: Could not find package root.");

            return;
        }

        string scenePath = Path.Combine(packageRoot, SceneRelativePath);

        scenePath = scenePath.Replace("\\", "/");

        if (!File.Exists(scenePath)) {
            Debug.LogError($"DAFSA: Converter scene not found:\n{scenePath}");

            return;
        }

        EditorSceneManager.OpenScene(scenePath);
    }

    private static string FindPackageRoot() {
        string[] guids = AssetDatabase.FindAssets("DafsaMenu t:MonoScript");

        foreach (string guid in guids) {
            string scriptPath = AssetDatabase.GUIDToAssetPath(guid);

            if (!scriptPath.EndsWith("DafsaMenu.cs", System.StringComparison.OrdinalIgnoreCase)) {
                continue;
            }

            string directory = Path.GetDirectoryName(scriptPath);

            while (!string.IsNullOrEmpty(directory)) {
                string packageJsonPath = Path.Combine(directory, "package.json");

                if (File.Exists(packageJsonPath))
                    return directory;

                directory = Path.GetDirectoryName(directory);
            }
        }

        return null;
    }
}

#endif

