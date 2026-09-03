using System.Collections.Generic;

namespace Dafsa {
    public class DafsaTrieNode {
        public bool IsTerminal;

        public readonly Dictionary<char, DafsaTrieNode> Children = new();

        public string Signature;
    }
}