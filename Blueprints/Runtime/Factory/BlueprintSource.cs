using System;
using MisterGames.Blueprints.Factory;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Blueprints {

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

        public Type NodeType => typeof(TNode);

        public ref T GetNode<T>(int id) where T : struct, IBlueprintNode {
            if (this is not BlueprintSource<T> factory) {
                throw new InvalidOperationException($"{nameof(BlueprintSource<TNode>)}: " +
                                                    $"can not get node of type {typeof(T).Name} " +
                                                    $"from factory with nodes of type {typeof(TNode).Name}");
            }

            return ref factory._nodeMap.GetValue(id);
        }

        public int AddNode() {
            int id = _lastId++;
            _nodeMap.Add(id, default);
            return id;
        }

        public void SetNode(int id, string str) {
            ref var node = ref _nodeMap.GetValue(id);
            node = JsonUtility.FromJson<TNode>(str);
        }

        public void SetNode(int id, IBlueprintSource source, int copyId) {
            ref var node = ref _nodeMap.GetValue(id);
            node = source.GetNode<TNode>(copyId);
        }

        public string GetNodeAsString(int id) {
            ref var node = ref _nodeMap.GetValue(id);
            return JsonUtility.ToJson(node);
        }

        public void RemoveNode(int id) {
            _nodeMap.Remove(id);
        }

        public bool TryGetNodePath(int id, out int index) {
            index = _nodeMap.IndexOf(id);
            return index >= 0;
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

        public void OnInitialize(IBlueprint blueprint, NodeToken token) {
            ref var node = ref _nodeMap.GetValue(token.node.node);
            node.OnInitialize(blueprint, token);
        }

        public void OnDeInitialize(IBlueprint blueprint, NodeToken token) {
            ref var node = ref _nodeMap.GetValue(token.node.node);
            node.OnDeInitialize(blueprint, token);
        }

        public override string ToString() {
            return $"{nameof(BlueprintSource<TNode>)}: nodes: {_nodeMap}";
        }
    }

}
