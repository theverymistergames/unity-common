using System;
using MisterGames.Blueprints.Factory;
using MisterGames.Blueprints.Nodes;
using MisterGames.Common.Data;
using MisterGames.Common.Types;
using UnityEngine;

namespace MisterGames.Blueprints {

    /// <summary>
    /// Base class for deriving user defined blueprint source with struct node type. <see cref="IBlueprintSource"/>.
    /// </summary>
    /// <typeparam name="TNode">Struct node type</typeparam>
    [Serializable]
    public abstract class BlueprintSource<TNode> : IBlueprintSource
        where TNode : struct, IBlueprintNode
    {
        [SerializeField] private Map<int, TNode> _nodeMap = new Map<int, TNode>();
        [SerializeField] private int _lastId;

        public int Count => _nodeMap.Count;

        public Type GetNodeType(int id) {
            return typeof(TNode);
        }

        public ref T GetNodeByRef<T>(int id) where T : struct, IBlueprintNode {
            if (this is not BlueprintSource<T> factory) {
                throw new InvalidOperationException($"{nameof(BlueprintSource<TNode>)}: " +
                                                    $"can not get node of type {typeof(T).Name} " +
                                                    $"from struct source with nodes of type {typeof(TNode).Name}");
            }

            return ref factory._nodeMap.Get(id);
        }

        public IBlueprintNode GetNodeAsInterface(int id) {
            return _nodeMap.Get(id);
        }

        public string GetNodeAsString(int id) {
            ref var node = ref _nodeMap.Get(id);
            return JsonUtility.ToJson(node);
        }

        public int AddNode(Type nodeType = null) {
            _lastId++;
            if (_lastId == 0) _lastId++;

            int id = _lastId;
            _nodeMap.Add(id, default);

            return id;
        }

        public int AddNodeClone(IBlueprintSource source, int cloneId) {
            _lastId++;
            if (_lastId == 0) _lastId++;

            int targetId = _lastId;
            _nodeMap.Add(targetId, source.GetNodeByRef<TNode>(cloneId));

            return targetId;
        }

        public void AddNodeClone(int id, IBlueprintSource source, int cloneId) {
            _nodeMap.Add(id, source.GetNodeByRef<TNode>(cloneId));
        }

        public void SetNodeClone(int id, IBlueprintSource source, int cloneId) {
            if (_nodeMap.ContainsKey(id)) _nodeMap[id] = source.GetNodeByRef<TNode>(cloneId);
        }

        public int AddNodeFromString(string value, Type nodeType) {
            _lastId++;
            if (_lastId == 0) _lastId++;

            int id = _lastId;
            _nodeMap.Add(id, JsonUtility.FromJson<TNode>(value));

            return id;
        }

        public void AddNodeFromString(int id, string value, Type nodeType) {
            _nodeMap.Add(id, JsonUtility.FromJson<TNode>(value));
        }

        public void SetNodeFromString(int id, string value, Type nodeType) {
            if (_nodeMap.ContainsKey(id)) _nodeMap[id] = JsonUtility.FromJson<TNode>(value);
        }

        public void RemoveNode(int id) {
            _nodeMap.Remove(id);
        }

        public bool ContainsNode(int id) {
            return _nodeMap.ContainsKey(id);
        }

        public bool TryGetNodePath(int id, out int index) {
            index = _nodeMap.IndexOf(id);
            return index >= 0;
        }

        public void Clear() {
            _nodeMap.Clear();
            _lastId = 0;
        }

        public bool MatchNodesWith(IBlueprintSource source) {
            return _nodeMap.RemoveIf(source, (s, id) => !s.ContainsNode(id));
        }

        public void CopyInto(IBlueprintSource source) {
            if (source is not BlueprintSource<TNode> s) return;

            foreach (int id in _nodeMap.Keys) {
                var nodeCopy = s is IBlueprintCloneable
                    ? GetNodeByRef<TNode>(id)
                    : JsonUtility.FromJson<TNode>(GetNodeAsString(id));

                s._nodeMap.Add(id, nodeCopy);
            }
        }

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            ref var node = ref _nodeMap.Get(id.node);
            node.CreatePorts(meta, id);
        }

        public void OnSetDefaults(IBlueprintMeta meta, NodeId id) {
            ref var node = ref _nodeMap.Get(id.node);
            node.OnSetDefaults(meta, id);
        }

        public void OnValidate(IBlueprintMeta meta, NodeId id) {
            ref var node = ref _nodeMap.Get(id.node);
            node.OnValidate(meta, id);
        }

        public void OnInitialize(IBlueprint blueprint, NodeToken token, NodeId root) {
            ref var node = ref _nodeMap.Get(token.node.node);
            node.OnInitialize(blueprint, token, root);
        }

        public void OnDeInitialize(IBlueprint blueprint, NodeToken token, NodeId root) {
            ref var node = ref _nodeMap.Get(token.node.node);
            node.OnDeInitialize(blueprint, token, root);
        }

        public override string ToString() {
            return $"{TypeNameFormatter.GetShortTypeName(typeof(BlueprintSource<TNode>))}: nodes: {_nodeMap}";
        }
    }
}
