using System;
using System.Collections.Generic;
using System.IO;

namespace WordTrie {
    public static class TrieSerializer<TWordMetadata> where TWordMetadata : WordMetadata, new() {
        public static void Save(Trie<TWordMetadata> trie, string path) {
            if (trie == null)
                throw new ArgumentNullException(nameof(trie));

            var nodes = new List<DafsaTrieNode>();
            var ids = new Dictionary<DafsaTrieNode, int>();

            CollectNodes(trie.Root, nodes, ids);

            using var stream = File.Create(path);
            using var writer = new BinaryWriter(stream);

            writer.Write(nodes.Count);

            foreach (var node in nodes) {
                writer.Write(node.IsTerminal);

                writer.Write(node.Children.Count);

                foreach (var pair in node.Children) {
                    writer.Write(pair.Key);
                    writer.Write(ids[pair.Value]);
                }
            }
        }


        public static Trie<TWordMetadata> Load(string path) {
            using var stream = File.OpenRead(path);
            using var reader = new BinaryReader(stream);

            int nodeCount = reader.ReadInt32();

            var nodes = new DafsaTrieNode[nodeCount];

            for (int i = 0; i < nodeCount; i++)
                nodes[i] = new DafsaTrieNode();


            for (int i = 0; i < nodeCount; i++) {
                var node = nodes[i];

                node.IsTerminal = reader.ReadBoolean();

                int childrenCount = reader.ReadInt32();

                for (int c = 0; c < childrenCount; c++) {
                    char symbol = reader.ReadChar();
                    int childId = reader.ReadInt32();

                    node.Children.Add(symbol, nodes[childId]);
                }
            }

            var trie = new Trie<TWordMetadata>();

            // // восстанавливаем корень
            // foreach (var pair in trie.Root.Children) {
            //     // пусто, нужен только доступ к Root
            // }

            CopyRoot(nodes[0], trie.Root);

            return trie;
        }


        private static void CollectNodes(DafsaTrieNode node, List<DafsaTrieNode> nodes, Dictionary<DafsaTrieNode, int> ids) {
            if (ids.ContainsKey(node))
                return;

            int id = nodes.Count;

            ids[node] = id;
            nodes.Add(node);

            foreach (var child in node.Children.Values)
                CollectNodes(child, nodes, ids);
        }


        private static void CopyRoot(DafsaTrieNode source, DafsaTrieNode target) {
            target.IsTerminal = source.IsTerminal;

            foreach (var pair in source.Children)
                target.Children.Add(pair.Key, pair.Value);
        }
    }
}