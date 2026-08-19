SQL → DAFSA

Unity package for converting SQLite word databases into compact DAFSA binary files.

Quick Start
Install the package from Git.
In Unity, open DAFSA → Open Converter.
Configure the converter scene.
Run the conversion.

Find the generated .dat files in:

Assets/StreamingAssets/GeneratedDafsaDatFiles/

Example Scene

After installing the package, open the converter scene first.

The example scene demonstrates the complete SQLite → Trie → DAFSA conversion workflow and provides a reference implementation that you can adapt to your own project.

In the example scene, you will find the following GameObject:

DatabasesToDafsaConverter

The GameObject has the following component attached:

ExampleDatabasesToDafsaConverter

Select DatabasesToDafsaConverter in the Hierarchy to inspect the converter configuration and see how the example database is connected to the DAFSA conversion pipeline.

You can study ExampleDatabasesToDafsaConverter to understand how the converter is configured and how the database data is passed to the DAFSA conversion pipeline.

To use the package with your own database, create a converter similar to ExampleDatabasesToDafsaConverter and adapt it to your database structure, table names, column names, and metadata.

The example converter is not required to have the same database structure as your project. It is provided only as a reference implementation showing how to connect your SQLite database to the DAFSA conversion pipeline.

The example scene is intended to be the first place to look when getting started with the package.

Converter UI

The converter scene provides two buttons for working with DAFSA files.

Convert All Databases To DAFSA Dat Files

This button converts all configured SQLite word databases into DAFSA .dat files.

The generated files are saved to:

Assets/StreamingAssets/GeneratedDafsaDatFiles/

Load Dafsa Dat File

This button loads a generated DAFSA .dat file for testing.

The language used to select the .dat file is specified in the converter's Inspector.

For example, if English is selected, the converter will load:

Assets/StreamingAssets/GeneratedDafsaDatFiles/en_words.dat

This allows you to test the generated DAFSA file directly from the example scene without accessing the original SQLite database.

Requirements
Unity 2021.3 or newer
SQLite database
Microsoft.Data.Sqlite
Installation

The package can be installed directly from Git.

In Unity:

Open Window → Package Manager
Click +
Select Add package from git URL...
Enter the Git repository URL.

Alternatively, add the package to Packages/manifest.json.

How It Works

The package converts a word database into a compact DAFSA structure:

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

The generated DAFSA files are stored in:

Assets/StreamingAssets/GeneratedDafsaDatFiles/

For example:

Assets/
└── StreamingAssets/
    └── GeneratedDafsaDatFiles/
        ├── en_words.dat
        ├── de_words.dat
        ├── fr_words.dat
        └── ...

The generated .dat files contain the DAFSA data and serialized word metadata in a single file.

Basic Usage

Create a metadata class by inheriting from WordMetadata.

public class MyWordMetadata : WordMetadata
{
    public bool Good;
    public int Frequency;
}

You can add any supported fields to your metadata class.

For example:

public class MyWordMetadata : WordMetadata
{
    public bool Good;
    public int Frequency;
    public float Difficulty;
}
Custom Metadata

The metadata system is designed to be extensible.

You do not need to modify the DAFSA serializer when adding new metadata fields.

For example:

public class ExampleWordMetadata : WordMetadata
{
    public bool Good;
    public int SomeData;
}

Then use the metadata type in your converter:

public class ExampleDatabasesToDafsaConverter
    : DatabasesToDafsaConverter<ExampleWordMetadata>
{
    [SerializeField]
    protected string _goodTableName = default;


    [SerializeField]
    protected string _goodColumnName = default;


    private List<int> _goods = new();


    public override void FillNeededListsOfColumnElements(
        SimpleDatabase db)
    {
        _goods = db.GetColumn<int>(
            _goodTableName,
            _goodColumnName
        );
    }


    public override ExampleWordMetadata SetupMetadataForWord(
        SimpleDatabase db,
        int wordListIndex)
    {
        return new ExampleWordMetadata
        {
            Good = _goods[wordListIndex] == 1,
            SomeData = wordListIndex
        };
    }
}
Loading a DAFSA at Runtime

The generated .dat file can be loaded without accessing the original SQLite database.

