# SQL → DAFSA

Unity package for converting SQLite word databases into compact DAFSA binary files.

The package provides:

* SQLite → Trie → DAFSA conversion
* DAFSA minimization
* Compact binary `.dat` files
* Extensible word metadata
* Runtime word lookup
* Prefix lookup
* Word enumeration
* Word index lookup
* Runtime metadata lookup
* Example converter
* Editor conversion UI

---

# Quick Start

1. Install the package from Git.

2. Open the example scene:

`Databases_To_DAFSA_Example_Scene.unity`

3. In Unity, open:

`DAFSA → Databases To DAFSA`

4. Select the `ExampleDatabasesToDafsaConverter` component.

5. Configure the converter settings.

6. Add one or more SQLite databases.

7. Select the language for each database.

8. Use **Browse** to select the SQLite database file.

9. Press **Build All**.

10. Find the generated `.dat` files in:

`Assets/StreamingAssets/GeneratedDafsaDatFiles/`

The original SQLite databases are only required during the conversion process.

---

# Example Scene

The package contains an example scene:

`Databases_To_DAFSA_Example_Scene.unity`

The scene demonstrates the complete SQLite → Trie → DAFSA conversion workflow and provides a reference implementation that can be adapted to another database structure.

The example scene contains a GameObject with:

`ExampleDatabasesToDafsaConverter`

The example demonstrates:

* Selecting a SQLite database
* Configuring database table and column names
* Loading additional database data
* Creating custom word metadata
* Generating a DAFSA binary file
* Loading the generated DAFSA
* Testing word lookup
* Testing metadata lookup

The example scene is intended to be the first place to look when getting started with the package.

---

# Converter UI

The converter window can be opened from:

`DAFSA → Databases To DAFSA`

The converter window allows you to:

* Select the converter component
* Configure conversion settings
* Add multiple databases
* Assign a language to each database
* Select SQLite database files
* Build all configured databases
* Load a generated DAFSA file for testing

## Database Selection

Database files do not have to be located inside the Unity project.

Use the **Browse** button to select a SQLite database from anywhere on the computer.

The selected database path is stored in the converter configuration.

For example:

`/Users/username/Documents/Databases/en_words.db`

If the Unity project is moved to another computer, database paths may need to be selected again.

---

# Build All

**Build All** converts all configured SQLite databases into DAFSA `.dat` files.

The generated files are saved to:

`Assets/StreamingAssets/GeneratedDafsaDatFiles/`

The output filename is generated from the language ID and the configured common filename.

For example:

```text
en_words.dat
de_words.dat
fr_words.dat
```

The conversion pipeline is:

```text
SQLite Database
      ↓
    Words
      ↓
     Trie
      ↓
   Minimize
      ↓
    DAFSA
      ↓
Binary .dat
```

---

# Test Load DAFSA

The converter provides a test operation for loading a generated DAFSA file.

The language used for the test is specified by:

`Test Language To Load Generated Dafsa File`

The test word is specified by:

`Test Word To Find In Dafsa With Metadata`

The generated DAFSA is loaded directly from the `.dat` file without accessing the original SQLite database.

---

# Requirements

* Unity 2021.3 or newer
* SQLite database
* Microsoft.Data.Sqlite

---

# Installation

The package can be installed directly from Git.

In Unity:

1. Open **Window → Package Manager**
2. Click **+**
3. Select **Add package from git URL...**
4. Enter the Git repository URL

Alternatively, add the package directly to `Packages/manifest.json`.

---

# How It Works

The package converts a word database into a minimized DAFSA structure.

```text
SQLite Database
      ↓
     Trie
      ↓
   Minimize
      ↓
    DAFSA
      ↓
 Binary .dat
      ↓
Runtime loading
      ↓
 Fast word lookup
```

The generated `.dat` files are stored in:

`Assets/StreamingAssets/GeneratedDafsaDatFiles/`

Example:

```text
Assets/
└── StreamingAssets/
    └── GeneratedDafsaDatFiles/
        ├── en_words.dat
        ├── de_words.dat
        ├── fr_words.dat
        └── ...
```

