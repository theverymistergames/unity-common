using MisterGames.Blackboards.Core;
using MisterGames.Blueprints.Runtime;
using MisterGames.Blueprints.Factory;
using MisterGames.Blueprints.Meta;
using UnityEngine;

namespace MisterGames.Blueprints {

    [CreateAssetMenu(fileName = "Blueprint", menuName = "MisterGames/Blueprint")]
    public sealed class BlueprintAsset : ScriptableObject {

        [SerializeField] private BlueprintMeta _blueprintMeta;
        [SerializeField] private Blackboard _blackboard;

        public Blackboard Blackboard => _blackboard;

        public BlueprintMeta BlueprintMeta {
            get {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                _blueprintMeta.owner = this;
#endif
                return _blueprintMeta;
            }
        }

        private BlueprintCompiler _blueprintCompiler;

        public RuntimeBlueprint Compile(IBlueprintFactory factory, IBlueprintHost host) {
            _blueprintCompiler ??= new BlueprintCompiler();
            return _blueprintCompiler.Compile(BlueprintMeta, factory, host);
        }

        public void CompileSubgraph(SubgraphCompileData data) {
            _blueprintCompiler ??= new BlueprintCompiler();
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
