using System;
using MisterGames.Blueprints.Meta;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Blueprints {

    [Serializable]
    public struct Port : IEquatable<Port> {

        [SerializeField] private string _name;
        [SerializeField] private SerializedType _signature;
        [SerializeField] private PortMode _mode;

        private sealed class Null {}

        public string Name => _name;

        public Type Signature {
            get => _signature;
            private set => _signature = new SerializedType(value);
        }

        internal bool IsInput => _mode.HasFlag(PortMode.Input);
        internal bool IsExternal => _mode.HasFlag(PortMode.External);
        internal bool IsDisabled => _mode.HasFlag(PortMode.Disabled);

        internal bool IsMultiple =>
            !_mode.HasFlag(PortMode.CapacitySingle) && !_mode.HasFlag(PortMode.CapacityMultiple)
                ? _signature == null || !IsInput || IsAction
                : _mode.HasFlag(PortMode.CapacityMultiple);

        internal bool IsLeftLayout =>
            !_mode.HasFlag(PortMode.LayoutLeft) && !_mode.HasFlag(PortMode.LayoutRight)
                ? _mode.HasFlag(PortMode.Input)
                : _mode.HasFlag(PortMode.LayoutLeft);

        internal bool IsAction {
            get {
                var t = Signature;
                if (t == null) return false;
                if (!t.IsGenericType) return t == typeof(Action);

                var def = t.GetGenericTypeDefinition();
                return
                    def == typeof(Action<>) ||
                    def == typeof(Action<,>) ||
                    def == typeof(Action<,,>) ||
                    def == typeof(Action<,,,>) ||
                    def == typeof(Action<,,,,>) ||
                    def == typeof(Action<,,,,,>) ||
                    def == typeof(Action<,,,,,,>) ||
                    def == typeof(Action<,,,,,,,>) ||
                    def == typeof(Action<,,,,,,,,>) ||
                    def == typeof(Action<,,,,,,,,,>) ||
                    def == typeof(Action<,,,,,,,,,,>) ||
                    def == typeof(Action<,,,,,,,,,,,>) ||
                    def == typeof(Action<,,,,,,,,,,,,>) ||
                    def == typeof(Action<,,,,,,,,,,,,,>) ||
                    def == typeof(Action<,,,,,,,,,,,,,,>) ||
                    def == typeof(Action<,,,,,,,,,,,,,,,>);
            }
        }

        internal bool IsFunc {
            get {
                var t = Signature;
                if (t is not { IsGenericType: true }) return false;

                var def = t.GetGenericTypeDefinition();
                return
                    def == typeof(Func<>) ||
                    def == typeof(Func<,>) ||
                    def == typeof(Func<,,>) ||
                    def == typeof(Func<,,,>) ||
                    def == typeof(Func<,,,,>) ||
                    def == typeof(Func<,,,,,>) ||
                    def == typeof(Func<,,,,,,>) ||
                    def == typeof(Func<,,,,,,,>) ||
                    def == typeof(Func<,,,,,,,,>) ||
                    def == typeof(Func<,,,,,,,,,>) ||
                    def == typeof(Func<,,,,,,,,,,>) ||
                    def == typeof(Func<,,,,,,,,,,,>) ||
                    def == typeof(Func<,,,,,,,,,,,,>) ||
                    def == typeof(Func<,,,,,,,,,,,,,>) ||
                    def == typeof(Func<,,,,,,,,,,,,,,>) ||
                    def == typeof(Func<,,,,,,,,,,,,,,,>) ||
                    def == typeof(Func<,,,,,,,,,,,,,,,,>);
            }
        }

        internal bool IsAnyAction {
            get {
                if (!IsAction) return false;

                var t = Signature;
                if (!t.IsGenericType) return false;

                var args = t.GetGenericArguments();
                if (args.Length != 16) return false;

                for (int i = 0; i < args.Length; i++) {
                    if (!args[i].IsGenericTypeParameter) return false;
                }

                return true;
            }
        }

        internal bool IsAnyFunc {
            get {
                if (!IsFunc) return false;

                var args = Signature.GetGenericArguments();
                if (args.Length != 17) return false;

                for (int i = 0; i < args.Length; i++) {
                    if (!args[i].IsGenericTypeParameter) return false;
                }

                return true;
            }
        }

        internal bool IsDynamicFunc {
            get {
                if (!IsFunc) return false;

                var args = Signature.GetGenericArguments();
                return args.Length == 1 && args[0].IsGenericTypeParameter;
            }
        }

        internal int GetSignatureHashCode() => HashCode.Combine(
            _mode,
            string.IsNullOrWhiteSpace(_name) ? string.Empty : _name.Trim(),
            _signature ?? typeof(Null)
        );

        internal Port External(bool isExternal) {
            if (isExternal) _mode |= PortMode.External;
            else _mode &= ~PortMode.External;

            return this;
        }

        public Port Enable(bool isEnabled) {
            if (isEnabled) _mode &= ~PortMode.Disabled;
            else _mode |= PortMode.Disabled;

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

        public static Port Create(PortDirection direction, string name = null, Type signature = null) {
            var settings = direction == PortDirection.Input ? PortMode.Input : PortMode.None;
            return new Port { _name = name, _mode = settings, Signature = signature };
        }

        public static Port AnyAction(PortDirection direction, string name = null) {
            return Create(direction, name, typeof(Action<,,,,,,,,,,,,,,,>));
        }

        public static Port AnyFunc(PortDirection direction, string name = null) {
            return Create(direction, name, typeof(Func<,,,,,,,,,,,,,,,,>));
        }

        public static Port DynamicFunc(PortDirection direction, string name = null, Type returnType = null) {
            var signature = returnType == null ? typeof(Func<>) : typeof(Func<>).MakeGenericType(returnType);
            return Create(direction, name, signature);
        }

        public static Port Action(PortDirection direction, string name = null) {
            return Create(direction, name, typeof(Action));
        }
        public static Port Action<T>(PortDirection direction, string name = null) {
            return Create(direction, name, typeof(Action<T>));
        }
        public static Port Action<T1, T2>(PortDirection direction, string name = null) {
            return Create(direction, name, typeof(Action<T1, T2>));
        }
        public static Port Action<T1, T2, T3>(PortDirection direction, string name = null) {
            return Create(direction, name, typeof(Action<T1, T2, T3>));
        }
        public static Port Action<T1, T2, T3, T4>(PortDirection direction, string name = null) {
            return Create(direction, name, typeof(Action<T1, T2, T3, T4>));
        }
        public static Port Action<T1, T2, T3, T4, T5>(PortDirection direction, string name = null) {
            return Create(direction, name, typeof(Action<T1, T2, T3, T4, T5>));
        }
        public static Port Action<T1, T2, T3, T4, T5, T6>(PortDirection direction, string name = null) {
            return Create(direction, name, typeof(Action<T1, T2, T3, T4, T5, T6>));
        }
        public static Port Action<T1, T2, T3, T4, T5, T6, T7>(PortDirection direction, string name = null) {
            return Create(direction, name, typeof(Action<T1, T2, T3, T4, T5, T6, T7>));
        }
        public static Port Action<T1, T2, T3, T4, T5, T6, T7, T8>(PortDirection direction, string name = null) {
            return Create(direction, name, typeof(Action<T1, T2, T3, T4, T5, T6, T7, T8>));
        }
        public static Port Action<T1, T2, T3, T4, T5, T6, T7, T8, T9>(PortDirection direction, string name = null) {
            return Create(direction, name, typeof(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9>));
        }
        public static Port Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(PortDirection direction, string name = null) {
            return Create(direction, name, typeof(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>));
        }
        public static Port Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(PortDirection direction, string name = null) {
            return Create(direction, name, typeof(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>));
        }
        public static Port Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(PortDirection direction, string name = null) {
            return Create(direction, name, typeof(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>));
        }
        public static Port Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(PortDirection direction, string name = null) {
            return Create(direction, name, typeof(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>));
        }
        public static Port Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(PortDirection direction, string name = null) {
            return Create(direction, name, typeof(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>));
        }
        public static Port Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(PortDirection direction, string name = null) {
            return Create(direction, name, typeof(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>));
        }
        public static Port Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(PortDirection direction, string name = null) {
            return Create(direction, name, typeof(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>));
        }

        public static Port Func<R>(PortDirection direction, string name = null) {
            return Create(direction, name, typeof(Func<R>));
        }
        public static Port Func<T, R>(PortDirection direction, string name = null) {
            return Create(direction, name, typeof(Func<T, R>));
        }
        public static Port Func<T1, T2, R>(PortDirection direction, string name = null) {
            return Create(direction, name, typeof(Func<T1, T2, R>));
        }
        public static Port Func<T1, T2, T3, R>(PortDirection direction, string name = null) {
            return Create(direction, name, typeof(Func<T1, T2, T3, R>));
        }
        public static Port Func<T1, T2, T3, T4, R>(PortDirection direction, string name = null) {
            return Create(direction, name, typeof(Func<T1, T2, T3, T4, R>));
        }
        public static Port Func<T1, T2, T3, T4, T5, R>(PortDirection direction, string name = null) {
            return Create(direction, name, typeof(Func<T1, T2, T3, T4, T5, R>));
        }
        public static Port Func<T1, T2, T3, T4, T5, T6, R>(PortDirection direction, string name = null) {
            return Create(direction, name, typeof(Func<T1, T2, T3, T4, T5, T6, R>));
        }
        public static Port Func<T1, T2, T3, T4, T5, T6, T7, R>(PortDirection direction, string name = null) {
            return Create(direction, name, typeof(Func<T1, T2, T3, T4, T5, T6, T7, R>));
        }
        public static Port Func<T1, T2, T3, T4, T5, T6, T7, T8, R>(PortDirection direction, string name = null) {
            return Create(direction, name, typeof(Func<T1, T2, T3, T4, T5, T6, T7, T8, R>));
        }
        public static Port Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, R>(PortDirection direction, string name = null) {
            return Create(direction, name, typeof(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, R>));
        }
        public static Port Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, R>(PortDirection direction, string name = null) {
            return Create(direction, name, typeof(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, R>));
        }
        public static Port Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, R>(PortDirection direction, string name = null) {
            return Create(direction, name, typeof(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, R>));
        }
        public static Port Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, R>(PortDirection direction, string name = null) {
            return Create(direction, name, typeof(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, R>));
        }
        public static Port Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, R>(PortDirection direction, string name = null) {
            return Create(direction, name, typeof(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, R>));
        }
        public static Port Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, R>(PortDirection direction, string name = null) {
            return Create(direction, name, typeof(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, R>));
        }
        public static Port Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, R>(PortDirection direction, string name = null) {
            return Create(direction, name, typeof(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, R>));
        }
        public static Port Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, R>(PortDirection direction, string name = null) {
            return Create(direction, name, typeof(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, R>));
        }

        public bool Equals(Port other) {
            return _mode == other._mode &&
                   (string.IsNullOrWhiteSpace(_name) ? string.IsNullOrWhiteSpace(other._name) : _name == other._name) &&
                   (_signature == null && other._signature == null || _signature == other._signature);
        }

        public override bool Equals(object obj) {
            return obj is Port other && Equals(other);
        }

        public override int GetHashCode() {
            return HashCode.Combine(_name, _mode, _signature);
        }

        public static bool operator ==(Port left, Port right) {
            return left.Equals(right);
        }

        public static bool operator !=(Port left, Port right) {
            return !left.Equals(right);
        }

        public override string ToString() {
            return $"{nameof(Port)}(" +
                   $"name = {_name}, " +
                   $"{(IsExternal ? "external " : string.Empty)}" +
                   $"{(IsMultiple ? "multiple" : "single")} " +
                   $"{(IsInput ? "input" : "output")} " +
                   $"{(_signature == null ? string.Empty : $" {TypeNameFormatter.GetTypeName(_signature)}")}" +
                   $")";
        }
    }

}
