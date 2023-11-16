﻿using System;
using MisterGames.Blueprints.Factory;
using MisterGames.Blueprints.Nodes;
using MisterGames.Blueprints.Runtime;
using MisterGames.Common.Data;
using MisterGames.Common.Types;
using UnityEngine;
using Object = System.Object;

namespace MisterGames.Blueprints {

    /// <summary>
    /// Blueprint source for class node types. <see cref="IBlueprintSource"/>.
    /// </summary>
    [Serializable]
    internal sealed class BlueprintSourceRef :
        IBlueprintSource,
        IBlueprintEnter2,
        IBlueprintOutput2,
        IBlueprintStartCallback,
        IBlueprintEnableCallback,
        IBlueprintConnectionCallback,
        IBlueprintInternalLink,
        IBlueprintHashLink,
        IBlueprintCompilable,
        IBlueprintCreateSignaturePorts
    {
        [SerializeField] private RefArrayMap<int, IBlueprintNode> _nodeMap = new RefArrayMap<int, IBlueprintNode>();
        [SerializeField] private int _lastId;

        public int Count => _nodeMap.Count;

        public Type GetNodeType(int id) {
            return _nodeMap.TryGetValue(id, out var node) ? node.GetType() : null;
        }

        public ref T GetNodeByRef<T>(int id) where T : struct, IBlueprintNode {
            throw new InvalidOperationException($"{nameof(BlueprintSourceRef)}: " +
                                                $"can not get node of type {typeof(T).Name} " +
                                                $"from factory with nodes of type {nameof(Object)}");
        }

        public IBlueprintNode GetNodeAsInterface(int id) {
            return _nodeMap[id];
        }

        public string GetNodeAsString(int id) {
            return JsonUtility.ToJson(_nodeMap.GetValue(id));
        }

        public int AddNode(Type nodeType) {
            int id = _lastId++;
            _nodeMap.Add(id, Activator.CreateInstance(nodeType) as IBlueprintNode);
            return id;
        }

        public int AddNodeClone(IBlueprintSource source, int id) {
            throw new InvalidOperationException($"{nameof(BlueprintSourceRef)}: " +
                                                $"can not clone nodes of type {nameof(IBlueprintNode)}");
        }

        public int AddNodeFromString(string value, Type nodeType) {
            int id = _lastId++;
            _nodeMap.Add(id, JsonUtility.FromJson(value, nodeType) as IBlueprintNode);
            return id;
        }

        public void SetNodeFromString(int id, string value, Type nodeType) {
            if (_nodeMap.ContainsKey(id)) _nodeMap[id] = JsonUtility.FromJson(value, nodeType) as IBlueprintNode;
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
            _nodeMap.GetValue(id.node).CreatePorts(meta, id);
        }

        public void OnSetDefaults(IBlueprintMeta meta, NodeId id) {
            _nodeMap.GetValue(id.node).OnSetDefaults(meta, id);
        }

        public void OnValidate(IBlueprintMeta meta, NodeId id) {
            _nodeMap.GetValue(id.node).OnValidate(meta, id);
        }

        public void OnInitialize(IBlueprint blueprint, NodeToken token) {
            _nodeMap.GetValue(token.node.node).OnInitialize(blueprint, token);
        }

        public void OnDeInitialize(IBlueprint blueprint, NodeToken token) {
            _nodeMap.GetValue(token.node.node).OnDeInitialize(blueprint, token);
        }

        public void OnEnterPort(IBlueprint blueprint, NodeToken token, int port) {
            if (_nodeMap.GetValue(token.node.node) is IBlueprintEnter2 enter) {
                enter.OnEnterPort(blueprint, token, port);
            }
        }

        public T GetPortValue<T>(IBlueprint blueprint, NodeToken token, int port) {
            return _nodeMap.GetValue(token.node.node) switch {
                IBlueprintOutput2<T> outputT => outputT.GetPortValue(blueprint, token, port),
                IBlueprintOutput2 output => output.GetPortValue<T>(blueprint, token, port),
                _ => default,
            };
        }

        public void OnStart(IBlueprint blueprint, NodeToken token) {
            if (_nodeMap.GetValue(token.node.node) is IBlueprintStartCallback callback) {
                callback.OnStart(blueprint, token);
            }
        }

        public void OnEnable(IBlueprint blueprint, NodeToken token, bool enabled) {
            if (_nodeMap.GetValue(token.node.node) is IBlueprintEnableCallback callback) {
                callback.OnEnable(blueprint, token, enabled);
            }
        }

        public void OnLinksChanged(IBlueprintMeta meta, NodeId id, int port) {
            if (_nodeMap.GetValue(id.node) is IBlueprintConnectionCallback callback) {
                callback.OnLinksChanged(meta, id, port);
            }
        }

        public void GetLinkedPorts(NodeId id, int port, out int index, out int count) {
            if (_nodeMap.GetValue(id.node) is IBlueprintInternalLink internalLink) {
                internalLink.GetLinkedPorts(id, port, out index, out count);
            }

            index = -1;
            count = 0;
        }

        public bool TryGetLinkedPort(NodeId id, out int hash, out int port) {
            if (_nodeMap.GetValue(id.node) is IBlueprintHashLink hashLink) {
                return hashLink.TryGetLinkedPort(id, out hash, out port);
            }

            hash = 0;
            port = 0;
            return false;
        }

        public bool HasSignaturePorts(NodeId id) {
            return _nodeMap.GetValue(id.node) is IBlueprintCreateSignaturePorts p && p.HasSignaturePorts(id);
        }

        public void Compile(NodeId id, BlueprintCompileData data) {
            if (_nodeMap.GetValue(id.node) is IBlueprintCompilable compilable) {
                compilable.Compile(id, data);
            }
        }

        public override string ToString() {
            return $"{TypeNameFormatter.GetShortTypeName(typeof(BlueprintSourceRef))}: nodes: {_nodeMap}";
        }
    }

}