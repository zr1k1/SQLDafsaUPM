#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public sealed class DatabasesToDafsaConverterWindow : EditorWindow {

    MonoBehaviour _converter;
    SerializedObject _serializedObject;
    SerializedProperty _databases;

    Vector2 _scrollPosition;

    [MenuItem("DAFSA/Databases To DAFSA")]
    public static void Open() {
        GetWindow<DatabasesToDafsaConverterWindow>("DAFSA Converter");
    }

    public static void Open(MonoBehaviour converter) {
        var window = GetWindow<DatabasesToDafsaConverterWindow>("DAFSA Converter");
        window.SetConverter(converter);
    }

    void OnGUI() {
        DrawConverterSelection();

        if (_converter == null)
            return;

        _serializedObject.Update();

        EditorGUILayout.Space(10);

        DrawDatabases();

        EditorGUILayout.Space(10);

        DrawTestSettings();

        EditorGUILayout.Space(10);

        DrawActions();

        _serializedObject.ApplyModifiedProperties();
    }

    void DrawConverterSelection() {
        EditorGUILayout.LabelField("Converter", EditorStyles.boldLabel);

        var converter = (MonoBehaviour)EditorGUILayout.ObjectField(
            "Target",
            _converter,
            typeof(MonoBehaviour),
            true
        );

        if (converter != _converter)
            SetConverter(converter);

        if (_converter == null) {
            EditorGUILayout.HelpBox(
                "Select a DatabasesToDafsaConverter component.",
                MessageType.Info
            );
        }
    }

    void DrawDatabases() {
        EditorGUILayout.LabelField("Databases", EditorStyles.boldLabel);

        if (_databases == null)
            return;

        _scrollPosition = EditorGUILayout.BeginScrollView(
            _scrollPosition,
            GUILayout.MinHeight(150)
        );

        for (int i = 0; i < _databases.arraySize; i++) {
            var database = _databases.GetArrayElementAtIndex(i);
            var language = database.FindPropertyRelative("Language");
            var databasePath = database.FindPropertyRelative("DatabasePath");

            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.PropertyField(language);

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.TextField(databasePath.stringValue);

            if (GUILayout.Button("Browse", GUILayout.Width(70)))
                SelectDatabase(databasePath);

            if (GUILayout.Button("X", GUILayout.Width(25))) {
                _databases.DeleteArrayElementAtIndex(i);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                break;
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space(5);

        if (GUILayout.Button("Add Database", GUILayout.Height(25))) {
            int index = _databases.arraySize;

            _databases.InsertArrayElementAtIndex(index);

            var element = _databases.GetArrayElementAtIndex(index);

            element.FindPropertyRelative("Language").enumValueIndex = 0;
            element.FindPropertyRelative("DatabasePath").stringValue = string.Empty;
        }
    }

    void DrawTestSettings() {
        EditorGUILayout.LabelField("Test", EditorStyles.boldLabel);

        DrawProperty("_testLanguageToLoadGeneratedDafsaFile");
        DrawProperty("_testWordToFindInDafsaWithMetadata");
    }

    void DrawActions() {
        EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);

        if (GUILayout.Button("Build All", GUILayout.Height(30)))
            Build();

        if (GUILayout.Button("Test Load DAFSA", GUILayout.Height(25)))
            TestLoad();
    }

    void DrawProperty(string propertyName) {
        var property = _serializedObject.FindProperty(propertyName);

        if (property != null)
            EditorGUILayout.PropertyField(property);
    }

    void SelectDatabase(SerializedProperty databaseProperty) {
        string path = EditorUtility.OpenFilePanel(
            "Select Database",
            "",
            ""
        );

        if (string.IsNullOrEmpty(path))
            return;

        databaseProperty.stringValue = path.Replace('\\', '/');

        _serializedObject.ApplyModifiedProperties();

        Repaint();
    }

    void Build() {
        if (_converter == null)
            return;

        _serializedObject.Update();
        _serializedObject.ApplyModifiedProperties();

        var tableName = _serializedObject.FindProperty("_wordsTableName");
        var columnName = _serializedObject.FindProperty("_wordsColumnName");

        if (string.IsNullOrWhiteSpace(tableName.stringValue)) {
            EditorUtility.DisplayDialog(
                "DAFSA Converter",
                "Words Table Name is empty.",
                "OK"
            );

            return;
        }

        if (string.IsNullOrWhiteSpace(columnName.stringValue)) {
            EditorUtility.DisplayDialog(
                "DAFSA Converter",
                "Words Column Name is empty.",
                "OK"
            );

            return;
        }

        var method = _converter.GetType().GetMethod(
            "BuildAll",
            BindingFlags.Instance | BindingFlags.Public
        );

        if (method == null) {
            Debug.LogError(
                $"BuildAll() was not found on {_converter.GetType().Name}."
            );

            return;
        }

        method.Invoke(_converter, null);
    }

    void TestLoad() {
        if (_converter == null)
            return;

        _serializedObject.ApplyModifiedProperties();

        var method = _converter.GetType().GetMethod(
            "TestLoadDafsa",
            BindingFlags.Instance | BindingFlags.Public
        );

        if (method == null) {
            Debug.LogError(
                $"TestLoadDafsa() was not found on {_converter.GetType().Name}."
            );

            return;
        }

        method.Invoke(_converter, null);
    }

    void SetConverter(MonoBehaviour converter) {
        _converter = converter;

        if (_converter == null) {
            _serializedObject = null;
            _databases = null;
            return;
        }

        _serializedObject = new SerializedObject(_converter);
        _databases = _serializedObject.FindProperty("_databases");
    }
}
#endif