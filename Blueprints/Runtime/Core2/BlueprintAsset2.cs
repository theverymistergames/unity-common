using MisterGames.Blackboards.Core;
using MisterGames.Blueprints.Compile;
using UnityEngine;

namespace MisterGames.Blueprints.Core2 {

    [CreateAssetMenu(fileName = "Blueprint2", menuName = "MisterGames/Blueprint2")]
    public sealed class BlueprintAsset2 : ScriptableObject {

        [SerializeField] private BlueprintMeta2 _blueprintMeta;
        [SerializeField] private Blackboard _blackboard;

        private readonly BlueprintCompiler2 _blueprintCompiler = new BlueprintCompiler2();

        public BlueprintMeta2 BlueprintMeta => _blueprintMeta;
        public Blackboard Blackboard => _blackboard;

        public RuntimeBlueprint2 Compile() {
            return _blueprintCompiler.Compile(this);
        }

        public RuntimeBlueprint2 CompileSubgraph(BlueprintNode subgraphNode, Port[] ports) {
            return _blueprintCompiler.CompileSubgraph(this, subgraphNode, ports);
        }
    }
}
