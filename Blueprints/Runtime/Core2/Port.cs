using System;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Blueprints.Core2 {

    [Serializable]
    public struct Port : IEquatable<Port> {

        public string name;
        public bool isDataPort;
        public bool isExitPort;
        public bool hasDataType;
        public int dataTypeHash;

        [SerializeField] private string _serializedDataType;

        public Type DataType {
            get => SerializedType.FromString(_serializedDataType);
            set {
                dataTypeHash = value.GetHashCode();
                _serializedDataType = SerializedType.ToString(value);
            }
        }

        public bool Equals(Port other) {
            return name == other.name &&
                   isDataPort == other.isDataPort &&
                   isExitPort == other.isExitPort &&
                   hasDataType == other.hasDataType &&
                   dataTypeHash == other.dataTypeHash &&
                   _serializedDataType == other._serializedDataType;
        }

        public override bool Equals(object obj) {
            return obj is Port other && Equals(other);
        }

        public override int GetHashCode() {
            return HashCode.Combine(name, isDataPort, isExitPort, hasDataType, dataTypeHash, _serializedDataType);
        }

        public static bool operator ==(Port left, Port right) {
            return left.Equals(right);
        }

        public static bool operator !=(Port left, Port right) {
            return !left.Equals(right);
        }

        public override string ToString() {
            string mode = isDataPort
                ? $"{(isExitPort ? "output" : "input")}{(hasDataType ? $"<{_serializedDataType}>" : "")}"
                : isExitPort
                    ? "exit"
                    : "enter";

            return $"Port({name}, mode {mode})";
        }
    }
}