Each generated `.dat` file contains the DAFSA structure and serialized word metadata.

---

# Creating a Custom Converter

To use the package with your own database:

1. Create a metadata class derived from `WordMetadata`.
2. Create a converter derived from `DatabasesToDafsaConverter<TWordMetadata>`.
3. Implement `FillNeededListsOfColumnElements()`.
4. Implement `SetupMetadataForWord()`.
5. Configure the converter for your database structure.

Example metadata:

```csharp
public class MyWordMetadata : WordMetadata {
    public bool Good;
    public int Frequency;
    public float Difficulty;
}
```

Example converter:

```csharp
public class MyDafsaConverter : DatabasesToDafsaConverter<MyWordMetadata> {

    public override void FillNeededListsOfColumnElements(SimpleDatabase db) {
        // Load any additional database columns required by the metadata.
    }

    public override MyWordMetadata SetupMetadataForWord(
        SimpleDatabase db,
        int wordListIndex) {

        return new MyWordMetadata {
            Good = true,
            Frequency = 10,
            Difficulty = 0.5f
        };
    }
}
```

The database structure does not have to match the example database.

The example converter is only a reference implementation showing how to connect a SQLite database to the DAFSA conversion pipeline.

---

# Custom Metadata

The metadata system is designed to be extensible.

`WordMetadata` is the base type and can be extended with application-specific fields.

For example:

```csharp
public class MyWordMetadata : WordMetadata {
    public bool Good;
    public int Frequency;
    public float Difficulty;
    public string Category;
}
```

Another project may use completely different metadata:

```csharp
public class MyWordMetadata : WordMetadata {
    public int Score;
    public bool IsRare;
    public double Weight;
}
```

The generic converter works with the selected metadata type:

```csharp
DatabasesToDafsaConverter<MyWordMetadata>
```

The base converter does not need to know which fields exist in the metadata class.

---

# Metadata Loading

If metadata requires additional data from the SQLite database, the converter can load that data in:

```csharp
FillNeededListsOfColumnElements()
```

This method is intentionally **abstract** and must be implemented by every concrete converter.

For example:

```csharp
public override void FillNeededListsOfColumnElements(SimpleDatabase db) {
    _goods = db.GetColumn<int>(
        _goodTableName,
        _goodColumnName
    );
}
```

The metadata for each word is then created in:

```csharp
SetupMetadataForWord()
```

For example:

```csharp
public override ExampleWordMetadata SetupMetadataForWord(
    SimpleDatabase db,
    int wordListIndex) {

    return new ExampleWordMetadata {
        Good = _goods[wordListIndex] == 1
    };
}
```

This allows every converter to decide which database columns are required and how they should be mapped to metadata.

---

# Supported Metadata Types

The reflection serializer currently supports:

* `bool`
* `byte`
* `short`
* `int`
* `long`
* `float`
* `double`
* `string`
* `enum`

For example:

```csharp
public class MyWordMetadata : WordMetadata {
    public bool Good;
    public int Frequency;
    public float Difficulty;
    public string Category;
}
```

The reflection serializer serializes supported public instance fields.

Adding supported fields to the metadata class does not require changes to the DAFSA serializer.

---

# Loading a DAFSA at Runtime

The generated `.dat` file can be loaded without accessing the original SQLite database.

For example:

```csharp
string path = Path.Combine(
    Application.streamingAssetsPath,
    "GeneratedDafsaDatFiles",
    "en_words.dat"
);

var dafsa = DafsaRuntimeSerializer<ExampleWordMetadata>.Load(path);
```

The runtime DAFSA provides several search operations.

## Check Whether a Word Exists

```csharp
bool exists = dafsa.Contains("APPLE");
```

## Check Whether a Prefix Exists

```csharp
bool exists = dafsa.StartsWith("APP");
```

## Get Words Matching a Prefix

```csharp
var words = dafsa.GetWords("APP");
```

## Find a Word Index

```csharp
int index = dafsa.FindWordIndex("APPLE");
```

## Retrieve Word Metadata

```csharp
if (dafsa.TryGetMetadata("APPLE", out var metadata)) {
    Debug.Log($"Good: {metadata.Good}");
}
```

