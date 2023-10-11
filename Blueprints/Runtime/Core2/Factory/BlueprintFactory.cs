﻿using System;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Blueprints.Core2 {

    /// <summary>
    /// Base class for deriving user defined factories. <see cref="IBlueprintFactory"/>.
    /// </summary>
    /// <typeparam name="TNode">Struct node type</typeparam>
    [Serializable]
    public abstract class BlueprintFactory<TNode> : IBlueprintFactory
        where TNode : struct, IBlueprintNode
    {
        [SerializeField] private ArrayMap<int, TNode> _nodeMap = new ArrayMap<int, TNode>();
        [SerializeField] private int _lastId;

        public int Count => _nodeMap.Count;

        public ref T GetNode<T>(int id) where T : struct, IBlueprintNode {
            if (this is not BlueprintFactory<T> factory) {
                throw new InvalidOperationException($"{nameof(BlueprintFactory<TNode>)}: " +
                                                    $"can not get node of type {typeof(T).Name} " +
                                                    $"from factory with nodes of type {typeof(TNode).Name}");
            }

            return ref factory._nodeMap.GetValueByRef(id);
        }

        public int AddNode() {
            _lastId++;
            if (_lastId == 0) _lastId++;

            _nodeMap.Add(_lastId, default);

            return _lastId;
        }

        public int AddNodeCopy(IBlueprintFactory factory, int id) {
            int localId = AddNode();
            ref var data = ref _nodeMap.GetValueByRef(localId);

            data = factory.GetNode<TNode>(id);

            return localId;
        }

        public void RemoveNode(int id) {
            _nodeMap.Remove(id);
        }

        public string GetNodePath(int id) {
#if UNITY_EDITOR
            if (!_nodeMap.ContainsKey(id)) {
                Debug.LogWarning($"{nameof(BlueprintFactory<TNode>)}: " +
                                 $"trying to get node path by id {id}, " +
                                 $"but node with this id is not found: " +
                                 $"node map has no entry with id {id}.");

                return null;
            }

            return $"{nameof(_nodeMap)}._entries.Array.data[{_nodeMap.IndexOf(id)}].value";
#endif

            throw new InvalidOperationException($"{nameof(BlueprintFactory<TNode>)}: " +
                                                $"calling method {nameof(GetNodePath)} is only allowed in the Unity Editor.");
        }

        public void Clear() {
            _nodeMap.Clear();
            _lastId = 0;
        }

        public Port[] CreatePorts(IBlueprintMeta blueprintMeta, long id) {
            BlueprintNodeAddress.Unpack(id, out _, out int nodeId);

            ref var node = ref _nodeMap.GetValueByRef(nodeId);
            return node.CreatePorts(blueprintMeta, id);
        }

        public void SetDefaultValues(IBlueprintMeta blueprintMeta, long id) {
            BlueprintNodeAddress.Unpack(id, out _, out int nodeId);

            ref var node = ref _nodeMap.GetValueByRef(nodeId);
            node.SetDefaultValues(blueprintMeta, id);
        }

        public void OnValidate(IBlueprintMeta blueprintMeta, long id) {
            BlueprintNodeAddress.Unpack(id, out _, out int nodeId);

            ref var node = ref _nodeMap.GetValueByRef(nodeId);
            node.OnValidate(blueprintMeta, id);
        }

        public void OnInitialize(IBlueprint blueprint, long id) {
            BlueprintNodeAddress.Unpack(id, out _, out int nodeId);

            ref var node = ref _nodeMap.GetValueByRef(nodeId);
            node.OnInitialize(blueprint, id);
        }

        public void OnDeInitialize(IBlueprint blueprint, long id) {
            BlueprintNodeAddress.Unpack(id, out _, out int nodeId);

            ref var node = ref _nodeMap.GetValueByRef(nodeId);
            node.OnDeInitialize(blueprint, id);
        }

        public override string ToString() {
            return _nodeMap.ToString();
        }
    }
}
