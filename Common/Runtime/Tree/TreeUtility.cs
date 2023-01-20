using System;
using System.Collections.Generic;
using System.Linq;
using MisterGames.Common.Lists;

namespace MisterGames.Common.Trees {

    public static class TreeUtility {

        public static int Depth<T>(this TreeEntry<T> entry) {
            return entry.LevelOrder().Last().level + 1;
        }

        public static IEnumerable<TreeEntry<T>> RemoveRoot<T>(this IEnumerable<TreeEntry<T>> tree) {
            return tree.RemoveIf(e => e.level == 0);
        }
        
        public static TreeEntry<T> SortBranchesInChildrenFirst<T>(this TreeEntry<T> entry) {
            if (entry.children.Count == 0) return entry;
            
            var children = new List<TreeEntry<T>>();
            foreach (var child in entry.children) {
                if (child.children.Count == 0 || children.Count == 0 || children.Last().children.Count > 0) {
                    children.Add(child);
                    continue;
                }
                
                int insertionIndex = children.FindIndex(e => e.children.Count == 0);
                children.Insert(insertionIndex, child);
            }
            
            entry.children = children.Select(SortBranchesInChildrenFirst).ToList();
            return entry;
        }

        public static TreeEntry<T> RemoveLeafsIf<T>(this TreeEntry<T> entry, Func<TreeEntry<T>, bool> predicate) {
            entry.children = entry.children
                .Where(predicate.Invoke)
                .Select(e => RemoveLeafsIf(e, predicate))
                .ToList();

            return entry;
        }

        public static IEnumerable<TreeEntry<T>> PreOrder<T>(this TreeEntry<T> entry) {
            var list = new List<TreeEntry<T>>();
            var stack = new Stack<TreeEntry<T>>();

            stack.Push(entry);
            
            while (stack.Count > 0) {
                var current = stack.Pop();
                list.Add(current);
                current.children.Reversed().ForEach(stack.Push);
            }

            return list;
        }

        public static IEnumerable<TreeEntry<T>> LevelOrder<T>(this TreeEntry<T> entry) {
            var list = new List<TreeEntry<T>>();
            var queue = new Queue<TreeEntry<T>>();

            queue.Enqueue(entry);

            while (queue.Count > 0) {
                int n = queue.Count;

                while (n > 0) {
                    var current = queue.Dequeue();
                    list.Add(current);

                    current.children.ForEach(queue.Enqueue);
                    n--;
                }
            }

            return list;
        }
    }

}
