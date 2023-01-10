using System;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Blueprints.Ports {

    [Serializable]
    public struct PortMeta {

        public string name;
        public bool isDataPort;
        public bool isExitPort;
        public bool hasDataType;

        public Type dataType {
            get => SerializedType.FromString(_serializedDataType);
            set => _serializedDataType = SerializedType.ToString(value);
        }

        [SerializeField] private string _serializedDataType;

        public override string ToString() {
            string mode = isDataPort
                ? isExitPort ? "output" : "input"
                : isExitPort ? "exit" : "enter";

            return $"{name}, mode = {mode}";
        }
    }

}
