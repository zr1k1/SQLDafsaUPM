#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Dafsa;


/// <summary>
/// Example converter that builds a DAFSA using ExampleWordMetadata.
///
/// The generated DAFSA files are saved to Unity's StreamingAssets folder.
/// This allows the files to be included in the final application and
/// loaded at runtime.
///
/// To create another converter:
/// 1. Create a metadata class derived from WordMetadata.
/// 2. Inherit from DatabasesToDafsaConverter<YourMetadata>.
/// 3. Load the required database columns in
///    FillNeededListsOfColumnElements().
/// 4. Create and return your metadata in SetupMetadataForWord().
///
/// The DAFSA and serialization code do not need to be changed.
/// </summary>
public class ExampleDatabasesToDafsaConverter : DatabasesToDafsaConverter<ExampleWordMetadata> {

    [Header("Example things")]

    // Database table containing the "Good" values.
    [SerializeField] protected string _goodTableName = default;

    // Database column containing the "Good" values.
    [SerializeField] protected string _goodColumnName = default;

    // Values loaded from the database.
    // The index corresponds to the word index in the source word list.
    private List<int> _goods = new();


    /// <summary>
    /// Loads all database columns required to create word metadata.
    /// </summary>
    public override void FillNeededListsOfColumnElements(SimpleDatabase db) {
        _goods = db.GetColumn<int>(_goodTableName, _goodColumnName);
    }


    /// <summary>
    /// Creates metadata for a specific word.
    ///
    /// wordWordsListIndex is the index of the word in the source
    /// word list and is used to retrieve the corresponding database data.
    /// </summary>
    public override ExampleWordMetadata SetupMetadataForWord(SimpleDatabase db, int wordWordsListIndex) {
        return new ExampleWordMetadata {
            // Read the "Good" value from the database.
            Good = _goods[wordWordsListIndex] == 1,

            // Example of additional custom metadata.
            // SomeData = wordWordsListIndex
        };
    }

    /// <summary>
    /// Example of loading the generated DAFSA and reading metadata
    /// for a specific word.
    public override void TestLoadDafsa() {
        string path = Path.Combine(
            Application.streamingAssetsPath,
            FOLDER_IN_STREAMING_ASSETS,
            $"{LanguageUtils.LanguageIdStringForType(_testLanguageToLoadGeneratedDafsaFile)}{_dafseCommonFileName}.dat"
        );

        // Load the DAFSA using the same metadata type that was used
        // when the file was created.
        _dafsaRuntime = DafsaRuntimeSerializer<ExampleWordMetadata>.Load(path);

        Debug.Log(
            $"DatabasesToDafsaConverter : " +
            $"Loaded words: {_dafsaRuntime.WordCount}"
        );

        if (_dafsaRuntime.TryGetMetadata(_testWordToFindInDafsaWithMetadata, out var metadata)) {
            Debug.Log(
                $"DatabasesToDafsaConverter : " +
                $"LoadDafsa _testWordToFindInDafsaWithMetadata Good={metadata.Good}"
            );

            Debug.Log(
                $"DatabasesToDafsaConverter : "
            // + $"LoadDafsa _testWordToFindInDafsaWithMetadata SomeData={metadata.SomeData}"
            );
        } else {
            Debug.LogError(
                $"Test word = {_testWordToFindInDafsaWithMetadata} is not exist in dafsa!"
            );
        }
    }
}
#endif