using System.Collections.Generic;

namespace MisterGames.Common.Editor.Tree {

    public struct TreeEntry<T> {
        
        public T data;
        public int level;
        public List<TreeEntry<T>> children;

        public override string ToString() {
            return $"[{level}: {data}]";
        }
        
    }

}
