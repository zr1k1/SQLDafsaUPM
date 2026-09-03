using Dafsa;


/// <summary>
/// Example metadata stored for each word in the DAFSA.
///
/// Inherit from WordMetadata and add any fields that should be
/// stored for each word.
///
/// The ReflectionBinarySerializer automatically serializes the
/// public instance fields of this class.
///
/// To create custom metadata, create another class derived from
/// WordMetadata and add the required fields.
/// </summary>
public class ExampleWordMetadata : WordMetadata {
    // Example metadata field.
    public bool Good;

    // Example of an additional custom metadata field.
    // public int SomeData;
}
