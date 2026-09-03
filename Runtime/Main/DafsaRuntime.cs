namespace Dafsa {
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using System.Text;

    public sealed class DafsaRuntime<TWordMetadata> where TWordMetadata : WordMetadata, new() {
        private readonly Node[] _nodes;
        private readonly Edge[] _edges;
        private readonly TWordMetadata[] _metadata;

        // Не сохраняем в файл.
        // Вычисляем после Load().
        private readonly int[] _subtreeWordCounts;

        public int WordCount { get; }

        public int NodeCount => _nodes.Length;
        public int EdgeCount => _edges.Length;

        public struct Node {
            public byte Flags;
            public int FirstEdge;
            public ushort EdgeCount;

            public bool IsTerminal => (Flags & 1) != 0;
        }

        public struct Edge {
            public char Symbol;
            public int Target;
        }

        internal DafsaRuntime(Node[] nodes, Edge[] edges, int wordCount, TWordMetadata[] metadata) {
            _nodes = nodes;
            _edges = edges;
            _metadata = metadata;

            WordCount = wordCount;

            if (metadata.Length != wordCount) {
                throw new InvalidOperationException(
                    $"Word count mismatch. " +
                    $"wordCount={wordCount}, " +
                    $"metadata={metadata.Length}"
                );
            }

            _subtreeWordCounts = BuildSubtreeWordCounts();

            Debug.Log(
                $"DAFSA words: {_subtreeWordCounts[0]}, " +
                $"expected: {wordCount}"
            );
        }

        private int[] BuildSubtreeWordCounts() {
            var counts = new int[_nodes.Length];

            Array.Fill(counts, -1);

            CountWords(0, counts);

            return counts;
        }

        private int CountWords(int nodeId, int[] counts) {
            if (counts[nodeId] >= 0)
                return counts[nodeId];

            var node = _nodes[nodeId];

            int count = node.IsTerminal ? 1 : 0;

            int start = node.FirstEdge;

            for (int i = 0; i < node.EdgeCount; i++) {
                var edge = _edges[start + i];

                count += CountWords(edge.Target, counts);
            }

            counts[nodeId] = count;

            return count;
        }

        public bool Contains(string word) {
            if (string.IsNullOrEmpty(word))
                return false;

            int nodeId = FindNode(word);

            return nodeId >= 0 && _nodes[nodeId].IsTerminal;
        }

        public bool TryGetMetadata(string word, out TWordMetadata metadata) {
            int index = FindWordIndex(word);

            if (index < 0) {
                metadata = null;
                return false;
            }

            metadata = _metadata[index];

            return true;
        }

        public int FindWordIndex(string word) {
            if (string.IsNullOrEmpty(word))
                return -1;

            int nodeId = 0;
            int wordIndex = 0;

            for (int i = 0; i < word.Length; i++) {
                Node node = _nodes[nodeId];

                if (node.IsTerminal)
                    wordIndex++;

                int edgeStart = node.FirstEdge;

                for (int e = 0; e < node.EdgeCount; e++) {
                    Edge edge = _edges[edgeStart + e];

                    if (edge.Symbol >= word[i])
                        break;

                    wordIndex += _subtreeWordCounts[edge.Target];
                }

                int nextNode = FindChild(nodeId, word[i]);

                if (nextNode < 0)
                    return -1;

                nodeId = nextNode;
            }

            return _nodes[nodeId].IsTerminal ? wordIndex : -1;
        }

        public bool StartsWith(string prefix) {
            if (string.IsNullOrEmpty(prefix))
                return false;

            return FindNode(prefix) >= 0;
        }

        private int FindNode(string text) {
            if (string.IsNullOrEmpty(text))
                return 0;

            int node = 0;

            for (int i = 0; i < text.Length; i++) {
                node = FindChild(node, text[i]);

                if (node < 0)
                    return -1;
            }

            return node;
        }

        private int FindChild(int nodeId, char symbol) {
            ref readonly Node node = ref _nodes[nodeId];

            int start = node.FirstEdge;
            int count = node.EdgeCount;

            if (count <= 8) {
                int end = start + count;

                for (int i = start; i < end; i++) {
                    if (_edges[i].Symbol == symbol)
                        return _edges[i].Target;
                }

                return -1;
            }

            int left = start;
            int right = start + count - 1;

            while (left <= right) {
                int mid = left + ((right - left) >> 1);

                char current = _edges[mid].Symbol;

                if (current == symbol)
                    return _edges[mid].Target;

                if (current < symbol)
                    left = mid + 1;
                else
                    right = mid - 1;
            }

            return -1;
        }

        public List<string> GetWords(string prefix, int limit = int.MaxValue) {
            var result = new List<string>();

            int node = FindNode(prefix);

            if (node < 0)
                return result;

            var sb = new StringBuilder(prefix);

            CollectWords(node, sb, result, limit);

            return result;
        }

        private void CollectWords(int nodeId, StringBuilder sb, List<string> result, int limit) {
            if (result.Count >= limit)
                return;

            Node node = _nodes[nodeId];

            if (node.IsTerminal)
                result.Add(sb.ToString());

            int start = node.FirstEdge;
            int end = start + node.EdgeCount;

            for (int i = start; i < end; i++) {
                Edge edge = _edges[i];

                sb.Append(edge.Symbol);

                CollectWords(edge.Target, sb, result, limit);

                sb.Length--;
            }
        }
    }
}
