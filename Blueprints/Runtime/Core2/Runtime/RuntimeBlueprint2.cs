using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace MisterGames.Blueprints.Core2 {

    public sealed class RuntimeBlueprint2 : IBlueprint {

        public IBlueprintHost2 Host { get; private set; }

        /// <summary>
        /// Blueprint node id list.
        /// </summary>
        public IReadOnlyList<long> Nodes => _nodes;

        private readonly IBlueprintFactory _factories;
        private readonly long[] _nodes;
        private readonly RuntimeLink2[] _links;
        private readonly Dictionary<RuntimePortAddress, int> _portIndexMap;

        private int _nodePointer;
        private int _linkPointer;

        private RuntimeBlueprint2() { }

        public RuntimeBlueprint2(IBlueprintFactory factories, int nodeCount, int portCount, int linkCount) {
            _factories = factories;
            _nodes = new long[nodeCount];
            _links = new RuntimeLink2[portCount + linkCount];
            _portIndexMap = new Dictionary<RuntimePortAddress, int>(portCount + linkCount);
        }

        public void Initialize(IBlueprintHost2 host) {
            Host = host;

            for (int i = 0; i < _nodes.Length; i++) {
                long id = _nodes[i];
                BlueprintNodeAddress.Unpack(id, out int factoryId, out int _);

                _factories.GetSource(factoryId).OnInitialize(this, id);
            }
        }

        public void DeInitialize() {
            for (int i = 0; i < _nodes.Length; i++) {
                long id = _nodes[i];
                BlueprintNodeAddress.Unpack(id, out int factoryId, out int _);

                _factories.GetSource(factoryId).OnDeInitialize(this, id);
            }
        }

        public void OnEnable() {
            for (int i = 0; i < _nodes.Length; i++) {
                long id = _nodes[i];
                BlueprintNodeAddress.Unpack(id, out int factoryId, out int _);

                if (_factories.GetSource(factoryId) is IBlueprintEnableDisable2 enableDisable) {
                    enableDisable.OnEnable(this, id);
                }
            }
        }

        public void OnDisable() {
            for (int i = 0; i < _nodes.Length; i++) {
                long id = _nodes[i];
                BlueprintNodeAddress.Unpack(id, out int factoryId, out int _);

                if (_factories.GetSource(factoryId) is IBlueprintEnableDisable2 enableDisable) {
                    enableDisable.OnDisable(this, id);
                }
            }
        }

        public void Start() {
            for (int i = 0; i < _nodes.Length; i++) {
                long id = _nodes[i];
                BlueprintNodeAddress.Unpack(id, out int factoryId, out int _);

                if (_factories.GetSource(factoryId) is IBlueprintStart2 start) {
                    start.OnStart(this, id);
                }
            }
        }

        public void Call(long id, int port) {
            GetLinks(id, port, out int index, out int count);
            int end = index + count;

            for (int i = index; i < end; i++) {
                var link = GetLink(i);
                BlueprintNodeAddress.Unpack(link.nodeId, out int factoryId, out _);

                if (_factories.GetSource(factoryId) is not IBlueprintEnter2 enter) continue;

                enter.OnEnterPort(this, link.nodeId, link.port);
            }
        }

        public T Read<T>(long id, int port, T defaultValue = default) {
            GetLinks(id, port, out int index, out int count);
            return count > 0 ? ReadLink(index, defaultValue) : defaultValue;
        }

        public T ReadLink<T>(int index, T defaultValue = default) {
            var link = GetLink(index);
            BlueprintNodeAddress.Unpack(link.nodeId, out int factoryId, out _);

            return _factories.GetSource(factoryId) switch {
                IBlueprintOutput2<T> outputT => outputT.GetOutputPortValue(this, link.nodeId, link.port),
                IBlueprintOutput2 output => output.GetOutputPortValue<T>(this, link.nodeId, link.port),
                _ => defaultValue
            };
        }

        /// <summary>
        /// Add blueprint node by address.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddNode(long id) {
            _nodes[_nodePointer++] = id;
        }

        /// <summary>
        /// Return a link by index in the links array.
        /// Index to use can be retrieved by calling <see cref="GetLinks"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RuntimeLink2 GetLink(int index) {
            return _links[index];
        }

        /// <summary>
        /// Get first index in the links array and count of the links holding by passed node and port.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetLinks(long id, int port, out int index, out int count) {
            if (!_portIndexMap.TryGetValue(new RuntimePortAddress(id, port), out index)) {
                index = -1;
                count = 0;
                return;
            }

            ref var link = ref _links[index];
            count = link.connections;
        }

        /// <summary>
        /// Set a link by index in the links array. Link will be created from passed node id and port.
        /// Index to use can be retrieved by calling <see cref="AllocateLinks"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetLink(int index, long nodeId, int port) {
            _links[index] = new RuntimeLink2(nodeId, port);
        }

        /// <summary>
        /// Return index in the links array, starting from which there can be set passed count of links.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int AllocateLinks(long id, int port, int count) {
            var portAddress = new RuntimePortAddress(id, port);
            if (_portIndexMap.TryGetValue(portAddress, out int index)) return -1;

            index = _linkPointer;
            _portIndexMap[portAddress] = index;

            _links[index] = new RuntimeLink2(id, port, count);
            _linkPointer += count + 1;

            return index + 1;
        }
    }

}
