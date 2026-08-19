using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Data.Sqlite;
using UnityEngine;

public sealed class SimpleDatabase : IDisposable {
    private SqliteConnection _connection;

    public bool IsOpen => _connection != null;

    public SimpleDatabase(string path) {
        Open(path);
    }

    public void Open(string path) {
        Close();

        if (string.IsNullOrEmpty(path))
            throw new ArgumentException(
                "Database path is empty.",
                nameof(path));

        string fullPath = Path.GetFullPath(path);
        string directory = Path.GetDirectoryName(fullPath);

        Debug.Log($"DB PATH: {path}");
        Debug.Log($"DB FULL PATH: {fullPath}");
        Debug.Log($"DB EXISTS: {File.Exists(fullPath)}");
        Debug.Log($"DIR: {directory}");
        Debug.Log($"DIR EXISTS: {Directory.Exists(directory)}");

        if (!File.Exists(fullPath))
            throw new FileNotFoundException(
                "Database file not found.",
                fullPath);

        var connectionString = new SqliteConnectionStringBuilder {
            DataSource = fullPath,
            Mode = SqliteOpenMode.ReadOnly
        };

        _connection = new SqliteConnection(connectionString.ToString());

        _connection.Open();
    }

    public List<T> GetColumn<T>(string tableName, string columnName) {
        if (!IsOpen)
            throw new InvalidOperationException(
                "Database is not open.");

        var result = new List<T>();

        using var command = _connection.CreateCommand();

        command.CommandText = $"SELECT [{columnName}] FROM [{tableName}]";

        using var reader = command.ExecuteReader();

        while (reader.Read()) {
            if (reader.IsDBNull(0)) {
                result.Add(default);
                continue;
            }

            object value = reader.GetValue(0);

            result.Add((T)Convert.ChangeType(value, typeof(T)));
        }

        return result;
    }

    public void Close() {
        if (_connection == null)
            return;

        _connection.Close();
        _connection.Dispose();
        _connection = null;
    }

    public void Dispose() {
        Close();
    }
}
