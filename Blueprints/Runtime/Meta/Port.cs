using System;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Blueprints {

    [Serializable]
    public struct Port {

        public string name;
        public bool isExternalPort;
        public Mode mode;

        [SerializeField] private string _serializedDataType;

        public Type DataType {
            get => SerializedType.FromString(_serializedDataType);
            private set => _serializedDataType = SerializedType.ToString(value);
        }

        public enum Mode {
            Enter,
            Exit,
            Input,
            Output,
            InputArray,
            NonTypedInput,
            NonTypedOutput,
        }

        internal Port SetExternal(bool isExternal) {
            isExternalPort = isExternal;
            return this;
        }

        internal Port SetType(Type type) {
            DataType = type;
            return this;
        }

        public int GetSignature() => HashCode.Combine(
            mode,
            string.IsNullOrWhiteSpace(name) ? string.Empty : name,
            string.IsNullOrEmpty(_serializedDataType) ? string.Empty : _serializedDataType
        );

        public static Port Enter(string name = null) {
            return new Port {
                name = name,
                mode = Mode.Enter,
            };
        }

        public static Port Exit(string name = null) {
            return new Port {
                name = name,
                mode = Mode.Exit,
            };
        }

        public static Port Input<T>(string name = null) {
            return new Port {
                name = name,
                mode = Mode.Input,
                DataType = typeof(T),
            };
        }

        public static Port Output<T>(string name = null) {
            return new Port {
                name = name,
                mode = Mode.Output,
                DataType = typeof(T),
            };
        }

        public static Port InputArray<T>(string name = null) {
            return new Port {
                name = name,
                mode = Mode.InputArray,
                DataType = typeof(T),
            };
        }

        internal static Port Input(string name = null, Type type = null) {
            return new Port {
                name = name,
                mode = type == null ? Mode.NonTypedInput : Mode.Input,
                DataType = type,
            };
        }

        internal static Port Output(string name = null, Type type = null) {
            return new Port {
                name = name,
                mode = type == null ? Mode.NonTypedOutput : Mode.Output,
                DataType = type,
            };
        }

        internal static Port InputArray(string name = null, Type type = null) {
            return new Port {
                name = name,
                mode = type == null ? Mode.NonTypedInput : Mode.InputArray,
                DataType = type,
            };
        }

        public override string ToString() {
            string externalText = isExternalPort ? "external " : string.Empty;

            string modeText = mode switch {
                    Mode.Enter => "enter",
                    Mode.Exit => "exit",
                    Mode.Input => $"input<{DataType.Name}>",
                    Mode.Output => $"output<{DataType.Name}>",
                    Mode.InputArray => $"inputArray<{DataType.Name}>",
                    Mode.NonTypedInput => "input",
                    Mode.NonTypedOutput => "output",
                    _ => throw new NotSupportedException($"Port mode {mode} is not supported")
            };

            return $"{nameof(Port)}(name = {name}, mode = {externalText}{modeText}, signature = {GetSignature()})";
        }
    }

}
