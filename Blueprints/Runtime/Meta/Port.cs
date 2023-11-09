using System;
using MisterGames.Blueprints.Meta;
using MisterGames.Common.Types;
using UnityEngine;

namespace MisterGames.Blueprints {

    [Serializable]
    public struct Port : IEquatable<Port> {

        [SerializeField] internal string name;
        [SerializeField] internal PortMode mode;
        [SerializeField] internal PortOptions options;
        [SerializeField] internal SerializedType dataType;

        public string Name => string.IsNullOrWhiteSpace(name) ? string.Empty : name.Trim();

        public Type DataType {
            get => dataType.ToType();
            private set => dataType = new SerializedType(value);
        }

        public static Port Enter(string name = null) {
            return new Port { name = name, mode = PortMode.Input };
        }

        public static Port Exit(string name = null) {
            return new Port { name = name, mode = PortMode.None };
        }

        public static Port Input<T>(string name = null) {
            return new Port { name = name, mode = PortMode.Input | PortMode.Data, DataType = typeof(T) };
        }

        public static Port Output<T>(string name = null) {
            return new Port { name = name, mode = PortMode.Data, DataType = typeof(T) };
        }

        public static Port DynamicInput(string name = null, Type type = null) {
            return new Port { name = name, mode = PortMode.Input | PortMode.Data, DataType = type };
        }

        public static Port DynamicOutput(string name = null, Type type = null, bool acceptSubclass = false) {
            var options = acceptSubclass ? PortOptions.AcceptSubclass : PortOptions.None;
            return new Port { name = name, mode = PortMode.Data, options = options, DataType = type };
        }

        public bool Equals(Port other) {
            return mode == other.mode &&
                   (string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(other.name) || name == other.name) &&
                   dataType == other.dataType;
        }

        public override bool Equals(object obj) {
            return obj is Port other && Equals(other);
        }

        public override int GetHashCode() {
            return HashCode.Combine(name, mode, dataType);
        }

        public static bool operator ==(Port left, Port right) {
            return left.Equals(right);
        }

        public static bool operator !=(Port left, Port right) {
            return !left.Equals(right);
        }

        public override string ToString() {
            string config = this.IsData()
                ? this.IsInput()
                    ? dataType.ToType() is {} t0 ? $"input<{TypeNameFormatter.GetShortTypeName(t0)}>" : "dynamic input"
                    : dataType.ToType() is {} t1 ? $"output<{TypeNameFormatter.GetShortTypeName(t1)}>" : "dynamic output"
                : this.IsInput()
                    ? "enter"
                    : "exit";

            return $"{nameof(Port)}(" +
                   $"name = {name}, " +
                   $"{(this.IsHidden() ? "hidden " : string.Empty)}" +
                   $"{(this.IsExternal() ? "external " : string.Empty)}" +
                   $"{(this.IsMultiple() ? "multiple" : "single")} " +
                   $"{config}" +
                   $"{(this.IsInput() != this.IsLeftLayout() ? this.IsLeftLayout() ? " (left layout)" : " (right layout)" : string.Empty)}" +
                   $")";
        }
    }

}
