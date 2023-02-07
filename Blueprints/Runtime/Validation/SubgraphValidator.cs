using UnityEngine;

namespace MisterGames.Blueprints.Validation {

    internal static class SubgraphValidator {

        private const int MAX_SUBGRAPH_LEVELS = 100;

        public static BlueprintAsset ValidateBlueprintAssetForSubgraph(BlueprintAsset ownerAsset, BlueprintAsset subgraphAsset) {
            return IsValidBlueprintAssetForSubgraph(ownerAsset, subgraphAsset, 0, $"`{ownerAsset.name}`")
                ? subgraphAsset
                : null;
        }

        private static bool IsValidBlueprintAssetForSubgraph(
            BlueprintAsset ownerAsset,
            BlueprintAsset subgraphAsset,
            int level,
            string path
        ) {
            if (subgraphAsset == null) return true;

            path += $" <- `{subgraphAsset.name}`";
            level++;

            if (level >= MAX_SUBGRAPH_LEVELS) {
                Debug.LogWarning($"Subgraph node of `{ownerAsset.name}` " +
                                 $"cannot accept `{subgraphAsset.name}` as parameter: " +
                                 $"subgraph depth is reached max level {MAX_SUBGRAPH_LEVELS}. " +
                                 $"Path: [{path}]");
                return false;
            }

            if (subgraphAsset == ownerAsset) {
                Debug.LogWarning($"Subgraph node of `{ownerAsset.name}` " +
                                 $"cannot accept `{subgraphAsset.name}` as parameter: " +
                                 $"this will produce cyclic references. " +
                                 $"Path: [{path}]");
                return false;
            }

            var references = subgraphAsset.BlueprintMeta.SubgraphReferencesMap.Values;
            foreach (var asset in references) {
                if (!IsValidBlueprintAssetForSubgraph(ownerAsset, asset, level, path)) return false;
            }

            return true;
        }
    }

}
