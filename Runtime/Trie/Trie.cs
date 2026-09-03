using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dafsa {
    public readonly struct WordEntry<TWordMetadata> {
        public readonly string Word;
        public readonly TWordMetadata Metadata;

        public WordEntry(string word, TWordMetadata metadata) {
            Word = word;
            Metadata = metadata;
        }
    }

    public sealed class Trie<TWordMetadata> {
        private DafsaTrieNode _root;

        public DafsaTrieNode Root => _root;

        public Trie() {
            _root = new DafsaTrieNode();
        }

        public Trie(IEnumerable<WordEntry<TWordMetadata>> words) {
            _root = new DafsaTrieNode();

            int count = 0;

            foreach (var entry in words) {
                Add(entry);

                count++;

                if (count % 10000 == 0)
                    Debug.Log($"Added {count}");
            }

            Debug.Log($"Trie added: {count}");
        }

        /// <summary>
        /// Adds a word to the Trie.
        /// </summary>
        public void Add(WordEntry<TWordMetadata> entry) {
            var node = Root;

            foreach (char c in entry.Word) {
                if (!node.Children.TryGetValue(c, out var next)) {
                    next = new DafsaTrieNode();
                    node.Children.Add(c, next);
                }

                node = next;
            }

            node.IsTerminal = true;
        }

        /// <summary>
        /// Checks whether the exact word exists in the Trie.
        /// </summary>
        public bool Contains(string word) {
            if (string.IsNullOrEmpty(word))
                return false;

            var node = FindNode(word);

            return node != null && node.IsTerminal;
        }

        /// <summary>
        /// Checks whether there are any words with the specified prefix.
        /// </summary>
        public bool StartsWith(string prefix) {
            if (string.IsNullOrEmpty(prefix))
                return false;

            return FindNode(prefix) != null;
        }

        private DafsaTrieNode FindNode(string text) {
            var node = _root;

            foreach (char c in text) {
                if (!node.Children.TryGetValue(c, out node))
                    return null;
            }

            return node;
        }

        internal void ReplaceRoot(DafsaTrieNode root) {
            if (root == null)
                throw new ArgumentNullException(nameof(root));

            _root = root;
        }
    }
}