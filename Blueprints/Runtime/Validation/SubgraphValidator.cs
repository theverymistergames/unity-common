#if DEVELOPMENT_BUILD || UNITY_EDITOR

using MisterGames.Blueprints.Meta;
using MisterGames.Common.GameObjects;
using UnityEngine;

namespace MisterGames.Blueprints.Validation {

    internal static class SubgraphValidator {

        private const int MAX_SUBGRAPH_LEVELS = 100;

        public static bool ValidateExternalBlueprint(
            MonoBehaviour owner,
            BlueprintRunner externalRunner,
            BlueprintAsset externalAsset
        ) {
            if (externalAsset == null) {
                Debug.LogError($"External blueprint node, launched from runner {owner.GetPathInScene()}, is invalid: " +
                               $"provided external {nameof(BlueprintAsset)} is null.");
                return false;
            }

            if (externalRunner == null) {
                Debug.LogError($"External blueprint node, launched from runner {owner.GetPathInScene()}, is invalid: " +
                               $"provided external {nameof(BlueprintRunner)} is null.");
                return false;
            }

            if (externalAsset != externalRunner.BlueprintAsset) {
                Debug.LogError($"External blueprint node, launched from runner {owner.GetPathInScene()}, is invalid: " +
                               $"external blueprint node has external {nameof(BlueprintAsset)} `{externalAsset}`, " +
                               $"but provided {nameof(BlueprintRunner)} {externalRunner.GetPathInScene()} " +
                               $"has different {nameof(BlueprintAsset)} `{externalRunner.BlueprintAsset}`. " +
                               $"Blueprint assets must be same.");
                return false;
            }

            return true;
        }

        public static void ValidateSubgraphAsset(IBlueprintMeta meta, ref BlueprintAsset subgraph) {
            Object root = (meta as BlueprintMeta)?.owner switch {
                BlueprintAsset asset => asset,
                BlueprintRunner runner => runner,
                _ => null,
            };

            string rootName = root == null ? string.Empty : $"`{root.name}`";

            if (!IsValidSubgraphAsset(root, rootName, subgraph, 0, rootName)) subgraph = null;
        }

        private static bool IsValidSubgraphAsset(Object root, string rootName, BlueprintAsset subgraph, int level, string path) {
            if (subgraph == null) return true;

            path += $" <- `{subgraph.name}`";
            level++;

            if (level >= MAX_SUBGRAPH_LEVELS) {
                Debug.LogWarning($"Subgraph node of `{rootName}` " +
                                 $"cannot accept `{subgraph.name}` as parameter: " +
                                 $"subgraph depth is reached max level {MAX_SUBGRAPH_LEVELS}. " +
                                 $"Path: [{path}]");
                return false;
            }

            if (subgraph == root) {
                Debug.LogWarning($"Subgraph node of `{rootName}` " +
                                 $"cannot accept `{subgraph.name}` as parameter: " +
                                 $"this will produce cyclic references. " +
                                 $"Path: [{path}]");
                return false;
            }

            var assets = subgraph.BlueprintMeta.SubgraphAssetMap;
            foreach (var asset in assets.Values) {
                if (!IsValidSubgraphAsset(root, rootName, asset, level, path)) return false;
            }

            return true;
        }
    }

}

#endif
