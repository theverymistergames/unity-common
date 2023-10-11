using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace MisterGames.Blueprints.Core2 {

    public sealed class RuntimeBlueprintStorage : IRuntimeBlueprintStorage {

        public IReadOnlyList<long> Nodes => _nodes;

        private readonly long[] _nodes;
        private readonly RuntimeLink2[] _links;
        private readonly Dictionary<RuntimePortAddress, int> _portIndexMap;

        private int _nodePointer;
        private int _linkPointer;

        private RuntimeBlueprintStorage() { }

        public RuntimeBlueprintStorage(int nodeCount, int portCount, int linkCount) {
            _nodes = new long[nodeCount];
            _links = new RuntimeLink2[portCount + linkCount];
            _portIndexMap = new Dictionary<RuntimePortAddress, int>(portCount + linkCount);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddNode(long id) {
            _nodes[_nodePointer++] = id;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RuntimeLink2 GetLink(int index) {
            return _links[index];
        }

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetLink(int index, long nodeId, int port) {
            _links[index] = new RuntimeLink2(nodeId, port);
        }

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