---

# DAFSA Runtime

The runtime representation contains:

* Nodes
* Edges
* Terminal word information
* Word metadata
* Word count

The runtime structure is designed for searching rather than editing.

The DAFSA is immutable after loading.

Supported operations include:

```csharp
dafsa.Contains("APPLE");
```

```csharp
dafsa.StartsWith("APP");
```

```csharp
dafsa.GetWords("APP");
```

```csharp
dafsa.FindWordIndex("APPLE");
```

```csharp
dafsa.TryGetMetadata(
    "APPLE",
    out var metadata
);
```

The runtime does not require the original SQLite database.

---

# Binary File Format

The generated `.dat` file contains:

* File magic
* File format version
* Word count
* Node count
* Edge count
* DAFSA nodes
* DAFSA edges
* Word metadata

The file uses a version number to detect incompatible binary formats.

The current file format version is:

`2`

If the binary file format changes in a future version, the version number should be increased to prevent incompatible files from being loaded incorrectly.

---

# Language Support

Language IDs are used for generated database names.

Supported language IDs include:

* `en`
* `de`
* `es`
* `fr`
* `it`
* `ja`
* `ko`
* `nl`
* `pt`
* `ru`
* `tr`

For example:

```text
en_words.dat
de_words.dat
fr_words.dat
```

Language conversion is handled by `LanguageUtils`.

Convert `SystemLanguage` to a language ID:

```csharp
string id = LanguageUtils.LanguageIdStringForType(
    SystemLanguage.English
);
```

Result:

```text
en
```

You can also convert a language ID back to `SystemLanguage`:

```csharp
SystemLanguage language =
    LanguageUtils.LanguageForIdString("en");
```

---

# Runtime Integration

The package can be used directly through `DafsaRuntime<TWordMetadata>` or through a custom application-level provider.

For example, a project can select between:

```text
SQLite database provider
        or
DAFSA provider
```

This makes it possible to use SQLite during development and DAFSA files in the final build.

A DAFSA-based runtime does not require the original word database to remain loaded in memory.

---

# Example Project Structure

A project using the package may look like this:

```text
Packages/
└── com.r.sqldafsa/

Assets/
├── Scripts/
│   └── WordDatabase/
│       ├── MyWordMetadata.cs
│       └── MyDafsaConverter.cs
│
└── StreamingAssets/
    └── GeneratedDafsaDatFiles/
        ├── en_words.dat
        ├── de_words.dat
        └── fr_words.dat
```

The package itself contains the converter/runtime code and example assets.

Project-specific converter scripts and generated DAFSA files belong to the Unity project.

The SQLite database is used during the conversion process.

The final game can use the generated `.dat` files without SQLite.

---

# Why DAFSA?

A regular Trie stores separate nodes for many repeated suffix structures.

DAFSA minimizes equivalent structures and can significantly reduce the number of nodes.

For a large word dictionary this can provide:

* Lower memory usage
* Smaller binary files
* Fast lookup
* Fast prefix traversal

The runtime representation uses compact arrays of nodes and edges instead of managed dictionaries.

---

# Runtime Search

Word lookup is performed directly against the DAFSA.

For nodes with a small number of outgoing edges, linear search is used.

For nodes with more outgoing edges, binary search is used.

Edges are stored in sorted order by character.

This provides efficient lookup while keeping the runtime representation compact.

---

# Important Notes

The generated `.dat` files should be regenerated when:

* The source database changes
* Words are added or removed
* Metadata values change
* The metadata class structure changes
* The binary file format changes

Do not manually edit `.dat` files.

The SQLite database is not required at runtime when using generated DAFSA files.

Database paths are stored in the converter configuration. If a project is moved to another computer, the database paths may need to be configured again.

---

# Generated Files

Generated DAFSA files are not part of the package itself.

They are generated for the target Unity project and stored in:

`Assets/StreamingAssets/GeneratedDafsaDatFiles/`

For example:

```text
en_words.dat
de_words.dat
fr_words.dat
```

These files should be included in the final Unity build through the `StreamingAssets` pipeline.

---

# License

MIT License.

See `LICENSE` for details.

