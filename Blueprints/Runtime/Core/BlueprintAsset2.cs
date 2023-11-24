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

        public Blackboard Blackboard {
            get => _blackboard;
            internal set => _blackboard = value;
        }

        public BlueprintMeta2 BlueprintMeta {
            get {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                _blueprintMeta.owner = this;
#endif
                return _blueprintMeta;
            }
            internal set {
                _blueprintMeta = value;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                _blueprintMeta.owner = this;
#endif
            }
        }

        private BlueprintCompiler2 _blueprintCompiler;

        public RuntimeBlueprint2 Compile(IBlueprintFactory factory, IBlueprintHost2 host) {
            _blueprintCompiler ??= new BlueprintCompiler2();
            return _blueprintCompiler.Compile(BlueprintMeta, factory, host);
        }

        public void CompileSubgraph(SubgraphCompileData data) {
            _blueprintCompiler ??= new BlueprintCompiler2();
            _blueprintCompiler.CompileSubgraph(BlueprintMeta, data);
        }

#if UNITY_EDITOR
        internal string GetNodePath(NodeId id) {
            return _blueprintMeta.TryGetNodePath(id, out int si, out int ni)
                ? $"_blueprintMeta._factory._sources._entries.Array.data[{si}].value._nodeMap._entries.Array.data[{ni}].value"
                : null;
        }
#endif
    }

}
