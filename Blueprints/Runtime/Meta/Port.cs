using System;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Blueprints {

    [Serializable]
    public struct Port {

        public string name;
        public bool isExternalPort;
        public Mode mode;
        public Type dataType => _serializedType;

        [SerializeField] private SerializedType _serializedType;

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

        public int GetSignature() => HashCode.Combine(
            mode,
            string.IsNullOrWhiteSpace(name) ? string.Empty : name,
            _serializedType == null ? typeof(int) : dataType
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
                _serializedType = new SerializedType(typeof(T)),
            };
        }

        public static Port Output<T>(string name = null) {
            return new Port {
                name = name,
                mode = Mode.Output,
                _serializedType = new SerializedType(typeof(T)),
            };
        }

        public static Port InputArray<T>(string name = null) {
            return new Port {
                name = name,
                mode = Mode.InputArray,
                _serializedType = new SerializedType(typeof(T)),
            };
        }

        internal static Port Input(string name = null, Type type = null) {
            return new Port {
                name = name,
                mode = type == null ? Mode.NonTypedInput : Mode.Input,
                _serializedType = new SerializedType(type),
            };
        }

        internal static Port Output(string name = null, Type type = null) {
            return new Port {
                name = name,
                mode = type == null ? Mode.NonTypedOutput : Mode.Output,
                _serializedType = new SerializedType(type),
            };
        }

        internal static Port InputArray(string name = null, Type type = null) {
            return new Port {
                name = name,
                mode = type == null ? Mode.NonTypedInput : Mode.InputArray,
                _serializedType = new SerializedType(type),
            };
        }

        public override string ToString() {
            string externalText = isExternalPort ? "external " : string.Empty;

            string modeText = mode switch {
                    Mode.Enter => "enter",
                    Mode.Exit => "exit",
                    Mode.Input => $"input<{dataType.Name}>",
                    Mode.Output => $"output<{dataType.Name}>",
                    Mode.InputArray => $"inputArray<{dataType.Name}>",
                    Mode.NonTypedInput => "input",
                    Mode.NonTypedOutput => "output",
                    _ => throw new NotSupportedException($"Port mode {mode} is not supported")
            };

            return $"{nameof(Port)}(name = {name}, mode = {externalText}{modeText}, signature = {GetSignature()})";
        }
    }

}
