using MisterGames.Blackboards.Core;
using UnityEngine;

namespace MisterGames.Blueprints.Core2 {

    [CreateAssetMenu(fileName = "Blueprint2", menuName = "MisterGames/Blueprint2")]
    public sealed class BlueprintAsset2 : ScriptableObject {

        [SerializeField] private BlueprintMeta2 _blueprintMeta;
        [SerializeField] private Blackboard _blackboard;

        public Blackboard Blackboard => _blackboard;

        public BlueprintMeta2 BlueprintMeta {
            get {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                _blueprintMeta.Owner = this;
#endif
                return _blueprintMeta;
            }
        }

        private BlueprintCompiler2 _blueprintCompiler;

        public RuntimeBlueprint2 Compile(IBlueprintFactory factory) {
            _blueprintCompiler ??= new BlueprintCompiler2();
            return _blueprintCompiler.Compile(factory, BlueprintMeta);
        }

        public void CompileSubgraph(BlueprintCompileData data) {
            _blueprintCompiler ??= new BlueprintCompiler2();
            _blueprintCompiler.CompileSubgraph(BlueprintMeta, data);
        }
    }

}
