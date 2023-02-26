﻿using System;
using System.Collections.Generic;
using MisterGames.Common.Data;
using MisterGames.Common.Strings;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MisterGames.Blackboards.Core {

    [Serializable]
    public sealed class Blackboard {

        [SerializeField] private List<int> _properties;
        [SerializeField] private SerializedDictionary<int, BlackboardProperty> _propertiesMap;

        [SerializeField] private SerializedDictionary<int, bool> _bools;
        [SerializeField] private SerializedDictionary<int, int> _ints;
        [SerializeField] private SerializedDictionary<int, long> _longs;
        [SerializeField] private SerializedDictionary<int, float> _floats;
        [SerializeField] private SerializedDictionary<int, double> _doubles;
        [SerializeField] private SerializedDictionary<int, string> _strings;

        [SerializeField] private SerializedDictionary<int, Vector2Int> _vectors2Int;
        [SerializeField] private SerializedDictionary<int, Vector3Int> _vectors3Int;

        [SerializeField] private SerializedDictionary<int, Vector2> _vectors2;
        [SerializeField] private SerializedDictionary<int, Vector3> _vectors3;
        [SerializeField] private SerializedDictionary<int, Vector4> _vectors4;

        [SerializeField] private SerializedDictionary<int, Quaternion> _quaternions;

        [SerializeField] private SerializedDictionary<int, Color> _colors;
        [SerializeField] private SerializedDictionary<int, LayerMask> _layerMasks;
        [SerializeField] private SerializedDictionary<int, AnimationCurve> _curves;

        [SerializeField] private SerializedDictionary<int, BlackboardValue<Object>> _objects;
        [SerializeField] private SerializedDictionary<int, BlackboardValue<long>> _enums;
        [SerializeField] private SerializedDictionary<int, BlackboardReference> _references;

        public IReadOnlyList<int> Properties => _properties;

        public Blackboard() {
#if UNITY_EDITOR
            _properties = new List<int>();
            _propertiesMap = new SerializedDictionary<int, BlackboardProperty>();
#endif

            _bools = new SerializedDictionary<int, bool>();
            _ints = new SerializedDictionary<int, int>();
            _longs = new SerializedDictionary<int, long>();
            _floats = new SerializedDictionary<int, float>();
            _doubles = new SerializedDictionary<int, double>();
            _strings = new SerializedDictionary<int, string>();

            _vectors2Int = new SerializedDictionary<int, Vector2Int>();
            _vectors3Int = new SerializedDictionary<int, Vector3Int>();

            _vectors2 = new SerializedDictionary<int, Vector2>();
            _vectors3 = new SerializedDictionary<int, Vector3>();
            _vectors4 = new SerializedDictionary<int, Vector4>();

            _quaternions = new SerializedDictionary<int, Quaternion>();

            _colors = new SerializedDictionary<int, Color>();
            _layerMasks = new SerializedDictionary<int, LayerMask>();
            _curves = new SerializedDictionary<int, AnimationCurve>();

            _objects = new SerializedDictionary<int, BlackboardValue<Object>>();
            _enums = new SerializedDictionary<int, BlackboardValue<long>>();
            _references = new SerializedDictionary<int, BlackboardReference>();
        }

        public Blackboard(Blackboard source) {
#if UNITY_EDITOR
            _properties = new List<int>(source._properties);
            _propertiesMap = new SerializedDictionary<int, BlackboardProperty>(source._propertiesMap);
#endif

            _bools = new SerializedDictionary<int, bool>(source._bools);
            _ints = new SerializedDictionary<int, int>(source._ints);
            _longs = new SerializedDictionary<int, long>(source._longs);
            _floats = new SerializedDictionary<int, float>(source._floats);
            _doubles = new SerializedDictionary<int, double>(source._doubles);
            _strings = new SerializedDictionary<int, string>(source._strings);

            _vectors2Int = new SerializedDictionary<int, Vector2Int>(source._vectors2Int);
            _vectors3Int = new SerializedDictionary<int, Vector3Int>(source._vectors3Int);

            _vectors2 = new SerializedDictionary<int, Vector2>(source._vectors2);
            _vectors3 = new SerializedDictionary<int, Vector3>(source._vectors3);
            _vectors4 = new SerializedDictionary<int, Vector4>(source._vectors4);

            _quaternions = new SerializedDictionary<int, Quaternion>(source._quaternions);

            _colors = new SerializedDictionary<int, Color>(source._colors);
            _layerMasks = new SerializedDictionary<int, LayerMask>(source._layerMasks);
            _curves = new SerializedDictionary<int, AnimationCurve>(source._curves);

            _objects = new SerializedDictionary<int, BlackboardValue<Object>>(source._objects);
            _enums = new SerializedDictionary<int, BlackboardValue<long>>(source._enums);
            _references = new SerializedDictionary<int, BlackboardReference>(source._references);
        }

        public T Get<T>(int hash) {
            var type = typeof(T);

            if (type.IsValueType) {
                if (type == typeof(bool)) return _bools[hash] is T t ? t : default;

                if (type == typeof(float)) return _floats[hash] is T t ? t : default;
                if (type == typeof(double)) return _doubles[hash] is T t ? t : default;

                if (type == typeof(int)) return _ints[hash] is T t ? t : default;
                if (type == typeof(long)) return _longs[hash] is T t ? t : default;

                if (type == typeof(Vector2)) return _vectors2[hash] is T t ? t : default;
                if (type == typeof(Vector3)) return _vectors3[hash] is T t ? t : default;
                if (type == typeof(Vector4)) return _vectors4[hash] is T t ? t : default;

                if (type == typeof(Quaternion)) return _quaternions[hash] is T t ? t : default;

                if (type == typeof(Vector2Int)) return _vectors2Int[hash] is T t ? t : default;
                if (type == typeof(Vector3Int)) return _vectors3Int[hash] is T t ? t : default;

                if (type == typeof(LayerMask)) return _layerMasks[hash] is T t ? t : default;
                if (type == typeof(Color)) return _colors[hash] is T t ? t : default;

                if (type.IsEnum) {
                    var enumUnderlyingType = type.GetEnumUnderlyingType();

                    if (enumUnderlyingType == typeof(int)) return Enum.ToObject(type, (int) _enums[hash].value) is T t ? t : default;
                    if (enumUnderlyingType == typeof(short)) return Enum.ToObject(type, (short) _enums[hash].value) is T t ? t : default;
                    if (enumUnderlyingType == typeof(byte)) return Enum.ToObject(type, (byte) _enums[hash].value) is T t ? t : default;
                    if (enumUnderlyingType == typeof(long)) return Enum.ToObject(type, _enums[hash].value) is T t ? t : default;
                    if (enumUnderlyingType == typeof(sbyte)) return Enum.ToObject(type, (sbyte) _enums[hash].value) is T t ? t : default;
                    if (enumUnderlyingType == typeof(ushort)) return Enum.ToObject(type, (ushort) _enums[hash].value) is T t ? t : default;
                    if (enumUnderlyingType == typeof(uint)) return Enum.ToObject(type, (uint) _enums[hash].value) is T t ? t : default;

                    return default;
                }

                return default;
            }

            if (type == typeof(string)) return _strings[hash] is T t ? t : default;
            if (type == typeof(AnimationCurve)) return _curves[hash] is T t ? t : default;

            if (typeof(Object).IsAssignableFrom(type)) {
                return _objects.TryGetValue(hash, out var value) && value.value is T t ? t : default;
            }

            if (type.IsSubclassOf(typeof(object)) || type.IsInterface) {
                return _references.TryGetValue(hash, out var reference) && reference.value is T t ? t : default;
            }

            return default;
        }

#if UNITY_EDITOR
        private const string EDITOR = "editor";

        private static readonly HashSet<Type> SupportedValueTypes = new HashSet<Type> {
            typeof(bool),

            typeof(float),
            typeof(double),

            typeof(int),
            typeof(long),

            typeof(Vector2),
            typeof(Vector3),
            typeof(Vector4),

            typeof(Vector2Int),
            typeof(Vector3Int),

            typeof(Quaternion),

            typeof(LayerMask),
            typeof(Color),
        };

        private static readonly HashSet<Type> SupportedUnityManagedTypes = new HashSet<Type> {
            typeof(AnimationCurve),
        };

        private static readonly HashSet<Type> SupportedEnumUnderlyingTypes = new HashSet<Type> {
            typeof(int),
            typeof(short),
            typeof(byte),
            typeof(long),
            typeof(sbyte),
            typeof(ushort),
            typeof(uint),
        };

        private Blackboard _overridenBlackboard;

        public static bool IsSupportedType(Type type) {
            return
                type.IsVisible && (type.IsPublic || type.IsNestedPublic) && !type.IsGenericType &&
                type.FullName is not null && !type.FullName.Contains(EDITOR, StringComparison.OrdinalIgnoreCase) &&
                (
                    type.IsValueType && (
                        type.IsEnum && SupportedEnumUnderlyingTypes.Contains(type.GetEnumUnderlyingType()) ||
                        !type.IsEnum && SupportedValueTypes.Contains(type)
                    ) ||
                    !type.IsValueType && type != typeof(Blackboard) && (
                        typeof(Object).IsAssignableFrom(type) ||
                        Attribute.IsDefined(type, typeof(SerializableAttribute)) ||
                        type.IsInterface || SupportedUnityManagedTypes.Contains(type)
                    )
                );
        }

        public static int StringToHash(string name) {
            return name.GetHashCode();
        }

        public bool OverrideBlackboard(Blackboard blackboard) {
            _overridenBlackboard = blackboard;

            bool changed = false;
            for (int i = _properties.Count - 1; i >= 0; i--) {
                int hash = _properties[i];

                if (!blackboard._propertiesMap.TryGetValue(hash, out var p) || p.type == null) {
                    _properties.RemoveAt(i);
                    _propertiesMap.Remove(hash);

                    RemoveValue(hash);

                    changed = true;
                    continue;
                }

                var property = _propertiesMap[hash];

                if (p.type == property.type) continue;

                RemoveValue(hash);

                property.type = p.type;
                _propertiesMap[hash] = property;

                SetValue(property.type, hash, blackboard.GetValue(property.type, hash));

                changed = true;
            }

            for (int i = 0; i < blackboard._properties.Count; i++) {
                int hash = blackboard._properties[i];
                var property = blackboard._propertiesMap[hash];

                if (property.type == null) continue;

                if (!_propertiesMap.ContainsKey(hash)) {
                    _propertiesMap.Add(hash, property);
                    _properties.Add(hash);

                    SetValue(property.type, hash, blackboard.GetValue(property.type, hash));
                    changed = true;
                }

                changed |= TrySetPropertyIndex(hash, i);
            }

            return changed;
        }

        public bool TryAddProperty(string name, Type type) {
            if (!ValidateType(type)) return false;
            
            name = ValidateName(name);
            int hash = StringToHash(name);
            if (_propertiesMap.ContainsKey(hash)) return false;

            var property = new BlackboardProperty {
                name = name,
                type = new SerializedType(type),
            };

            SetValue(type, hash, default);

            _propertiesMap.Add(hash, property);
            _properties.Add(hash);

            return true;
        }

        public bool TryGetProperty(int hash, out BlackboardProperty property) {
            return _propertiesMap.TryGetValue(hash, out property);
        }

        public bool TryGetPropertyValue(int hash, out object value) {
            if (!_propertiesMap.TryGetValue(hash, out var property) || property.type == null) {
                value = default;
                return false;
            }

            value = GetValue(property.type, hash);
            return true;
        }

        public bool TrySetPropertyValue(int hash, object value) {
            if (!_propertiesMap.TryGetValue(hash, out var property) || property.type == null) return false;

            SetValue(property.type, hash, value);
            return true;
        }

        public bool TryResetPropertyValues() {
            bool changed = false;

            for (int i = 0; i < _properties.Count; i++) {
                int hash = _properties[i];
                var property = _propertiesMap[hash];

                if (property.type == null) continue;

                var type = (Type) property.type;
                object value = _overridenBlackboard != null &&
                               _overridenBlackboard.TryGetPropertyValue(hash, out object v) &&
                               v != null &&
                               type.IsInstanceOfType(v)
                    ? v : default;

                SetValue(property.type, hash, value);
                changed = true;
            }

            return changed;
        }

        public bool TryResetPropertyValue(int hash) {
            if (!_propertiesMap.TryGetValue(hash, out var property) || property.type == null) return false;

            var type = (Type) property.type;
            object value = _overridenBlackboard != null &&
                           _overridenBlackboard.TryGetPropertyValue(hash, out object v) &&
                           v != null &&
                           type.IsInstanceOfType(v)
                ? v : default;

            SetValue(type, hash, value);
            return true;
        }

        public bool TrySetPropertyName(int hash, string newName) {
            if (!_propertiesMap.TryGetValue(hash, out var property) || property.type == null) return false;

            newName = ValidateName(newName);
            int newHash = StringToHash(newName);
            if (_propertiesMap.ContainsKey(newHash)) return false;

            property.name = newName;
            _propertiesMap[hash] = property;

            for (int i = 0; i < _properties.Count; i++) {
                if (_properties[i] != hash) continue;

                _properties[i] = newHash;
                break;
            }

            object value = GetValue(property.type, hash);

            RemoveValue(hash);
            SetValue(property.type, newHash, value);

            return true;
        }

        public bool TrySetPropertyIndex(int hash, int newIndex) {
            if (newIndex < 0) return false;

            int oldIndex = -1;
            for (int i = 0; i < _properties.Count; i++) {
                if (_properties[i] != hash) continue;

                oldIndex = i;
                break;
            }

            if (oldIndex < 0 || oldIndex == newIndex) return false;

            if (newIndex >= _properties.Count) {
                _properties.RemoveAt(oldIndex);
                _properties.Add(hash);
                return true;
            }

            _properties[oldIndex] = _properties[newIndex];
            _properties[newIndex] = hash;

            return true;
        }

        public void RemoveProperty(int hash) {
            if (_propertiesMap.ContainsKey(hash)) {
                _propertiesMap.Remove(hash);
            }

            for (int i = _properties.Count - 1; i >= 0; i--) {
                if (_properties[i] != hash) continue;

                _properties.RemoveAt(i);
                break;
            }

            RemoveValue(hash);
        }

        private object GetValue(Type type, int hash) {
            if (type == null) return default;

            if (type.IsValueType) {
                if (type == typeof(bool)) return _bools[hash];

                if (type == typeof(float)) return _floats[hash];
                if (type == typeof(double)) return _doubles[hash];

                if (type == typeof(int)) return _ints[hash];
                if (type == typeof(long)) return _longs[hash];

                if (type == typeof(Vector2)) return _vectors2[hash];
                if (type == typeof(Vector3)) return _vectors3[hash];
                if (type == typeof(Vector4)) return _vectors4[hash];

                if (type == typeof(Quaternion)) return _quaternions[hash];

                if (type == typeof(Vector2Int)) return _vectors2Int[hash];
                if (type == typeof(Vector3Int)) return _vectors3Int[hash];

                if (type == typeof(LayerMask)) return _layerMasks[hash];
                if (type == typeof(Color)) return _colors[hash];

                if (type.IsEnum) {
                    var enumUnderlyingType = type.GetEnumUnderlyingType();

                    if (enumUnderlyingType == typeof(int)) return Enum.ToObject(type, (int) _enums[hash].value);
                    if (enumUnderlyingType == typeof(short)) return Enum.ToObject(type, (short) _enums[hash].value);
                    if (enumUnderlyingType == typeof(byte)) return Enum.ToObject(type, (byte) _enums[hash].value);
                    if (enumUnderlyingType == typeof(long)) return Enum.ToObject(type, _enums[hash].value);
                    if (enumUnderlyingType == typeof(sbyte)) return Enum.ToObject(type, (sbyte) _enums[hash].value);
                    if (enumUnderlyingType == typeof(ushort)) return Enum.ToObject(type, (ushort) _enums[hash].value);
                    if (enumUnderlyingType == typeof(uint)) return Enum.ToObject(type, (uint) _enums[hash].value);

                    return default;
                }

                return default;
            }

            if (type == typeof(string)) return _strings[hash];
            if (type == typeof(AnimationCurve)) return _curves[hash];

            if (typeof(Object).IsAssignableFrom(type)) return _objects[hash].value;
            if (type.IsSubclassOf(typeof(object)) || type.IsInterface) return _references[hash].value;

            return default;
        }

        private void SetValue(Type type, int hash, object value) {
            if (type == null) return;

            if (type.IsValueType) {
                if (type == typeof(bool)) {
                    _bools[hash] = value is bool b ? b : default;
                    return;
                }

                if (type == typeof(float)) {
                    _floats[hash] = value is float f ? f : default;
                    return;
                }

                if (type == typeof(double)) {
                    _doubles[hash] = value is double d ? d : default;
                    return;
                }

                if (type == typeof(int)) {
                    _ints[hash] = value is int i ? i : default;
                    return;
                }

                if (type == typeof(long)) {
                    _longs[hash] = value is long l ? l : default;
                    return;
                }

                if (type == typeof(Vector2)) {
                    _vectors2[hash] = value is Vector2 v2 ? v2 : default;
                    return;
                }

                if (type == typeof(Vector3)) {
                    _vectors3[hash] = value is Vector3 v3 ? v3 : default;
                    return;
                }

                if (type == typeof(Vector4)) {
                    _vectors4[hash] = value is Vector4 v4 ? v4 : default;
                    return;
                }

                if (type == typeof(Vector2Int)) {
                    _vectors2Int[hash] = value is Vector2Int v2 ? v2 : default;
                    return;
                }

                if (type == typeof(Vector3Int)) {
                    _vectors3Int[hash] = value is Vector3Int v3 ? v3 : default;
                    return;
                }

                if (type == typeof(Quaternion)) {
                    _quaternions[hash] = value is Quaternion q ? q : default;
                    return;
                }

                if (type == typeof(Color)) {
                    _colors[hash] = value is Color c ? c : default;
                    return;
                }

                if (type == typeof(LayerMask)) {
                    _layerMasks[hash] = value is LayerMask m ? m : default;
                    return;
                }

                if (type.IsEnum) {
                    if (value == null) {
                        _enums[hash] = default;
                        return;
                    }

                    var enumUnderlyingType = type.GetEnumUnderlyingType();

                    if (enumUnderlyingType == typeof(int)) {
                        _enums[hash] = new BlackboardValue<long> { value = (int) Enum.ToObject(type, value) };
                        return;
                    }

                    if (enumUnderlyingType == typeof(short)) {
                        _enums[hash] = new BlackboardValue<long> { value = (short) Enum.ToObject(type, value) };
                        return;
                    }

                    if (enumUnderlyingType == typeof(byte)) {
                        _enums[hash] = new BlackboardValue<long> { value = (byte) Enum.ToObject(type, value) };
                        return;
                    }

                    if (enumUnderlyingType == typeof(long)) {
                        _enums[hash] = new BlackboardValue<long> { value = (long) Enum.ToObject(type, value) };
                        return;
                    }

                    if (enumUnderlyingType == typeof(sbyte)) {
                        _enums[hash] = new BlackboardValue<long> { value = (sbyte) Enum.ToObject(type, value) };
                        return;
                    }

                    if (enumUnderlyingType == typeof(ushort)) {
                        _enums[hash] = new BlackboardValue<long> { value = (ushort) Enum.ToObject(type, value) };
                        return;
                    }

                    if (enumUnderlyingType == typeof(uint)) {
                        _enums[hash] = new BlackboardValue<long> { value = (uint) Enum.ToObject(type, value) };
                        return;
                    }

                    return;
                }

                return;
            }

            if (type == typeof(string)) {
                _strings[hash] = value as string;
                return;
            }

            if (type == typeof(AnimationCurve)) {
                _curves[hash] = value as AnimationCurve;
                return;
            }

            if (typeof(Object).IsAssignableFrom(type)) {
                _objects[hash] = new BlackboardValue<Object> { value = value as Object };
                return;
            }

            if (type.IsSubclassOf(typeof(object)) || type.IsInterface) {
                _references[hash] = new BlackboardReference {
                    value = value == null ? null : JsonUtility.FromJson(JsonUtility.ToJson(value), value.GetType()),
                };
                return;
            }
        }

        private void RemoveValue(int hash) {
            if (_bools.ContainsKey(hash)) {
                _bools.Remove(hash);
                return;
            }

            if (_ints.ContainsKey(hash)) {
                _ints.Remove(hash);
                return;
            }

            if (_longs.ContainsKey(hash)) {
                _longs.Remove(hash);
                return;
            }

            if (_floats.ContainsKey(hash)) {
                _floats.Remove(hash);
                return;
            }

            if (_doubles.ContainsKey(hash)) {
                _doubles.Remove(hash);
                return;
            }

            if (_vectors2.ContainsKey(hash)) {
                _vectors2.Remove(hash);
                return;
            }

            if (_vectors3.ContainsKey(hash)) {
                _vectors3.Remove(hash);
                return;
            }

            if (_vectors4.ContainsKey(hash)) {
                _vectors4.Remove(hash);
                return;
            }

            if (_vectors2Int.ContainsKey(hash)) {
                _vectors2.Remove(hash);
                return;
            }

            if (_vectors3Int.ContainsKey(hash)) {
                _vectors3.Remove(hash);
                return;
            }

            if (_quaternions.ContainsKey(hash)) {
                _quaternions.Remove(hash);
                return;
            }

            if (_colors.ContainsKey(hash)) {
                _colors.Remove(hash);
                return;
            }

            if (_layerMasks.ContainsKey(hash)) {
                _layerMasks.Remove(hash);
                return;
            }

            if (_strings.ContainsKey(hash)) {
                _strings.Remove(hash);
                return;
            }

            if (_curves.ContainsKey(hash)) {
                _curves.Remove(hash);
                return;
            }

            if (_objects.ContainsKey(hash)) {
                _objects.Remove(hash);
                return;
            }

            if (_references.ContainsKey(hash)) {
                _references.Remove(hash);
                return;
            }
        }

        private string ValidateName(string name) {
            int hash = StringToHash(name);
            if (!_propertiesMap.ContainsKey(hash)) return name;

            int count = 1;
            string pattern = $@"{name} \([0-9]+\)";

            for (int i = 0; i < _properties.Count; i++) {
                if (_propertiesMap[_properties[i]].name.IsValidForPattern(pattern)) count++;
            }

            return $"{name} ({count})";
        }

        private static bool ValidateType(Type type) {
            if (IsSupportedType(type)) return true;

            Debug.LogError($"Blackboard does not support type {type.Name}");
            return false;
        }
#endif
    }

}
