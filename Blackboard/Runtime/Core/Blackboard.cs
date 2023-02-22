using System;
using System.Collections.Generic;
using MisterGames.Common.Data;
using MisterGames.Common.Strings;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MisterGames.Blackboards.Core {

    [Serializable]
    public sealed class Blackboard {

        [SerializeField] private List<BlackboardProperty> _properties = new List<BlackboardProperty>();

        // Primitives
        [SerializeField] private SerializedDictionary<int, bool> _bools = new SerializedDictionary<int, bool>();
        [SerializeField] private SerializedDictionary<int, long> _longs = new SerializedDictionary<int, long>();
        [SerializeField] private SerializedDictionary<int, double> _doubles = new SerializedDictionary<int, double>();
        [SerializeField] private SerializedDictionary<int, string> _strings = new SerializedDictionary<int, string>();

        // Unity native types
        [SerializeField] private SerializedDictionary<int, Vector2Int> _vectors2Int = new SerializedDictionary<int, Vector2Int>();
        [SerializeField] private SerializedDictionary<int, Vector3Int> _vectors3Int = new SerializedDictionary<int, Vector3Int>();

        [SerializeField] private SerializedDictionary<int, Vector2> _vectors2 = new SerializedDictionary<int, Vector2>();
        [SerializeField] private SerializedDictionary<int, Vector3> _vectors3 = new SerializedDictionary<int, Vector3>();
        [SerializeField] private SerializedDictionary<int, Vector4> _vectors4 = new SerializedDictionary<int, Vector4>();

        [SerializeField] private SerializedDictionary<int, Quaternion> _quaternions = new SerializedDictionary<int, Quaternion>();

        [SerializeField] private SerializedDictionary<int, Color> _colors = new SerializedDictionary<int, Color>();
        [SerializeField] private SerializedDictionary<int, LayerMask> _layerMasks = new SerializedDictionary<int, LayerMask>();
        [SerializeField] private SerializedDictionary<int, AnimationCurve> _curves = new SerializedDictionary<int, AnimationCurve>();

        // Unity Objects
        [SerializeField] private SerializedDictionary<int, Object> _objects = new SerializedDictionary<int, Object>();

        // References
        [SerializeField] private SerializedDictionary<int, BlackboardReference> _references = new SerializedDictionary<int, BlackboardReference>();

        public IReadOnlyList<BlackboardProperty> Properties => _properties;

        public Blackboard() { }

        public Blackboard(Blackboard source) {
#if UNITY_EDITOR
            _properties = new List<BlackboardProperty>(source._properties);
#endif

            _bools = new SerializedDictionary<int, bool>(source._bools);
            _longs = new SerializedDictionary<int, long>(source._longs);
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

            _objects = new SerializedDictionary<int, Object>(source._objects);
            _references = new SerializedDictionary<int, BlackboardReference>(source._references);
        }

        public T Get<T>(int hash) {
            var type = typeof(T);

            if (type.IsValueType) {
                if (type == typeof(bool)) return _bools[hash] is T t ? t : default;

                if (type == typeof(float)) return (float) _doubles[hash] is T t ? t : default;
                if (type == typeof(double)) return _doubles[hash] is T t ? t : default;

                if (type == typeof(int)) return (int) _longs[hash] is T t ? t : default;
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

                    if (enumUnderlyingType == typeof(int)) return Enum.ToObject(type, (int) _longs[hash]) is T t ? t : default;
                    if (enumUnderlyingType == typeof(short)) return Enum.ToObject(type, (short) _longs[hash]) is T t ? t : default;
                    if (enumUnderlyingType == typeof(byte)) return Enum.ToObject(type, (byte) _longs[hash]) is T t ? t : default;
                    if (enumUnderlyingType == typeof(long)) return Enum.ToObject(type, _longs[hash]) is T t ? t : default;
                    if (enumUnderlyingType == typeof(sbyte)) return Enum.ToObject(type, (sbyte) _longs[hash]) is T t ? t : default;
                    if (enumUnderlyingType == typeof(ushort)) return Enum.ToObject(type, (ushort) _longs[hash]) is T t ? t : default;
                    if (enumUnderlyingType == typeof(uint)) return Enum.ToObject(type, (uint) _longs[hash]) is T t ? t : default;

                    return default;
                }

                return default;
            }

            if (type == typeof(string)) return _strings[hash] is T t ? t : default;
            if (type == typeof(AnimationCurve)) return _curves[hash] is T t ? t : default;

            if (typeof(Object).IsAssignableFrom(type)) {
                return _objects.TryGetValue(hash, out var obj) && obj is T t ? t : default;
            }

            if (type.IsSubclassOf(typeof(object)) || type.IsInterface) {
                return _references.TryGetValue(hash, out var reference) && reference.data is T t ? t : default;
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
                var property = _properties[i];

                if (!blackboard.HasProperty(property.hash) || !blackboard.TryGetProperty(property.hash, out var p) || p.type == null) {
                    _properties.RemoveAt(i);

                    if (property.type == null) RemoveValueByHash(property.hash);
                    else RemoveValue(property.type, property.hash);

                    changed = true;
                    continue;
                }

                if (p.type == property.type) continue;

                if (property.type == null) RemoveValueByHash(property.hash);
                else RemoveValue(property.type, property.hash);

                property.type = p.type;
                _properties[i] = property;
                SetValue(property.type, property.hash, blackboard.GetValue(property.type, property.hash));

                changed = true;
            }

            for (int i = 0; i < blackboard.Properties.Count; i++) {
                var property = blackboard.Properties[i];
                if (property.type == null) continue;

                if (!HasProperty(property.hash)) {
                    _properties.Add(property);
                    SetValue(property.type, property.hash, blackboard.GetValue(property.type, property.hash));
                    changed = true;
                }

                changed |= TrySetPropertyIndex(property.hash, i);
            }

            return changed;
        }

        public bool TryAddProperty(string name, Type type, out BlackboardProperty property) {
            property = default;
            if (!ValidateType(type)) return false;
            
            name = ValidateName(name);
            int hash = StringToHash(name);
            if (HasProperty(hash)) return false;

            property = new BlackboardProperty {
                hash = hash,
                name = name,
                type = new SerializedType(type),
            };

            SetValue(type, hash, default);

            _properties.Add(property);
            return true;
        }

        public bool TryGetPropertyValue(int hash, out object value) {
            if (!TryGetProperty(hash, out var property) || property.type == null) {
                value = default;
                return false;
            }

            value = GetValue(property.type, hash);
            return true;
        }

        public bool TryGetProperty(int hash, out BlackboardProperty property) {
            return TryGetProperty(hash, out int _, out property);
        }

        public bool TrySetPropertyValue(int hash, object value) {
            if (!TryGetProperty(hash, out var property) || property.type == null) return false;

            SetValue(property.type, hash, value);
            return true;
        }

        public bool TrySetPropertyValueAtIndex(int index, object value) {
            if (index < 0 || index > _properties.Count - 1) return false;

            var property = _properties[index];
            if (property.type == null) return false;

            SetValue(property.type, property.hash, value);
            return true;
        }

        public bool TryResetPropertyValues() {
            bool changed = false;

            for (int i = 0; i < _properties.Count; i++) {
                var property = _properties[i];
                if (property.type == null) continue;

                var type = (Type) property.type;
                object value = _overridenBlackboard != null &&
                               _overridenBlackboard.TryGetPropertyValue(property.hash, out object v) &&
                               v != null &&
                               type.IsInstanceOfType(v)
                    ? v : default;

                SetValue(property.type, property.hash, value);
                changed = true;
            }

            return changed;
        }

        public bool TryResetPropertyValue(int hash) {
            if (!TryGetProperty(hash, out var property) || property.type == null) return false;

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
            if (!TryGetProperty(hash, out int index, out var property) || property.type == null) return false;

            newName = ValidateName(newName);
            int newHash = StringToHash(newName);
            if (HasProperty(newHash)) return false;

            property.name = newName;
            property.hash = newHash;
            _properties[index] = property;

            object value = GetValue(property.type, hash);

            RemoveValue(property.type, hash);
            SetValue(property.type, newHash, value);

            return true;
        }

        public bool TrySetPropertyIndex(int hash, int newIndex) {
            if (newIndex < 0) return false;
            if (!TryGetProperty(hash, out int oldIndex, out var property) || oldIndex == newIndex) return false;

            if (newIndex >= _properties.Count) {
                _properties.RemoveAt(oldIndex);
                _properties.Add(property);
                return true;
            }

            _properties[oldIndex] = _properties[newIndex];
            _properties[newIndex] = property;

            return true;
        }

        public void RemoveProperty(int hash) {
            if (!TryGetProperty(hash, out int index, out var property)) return;

            _properties.RemoveAt(index);

            if (property.type == null) RemoveValueByHash(hash);
            else RemoveValue(property.type, hash);
        }

        private bool TryGetProperty(int hash, out int index, out BlackboardProperty property) {
            for (int i = 0; i < _properties.Count; i++) {
                var p = _properties[i];
                if (p.hash != hash) continue;

                property = p;
                index = i;
                return true;
            }

            property = default;
            index = -1;

            return false;
        }

        private bool HasProperty(int hash) {
            return
                _bools.ContainsKey(hash) ||
                _longs.ContainsKey(hash) ||
                _doubles.ContainsKey(hash) ||
                _strings.ContainsKey(hash) ||

                _vectors2.ContainsKey(hash) ||
                _vectors3.ContainsKey(hash) ||
                _vectors4.ContainsKey(hash) ||

                _vectors2Int.ContainsKey(hash) ||
                _vectors3Int.ContainsKey(hash) ||

                _quaternions.ContainsKey(hash) ||

                _colors.ContainsKey(hash) ||
                _layerMasks.ContainsKey(hash) ||
                _curves.ContainsKey(hash) ||

                _objects.ContainsKey(hash) ||
                _references.ContainsKey(hash);
        }

        private object GetValue(Type type, int hash) {
            if (type == null) return default;

            if (type.IsValueType) {
                if (type == typeof(bool)) return _bools[hash];

                if (type == typeof(float)) return (float) _doubles[hash];
                if (type == typeof(double)) return _doubles[hash];

                if (type == typeof(int)) return (int) _longs[hash];
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

                    if (enumUnderlyingType == typeof(int)) return Enum.ToObject(type, (int) _longs[hash]);
                    if (enumUnderlyingType == typeof(short)) return Enum.ToObject(type, (short) _longs[hash]);
                    if (enumUnderlyingType == typeof(byte)) return Enum.ToObject(type, (byte) _longs[hash]);
                    if (enumUnderlyingType == typeof(long)) return Enum.ToObject(type, _longs[hash]);
                    if (enumUnderlyingType == typeof(sbyte)) return Enum.ToObject(type, (sbyte) _longs[hash]);
                    if (enumUnderlyingType == typeof(ushort)) return Enum.ToObject(type, (ushort) _longs[hash]);
                    if (enumUnderlyingType == typeof(uint)) return Enum.ToObject(type, (uint) _longs[hash]);

                    return default;
                }

                return default;
            }

            if (type == typeof(string)) return _strings[hash];
            if (type == typeof(AnimationCurve)) return _curves[hash];

            if (typeof(Object).IsAssignableFrom(type)) return _objects[hash];
            if (type.IsSubclassOf(typeof(object)) || type.IsInterface) return _references[hash].data;

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
                    _doubles[hash] = value is float f ? f : default;
                    return;
                }

                if (type == typeof(double)) {
                    _doubles[hash] = value is double d ? d : default;
                    return;
                }

                if (type == typeof(int)) {
                    _longs[hash] = value is int i ? i : default;
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
                        _longs[hash] = 0;
                        return;
                    }

                    var enumUnderlyingType = type.GetEnumUnderlyingType();

                    if (enumUnderlyingType == typeof(int)) {
                        _longs[hash] = (int) Enum.ToObject(type, value);
                        return;
                    }

                    if (enumUnderlyingType == typeof(short)) {
                        _longs[hash] = (short) Enum.ToObject(type, value);
                        return;
                    }

                    if (enumUnderlyingType == typeof(byte)) {
                        _longs[hash] = (byte) Enum.ToObject(type, value);
                        return;
                    }

                    if (enumUnderlyingType == typeof(long)) {
                        _longs[hash] = (long) Enum.ToObject(type, value);
                        return;
                    }

                    if (enumUnderlyingType == typeof(sbyte)) {
                        _longs[hash] = (sbyte) Enum.ToObject(type, value);
                        return;
                    }

                    if (enumUnderlyingType == typeof(ushort)) {
                        _longs[hash] = (ushort) Enum.ToObject(type, value);
                        return;
                    }

                    if (enumUnderlyingType == typeof(uint)) {
                        _longs[hash] = (uint) Enum.ToObject(type, value);
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
                _objects[hash] = value as Object;
                return;
            }

            if (type.IsSubclassOf(typeof(object)) || type.IsInterface) {
                _references[hash] = new BlackboardReference {
                    data = value == null ? null : JsonUtility.FromJson(JsonUtility.ToJson(value), value.GetType()),
                    type = new SerializedType(type),
                };
                return;
            }
        }

        private void RemoveValue(Type type, int hash) {
            if (type.IsValueType) {
                if (type == typeof(bool)) {
                    _bools.Remove(hash);
                    return;
                }

                if (type == typeof(float)) {
                    _doubles.Remove(hash);
                    return;
                }

                if (type == typeof(double)) {
                    _doubles.Remove(hash);
                    return;
                }

                if (type == typeof(int)) {
                    _longs.Remove(hash);
                    return;
                }

                if (type == typeof(long)) {
                    _longs.Remove(hash);
                    return;
                }

                if (type == typeof(Vector2)) {
                    _vectors2.Remove(hash);
                    return;
                }

                if (type == typeof(Vector3)) {
                    _vectors3.Remove(hash);
                    return;
                }

                if (type == typeof(Vector4)) {
                    _vectors4.Remove(hash);
                    return;
                }

                if (type == typeof(Vector2Int)) {
                    _vectors2Int.Remove(hash);
                    return;
                }

                if (type == typeof(Vector3Int)) {
                    _vectors3Int.Remove(hash);
                    return;
                }

                if (type == typeof(Quaternion)) {
                    _quaternions.Remove(hash);
                    return;
                }

                if (type == typeof(Color)) {
                    _colors.Remove(hash);
                    return;
                }

                if (type == typeof(LayerMask)) {
                    _layerMasks.Remove(hash);
                    return;
                }

                if (type.IsEnum) {
                    _longs.Remove(hash);
                    return;
                }
            }

            if (type == typeof(string)) {
                _strings.Remove(hash);
                return;
            }

            if (type == typeof(AnimationCurve)) {
                _curves.Remove(hash);
                return;
            }

            if (typeof(Object).IsAssignableFrom(type)) {
                _objects.Remove(hash);
                return;
            }

            if (type.IsSubclassOf(typeof(object)) || type.IsInterface) {
                _references.Remove(hash);
                return;
            }
        }

        private void RemoveValueByHash(int hash) {
            if (_bools.ContainsKey(hash)) {
                _bools.Remove(hash);
                return;
            }

            if (_doubles.ContainsKey(hash)) {
                _doubles.Remove(hash);
                return;
            }

            if (_longs.ContainsKey(hash)) {
                _longs.Remove(hash);
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
            if (!HasProperty(hash)) return name;

            int count = 1;
            string pattern = $@"{name} \([0-9]+\)";

            for (int i = 0; i < _properties.Count; i++) {
                var property = _properties[i];
                if (property.name.IsValidForPattern(pattern)) count++;
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
