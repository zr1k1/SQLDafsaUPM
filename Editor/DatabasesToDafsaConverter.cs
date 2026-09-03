using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;
using UnityEditor;
using Dafsa;

[Serializable]
public class LanguageDatabase {
    public SystemLanguage Language;
    public DefaultAsset Database;
}

public abstract class DatabasesToDafsaConverter<TWordMetadata> : MonoBehaviour where TWordMetadata : WordMetadata, new() {
    public const string FOLDER_IN_STREAMING_ASSETS = "GeneratedDafsaDatFiles";

    [Tooltip("Result name will be en{dafseCommonFileName}, ru{dafseCommonFileName})")]
    [SerializeField] protected string _dafseCommonFileName = default;
    [SerializeField] protected string _wordsTableName = default;
    [SerializeField] protected string _wordsColumnName = default;
    [SerializeField] protected List<LanguageDatabase> _databases = new();

    [Header("Test thing after generation dafsa files")]
    [SerializeField] protected SystemLanguage _testLanguageToLoadGeneratedDafsaFile = default;
    [Tooltip("Letter case matters")]
    [SerializeField] protected string _testWordToFindInDafsaWithMetadata = default;

    protected DafsaRuntime<TWordMetadata> _dafsaRuntime;

    public void BuildAll() {

        Directory.CreateDirectory(Application.streamingAssetsPath);

        var languages = new HashSet<SystemLanguage>();

        foreach (var database in _databases) {
            if (database.Database == null) {
                Debug.LogWarning($"Database for {database.Language} is null.");
                continue;
            }

            if (!languages.Add(database.Language)) {
                Debug.LogError($"Duplicate language: {database.Language}");
                continue;
            }

            try {
                Build(database);
            } catch (Exception e) {
                Debug.LogException(e);
            }
        }

#if UNITY_EDITOR
        AssetDatabase.Refresh();
#endif

        Debug.Log("DAFSA build finished.");
    }

    private void Build(LanguageDatabase database) {
        var language = database.Language;

        Debug.Log($"Building {language}...");

        string databasePath = AssetDatabase.GetAssetPath(database.Database);

        if (string.IsNullOrEmpty(databasePath)) {
            Debug.LogError($"Failed to get path for {language} database.");
            return;
        }

        if (!File.Exists(databasePath)) {
            Debug.LogError($"Database file not found: {databasePath}");
            return;
        }

        // DAFSA files are generated into Unity's StreamingAssets folder.
        // The files are loaded from this folder at runtime.
        string outputPath = Path.Combine(
            Application.streamingAssetsPath,
            FOLDER_IN_STREAMING_ASSETS,
            $"{LanguageUtils.LanguageIdStringForType(language)}{_dafseCommonFileName}.dat"
        );

        Directory.CreateDirectory(Path.GetDirectoryName(outputPath));

        FillWordsDatasFromDB(databasePath, out var wordEntries);

        if (wordEntries.Count == 0) {
            Debug.LogError($"No words loaded for {language}.");
            return;
        }

        Debug.Log($"Loaded {wordEntries.Count} words.");

        var trie = new Trie<TWordMetadata>(wordEntries);

        DafsaBuilder<TWordMetadata>.Minimize(trie);

        var nodes = new List<DafsaRuntime<TWordMetadata>.Node>();
        var edges = new List<DafsaRuntime<TWordMetadata>.Edge>();
        var ids = new Dictionary<DafsaTrieNode, int>();

        DafsaBuilder<TWordMetadata>.BuildRuntime(trie.Root, nodes, edges, ids);

        var metadata = wordEntries.Select(x => x.Metadata).ToArray();

        DafsaRuntimeSerializer<TWordMetadata>.Save(nodes, edges, outputPath, wordEntries.Count, metadata);
        Debug.Log($"Saved: {outputPath}");
    }

    void FillWordsDatasFromDB(string databasePath, out List<WordEntry<TWordMetadata>> wordEntries) {
        using (var db = new SimpleDatabase(databasePath)) {
            SetupDataFromDatabase(db, out wordEntries);
        }
    }

    public virtual void SetupDataFromDatabase(SimpleDatabase db, out List<WordEntry<TWordMetadata>> wordEntries) {
        wordEntries = new();

        var words = db.GetColumn<string>(_wordsTableName, _wordsColumnName);
        if (words.Count == 0) {
            Debug.LogError("Words database is empty.");
            return;
        }
        FillNeededListsOfColumnElements(db);

        int duplicates = 0;
        for (int i = 0; i < words.Count; i++) {

            string word = words[i];

            if (string.IsNullOrEmpty(word))
                continue;

            var metadata = SetupMetadataForWord(db, i);

            wordEntries.Add(new WordEntry<TWordMetadata>(word, metadata));
        }

        wordEntries.Sort(static (a, b) => string.CompareOrdinal(a.Word, b.Word));

        Debug.Log($"Loaded {wordEntries.Count} words, duplicates skipped: {duplicates}");
    }

    public abstract void FillNeededListsOfColumnElements(SimpleDatabase db);

    public abstract TWordMetadata SetupMetadataForWord(SimpleDatabase db, int wordWordsListIndex);

    /// <summary>
    /// Example of loading the generated DAFSA and reading metadata
    /// for a specific word.
    public virtual void TestLoadDafsa() {
        string path = Path.Combine(
            Application.streamingAssetsPath,
            FOLDER_IN_STREAMING_ASSETS,
            $"{LanguageUtils.LanguageIdStringForType(_testLanguageToLoadGeneratedDafsaFile)}{_dafseCommonFileName}.dat"
        );

        // Load the DAFSA using the same metadata type that was used
        // when the file was created.
        _dafsaRuntime = DafsaRuntimeSerializer<TWordMetadata>.Load(path);

        Debug.Log(
            $"DatabasesToDafsaConverter : " +
            $"Loaded words: {_dafsaRuntime.WordCount}"
        );
    }
}
