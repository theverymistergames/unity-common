using UnityEngine;

namespace MisterGames.Blueprints.Core2 {

    /// <summary>
    /// Asset to store blueprint meta data that can be compiled into runtime blueprint instance.
    /// </summary>
    [CreateAssetMenu(fileName = nameof(BlueprintAsset), menuName = "MisterGames/" + nameof(BlueprintAsset))]
    public sealed class BlueprintAsset : ScriptableObject {

        [SerializeField] [HideInInspector]
        private BlueprintMeta _blueprintMeta;

        private readonly BlueprintCompiler _blueprintCompiler = new BlueprintCompiler();

        public BlueprintMeta BlueprintMeta => _blueprintMeta;

        public RuntimeBlueprint Compile() {
            return _blueprintCompiler.Compile(_blueprintMeta);
        }

        public RuntimeBlueprint CompileSubgraph(BlueprintNode subgraph, BlueprintNodeMeta subgraphMeta) {
            return _blueprintCompiler.CompileSubgraph(_blueprintMeta, subgraph, subgraphMeta);
        }

        private void OnValidate() {
            _blueprintMeta.OnValidate(this);
        }
    }

}
