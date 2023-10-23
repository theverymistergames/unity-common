using MisterGames.Blackboards.Core;
using UnityEngine;

namespace MisterGames.Blueprints.Core2 {

    [CreateAssetMenu(fileName = "Blueprint2", menuName = "MisterGames/Blueprint2")]
    public sealed class BlueprintAsset2 : ScriptableObject {

        [SerializeField] private BlueprintMeta2 _blueprintMeta;
        [SerializeField] private Blackboard _blackboard;

        private readonly BlueprintCompiler2 _blueprintCompiler = new BlueprintCompiler2();

        public BlueprintMeta2 BlueprintMeta {
            get {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                _blueprintMeta.Asset = this;
#endif
                return _blueprintMeta;
            }
        }

        public Blackboard Blackboard => _blackboard;

        public RuntimeBlueprint2 Compile(IBlueprintFactory factory) {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            _blueprintMeta.Asset = this;
#endif
            return _blueprintCompiler.Compile(factory, _blueprintMeta);
        }

        public void CompileSubgraph(IBlueprintFactory factory, BlueprintCompileData data) {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            _blueprintMeta.Asset = this;
#endif
            _blueprintCompiler.CompileSubgraph(factory, _blueprintMeta, data);
        }
    }

}
