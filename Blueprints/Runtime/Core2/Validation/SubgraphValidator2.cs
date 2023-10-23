#if DEVELOPMENT_BUILD || UNITY_EDITOR

using UnityEngine;

namespace MisterGames.Blueprints.Core2 {

    internal static class SubgraphValidator2 {

        private const int MAX_SUBGRAPH_LEVELS = 100;

        public static void ValidateSubgraphAsset(IBlueprintMeta meta, ref BlueprintAsset2 subgraph) {
            var root = ((BlueprintMeta2) meta).Asset;
            if (!IsValidSubgraphAsset(root, subgraph, 0, $"`{root.name}`")) subgraph = null;
        }

        private static bool IsValidSubgraphAsset(BlueprintAsset2 root, BlueprintAsset2 subgraph, int level, string path) {
            if (subgraph == null) return true;

            path += $" <- `{subgraph.name}`";
            level++;

            if (level >= MAX_SUBGRAPH_LEVELS) {
                Debug.LogWarning($"Subgraph node of `{root.name}` " +
                                 $"cannot accept `{subgraph.name}` as parameter: " +
                                 $"subgraph depth is reached max level {MAX_SUBGRAPH_LEVELS}. " +
                                 $"Path: [{path}]");
                return false;
            }

            if (subgraph == root) {
                Debug.LogWarning($"Subgraph node of `{root.name}` " +
                                 $"cannot accept `{subgraph.name}` as parameter: " +
                                 $"this will produce cyclic references. " +
                                 $"Path: [{path}]");
                return false;
            }

            var assets = subgraph.BlueprintMeta.SubgraphAssets;
            foreach (var asset in assets) {
                if (!IsValidSubgraphAsset(root, asset, level, path)) return false;
            }

            return true;
        }
    }

}

#endif
