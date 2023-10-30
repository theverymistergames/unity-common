using MisterGames.Blackboards.Core;
using MisterGames.Blueprints.Compile;
using MisterGames.Blueprints.Meta;
using UnityEngine;

namespace MisterGames.Blueprints {

    /// <summary>
    /// Asset to store blueprint meta data, that can be compiled into runtime blueprint instance.
    /// </summary>
    [CreateAssetMenu(fileName = "Blueprint", menuName = "MisterGames/Blueprint")]
    public sealed class BlueprintAsset : ScriptableObject {

        [SerializeField] private BlueprintMeta _blueprintMeta;
        [SerializeField] private Blackboard _blackboard;

        private readonly BlueprintCompiler _blueprintCompiler = new BlueprintCompiler();

        public BlueprintMeta BlueprintMeta => _blueprintMeta;
        public Blackboard Blackboard => _blackboard;

        public RuntimeBlueprint Compile() {
            return _blueprintCompiler.Compile(this);
        }

        public RuntimeBlueprint CompileSubgraph(BlueprintNode subgraphNode, Port[] ports) {
            return _blueprintCompiler.CompileSubgraph(this, subgraphNode, ports);
        }
    }

}
