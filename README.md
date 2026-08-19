SQL → DAFSA

Unity package for converting SQLite word databases into compact DAFSA binary files.

The package provides:

SQLite database → Trie → DAFSA conversion
Compact binary .dat files
Fast word lookup with Contains
Prefix lookup with StartsWith
Extensible word metadata
Editor tools for converting databases
Example scene and databases
Installation

Install the package through Unity Package Manager using the Git URL:

https://github.com/zr1k1/WordDatabasesToDafsaConverter.git

In Unity:

Open Window → Package Manager
Click +
Select Add package from git URL...
Enter the repository URL
Click Add
Example

The package includes an example scene demonstrating the SQLite → DAFSA conversion workflow.

After installing the package:

Open Window → Package Manager
Select SQL → DAFSA
Open the Samples section
Import SQL → DAFSA Example

The example contains:

Scenes/
    Databases_To_DAFSA_Converter_Scene.unity


DatabasesToConvert/
    Example SQLite databases

After importing the example, the scene can also be opened from:

DAFSA → Open Converter

The menu opens the imported copy of the scene from Assets/Samples, so the scene remains editable.

Database → DAFSA

The converter reads words and their metadata from a SQLite database, builds a Trie, minimizes it into a DAFSA, and saves the result as a compact binary file.

The general workflow is:

SQLite Database
      ↓
     Trie
      ↓
    DAFSA
      ↓
   Binary .dat

The resulting DAFSA can then be loaded at runtime without keeping the original SQLite database.

Runtime

The generated binary file contains the runtime DAFSA representation and metadata required for word lookup.

The runtime API supports operations such as:

dafsa.Contains("word");
dafsa.StartsWith("wor");

Contains checks whether a complete word exists in the DAFSA.

StartsWith checks whether the DAFSA contains words beginning with the specified prefix.

Metadata

Words can have associated metadata.

The default example metadata contains:

public class WordMetadata
{
    public bool Good;
}

The metadata system is designed to be extensible, so additional fields can be added when needed.

For example:

public class WordMetadata
{
    public bool Good;
    public int Frequency;
    public string Category;
}
Generated Files

The converter generates compact binary .dat files containing:

DAFSA nodes
DAFSA edges
Word metadata
File header and version information

The generated files can be stored in the project's StreamingAssets folder and loaded at runtime.

Requirements
Unity 2021.3 or newer
SQLite database containing the word data
Package Structure
SQLDAFSA/
├── Editor/
├── Runtime/
├── Plugins/
├── Samples~/
│   └── Example/
│       ├── Scenes/
│       ├── DatabasesToConvert/
│       └── README.md
├── package.json
├── README.md
└── LICENSE

Runtime contains the runtime DAFSA implementation.

Editor contains database conversion and Unity Editor tools.

Plugins contains the SQLite dependencies required by the package.

Samples~ contains the example scene and example databases. Samples are imported separately through Unity Package Manager.

License

MIT License.
