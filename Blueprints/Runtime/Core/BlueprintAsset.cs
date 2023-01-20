using MisterGames.Blueprints.Compile;
using MisterGames.Blueprints.Meta;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Blueprints {

    /// <summary>
    /// Asset to store blueprint meta data that can be compiled into runtime blueprint instance.
    /// Blueprint meta data is edited from Blueprint Editor.
    /// </summary>
    [CreateAssetMenu(fileName = nameof(BlueprintAsset), menuName = "MisterGames/" + nameof(BlueprintAsset))]
    public sealed class BlueprintAsset : ScriptableObject {

        [SerializeField] [HideInInspector]
        private BlueprintMeta _blueprintMeta;

        [SerializeField] [HideInInspector]
        private Blackboard _blackboard;

        private readonly BlueprintCompiler _blueprintCompiler = new BlueprintCompiler();

        public BlueprintMeta BlueprintMeta => _blueprintMeta;
        public Blackboard Blackboard => _blackboard;

        public RuntimeBlueprint Compile() {
            return _blueprintCompiler.Compile(_blueprintMeta);
        }

        public RuntimeBlueprint CompileSubgraph(BlueprintNode subgraph, BlueprintNodeMeta subgraphMeta) {
            return _blueprintCompiler.CompileSubgraph(_blueprintMeta, subgraph, subgraphMeta);
        }
    }

}
