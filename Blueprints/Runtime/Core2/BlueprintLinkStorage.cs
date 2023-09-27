using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace MisterGames.Blueprints.Core2 {

    public sealed class BlueprintLinkStorage : IBlueprintLinkStorage {

        private readonly BlueprintLink[] _links;
        private readonly Dictionary<long, int> _idToIndexMap;

        private int _pointer;

        public BlueprintLinkStorage(int nodeCount, int portCount, int linkCount) {
            _links = new BlueprintLink[portCount + linkCount];
            _idToIndexMap = new Dictionary<long, int>(nodeCount);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BlueprintLink GetLink(int index) {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (index < 0 || index >= _links.Length) {
                Debug.LogError($"{nameof(BlueprintLinkStorage)}: " +
                               $"trying to get link by index {index}, " +
                               $"but index is incorrect. " +
                               $"Links array size is {_links.Length}.");

                return default;
            }
#endif

            return _links[index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetLinks(long id, int port, out int index, out int count) {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            BlueprintNodeAddress.Parse(id, out int factoryId, out int nodeId);
#endif

            if (!_idToIndexMap.TryGetValue(id, out index)) {
                index = -1;
                count = 0;
                return;
            }

            while (true) {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                if (index < 0 || index >= _links.Length) {
                    Debug.LogError($"{nameof(BlueprintLinkStorage)}: " +
                                   $"trying to get links for factory {factoryId} node {nodeId} port {port}, " +
                                   $"but while iterating through links of this node, " +
                                   $"there was retrieved an incorrect index {index}. " +
                                   $"First entry for this node has index {_idToIndexMap[id]}. " +
                                   $"Links array size is {_links.Length}.");
                    index = -1;
                    count = 0;
                    return;
                }
#endif

                ref var link = ref _links[index];

#if UNITY_EDITOR || DEVELOPMENT_BUILD
                if (link.factoryId != factoryId || link.nodeId != nodeId) {
                    Debug.LogError($"{nameof(BlueprintLinkStorage)}: " +
                                   $"trying to get links for factory {factoryId} node {nodeId} port {port}, " +
                                   $"but while iterating through links of this node, " +
                                   $"there was retrieved an incorrect index {index}. " +
                                   $"First entry for this node has index {_idToIndexMap[id]}. " +
                                   $"Links array size is {_links.Length}.");
                    index = -1;
                    count = 0;
                    return;
                }
#endif

                if (link.port == port) {
                    index++;
                    count = link.connections;
                    return;
                }

                if (link.port > port) {
                    index = -1;
                    count = 0;
                    return;
                }

                index += link.connections + 1;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetLink(int index, int factoryId, int nodeId, int port) {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (index < 0 || index >= _links.Length) {
                Debug.LogError($"{nameof(BlueprintLinkStorage)}: " +
                               $"trying to set link to factory {factoryId} node {nodeId} port {port}, " +
                               $"but input link index {index} is incorrect.");

                return;
            }
#endif

            _links[index] = new BlueprintLink(factoryId, nodeId, port);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int AddLinks(int factoryId, int nodeId, int port, int count) {
            long id = BlueprintNodeAddress.Create(factoryId, nodeId);

            if (_idToIndexMap.TryGetValue(id, out int index)) {
                while (true) {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    if (index < 0 || index >= _links.Length) {
                        Debug.LogError($"{nameof(BlueprintLinkStorage)}: " +
                                       $"trying to add links for factory {factoryId} node {nodeId} port {port}, " +
                                       $"but while iterating through links of this node, " +
                                       $"there was retrieved an incorrect index {index}. " +
                                       $"First entry for this node has index {_idToIndexMap[id]}. " +
                                       $"Links array size is {_links.Length}.");

                        return -1;
                    }
#endif

                    ref var link = ref _links[index];

                    if (link.factoryId != factoryId || link.nodeId != nodeId) {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                        if (link.factoryId != 0 || link.nodeId != 0) {
                            Debug.LogError($"{nameof(BlueprintLinkStorage)}: " +
                                           $"trying to add links for factory {factoryId} node {nodeId} port {port}, " +
                                           $"but retrieved index {index} for new entry already has data: " +
                                           $"factoryId {link.factoryId}, nodeId {link.nodeId}, port {link.port}.");

                            return -1;
                        }
#endif
                        break;
                    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    if (link.port == port) {
                        Debug.LogError($"{nameof(BlueprintLinkStorage)}: " +
                                       $"trying to add links for factory {factoryId} node {nodeId} port {port}, " +
                                       $"but this port has already been added: {link}. " +
                                       $"Node ports should be added in ascending order.");

                        return -1;
                    }

                    if (link.port > port) {
                        Debug.LogError($"{nameof(BlueprintLinkStorage)}: " +
                                       $"trying to add links for factory {factoryId} node {nodeId} port {port}, " +
                                       $"but port {link.port} has already been added: {link}. " +
                                       $"Node ports must be added in ascending order.");

                        return -1;
                    }
#endif

                    index += link.connections + 1;
                }

                _pointer = index;
            }
            else {
                index = _pointer;
                _idToIndexMap[id] = index;
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (index < 0 || index >= _links.Length) {
                Debug.LogError($"{nameof(BlueprintLinkStorage)}: " +
                               $"trying to add links for factory {factoryId} node {nodeId} port {port}, " +
                               $"but there was retrieved an incorrect index {index}. " +
                               $"Links array size is {_links.Length}.");

                return -1;
            }
#endif

            _links[index] = new BlueprintLink(factoryId, nodeId, port, count);
            _pointer += count + 1;

            return index;
        }
    }

}
