using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace WordTrie {
    public static partial class DafsaRuntimeSerializer<TWordMetadata> where TWordMetadata : WordMetadata, new() {
        private const int Magic = 0x44414653; // DAFS
        private const int Version = 1;

        public static void Save(
            List<DafsaRuntime<TWordMetadata>.Node> nodes,
            List<DafsaRuntime<TWordMetadata>.Edge> edges,
            string path,
            int wordCount,
            TWordMetadata[] metadata) {

            if (nodes == null)
                throw new ArgumentNullException(nameof(nodes));

            if (edges == null)
                throw new ArgumentNullException(nameof(edges));

            if (metadata == null)
                throw new ArgumentNullException(nameof(metadata));

            if (wordCount < 0)
                throw new ArgumentOutOfRangeException(nameof(wordCount));

            if (metadata.Length != wordCount) {
                throw new InvalidOperationException(
                    $"Metadata count mismatch. " +
                    $"wordCount={wordCount}, " +
                    $"metadata={metadata.Length}"
                );
            }

            if (nodes.Count == 0) {
                throw new InvalidOperationException(
                    "DAFSA is empty."
                );
            }

            Debug.Log(
                $"Runtime: " +
                $"nodes={nodes.Count}, " +
                $"edges={edges.Count}, " +
                $"words={wordCount}"
            );

            using var stream = File.Create(path);
            using var writer = new BinaryWriter(stream);

            // =================================================
            // HEADER
            // =================================================

            writer.Write(Magic);
            writer.Write(Version);

            writer.Write(wordCount);
            writer.Write(nodes.Count);
            writer.Write(edges.Count);

            Debug.Log(
                $"After header: {stream.Position} bytes"
            );

            // =================================================
            // NODES
            // =================================================

            foreach (var node in nodes) {
                writer.Write(node.Flags);
                writer.Write(node.FirstEdge);

                // ushort = 2 bytes
                writer.Write(node.EdgeCount);
            }

            Debug.Log(
                $"After nodes: {stream.Position} bytes"
            );

            // =================================================
            // EDGES
            // =================================================

            foreach (var edge in edges) {
                // char -> ushort = exactly 2 bytes
                writer.Write((ushort)edge.Symbol);

                writer.Write(edge.Target);
            }

            Debug.Log(
                $"After edges: {stream.Position} bytes"
            );

            // =================================================
            // METADATA
            // =================================================

            ReflectionBinarySerializer.WriteArray(
                writer,
                metadata
            );

            Debug.Log(
                $"After metadata: {stream.Position} bytes"
            );

            writer.Flush();

            Debug.Log(
                $"DAFSA FILE SIZE: {stream.Length} bytes"
            );
        }


        public static DafsaRuntime<TWordMetadata> Load(string path) {
            if (!File.Exists(path)) {
                throw new FileNotFoundException(
                    "DAFSA file not found.",
                    path
                );
            }

            using var stream = File.OpenRead(path);

            using var reader = new BinaryReader(stream);

            // =================================================
            // HEADER
            // =================================================

            int magic = reader.ReadInt32();

            if (magic != Magic) {
                throw new InvalidDataException(
                    "Invalid DAFSA file."
                );
            }

            int version = reader.ReadInt32();

            if (version != Version) {
                throw new InvalidDataException(
                    $"Unsupported DAFSA version: {version}"
                );
            }

            int wordCount = reader.ReadInt32();

            int nodeCount = reader.ReadInt32();

            int edgeCount = reader.ReadInt32();

            if (wordCount < 0) {
                throw new InvalidDataException(
                    $"Invalid word count: {wordCount}"
                );
            }

            if (nodeCount <= 0) {
                throw new InvalidDataException(
                    $"Invalid node count: {nodeCount}"
                );
            }

            if (edgeCount < 0) {
                throw new InvalidDataException(
                    $"Invalid edge count: {edgeCount}"
                );
            }

            Debug.Log(
                $"DAFSA header: " +
                $"words={wordCount}, " +
                $"nodes={nodeCount}, " +
                $"edges={edgeCount}"
            );

            // =================================================
            // NODES
            // =================================================

            var nodes = new DafsaRuntime<TWordMetadata>.Node[nodeCount];

            for (int i = 0; i < nodeCount; i++) {
                nodes[i] = new DafsaRuntime<TWordMetadata>.Node {
                    Flags = reader.ReadByte(),
                    FirstEdge = reader.ReadInt32(),
                    EdgeCount = reader.ReadUInt16()
                };
            }

            Debug.Log(
                $"After nodes: {stream.Position} bytes"
            );

            // =================================================
            // EDGES
            // =================================================

            var edges = new DafsaRuntime<TWordMetadata>.Edge[edgeCount];

            for (int i = 0; i < edgeCount; i++) {
                edges[i] =
                    new DafsaRuntime<TWordMetadata>.Edge {
                        Symbol = (char)reader.ReadUInt16(),
                        Target = reader.ReadInt32()
                    };
            }

            Debug.Log(
                $"After edges: {stream.Position} bytes"
            );

            // =================================================
            // METADATA
            // =================================================

            TWordMetadata[] metadata =
                ReflectionBinarySerializer
                    .ReadArray<TWordMetadata>(
                        reader
                    );

            if (metadata == null) {
                throw new InvalidDataException(
                    "Metadata is null."
                );
            }

            if (metadata.Length != wordCount) {
                throw new InvalidDataException(
                    $"Metadata count mismatch. " +
                    $"Header={wordCount}, " +
                    $"Metadata={metadata.Length}"
                );
            }

            Debug.Log(
                $"After metadata: {stream.Position} bytes"
            );

            // =================================================
            // VALIDATION
            // =================================================

            if (stream.Position != stream.Length) {
                throw new InvalidDataException(
                    $"Unexpected data at end of file. " +
                    $"Position={stream.Position}, " +
                    $"Length={stream.Length}"
                );
            }

            Debug.Log(
                $"Loaded: " +
                $"nodes={nodes.Length}, " +
                $"edges={edges.Length}, " +
                $"metadata={metadata.Length}"
            );

            Debug.Log(
                $"Root: " +
                $"FirstEdge={nodes[0].FirstEdge}, " +
                $"EdgeCount={nodes[0].EdgeCount}, " +
                $"Flags={nodes[0].Flags}"
            );

            return new DafsaRuntime<TWordMetadata>(nodes, edges, wordCount, metadata);
        }


        // =====================================================
        // BUILD RUNTIME
        // =====================================================

        private static void BuildRuntime(DafsaTrieNode root, List<DafsaRuntime<TWordMetadata>.Node> nodes, List<DafsaRuntime<TWordMetadata>.Edge> edges, Dictionary<DafsaTrieNode, int> ids) {
            int rootId =
                AddNode(root, nodes, edges, ids);

            if (rootId != 0) {
                throw new InvalidOperationException(
                    $"Root node must have id 0, got {rootId}"
                );
            }
        }

        private static int AddNode(DafsaTrieNode node, List<DafsaRuntime<TWordMetadata>.Node> nodes, List<DafsaRuntime<TWordMetadata>.Edge> edges, Dictionary<DafsaTrieNode, int> ids) {
            if (ids.TryGetValue(node, out int existingId)) {
                return existingId;
            }

            int nodeId = nodes.Count;

            ids[node] = nodeId;

            nodes.Add(default);

            // =================================================
            // CHILDREN
            // =================================================

            var children = node.Children.OrderBy(x => x.Key).ToArray();

            if (children.Length >
                ushort.MaxValue) {
                throw new InvalidOperationException(
                    $"Node has too many edges: " +
                    $"{children.Length}"
                );
            }

            int firstEdge = edges.Count;

            // =================================================
            // RESERVE EDGES
            // =================================================

            for (int i = 0; i < children.Length; i++) {
                edges.Add(
                    new DafsaRuntime<TWordMetadata>.Edge {
                        Symbol = children[i].Key,
                        Target = -1
                    }
                );
            }

            // =================================================
            // BUILD CHILDREN
            // =================================================

            for (int i = 0; i < children.Length; i++) {
                int targetId = AddNode(children[i].Value, nodes, edges, ids);

                int edgeIndex = firstEdge + i;

                var edge = edges[edgeIndex];

                edge.Target = targetId;

                edges[edgeIndex] = edge;
            }

            // =================================================
            // NODE
            // =================================================

            byte flags = 0;

            if (node.IsTerminal)
                flags |= 1;

            nodes[nodeId] = new DafsaRuntime<TWordMetadata>.Node {
                Flags = flags,
                FirstEdge = firstEdge,
                EdgeCount = (ushort)children.Length
            };

            return nodeId;
        }
    }
}