using MisterGames.Blackboards.Core;
using MisterGames.Blueprints.Runtime;
using MisterGames.Blueprints.Factory;
using MisterGames.Blueprints.Meta;
using UnityEngine;

namespace MisterGames.Blueprints {

    [CreateAssetMenu(fileName = "Blueprint2", menuName = "MisterGames/Blueprint2")]
    public sealed class BlueprintAsset2 : ScriptableObject {

        [SerializeField] private BlueprintMeta2 _blueprintMeta;
        [SerializeField] private Blackboard _blackboard;

        public Blackboard Blackboard => _blackboard;

        public BlueprintMeta2 BlueprintMeta {
            get {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                _blueprintMeta.owner = this;
#endif
                return _blueprintMeta;
            }
        }

        private BlueprintCompiler2 _blueprintCompiler;

        public RuntimeBlueprint2 Compile(IBlueprintFactory factory, IBlueprintHost2 host) {
#if UNITY_EDITOR
            _blueprintMeta.NodeJsonMap.Clear();
#endif

            _blueprintCompiler ??= new BlueprintCompiler2();
            return _blueprintCompiler.Compile(factory, host);
        }

        public void CompileSubgraph(BlueprintCompileData data) {
#if UNITY_EDITOR
            _blueprintMeta.NodeJsonMap.Clear();
#endif

            _blueprintCompiler ??= new BlueprintCompiler2();
            _blueprintCompiler.CompileSubgraph(data);
        }
    }

}
