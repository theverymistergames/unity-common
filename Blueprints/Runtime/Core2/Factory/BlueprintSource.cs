using System;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Blueprints.Core2 {

    /// <summary>
    /// Base class for deriving user defined blueprint source. <see cref="IBlueprintSource"/>.
    /// </summary>
    /// <typeparam name="TNode">Struct node type</typeparam>
    [Serializable]
    public abstract class BlueprintSource<TNode> : IBlueprintSource
        where TNode : struct, IBlueprintNode
    {
        [SerializeField] private ArrayMap<int, TNode> _nodeMap = new ArrayMap<int, TNode>();
        [SerializeField] private int _lastId;

        public int Count => _nodeMap.Count;

        public ref T GetNode<T>(int id) where T : struct, IBlueprintNode {
            if (this is not BlueprintSource<T> factory) {
                throw new InvalidOperationException($"{nameof(BlueprintSource<TNode>)}: " +
                                                    $"can not get node of type {typeof(T).Name} " +
                                                    $"from factory with nodes of type {typeof(TNode).Name}");
            }

            return ref factory._nodeMap.GetValue(id);
        }

        public int AddNode() {
            _lastId++;
            if (_lastId == 0) _lastId++;

            _nodeMap.Add(_lastId, default);

            return _lastId;
        }

        public int AddNodeCopy(IBlueprintSource source, int id) {
            int localId = AddNode();
            ref var data = ref _nodeMap.GetValue(localId);

            data = source.GetNode<TNode>(id);

            return localId;
        }

        public void RemoveNode(int id) {
            _nodeMap.Remove(id);
        }

        public string GetNodePath(int id) {
#if UNITY_EDITOR
            if (!_nodeMap.ContainsKey(id)) {
                Debug.LogWarning($"{nameof(BlueprintSource<TNode>)}: " +
                                 $"trying to get node path by id {id}, " +
                                 $"but node with this id is not found: " +
                                 $"node map has no entry with id {id}.");

                return null;
            }

            return $"{nameof(_nodeMap)}._nodes.Array.data[{_nodeMap.IndexOf(id)}].value";
#endif

            throw new InvalidOperationException($"{nameof(BlueprintSource<TNode>)}: " +
                                                $"calling method {nameof(GetNodePath)} is only allowed in the Unity Editor.");
        }

        public void Clear() {
            _nodeMap.Clear();
            _lastId = 0;
        }

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            ref var node = ref _nodeMap.GetValue(id.node);
            node.CreatePorts(meta, id);
        }

        public void OnSetDefaults(IBlueprintMeta meta, NodeId id) {
            ref var node = ref _nodeMap.GetValue(id.node);
            node.OnSetDefaults(meta, id);
        }

        public void OnValidate(IBlueprintMeta meta, NodeId id) {
            ref var node = ref _nodeMap.GetValue(id.node);
            node.OnValidate(meta, id);
        }

        public void OnInitialize(IBlueprint blueprint, NodeId id) {
            ref var node = ref _nodeMap.GetValue(id.node);
            node.OnInitialize(blueprint, id);
        }

        public void OnDeInitialize(IBlueprint blueprint, NodeId id) {
            ref var node = ref _nodeMap.GetValue(id.node);
            node.OnDeInitialize(blueprint, id);
        }

        public override string ToString() {
            return $"{nameof(BlueprintSource<TNode>)}: nodes: {_nodeMap}";
        }
    }

}