The generated files are loaded from:

StreamingAssets/GeneratedDafsaDatFiles/

For example:

string path = Path.Combine(
    Application.streamingAssetsPath,
    "GeneratedDafsaDatFiles",
    "en_words.dat"
);


var dafsa =
    DafsaRuntimeSerializer<ExampleWordMetadata>.Load(path);

You can check whether a word exists:

bool exists = dafsa.Contains("APPLE");

You can check whether a prefix exists:

bool exists = dafsa.StartsWith("APP");

You can get all words matching a prefix:

var words = dafsa.GetWords("APP");

You can get the index of a word:

int index = dafsa.FindWordIndex("APPLE");

You can retrieve metadata:

if (dafsa.TryGetMetadata(
        "APPLE",
        out var metadata))
{
    Debug.Log(
        $"Good: {metadata.Good}"
    );


    Debug.Log(
        $"SomeData: {metadata.SomeData}"
    );
}
Word Metadata

Metadata belongs to words and is stored in the same .dat file as the DAFSA.

For example:

public class MyWordMetadata : WordMetadata
{
    public bool Good;
}

The metadata class can later be extended:

public class MyWordMetadata : WordMetadata
{
    public bool Good;
    public int Frequency;
    public float Difficulty;
    public string Category;
}

The serializer uses reflection to serialize the public instance fields.

This means that adding supported fields to the metadata class does not require changes to the serializer.

Supported Metadata Types

The reflection serializer currently supports:

bool
byte
short
int
long
float
double
string
enum

Example:

public class MyWordMetadata : WordMetadata
{
    public bool Good;
    public int Frequency;
    public float Difficulty;
    public string Category;
}
DAFSA Runtime

The runtime representation contains:

Nodes
Edges
Terminal word information
Word metadata
Word count

The runtime structure is optimized for searching rather than editing.

Supported operations include:

dafsa.Contains("APPLE");


dafsa.StartsWith("APP");


dafsa.GetWords("APP");


dafsa.FindWordIndex("APPLE");


dafsa.TryGetMetadata(
    "APPLE",
    out var metadata
);
Binary File Format

The generated .dat file contains:

File magic
File format version
Word count
Node count
Edge count
DAFSA nodes
DAFSA edges
Word metadata

The file uses a version number to detect incompatible file formats.

The current file format version is:

1

If the binary file format changes in a future version, the version number should be increased to prevent incompatible files from being loaded incorrectly.

Language Support

Language IDs can be used for generated database names.

Supported language IDs include:

en
de
es
fr
it
ja
ko
nl
pt
ru
tr

For example:

en_words.dat
de_words.dat
fr_words.dat

Language conversion is handled by LanguageUtils.

string id =
    LanguageUtils.LanguageIdStringForType(
        SystemLanguage.English
    );

Result:

en

You can also convert a language ID back to SystemLanguage:

SystemLanguage language =
    LanguageUtils.LanguageForIdString("en");
Example Project Structure

A project using the package may look like this:

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

The SQLite database is used during the conversion process.

The final game can use the generated .dat files without SQLite.

Why DAFSA?

A regular Trie stores separate nodes for many repeated suffixes.

DAFSA minimizes equivalent suffix structures and can significantly reduce the number of nodes.

For a large word dictionary this can provide:

Lower memory usage
Smaller binary files
Fast lookup
Fast prefix traversal

The runtime structure uses compact arrays of nodes and edges instead of managed dictionaries.

Runtime Search

Word lookup is performed directly against the DAFSA.

For nodes with a small number of outgoing edges, linear search is used.

For nodes with more edges, binary search is used.

Edges are stored in sorted order by character.

This allows efficient lookup while keeping the runtime representation compact.

Important Notes

The generated .dat files should be regenerated when:

The source database changes
Words are added or removed
Metadata values change
The metadata class structure changes
The binary file format changes

Do not manually edit .dat files.

Generated Files

Generated DAFSA files are not part of the package itself.

They are generated for the target project and stored in:

Assets/StreamingAssets/GeneratedDafsaDatFiles/

For example:

en_words.dat
de_words.dat
fr_words.dat
License

MIT License.

See LICENSE for details.