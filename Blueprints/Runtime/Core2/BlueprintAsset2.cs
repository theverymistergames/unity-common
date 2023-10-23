using MisterGames.Blackboards.Core;
using UnityEngine;

namespace MisterGames.Blueprints.Core2 {

    [CreateAssetMenu(fileName = "Blueprint2", menuName = "MisterGames/Blueprint2")]
    public sealed class BlueprintAsset2 : ScriptableObject {

        [SerializeField] private BlueprintMeta2 _blueprintMeta;
        [SerializeField] private Blackboard _blackboard;

        private readonly BlueprintCompiler2 _blueprintCompiler = new BlueprintCompiler2();

        public BlueprintMeta2 BlueprintMeta => _blueprintMeta;
        public Blackboard Blackboard => _blackboard;

        public RuntimeBlueprint2 Compile(IBlueprintFactory factory) {
            return _blueprintCompiler.Compile(factory, this);
        }

        public void CompileSubgraph(IBlueprintFactory factory, BlueprintCompileData data) {
            _blueprintCompiler.CompileSubgraph(factory, this, data);
        }
    }
}
