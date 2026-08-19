using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

public static class ReflectionBinarySerializer {
    private enum FieldType : byte {
        Bool = 1,
        Byte = 2,
        Short = 3,
        Int = 4,
        Long = 5,
        Float = 6,
        Double = 7,
        String = 8,
        Enum = 9
    }

    private struct FileField {
        public string Name;
        public FieldType Type;
    }

    private struct FieldMapping {
        public FieldInfo Field;
        public bool Exists;
    }

    // =========================================================
    // WRITE ARRAY
    // =========================================================

    public static void WriteArray<T>(BinaryWriter writer, T[] values) {
        if (values == null) {
            writer.Write(0);
            return;
        }

        writer.Write(values.Length);

        var fields = GetFields(typeof(T));

        writer.Write(fields.Length);

        foreach (var field in fields) {
            writer.Write(field.Name);
            writer.Write((byte)GetFieldType(field.FieldType));
        }

        foreach (var value in values) {
            WriteObject(writer, value, fields);
        }
    }

    // =========================================================
    // READ ARRAY
    // =========================================================

    public static T[] ReadArray<T>(BinaryReader reader) where T : new() {
        int count = reader.ReadInt32();

        if (count == 0)
            return Array.Empty<T>();

        int fileFieldCount = reader.ReadInt32();

        if (fileFieldCount < 0) {
            throw new InvalidDataException(
                $"Invalid metadata field count: {fileFieldCount}"
            );
        }

        var fileFields = new FileField[fileFieldCount];

        for (int i = 0; i < fileFieldCount; i++) {
            fileFields[i] = new FileField {
                Name = reader.ReadString(),
                Type = (FieldType)reader.ReadByte()
            };
        }

        var currentFields = GetFields(typeof(T));

        var currentFieldMap = new Dictionary<string, FieldInfo>(
            StringComparer.Ordinal
        );

        foreach (var field in currentFields) {
            currentFieldMap[field.Name] = field;
        }

        var mappings = new FieldMapping[fileFieldCount];

        for (int i = 0; i < fileFieldCount; i++) {
            var fileField = fileFields[i];

            if (!currentFieldMap.TryGetValue(fileField.Name, out var currentField)) {
                mappings[i] = default;
                continue;
            }

            FieldType currentType = GetFieldType(currentField.FieldType);

            if (fileField.Type != currentType) {
                throw new InvalidDataException(
                    $"Metadata field type mismatch. " +
                    $"Field='{fileField.Name}', " +
                    $"File={fileField.Type}, " +
                    $"Current={currentType}"
                );
            }

            mappings[i] = new FieldMapping {
                Field = currentField,
                Exists = true
            };
        }

        var result = new T[count];

        for (int i = 0; i < count; i++) {
            var value = new T();

            for (int f = 0; f < fileFieldCount; f++) {
                var fileField = fileFields[f];
                var mapping = mappings[f];

                Type enumType = null;

                if (mapping.Exists && mapping.Field.FieldType.IsEnum) {
                    enumType = mapping.Field.FieldType;
                }

                object fieldValue = ReadValue(reader, fileField.Type, enumType);

                if (mapping.Exists) {
                    mapping.Field.SetValue(value, fieldValue);
                }
            }

            result[i] = value;
        }

        return result;
    }

    // =========================================================
    // GET FIELDS
    // =========================================================

    private static FieldInfo[] GetFields(Type type) {
        return type.GetFields(BindingFlags.Instance | BindingFlags.Public);
    }

    // =========================================================
    // WRITE OBJECT
    // =========================================================

    private static void WriteObject(BinaryWriter writer, object value, FieldInfo[] fields) {
        foreach (var field in fields) {
            object fieldValue = field.GetValue(value);

            WriteValue(writer, field.FieldType, fieldValue);
        }
    }

    // =========================================================
    // WRITE VALUE
    // =========================================================

    private static void WriteValue(BinaryWriter writer, Type type, object value) {
        if (type == typeof(bool)) {
            writer.Write(value != null && (bool)value);

            return;
        }

        if (type == typeof(byte)) {
            writer.Write(value != null ? (byte)value : (byte)0);

            return;
        }

        if (type == typeof(short)) {
            writer.Write(value != null ? (short)value : (short)0);

            return;
        }

        if (type == typeof(int)) {
            writer.Write(value != null ? (int)value : 0);

            return;
        }

        if (type == typeof(long)) {
            writer.Write(value != null ? (long)value : 0L);

            return;
        }

        if (type == typeof(float)) {
            writer.Write(value != null ? (float)value : 0f);

            return;
        }

        if (type == typeof(double)) {
            writer.Write(value != null ? (double)value : 0d);

            return;
        }

        if (type == typeof(string)) {
            writer.Write(value as string ?? string.Empty);

            return;
        }

        if (type.IsEnum) {
            writer.Write(value != null ? Convert.ToInt32(value) : 0);

            return;
        }

        throw new NotSupportedException(
            $"Unsupported metadata field type: {type}"
        );
    }

    // =========================================================
    // READ VALUE
    // =========================================================

    private static object ReadValue(BinaryReader reader, FieldType type, Type enumType) {
        switch (type) {
            case FieldType.Bool:
                return reader.ReadBoolean();

            case FieldType.Byte:
                return reader.ReadByte();

            case FieldType.Short:
                return reader.ReadInt16();

            case FieldType.Int:
                return reader.ReadInt32();

            case FieldType.Long:
                return reader.ReadInt64();

            case FieldType.Float:
                return reader.ReadSingle();

            case FieldType.Double:
                return reader.ReadDouble();

            case FieldType.String:
                return reader.ReadString();

            case FieldType.Enum: {
                    int enumValue = reader.ReadInt32();

                    if (enumType == null) {

                        return enumValue;
                    }

                    return Enum.ToObject(
                        enumType,
                        enumValue
                    );
                }

            default:
                throw new InvalidDataException(
                    $"Unknown metadata field type: {type}"
                );
        }
    }

    // =========================================================
    // FIELD TYPE
    // =========================================================

    private static FieldType GetFieldType(Type type) {
        if (type == typeof(bool))
            return FieldType.Bool;

        if (type == typeof(byte))
            return FieldType.Byte;

        if (type == typeof(short))
            return FieldType.Short;

        if (type == typeof(int))
            return FieldType.Int;

        if (type == typeof(long))
            return FieldType.Long;

        if (type == typeof(float))
            return FieldType.Float;

        if (type == typeof(double))
            return FieldType.Double;

        if (type == typeof(string))
            return FieldType.String;

        if (type.IsEnum)
            return FieldType.Enum;

        throw new NotSupportedException(
            $"Unsupported metadata field type: {type}"
        );
    }
}