using System.Collections.Generic;

namespace MisterGames.Common.Trees {

    public struct TreeEntry<T> {
        
        public T data;
        public int level;
        public bool isLeaf;
        
        public List<TreeEntry<T>> children;

        public override string ToString() {
            return $"[{level}: {data}, isLeaf {isLeaf}]";
        }
        
    }

}