using System;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Blueprints.Core2 {

    [Serializable]
    public struct Port {

        public string name;
        public bool isDataPort;
        public bool isExitPort;
        public bool isExternalPort;
        public bool hasDataType;

        [SerializeField] private string _serializedDataType;

        public Type DataType {
            get => SerializedType.FromString(_serializedDataType);
            private set => _serializedDataType = SerializedType.ToString(value);
        }

        internal Port SetExternal(bool isExternal) {
            isExternalPort = isExternal;
            return this;
        }

        public int GetSignature() => HashCode.Combine(
            string.IsNullOrWhiteSpace(name) ? string.Empty : name ,
            isDataPort,
            isExitPort,
            isExternalPort,
            hasDataType,
            string.IsNullOrEmpty(_serializedDataType) ? string.Empty : _serializedDataType
        );

        public static Port Enter(string name = null) {
            return new Port {
                name = name,
                isDataPort = false,
                isExitPort = false,
                hasDataType = false,
            };
        }

        public static Port Exit(string name = null) {
            return new Port {
                name = name,
                isDataPort = false,
                isExitPort = true,
                hasDataType = false,
            };
        }

        public static Port Input<T>(string name = null) {
            return new Port {
                name = name,
                isDataPort = true,
                isExitPort = false,
                hasDataType = true,
                DataType = typeof(T),
            };
        }

        public static Port Output<T>(string name = null) {
            return new Port {
                name = name,
                isDataPort = true,
                isExitPort = true,
                hasDataType = true,
                DataType = typeof(T),
            };
        }

        internal static Port Input(string name = null) {
            return new Port {
                name = name,
                isDataPort = true,
                isExitPort = false,
                hasDataType = false,
            };
        }

        internal static Port Output(string name = null) {
            return new Port {
                name = name,
                isDataPort = true,
                isExitPort = true,
                hasDataType = false,
            };
        }

        public override string ToString() {
            string externalText = isExternalPort ? "external " : "";

            string modeText = isDataPort
                ? isExitPort ? "exit" : "enter"
                : $"{(isExitPort ? "output" : "input")}{(hasDataType ? $"<{DataType.Name}>" : "")}";

            return $"{nameof(Port)}(name = {name}, mode = {externalText}{modeText})";
        }
    }
}
