using System;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Blueprints {

    [Serializable]
    public struct Port : IEquatable<Port> {

        [SerializeField] private string _name;
        [SerializeField] private SerializedType _signature;
        [SerializeField] private PortSettings _settings;

        [Flags]
        private enum PortSettings {
            None = 0,

            Input = 1,

            CapacitySingle = 2,
            CapacityMultiple = 4,

            LayoutLeft = 8,
            LayoutRight = 16,

            External = 32,
        }

        public string Name => _name;

        public Type Signature {
            get => _signature;
            private set => _signature = new SerializedType(value);
        }

        internal bool IsInput => _settings.HasFlag(PortSettings.Input);
        internal bool IsExternal => _settings.HasFlag(PortSettings.External);

        internal bool IsMultiple =>
            !_settings.HasFlag(PortSettings.CapacitySingle) && !_settings.HasFlag(PortSettings.CapacityMultiple)
                ? _signature == null || !IsInput || IsAction
                : _settings.HasFlag(PortSettings.CapacityMultiple);

        internal bool IsLeftLayout =>
            !_settings.HasFlag(PortSettings.LayoutLeft) && !_settings.HasFlag(PortSettings.LayoutRight)
                ? _settings.HasFlag(PortSettings.Input)
                : _settings.HasFlag(PortSettings.LayoutLeft);

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
            _settings,
            string.IsNullOrWhiteSpace(_name) ? string.Empty : _name.Trim(),
            _signature ?? typeof(PortSettings)
        );

        internal Port External(bool external) {
            if (external) _settings |= PortSettings.External;
            else _settings &= ~PortSettings.External;

            return this;
        }

        public Port Layout(PortLayout layout) {
            switch (layout) {
                case PortLayout.Default:
                    _settings &= ~(PortSettings.LayoutLeft | PortSettings.LayoutRight);
                    break;

                case PortLayout.Left:
                    _settings &= ~PortSettings.LayoutRight;
                    _settings |= PortSettings.LayoutLeft;
                    break;

                case PortLayout.Right:
                    _settings &= ~PortSettings.LayoutLeft;
                    _settings |= PortSettings.LayoutRight;
                    break;
            }

            return this;
        }

        public Port Capacity(PortCapacity capacity) {
            switch (capacity) {
                case PortCapacity.Default:
                    _settings &= ~(PortSettings.CapacitySingle | PortSettings.CapacityMultiple);
                    break;

                case PortCapacity.Single:
                    _settings &= ~PortSettings.CapacityMultiple;
                    _settings |= PortSettings.CapacitySingle;
                    break;

                case PortCapacity.Multiple:
                    _settings &= ~PortSettings.CapacitySingle;
                    _settings |= PortSettings.CapacityMultiple;
                    break;
            }

            return this;
        }

        public static Port Create(PortMode mode, string name = null, Type signature = null) {
            var settings = mode == PortMode.Input ? PortSettings.Input : PortSettings.None;
            return new Port { _name = name, _settings = settings, Signature = signature };
        }

        public static Port AnyAction(PortMode mode, string name = null) {
            return Create(mode, name, typeof(Action<,,,,,,,,,,,,,,,>));
        }

        public static Port AnyFunc(PortMode mode, string name = null) {
            return Create(mode, name, typeof(Func<,,,,,,,,,,,,,,,,>));
        }

        public static Port Action(PortMode mode, string name = null) {
            return Create(mode, name, typeof(Action));
        }
        public static Port Action<T>(PortMode mode, string name = null) {
            return Create(mode, name, typeof(Action<T>));
        }
        public static Port Action<T1, T2>(PortMode mode, string name = null) {
            return Create(mode, name, typeof(Action<T1, T2>));
        }
        public static Port Action<T1, T2, T3>(PortMode mode, string name = null) {
            return Create(mode, name, typeof(Action<T1, T2, T3>));
        }
        public static Port Action<T1, T2, T3, T4>(PortMode mode, string name = null) {
            return Create(mode, name, typeof(Action<T1, T2, T3, T4>));
        }
        public static Port Action<T1, T2, T3, T4, T5>(PortMode mode, string name = null) {
            return Create(mode, name, typeof(Action<T1, T2, T3, T4, T5>));
        }
        public static Port Action<T1, T2, T3, T4, T5, T6>(PortMode mode, string name = null) {
            return Create(mode, name, typeof(Action<T1, T2, T3, T4, T5, T6>));
        }
        public static Port Action<T1, T2, T3, T4, T5, T6, T7>(PortMode mode, string name = null) {
            return Create(mode, name, typeof(Action<T1, T2, T3, T4, T5, T6, T7>));
        }
        public static Port Action<T1, T2, T3, T4, T5, T6, T7, T8>(PortMode mode, string name = null) {
            return Create(mode, name, typeof(Action<T1, T2, T3, T4, T5, T6, T7, T8>));
        }
        public static Port Action<T1, T2, T3, T4, T5, T6, T7, T8, T9>(PortMode mode, string name = null) {
            return Create(mode, name, typeof(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9>));
        }
        public static Port Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(PortMode mode, string name = null) {
            return Create(mode, name, typeof(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>));
        }
        public static Port Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(PortMode mode, string name = null) {
            return Create(mode, name, typeof(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>));
        }
        public static Port Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(PortMode mode, string name = null) {
            return Create(mode, name, typeof(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>));
        }
        public static Port Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(PortMode mode, string name = null) {
            return Create(mode, name, typeof(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>));
        }
        public static Port Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(PortMode mode, string name = null) {
            return Create(mode, name, typeof(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>));
        }
        public static Port Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(PortMode mode, string name = null) {
            return Create(mode, name, typeof(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>));
        }
        public static Port Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(PortMode mode, string name = null) {
            return Create(mode, name, typeof(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>));
        }

        public static Port Func(PortMode mode, string name = null, Type returnType = null) {
            return Create(mode, name, returnType == null ? typeof(Func<>) : typeof(Func<>).MakeGenericType(returnType));
        }
        public static Port Func<R>(PortMode mode, string name = null) {
            return Create(mode, name, typeof(Func<R>));
        }
        public static Port Func<T, R>(PortMode mode, string name = null) {
            return Create(mode, name, typeof(Func<T, R>));
        }
        public static Port Func<T1, T2, R>(PortMode mode, string name = null) {
            return Create(mode, name, typeof(Func<T1, T2, R>));
        }
        public static Port Func<T1, T2, T3, R>(PortMode mode, string name = null) {
            return Create(mode, name, typeof(Func<T1, T2, T3, R>));
        }
        public static Port Func<T1, T2, T3, T4, R>(PortMode mode, string name = null) {
            return Create(mode, name, typeof(Func<T1, T2, T3, T4, R>));
        }
        public static Port Func<T1, T2, T3, T4, T5, R>(PortMode mode, string name = null) {
            return Create(mode, name, typeof(Func<T1, T2, T3, T4, T5, R>));
        }
        public static Port Func<T1, T2, T3, T4, T5, T6, R>(PortMode mode, string name = null) {
            return Create(mode, name, typeof(Func<T1, T2, T3, T4, T5, T6, R>));
        }
        public static Port Func<T1, T2, T3, T4, T5, T6, T7, R>(PortMode mode, string name = null) {
            return Create(mode, name, typeof(Func<T1, T2, T3, T4, T5, T6, T7, R>));
        }
        public static Port Func<T1, T2, T3, T4, T5, T6, T7, T8, R>(PortMode mode, string name = null) {
            return Create(mode, name, typeof(Func<T1, T2, T3, T4, T5, T6, T7, T8, R>));
        }
        public static Port Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, R>(PortMode mode, string name = null) {
            return Create(mode, name, typeof(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, R>));
        }
        public static Port Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, R>(PortMode mode, string name = null) {
            return Create(mode, name, typeof(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, R>));
        }
        public static Port Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, R>(PortMode mode, string name = null) {
            return Create(mode, name, typeof(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, R>));
        }
        public static Port Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, R>(PortMode mode, string name = null) {
            return Create(mode, name, typeof(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, R>));
        }
        public static Port Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, R>(PortMode mode, string name = null) {
            return Create(mode, name, typeof(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, R>));
        }
        public static Port Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, R>(PortMode mode, string name = null) {
            return Create(mode, name, typeof(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, R>));
        }
        public static Port Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, R>(PortMode mode, string name = null) {
            return Create(mode, name, typeof(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, R>));
        }
        public static Port Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, R>(PortMode mode, string name = null) {
            return Create(mode, name, typeof(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, R>));
        }

        public bool Equals(Port other) {
            return _settings == other._settings &&
                   (string.IsNullOrWhiteSpace(_name) ? string.IsNullOrWhiteSpace(other._name) : _name == other._name) &&
                   (_signature == null && other._signature == null || _signature == other._signature);
        }

        public override bool Equals(object obj) {
            return obj is Port other && Equals(other);
        }

        public override int GetHashCode() {
            return HashCode.Combine(_name, _settings, _signature);
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
