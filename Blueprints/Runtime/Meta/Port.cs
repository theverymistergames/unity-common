using System;
using MisterGames.Blueprints.Meta;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Blueprints {

    [Serializable]
    public struct Port : IEquatable<Port> {

        [SerializeField] private string _name;
        [SerializeField] private PortMode _mode;
        [SerializeField] private SerializedType _dataType;

        private sealed class Null {}

        public string Name => _name;

        public Type DataType {
            get => _dataType;
            private set => _dataType = new SerializedType(value);
        }

        internal bool IsInput => _mode.HasFlag(PortMode.Input);
        internal bool IsData => _mode.HasFlag(PortMode.Data);
        internal bool IsExternal => _mode.HasFlag(PortMode.External);
        internal bool IsHidden => _mode.HasFlag(PortMode.Hidden);

        internal bool IsMultiple =>
            !_mode.HasFlag(PortMode.CapacitySingle) && !_mode.HasFlag(PortMode.CapacityMultiple)
                ? !IsInput || !IsData
                : _mode.HasFlag(PortMode.CapacityMultiple);

        internal bool IsLeftLayout =>
            !_mode.HasFlag(PortMode.LayoutLeft) && !_mode.HasFlag(PortMode.LayoutRight)
                ? _mode.HasFlag(PortMode.Input)
                : _mode.HasFlag(PortMode.LayoutLeft);

        internal int GetSignatureHashCode() => HashCode.Combine(
            _mode,
            string.IsNullOrWhiteSpace(_name) ? string.Empty : _name.Trim(),
            _dataType ?? typeof(Null)
        );

        internal Port External(bool isExternal) {
            if (isExternal) _mode |= PortMode.External;
            else _mode &= ~PortMode.External;

            return this;
        }

        public Port Hidden(bool isHidden) {
            if (isHidden) _mode |= PortMode.Hidden;
            else _mode &= ~PortMode.Hidden;

            return this;
        }

        public Port Layout(PortLayout layout) {
            switch (layout) {
                case PortLayout.Default:
                    _mode &= ~(PortMode.LayoutLeft | PortMode.LayoutRight);
                    break;

                case PortLayout.Left:
                    _mode &= ~PortMode.LayoutRight;
                    _mode |= PortMode.LayoutLeft;
                    break;

                case PortLayout.Right:
                    _mode &= ~PortMode.LayoutLeft;
                    _mode |= PortMode.LayoutRight;
                    break;
            }

            return this;
        }

        public Port Capacity(PortCapacity capacity) {
            switch (capacity) {
                case PortCapacity.Default:
                    _mode &= ~(PortMode.CapacitySingle | PortMode.CapacityMultiple);
                    break;

                case PortCapacity.Single:
                    _mode &= ~PortMode.CapacityMultiple;
                    _mode |= PortMode.CapacitySingle;
                    break;

                case PortCapacity.Multiple:
                    _mode &= ~PortMode.CapacitySingle;
                    _mode |= PortMode.CapacityMultiple;
                    break;
            }

            return this;
        }

        public static Port Enter(string name = null) {
            return new Port { _name = name, _mode = PortMode.Input };
        }
        public static Port Exit(string name = null) {
            return new Port { _name = name, _mode = PortMode.None };
        }

        public static Port Input<T>(string name = null) {
            return new Port { _name = name, _mode = PortMode.Data | PortMode.Input, DataType = typeof(T) };
        }
        public static Port Output<T>(string name = null) {
            return new Port { _name = name, _mode = PortMode.Data, DataType = typeof(T) };
        }

        public static Port DynamicInput(string name = null, Type type = null) {
            return new Port { _name = name, _mode = PortMode.Data | PortMode.Input, DataType = type };
        }
        public static Port DynamicOutput(string name = null, Type type = null) {
            return new Port { _name = name, _mode = PortMode.Data, DataType = type };
        }

        public bool Equals(Port other) {
            return _mode == other._mode &&
                   (string.IsNullOrWhiteSpace(_name) ? string.IsNullOrWhiteSpace(other._name) : _name == other._name) &&
                   (_dataType == null && other._dataType == null || _dataType == other._dataType);
        }

        public override bool Equals(object obj) {
            return obj is Port other && Equals(other);
        }

        public override int GetHashCode() {
            return HashCode.Combine(_name, _mode, _dataType);
        }

        public static bool operator ==(Port left, Port right) {
            return left.Equals(right);
        }

        public static bool operator !=(Port left, Port right) {
            return !left.Equals(right);
        }

        public override string ToString() {
            string config = IsData
                ? IsInput
                    ? _dataType == null ? "dynamic input" : $"input<{TypeNameFormatter.GetTypeName(_dataType)}>"
                    : _dataType == null ? "dynamic output" : $"output<{TypeNameFormatter.GetTypeName(_dataType)}>"
                : IsInput
                    ? "enter"
                    : "exit";

            return $"{nameof(Port)}(name = {_name}, " +
                   $"{(IsHidden ? "hidden " : string.Empty)}" +
                   $"{(IsExternal ? "external " : string.Empty)}" +
                   $"{(IsMultiple ? "multiple" : "single")} " +
                   $"{config})";
        }
    }

}
