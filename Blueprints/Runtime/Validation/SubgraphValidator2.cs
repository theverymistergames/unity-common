#if DEVELOPMENT_BUILD || UNITY_EDITOR

using MisterGames.Blueprints.Meta;
using UnityEngine;

namespace MisterGames.Blueprints.Validation {

    internal static class SubgraphValidator2 {

        private const int MAX_SUBGRAPH_LEVELS = 100;

        public static bool ValidateExternalBlueprint(
            MonoBehaviour owner,
            BlueprintRunner2 externalRunner,
            BlueprintAsset2 externalAsset
        ) {
            if (externalAsset == null) {
                Debug.LogError($"External blueprint node, launched from runner {owner}, is invalid: " +
                               $"provided external {nameof(BlueprintAsset2)} is null.");
                return false;
            }

            if (externalRunner == null) {
                Debug.LogError($"External blueprint node, launched from runner {owner}, is invalid: " +
                               $"provided external {nameof(BlueprintRunner2)} is null.");
                return false;
            }

            if (externalAsset != externalRunner.BlueprintAsset) {
                Debug.LogError($"External blueprint node, launched from runner {owner}, is invalid: " +
                               $"external blueprint node has external {nameof(BlueprintAsset2)} `{externalAsset}`, " +
                               $"but provided {nameof(BlueprintRunner2)} {externalRunner} " +
                               $"has different {nameof(BlueprintAsset2)} `{externalRunner.BlueprintAsset}`. " +
                               $"Blueprint assets must be same.");
                return false;
            }

            return true;
        }

        public static void ValidateSubgraphAsset(IBlueprintMeta meta, ref BlueprintAsset2 subgraph) {
            var root = ((BlueprintMeta2) meta).Owner as BlueprintAsset2;
            string rootName = root == null ? string.Empty : $"`{root.name}`";

            if (!IsValidSubgraphAsset(root, subgraph, 0, rootName)) subgraph = null;
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
