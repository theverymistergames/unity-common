using System;
using MisterGames.Blueprints;
using UnityEngine;

namespace MisterGames.unity_common.Blueprints.Runtime.Core2 {

    public readonly struct BlueprintNodeId {

    }

    public sealed class BlueprintNodeStorage {



    }

    public interface IBlueprintNodeDataStorage {
        ref T Get<T>(BlueprintNodeId id) where T : struct;
    }

    public interface IBlueprintNode {
        Port[] CreatePorts(IBlueprintNodeDataStorage storage, BlueprintNodeId id);


    }

    public interface IGenericStructGetter {
        ref T Get<T>(int index) where T : struct;
    }

    [Serializable]
    public sealed class SerializableStructArray<T> : IGenericStructGetter where T : struct {

        [SerializeField] private T[] array;

        private static T _default;

        public ref S Get<S>(int index) where S : struct {
            if (this is SerializableStructArray<S> s) return ref s.array[index];
            return ref SerializableStructArray<S>._default;
        }
    }

    [Serializable]
    public struct BlueprintNodeLogData {
        public string text;
    }

    public sealed class BlueprintNodeLog : IBlueprintNode {
        public Port[] CreatePorts(IBlueprintNodeDataStorage storage, BlueprintNodeId id) {
            ref var data = ref storage.Get<BlueprintNodeLogData>(id);
            return Array.Empty<Port>();
        }
    }

}
