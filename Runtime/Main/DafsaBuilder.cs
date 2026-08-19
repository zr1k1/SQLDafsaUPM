using System;
using System.Linq;
using System.Text;
using UnityEngine;

namespace WordTrie {

    using System.Collections.Generic;

    public static class DafsaBuilder<TWordMetadata> where TWordMetadata : WordMetadata, new() {

        public static void BuildRuntime(DafsaTrieNode root, List<DafsaRuntime<TWordMetadata>.Node> nodes, List<DafsaRuntime<TWordMetadata>.Edge> edges, Dictionary<DafsaTrieNode, int> ids) {
            int rootId = AddNode(root, nodes, edges, ids);

            if (rootId != 0) {
                throw new InvalidOperationException(
                    $"Root node must have id 0, got {rootId}"
                );
            }
        }

        private static int AddNode(DafsaTrieNode node, List<DafsaRuntime<TWordMetadata>.Node> nodes, List<DafsaRuntime<TWordMetadata>.Edge> edges, Dictionary<DafsaTrieNode, int> ids) {
            if (ids.TryGetValue(node, out int existingId))
                return existingId;

            int nodeId = nodes.Count;

            ids[node] = nodeId;

            nodes.Add(default);

            var children = node.Children.OrderBy(x => x.Key).ToList();

            int firstEdge = edges.Count;

            foreach (var child in children) {
                edges.Add(new DafsaRuntime<TWordMetadata>.Edge {
                    Symbol = child.Key,
                    Target = -1
                });
            }

            for (int i = 0; i < children.Count; i++) {
                int targetId = AddNode(children[i].Value, nodes, edges, ids);

                int edgeIndex = firstEdge + i;

                var edge = edges[edgeIndex];

                edge.Target = targetId;

                edges[edgeIndex] = edge;
            }

            if (children.Count > byte.MaxValue) {
                throw new InvalidOperationException(
                    $"Node has too many edges: {children.Count}"
                );
            }

            byte flags = 0;

            if (node.IsTerminal)
                flags |= 1;

            nodes[nodeId] = new DafsaRuntime<TWordMetadata>.Node {
                Flags = flags,
                FirstEdge = firstEdge,
                EdgeCount = (byte)children.Count
            };

            return nodeId;
        }

        public static void Minimize(Trie<TWordMetadata> trie) {
            if (trie == null)
                throw new ArgumentNullException(nameof(trie));

            // До минимизации
            var beforeVisited = new HashSet<DafsaTrieNode>();

            int beforeNodes = 0;
            int beforeTerminalStates = 0;

            CountStats(trie.Root, beforeVisited, ref beforeNodes, ref beforeTerminalStates);

            Debug.Log(
                $"Before Minimize: " +
                $"nodes={beforeNodes}, " +
                $"terminalStates={beforeTerminalStates}"
            );

            var registry = new Dictionary<string, DafsaTrieNode>();

            var root = MinimizeNode(trie.Root, registry);

            Debug.Log(
                $"Registry size: {registry.Count}"
            );

            trie.ReplaceRoot(root);

            var afterVisited = new HashSet<DafsaTrieNode>();

            int afterNodes = 0;
            int afterTerminalStates = 0;

            CountStats(trie.Root, afterVisited, ref afterNodes, ref afterTerminalStates);

            Debug.Log(
                $"After Minimize: " +
                $"nodes={afterNodes}, " +
                $"terminalStates={afterTerminalStates}"
            );
        }


        private static DafsaTrieNode MinimizeNode(DafsaTrieNode node, Dictionary<string, DafsaTrieNode> registry) {

            foreach (var key in node.Children.Keys.ToList()) {
                node.Children[key] = MinimizeNode(node.Children[key], registry);
            }

            string signature = CreateSignature(node);

            if (registry.TryGetValue(signature, out var existing)) {
                return existing;
            }

            node.Signature = signature;

            registry.Add(signature, node);

            return node;
        }


        private static string CreateSignature(DafsaTrieNode node) {
            var sb = new StringBuilder();

            sb.Append(node.IsTerminal ? '1' : '0');
            sb.Append('#');

            foreach (var child in node.Children.OrderBy(x => x.Key)) {
                sb.Append((int)child.Key);
                sb.Append('=');
                sb.Append(child.Value.Signature);
                sb.Append(';');
            }

            return sb.ToString();
        }


        public static int CountNodes(DafsaTrieNode node, HashSet<DafsaTrieNode> visited) {
            if (!visited.Add(node))
                return 0;

            int count = 1;

            foreach (var child in node.Children.Values) {
                count += CountNodes(child, visited);
            }

            return count;
        }

        public static void CountStats(DafsaTrieNode node, HashSet<DafsaTrieNode> visited, ref int nodes, ref int words) {
            if (!visited.Add(node))
                return;

            nodes++;

            if (node.IsTerminal)
                words++;

            foreach (var child in node.Children.Values) {
                CountStats(child, visited, ref nodes, ref words);
            }
        }
    }
}