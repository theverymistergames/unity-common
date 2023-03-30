using System;
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

        [SerializeField] private SerializedDictionary<int, bool[]> _boolArrays;
        [SerializeField] private SerializedDictionary<int, int[]> _intArrays;
        [SerializeField] private SerializedDictionary<int, long[]> _longArrays;
        [SerializeField] private SerializedDictionary<int, float[]> _floatArrays;
        [SerializeField] private SerializedDictionary<int, double[]> _doubleArrays;
        [SerializeField] private SerializedDictionary<int, string[]> _stringArrays;

        [SerializeField] private SerializedDictionary<int, Vector2Int[]> _vectors2IntArrays;
        [SerializeField] private SerializedDictionary<int, Vector3Int[]> _vectors3IntArrays;
        [SerializeField] private SerializedDictionary<int, Vector2[]> _vectors2Arrays;
        [SerializeField] private SerializedDictionary<int, Vector3[]> _vectors3Arrays;
        [SerializeField] private SerializedDictionary<int, Vector4[]> _vectors4Arrays;
        [SerializeField] private SerializedDictionary<int, Quaternion[]> _quaternionArrays;

        [SerializeField] private SerializedDictionary<int, Color[]> _colorArrays;
        [SerializeField] private SerializedDictionary<int, LayerMask[]> _layerMaskArrays;
        [SerializeField] private SerializedDictionary<int, AnimationCurve[]> _curveArrays;

        [SerializeField] private SerializedDictionary<int, BlackboardValue<Object>[]> _objectArrays;
        [SerializeField] private SerializedDictionary<int, BlackboardValue<long>[]> _enumArrays;
        [SerializeField] private SerializedDictionary<int, BlackboardReference[]> _referenceArrays;

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

            _boolArrays = new SerializedDictionary<int, bool[]>();
            _intArrays = new SerializedDictionary<int, int[]>();
            _longArrays = new SerializedDictionary<int, long[]>();
            _floatArrays = new SerializedDictionary<int, float[]>();
            _doubleArrays = new SerializedDictionary<int, double[]>();
            _stringArrays = new SerializedDictionary<int, string[]>();

            _vectors2IntArrays = new SerializedDictionary<int, Vector2Int[]>();
            _vectors3IntArrays = new SerializedDictionary<int, Vector3Int[]>();
            _vectors2Arrays = new SerializedDictionary<int, Vector2[]>();
            _vectors3Arrays = new SerializedDictionary<int, Vector3[]>();
            _vectors4Arrays = new SerializedDictionary<int, Vector4[]>();
            _quaternionArrays = new SerializedDictionary<int, Quaternion[]>();

            _colorArrays = new SerializedDictionary<int, Color[]>();
            _layerMaskArrays = new SerializedDictionary<int, LayerMask[]>();
            _curveArrays = new SerializedDictionary<int, AnimationCurve[]>();

            _objectArrays = new SerializedDictionary<int, BlackboardValue<Object>[]>();
            _enumArrays = new SerializedDictionary<int, BlackboardValue<long>[]>();
            _referenceArrays = new SerializedDictionary<int, BlackboardReference[]>();
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

            _boolArrays = new SerializedDictionary<int, bool[]>(source._boolArrays);
            _intArrays = new SerializedDictionary<int, int[]>(source._intArrays);
            _longArrays = new SerializedDictionary<int, long[]>(source._longArrays);
            _floatArrays = new SerializedDictionary<int, float[]>(source._floatArrays);
            _doubleArrays = new SerializedDictionary<int, double[]>(source._doubleArrays);
            _stringArrays = new SerializedDictionary<int, string[]>(source._stringArrays);

            _vectors2IntArrays = new SerializedDictionary<int, Vector2Int[]>(source._vectors2IntArrays);
            _vectors3IntArrays = new SerializedDictionary<int, Vector3Int[]>(source._vectors3IntArrays);
            _vectors2Arrays = new SerializedDictionary<int, Vector2[]>(source._vectors2Arrays);
            _vectors3Arrays = new SerializedDictionary<int, Vector3[]>(source._vectors3Arrays);
            _vectors4Arrays = new SerializedDictionary<int, Vector4[]>(source._vectors4Arrays);
            _quaternionArrays = new SerializedDictionary<int, Quaternion[]>(source._quaternionArrays);

            _colorArrays = new SerializedDictionary<int, Color[]>(source._colorArrays);
            _layerMaskArrays = new SerializedDictionary<int, LayerMask[]>(source._layerMaskArrays);
            _curveArrays = new SerializedDictionary<int, AnimationCurve[]>(source._curveArrays);

            _objectArrays = new SerializedDictionary<int, BlackboardValue<Object>[]>(source._objectArrays);
            _enumArrays = new SerializedDictionary<int, BlackboardValue<long>[]>(source._enumArrays);
            _referenceArrays = new SerializedDictionary<int, BlackboardReference[]>(source._referenceArrays);
        }

        public T Get<T>(int hash) {
            var type = typeof(T);

            if (type.IsArray) {
                type = type.GetElementType()!;

                if (type.IsValueType) {
                    if (type == typeof(bool)) return _boolArrays.TryGetValue(hash, out bool[] a) && a is T t ? t : default;
                    if (type == typeof(float)) return _floatArrays.TryGetValue(hash, out float[] a) && a is T t ? t : default;
                    if (type == typeof(double)) return _doubleArrays.TryGetValue(hash, out double[] a) && a is T t ? t : default;
                    if (type == typeof(int)) return _intArrays.TryGetValue(hash, out int[] a) && a is T t ? t : default;
                    if (type == typeof(long)) return _longArrays.TryGetValue(hash, out long[] a) && a is T t ? t : default;

                    if (type == typeof(Vector2)) return _vectors2Arrays.TryGetValue(hash, out var a) && a is T t ? t : default;
                    if (type == typeof(Vector3)) return _vectors3Arrays.TryGetValue(hash, out var a) && a is T t ? t : default;
                    if (type == typeof(Vector4)) return _vectors4Arrays.TryGetValue(hash, out var a) && a is T t ? t : default;
                    if (type == typeof(Vector2Int)) return _vectors2IntArrays.TryGetValue(hash, out var a) && a is T t ? t : default;
                    if (type == typeof(Vector3Int)) return _vectors3IntArrays.TryGetValue(hash, out var a) && a is T t ? t : default;
                    if (type == typeof(Quaternion)) return _quaternionArrays.TryGetValue(hash, out var a) && a is T t ? t : default;

                    if (type == typeof(LayerMask)) return _layerMaskArrays.TryGetValue(hash, out var a) && a is T t ? t : default;
                    if (type == typeof(Color)) return _colorArrays.TryGetValue(hash, out var a) && a is T t ? t : default;

                    if (type.IsEnum) {
                        if (!_enumArrays.TryGetValue(hash, out var array) || array == null) return default;

                        var res = Array.CreateInstance(type, array.Length);
                        for (int i = 0; i < array.Length; i++) {
                            res.SetValue(Enum.ToObject(type, array[i].value), i);
                        }
                        return res is T t ? t : default;
                    }

                    return default;
                }

                if (type == typeof(string)) return _stringArrays.TryGetValue(hash, out string[] a) && a is T t ? t : default;
                if (type == typeof(AnimationCurve)) return _curveArrays.TryGetValue(hash, out var a) && a is T t ? t : default;

                if (typeof(Object).IsAssignableFrom(type)) {
                    if (!_objectArrays.TryGetValue(hash, out var array) || array == null) return default;

                    var result = Array.CreateInstance(type, array.Length);
                    for (int i = 0; i < array.Length; i++) {
                        var value = array[i].value;
                        if (value != null) result.SetValue(value, i);
                    }
                    return result is T t ? t : default;
                }

                if (type.IsSubclassOf(typeof(object)) || type.IsInterface) {
                    if (!_referenceArrays.TryGetValue(hash, out var array) || array == null) return default;

                    var result = Array.CreateInstance(type, array.Length);
                    for (int i = 0; i < array.Length; i++) {
                        object value = array[i].value;
                        if (value != null) result.SetValue(value, i);
                    }
                    return result is T t ? t : default;
                }

                return default;
            }

            if (type.IsValueType) {
                if (type == typeof(bool)) return _bools.TryGetValue(hash, out bool b) && b is T t ? t : default;

                if (type == typeof(float)) return _floats.TryGetValue(hash, out float f) && f is T t ? t : default;
                if (type == typeof(double)) return _doubles.TryGetValue(hash, out double d) && d is T t ? t : default;

                if (type == typeof(int)) return _ints.TryGetValue(hash, out int i) && i is T t ? t : default;
                if (type == typeof(long)) return _longs.TryGetValue(hash, out long l) && l is T t ? t : default;

                if (type == typeof(Vector2)) return _vectors2.TryGetValue(hash, out var v) && v is T t ? t : default;
                if (type == typeof(Vector3)) return _vectors3.TryGetValue(hash, out var v) && v is T t ? t : default;
                if (type == typeof(Vector4)) return _vectors4.TryGetValue(hash, out var v) && v is T t ? t : default;

                if (type == typeof(Quaternion)) return _quaternions.TryGetValue(hash, out var v) && v is T t ? t : default;

                if (type == typeof(Vector2Int)) return _vectors2Int.TryGetValue(hash, out var v) && v is T t ? t : default;
                if (type == typeof(Vector3Int)) return _vectors3Int.TryGetValue(hash, out var v) && v is T t ? t : default;

                if (type == typeof(LayerMask)) return _layerMasks.TryGetValue(hash, out var v) && v is T t ? t : default;
                if (type == typeof(Color)) return _colors.TryGetValue(hash, out var v) && v is T t ? t : default;

                if (type.IsEnum) return _enums.TryGetValue(hash, out var v) && Enum.ToObject(type, v.value) is T t ? t : default;

                return default;
            }

            if (type == typeof(string)) return _strings.TryGetValue(hash, out string s) && s is T t ? t : default;
            if (type == typeof(AnimationCurve)) return _curves.TryGetValue(hash, out var v) && v is T t ? t : default;

            if (typeof(Object).IsAssignableFrom(type)) return _objects.TryGetValue(hash, out var v) && v.value is T t ? t : default;
            if (type.IsSubclassOf(typeof(object)) || type.IsInterface) return _references.TryGetValue(hash, out var v) && v.value is T t ? t : default;

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

        public static bool IsSupportedType(Type t) {
            if (t.IsArray) t = t.GetElementType()!;

            return
                t.IsVisible && (t.IsPublic || t.IsNestedPublic) && !t.IsGenericType &&
                t.FullName is not null && !t.FullName.Contains(EDITOR, StringComparison.OrdinalIgnoreCase) &&
                (
                    t.IsValueType && (
                        t.IsEnum && SupportedEnumUnderlyingTypes.Contains(t.GetEnumUnderlyingType()) ||
                        !t.IsEnum && SupportedValueTypes.Contains(t)
                    ) ||
                    !t.IsValueType && t != typeof(Blackboard) && (
                        typeof(Object).IsAssignableFrom(t) ||
                        Attribute.IsDefined(t, typeof(SerializableAttribute)) ||
                        t.IsInterface || SupportedUnityManagedTypes.Contains(t)
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

            var property = new BlackboardProperty { name = name, type = new SerializedType(type) };

            SetValue(type, hash, default);

            _propertiesMap.Add(hash, property);
            _properties.Add(hash);

            return true;
        }

        public bool TryGetProperty(int hash, out BlackboardProperty property) {
            return _propertiesMap.TryGetValue(hash, out property);
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

            _propertiesMap.Remove(hash);
            _propertiesMap[newHash] = property;

            for (int i = 0; i < _properties.Count; i++) {
                if (_properties[i] != hash) continue;

                _properties[i] = newHash;
                break;
            }

            var type = (Type) property.type;
            object value = GetValue(type, hash);

            RemoveValue(hash);
            SetValue(type, newHash, value);

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

        private bool TryGetPropertyValue(int hash, out object value) {
            if (!_propertiesMap.TryGetValue(hash, out var property) || property.type == null) {
                value = default;
                return false;
            }

            value = GetValue(property.type, hash);
            return true;
        }

        private object GetValue(Type type, int hash) {
            if (type == null) return default;

            if (type.IsArray) {
                var elementType = type.GetElementType()!;

                if (elementType.IsValueType) {
                    if (elementType == typeof(bool)) return _boolArrays[hash];
                    if (elementType == typeof(float)) return _floatArrays[hash];
                    if (elementType == typeof(double)) return _doubleArrays[hash];
                    if (elementType == typeof(int)) return _intArrays[hash];
                    if (elementType == typeof(long)) return _longArrays[hash];

                    if (elementType == typeof(Vector2)) return _vectors2Arrays[hash];
                    if (elementType == typeof(Vector3)) return _vectors3Arrays[hash];
                    if (elementType == typeof(Vector4)) return _vectors4Arrays[hash];
                    if (elementType == typeof(Vector2Int)) return _vectors2IntArrays[hash];
                    if (elementType == typeof(Vector3Int)) return _vectors3IntArrays[hash];
                    if (elementType == typeof(Quaternion)) return _quaternionArrays[hash];

                    if (elementType == typeof(LayerMask)) return _layerMaskArrays[hash];
                    if (elementType == typeof(Color)) return _colorArrays[hash];

                    if (elementType.IsEnum) {
                        var array = _enumArrays[hash];
                        if (array == null) return default;

                        var result = Array.CreateInstance(elementType, array.Length);
                        for (int i = 0; i < array.Length; i++) {
                            result.SetValue(Enum.ToObject(elementType, array[i].value), i);
                        }
                        return result;
                    }

                    return default;
                }

                if (elementType == typeof(string)) return _stringArrays[hash];
                if (elementType == typeof(AnimationCurve)) return _curveArrays[hash];

                if (typeof(Object).IsAssignableFrom(elementType)) {
                    var array = _objectArrays[hash];
                    if (array == null) return default;

                    var result = Array.CreateInstance(elementType, array.Length);
                    for (int i = 0; i < array.Length; i++) {
                        var value = array[i].value;
                        if (value != null) result.SetValue(value, i);
                    }
                    return result;
                }

                if (elementType.IsSubclassOf(typeof(object)) || elementType.IsInterface) {
                    var array = _referenceArrays[hash];
                    if (array == null) return default;

                    var result = Array.CreateInstance(elementType, array.Length);
                    for (int i = 0; i < array.Length; i++) {
                        object value = array[i].value;
                        if (value != null) result.SetValue(value, i);
                    }
                    return result;
                }

                return default;
            }

            if (type.IsValueType) {
                if (type == typeof(bool)) return _bools[hash];
                if (type == typeof(float)) return _floats[hash];
                if (type == typeof(double)) return _doubles[hash];
                if (type == typeof(int)) return _ints[hash];
                if (type == typeof(long)) return _longs[hash];

                if (type == typeof(Vector2)) return _vectors2[hash];
                if (type == typeof(Vector3)) return _vectors3[hash];
                if (type == typeof(Vector4)) return _vectors4[hash];
                if (type == typeof(Vector2Int)) return _vectors2Int[hash];
                if (type == typeof(Vector3Int)) return _vectors3Int[hash];
                if (type == typeof(Quaternion)) return _quaternions[hash];

                if (type == typeof(LayerMask)) return _layerMasks[hash];
                if (type == typeof(Color)) return _colors[hash];

                if (type.IsEnum) return Enum.ToObject(type, _enums[hash].value);

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

            if (type.IsArray) {
                var elementType = type.GetElementType()!;

                if (elementType.IsValueType) {
                    if (elementType == typeof(bool)) {
                        _boolArrays[hash] = value as bool[];
                        return;
                    }

                    if (elementType == typeof(float)) {
                        _floatArrays[hash] = value as float[];
                        return;
                    }

                    if (elementType == typeof(double)) {
                        _doubleArrays[hash] = value as double[];
                        return;
                    }

                    if (elementType == typeof(int)) {
                        _intArrays[hash] = value as int[];
                        return;
                    }

                    if (elementType == typeof(long)) {
                        _longArrays[hash] = value as long[];
                        return;
                    }

                    if (elementType == typeof(Vector2)) {
                        _vectors2Arrays[hash] = value as Vector2[];
                        return;
                    }

                    if (elementType == typeof(Vector3)) {
                        _vectors3Arrays[hash] = value as Vector3[];
                        return;
                    }

                    if (elementType == typeof(Vector4)) {
                        _vectors4Arrays[hash] = value as Vector4[];
                        return;
                    }

                    if (elementType == typeof(Vector2Int)) {
                        _vectors2IntArrays[hash] = value as Vector2Int[];
                        return;
                    }

                    if (elementType == typeof(Vector3Int)) {
                        _vectors3IntArrays[hash] = value as Vector3Int[];
                        return;
                    }

                    if (elementType == typeof(Quaternion)) {
                        _quaternionArrays[hash] = value as Quaternion[];
                        return;
                    }

                    if (elementType == typeof(Color)) {
                        _colorArrays[hash] = value as Color[];
                        return;
                    }

                    if (elementType == typeof(LayerMask)) {
                        _layerMaskArrays[hash] = value as LayerMask[];
                        return;
                    }

                    if (elementType.IsEnum) {
                        object[] array = value as object[];
                        _enumArrays[hash] = array == null ? null : array.Length > 0
                            ? Array.ConvertAll(array, e => new BlackboardValue<long> { value = Convert.ToInt64(e) })
                            : Array.Empty<BlackboardValue<long>>();
                        return;
                    }

                    return;
                }

                if (elementType == typeof(string)) {
                    _stringArrays[hash] = value as string[];
                    return;
                }

                if (elementType == typeof(AnimationCurve)) {
                    _curveArrays[hash] = value as AnimationCurve[];
                    return;
                }

                if (typeof(Object).IsAssignableFrom(elementType)) {
                    var array = value as Object[];
                    _objectArrays[hash] = array == null ? null : array.Length > 0
                        ? Array.ConvertAll(array, e => new BlackboardValue<Object> { value = e })
                        : Array.Empty<BlackboardValue<Object>>();
                    return;
                }

                if (elementType.IsSubclassOf(typeof(object)) || elementType.IsInterface) {
                    object[] array = value as object[];
                    _referenceArrays[hash] = array == null ? null : array.Length > 0
                        ? Array.ConvertAll(array, obj => new BlackboardReference { value = obj == null ? default : JsonUtility.FromJson(JsonUtility.ToJson(obj), obj.GetType()) })
                        : Array.Empty<BlackboardReference>();
                    return;
                }
            }

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
                    _enums[hash] = new BlackboardValue<long> { value = value == null ? default : Convert.ToInt64(value) };
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
                    value = value == null ? default : JsonUtility.FromJson(JsonUtility.ToJson(value), value.GetType()),
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

            if (_enums.ContainsKey(hash)) {
                _enums.Remove(hash);
                return;
            }

            if (_references.ContainsKey(hash)) {
                _references.Remove(hash);
                return;
            }

            if (_boolArrays.ContainsKey(hash)) {
                _boolArrays.Remove(hash);
                return;
            }

            if (_intArrays.ContainsKey(hash)) {
                _intArrays.Remove(hash);
                return;
            }

            if (_longArrays.ContainsKey(hash)) {
                _longArrays.Remove(hash);
                return;
            }

            if (_floatArrays.ContainsKey(hash)) {
                _floatArrays.Remove(hash);
                return;
            }

            if (_doubleArrays.ContainsKey(hash)) {
                _doubleArrays.Remove(hash);
                return;
            }

            if (_vectors2Arrays.ContainsKey(hash)) {
                _vectors2Arrays.Remove(hash);
                return;
            }

            if (_vectors3Arrays.ContainsKey(hash)) {
                _vectors3Arrays.Remove(hash);
                return;
            }

            if (_vectors4Arrays.ContainsKey(hash)) {
                _vectors4Arrays.Remove(hash);
                return;
            }

            if (_vectors2IntArrays.ContainsKey(hash)) {
                _vectors2Arrays.Remove(hash);
                return;
            }

            if (_vectors3IntArrays.ContainsKey(hash)) {
                _vectors3Arrays.Remove(hash);
                return;
            }

            if (_quaternionArrays.ContainsKey(hash)) {
                _quaternionArrays.Remove(hash);
                return;
            }

            if (_colorArrays.ContainsKey(hash)) {
                _colorArrays.Remove(hash);
                return;
            }

            if (_layerMaskArrays.ContainsKey(hash)) {
                _layerMaskArrays.Remove(hash);
                return;
            }

            if (_stringArrays.ContainsKey(hash)) {
                _stringArrays.Remove(hash);
                return;
            }

            if (_curveArrays.ContainsKey(hash)) {
                _curveArrays.Remove(hash);
                return;
            }

            if (_objectArrays.ContainsKey(hash)) {
                _objectArrays.Remove(hash);
                return;
            }

            if (_enumArrays.ContainsKey(hash)) {
                _enumArrays.Remove(hash);
                return;
            }

            if (_referenceArrays.ContainsKey(hash)) {
                _referenceArrays.Remove(hash);
                return;
            }
        }

        private string ValidateName(string name) {
            int hash = StringToHash(name);
            if (!_propertiesMap.ContainsKey(hash)) return name;

            int count = 1;
            string pattern = $@"{name} \([0-9]+\)";

            for (int i = 0; i < _properties.Count; i++) {
                if (_propertiesMap[_properties[i]].name.HasRegexPattern(pattern)) count++;
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
