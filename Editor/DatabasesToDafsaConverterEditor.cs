#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DatabasesToDafsaConverter<>), true)]
public class DatabasesToDafsaConverterEditor : Editor {

    public override void OnInspectorGUI() {
        serializedObject.Update();

        DrawProperty("_dafseCommonFileName");
        DrawProperty("_wordsTableName");
        DrawProperty("_wordsColumnName");

        DrawAdditionalProperties();

        serializedObject.ApplyModifiedProperties();

        EditorGUILayout.Space(10);

        EditorGUILayout.HelpBox(
            "Database configuration and DAFSA generation are available in " +
            "DAFSA → Databases To DAFSA.",
            MessageType.Info
        );

        if (GUILayout.Button("Open DAFSA Converter", GUILayout.Height(25))) {
            DatabasesToDafsaConverterWindow.Open((MonoBehaviour)target);
        }
    }

    void DrawAdditionalProperties() {
        var iterator = serializedObject.GetIterator();
        bool enterChildren = true;

        while (iterator.NextVisible(enterChildren)) {
            enterChildren = false;

            if (iterator.name == "m_Script" ||
                iterator.name == "_dafseCommonFileName" ||
                iterator.name == "_wordsTableName" ||
                iterator.name == "_wordsColumnName" ||
                iterator.name == "_databases" ||
                iterator.name == "_testLanguageToLoadGeneratedDafsaFile" ||
                iterator.name == "_testWordToFindInDafsaWithMetadata")
                continue;

            EditorGUILayout.PropertyField(iterator, true);
        }
    }

    void DrawProperty(string propertyName) {
        var property = serializedObject.FindProperty(propertyName);

        if (property != null)
            EditorGUILayout.PropertyField(property);
    }
}
#endif