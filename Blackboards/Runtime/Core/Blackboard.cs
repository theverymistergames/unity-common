using System;
using System.Collections.Generic;
using MisterGames.Common.Data;
using MisterGames.Common.Strings;
using MisterGames.Common.Types;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MisterGames.Blackboards.Core {

    [Serializable]
    public sealed class Blackboard {

        [SerializeField] private List<int> _properties;
        [SerializeField] private SerializedDictionary<int, BlackboardProperty> _propertiesMap;

#region Maps
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

        [SerializeField] private SerializedDictionary<int, LayerMask> _layerMasks;
        [SerializeField] private SerializedDictionary<int, Color> _colors;
        [SerializeField] private SerializedDictionary<int, AnimationCurve> _curves;

        [SerializeField] private SerializedDictionary<int, BlackboardValue<long>> _enums;
        [SerializeField] private SerializedDictionary<int, BlackboardValue<Object>> _objects;
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

        [SerializeField] private SerializedDictionary<int, BlackboardValue<long>[]> _enumArrays;
        [SerializeField] private SerializedDictionary<int, BlackboardValue<Object>[]> _objectArrays;
        [SerializeField] private SerializedDictionary<int, BlackboardReference[]> _referenceArrays;
#endregion

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

            _layerMasks = new SerializedDictionary<int, LayerMask>();
            _colors = new SerializedDictionary<int, Color>();
            _curves = new SerializedDictionary<int, AnimationCurve>();

            _enums = new SerializedDictionary<int, BlackboardValue<long>>();
            _objects = new SerializedDictionary<int, BlackboardValue<Object>>();
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

            _layerMaskArrays = new SerializedDictionary<int, LayerMask[]>();
            _colorArrays = new SerializedDictionary<int, Color[]>();
            _curveArrays = new SerializedDictionary<int, AnimationCurve[]>();

            _enumArrays = new SerializedDictionary<int, BlackboardValue<long>[]>();
            _objectArrays = new SerializedDictionary<int, BlackboardValue<Object>[]>();
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

            _layerMasks = new SerializedDictionary<int, LayerMask>(source._layerMasks);
            _colors = new SerializedDictionary<int, Color>(source._colors);
            _curves = new SerializedDictionary<int, AnimationCurve>(source._curves);

            _enums = new SerializedDictionary<int, BlackboardValue<long>>(source._enums);
            _objects = new SerializedDictionary<int, BlackboardValue<Object>>(source._objects);
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

            _layerMaskArrays = new SerializedDictionary<int, LayerMask[]>(source._layerMaskArrays);
            _colorArrays = new SerializedDictionary<int, Color[]>(source._colorArrays);
            _curveArrays = new SerializedDictionary<int, AnimationCurve[]>(source._curveArrays);

            _enumArrays = new SerializedDictionary<int, BlackboardValue<long>[]>(source._enumArrays);
            _objectArrays = new SerializedDictionary<int, BlackboardValue<Object>[]>(source._objectArrays);
            _referenceArrays = new SerializedDictionary<int, BlackboardReference[]>(source._referenceArrays);
        }

        public T Get<T>(int hash) {
            if (!_propertiesMap.TryGetValue(hash, out var property)) return default;

            switch (property.mapIndex) {
                case 0: { return _bools.TryGetValue(hash, out bool b) && b is T t ? t : default; }
                case 1: { return _ints.TryGetValue(hash, out int i) && i is T t ? t : default; }
                case 2: { return _longs.TryGetValue(hash, out long l) && l is T t ? t : default; }
                case 3: { return _floats.TryGetValue(hash, out float f) && f is T t ? t : default; }
                case 4: { return _doubles.TryGetValue(hash, out double d) && d is T t ? t : default; }
                case 5: { return _strings.TryGetValue(hash, out string s) && s is T t ? t : default; }

                case 6: { return _vectors2.TryGetValue(hash, out var v) && v is T t ? t : default; }
                case 7: { return _vectors3.TryGetValue(hash, out var v) && v is T t ? t : default; }
                case 8: { return _vectors4.TryGetValue(hash, out var v) && v is T t ? t : default; }
                case 9: { return _vectors2Int.TryGetValue(hash, out var v) && v is T t ? t : default; }
                case 10: { return _vectors3Int.TryGetValue(hash, out var v) && v is T t ? t : default; }
                case 11: { return _quaternions.TryGetValue(hash, out var v) && v is T t ? t : default; }

                case 12: { return _layerMasks.TryGetValue(hash, out var v) && v is T t ? t : default; }
                case 13: { return _colors.TryGetValue(hash, out var v) && v is T t ? t : default; }
                case 14: { return _curves.TryGetValue(hash, out var v) && v is T t ? t : default; }

                case 15: { return _enums.TryGetValue(hash, out var v) && Enum.ToObject(typeof(T), v.value) is T t ? t : default; }
                case 16: { return _objects.TryGetValue(hash, out var v) && v.value is T t ? t : default; }
                case 17: { return _references.TryGetValue(hash, out var v) && v.value is T t ? t : default; }

                case 18: { return _boolArrays.TryGetValue(hash, out bool[] a) && a is T t ? t : default; }
                case 19: { return _intArrays.TryGetValue(hash, out int[] a) && a is T t ? t : default; }
                case 20: { return _longArrays.TryGetValue(hash, out long[] a) && a is T t ? t : default; }
                case 21: { return _floatArrays.TryGetValue(hash, out float[] a) && a is T t ? t : default; }
                case 22: { return _doubleArrays.TryGetValue(hash, out double[] a) && a is T t ? t : default; }
                case 23: { return _stringArrays.TryGetValue(hash, out string[] a) && a is T t ? t : default; }

                case 24: { return _vectors2Arrays.TryGetValue(hash, out var v) && v is T t ? t : default; }
                case 25: { return _vectors3Arrays.TryGetValue(hash, out var v) && v is T t ? t : default; }
                case 26: { return _vectors4Arrays.TryGetValue(hash, out var v) && v is T t ? t : default; }
                case 27: { return _vectors2IntArrays.TryGetValue(hash, out var v) && v is T t ? t : default; }
                case 28: { return _vectors3IntArrays.TryGetValue(hash, out var v) && v is T t ? t : default; }
                case 29: { return _quaternionArrays.TryGetValue(hash, out var v) && v is T t ? t : default; }

                case 30: { return _layerMaskArrays.TryGetValue(hash, out var v) && v is T t ? t : default; }
                case 31: { return _colorArrays.TryGetValue(hash, out var v) && v is T t ? t : default; }
                case 32: { return _curveArrays.TryGetValue(hash, out var v) && v is T t ? t : default; }

                case 33: {
                    if (!_enumArrays.TryGetValue(hash, out var array) || array == null) return default;

                    var type = ((Type) property.type).GetElementType()!;

                    var res = Array.CreateInstance(type, array.Length);
                    for (int i = 0; i < array.Length; i++) {
                        res.SetValue(Enum.ToObject(type, array[i].value), i);
                    }

                    return res is T t ? t : default;
                }

                case 34: {
                    if (!_objectArrays.TryGetValue(hash, out var array) || array == null) return default;

                    var type = ((Type) property.type).GetElementType()!;

                    var result = Array.CreateInstance(type, array.Length);
                    for (int i = 0; i < array.Length; i++) {
                        var value = array[i].value;
                        if (value != null) result.SetValue(value, i);
                    }

                    return result is T t ? t : default;
                }

                case 35: {
                    if (!_referenceArrays.TryGetValue(hash, out var array) || array == null) return default;

                    var type = ((Type) property.type).GetElementType()!;

                    var result = Array.CreateInstance(type, array.Length);
                    for (int i = 0; i < array.Length; i++) {
                        object value = array[i].value;
                        if (value != null) result.SetValue(value, i);
                    }

                    return result is T t ? t : default;
                }

                default: return default;
            }
        }

#if UNITY_EDITOR
        private const string EDITOR = "editor";

        private static readonly HashSet<Type> SupportedValueTypes = new HashSet<Type> {
            typeof(bool),
            typeof(int),
            typeof(long),
            typeof(float),
            typeof(double),

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

                SetValue(property.mapIndex, hash, blackboard.GetValue(property.mapIndex, hash));

                changed = true;
            }

            for (int i = 0; i < blackboard._properties.Count; i++) {
                int hash = blackboard._properties[i];
                var property = blackboard._propertiesMap[hash];

                if (property.type == null) continue;

                if (!_propertiesMap.ContainsKey(hash)) {
                    _propertiesMap.Add(hash, property);
                    _properties.Add(hash);

                    SetValue(property.mapIndex, hash, blackboard.GetValue(property.mapIndex, hash));
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
                mapIndex = GetMapIndex(type),
            };

            SetValue(property.mapIndex, hash, default);

            _propertiesMap.Add(hash, property);
            _properties.Add(hash);

            return true;
        }

        public bool TryGetProperty(int hash, out BlackboardProperty property) {
            return _propertiesMap.TryGetValue(hash, out property);
        }

        public bool TrySetPropertyValue(int hash, object value) {
            if (!_propertiesMap.TryGetValue(hash, out var property) || property.type == null) return false;

            SetValue(property.mapIndex, hash, value);
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

                SetValue(property.mapIndex, hash, value);
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

            SetValue(property.mapIndex, hash, value);
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

            object value = GetValue(property.mapIndex, hash);
            RemoveValue(hash);
            SetValue(property.mapIndex, newHash, value);

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

            value = GetValue(property.mapIndex, hash);
            return true;
        }

        private object GetValue(int mapIndex, int hash) {
            return mapIndex switch {
                0 => _bools[hash],
                1 => _ints[hash],
                2 => _longs[hash],
                3 => _floats[hash],
                4 => _doubles[hash],
                5 => _strings[hash],

                6 => _vectors2[hash],
                7 => _vectors3[hash],
                8 => _vectors4[hash],
                9 => _vectors2Int[hash],
                10 => _vectors3Int[hash],
                11 => _quaternions[hash],

                12 => _layerMasks[hash],
                13 => _colors[hash],
                14 => _curves[hash],

                15 => _enums[hash],
                16 => _objects[hash],
                17 => _references[hash],

                18 => _boolArrays[hash],
                19 => _intArrays[hash],
                20 => _longArrays[hash],
                21 => _floatArrays[hash],
                22 => _doubleArrays[hash],
                23 => _stringArrays[hash],

                24 => _vectors2Arrays[hash],
                25 => _vectors3Arrays[hash],
                26 => _vectors4Arrays[hash],
                27 => _vectors2IntArrays[hash],
                28 => _vectors3IntArrays[hash],
                29 => _quaternionArrays[hash],

                30 => _layerMaskArrays[hash],
                31 => _colorArrays[hash],
                32 => _curveArrays[hash],

                33 => _enumArrays[hash],
                34 => _objectArrays[hash],
                35 => _referenceArrays[hash],

                _ => default
            };
        }

        private void SetValue(int mapIndex, int hash, object value) {
            switch (mapIndex) {
                case 0: {
                    _bools[hash] = value is bool b ? b : default;
                    return;
                }
                case 1: {
                    _ints[hash] = value is int i ? i : default;
                    return;
                }
                case 2: {
                    _longs[hash] = value is long l ? l : default;
                    return;
                }
                case 3: {
                    _floats[hash] = value is float f ? f : default;
                    return;
                }
                case 4: {
                    _doubles[hash] = value is double d ? d : default;
                    return;
                }
                case 5: {
                    _strings[hash] = value as string;
                    return;
                }

                case 6: {
                    _vectors2[hash] = value is Vector2 v2 ? v2 : default;
                    return;
                }
                case 7: {
                    _vectors3[hash] = value is Vector3 v3 ? v3 : default;
                    return;
                }
                case 8: {
                    _vectors4[hash] = value is Vector4 v4 ? v4 : default;
                    return;
                }
                case 9: {
                    _vectors2Int[hash] = value is Vector2Int v2 ? v2 : default;
                    return;
                }
                case 10: {
                    _vectors3Int[hash] = value is Vector3Int v3 ? v3 : default;
                    return;
                }
                case 11: {
                    _quaternions[hash] = value is Quaternion q ? q : default;
                    return;
                }

                case 12: {
                    _layerMasks[hash] = value is LayerMask m ? m : default;
                    return;
                }
                case 13: {
                    _colors[hash] = value is Color c ? c : default;
                    return;
                }
                case 14: {
                    _curves[hash] = value as AnimationCurve;
                    return;
                }

                case 15: {
                    _enums[hash] = new BlackboardValue<long> { value = value == null ? default : Convert.ToInt64(value) };
                    return;
                }
                case 16: {
                    _objects[hash] = new BlackboardValue<Object> { value = value as Object };
                    return;
                }
                case 17: {
                    _references[hash] = new BlackboardReference {
                        value = value == null ? default : JsonUtility.FromJson(JsonUtility.ToJson(value), value.GetType()),
                    };
                    return;
                }

                case 18: {
                    _boolArrays[hash] = value as bool[];
                    return;
                }
                case 19: {
                    _intArrays[hash] = value as int[];
                    return;
                }
                case 20: {
                    _longArrays[hash] = value as long[];
                    return;
                }
                case 21: {
                    _floatArrays[hash] = value as float[];
                    return;
                }
                case 22: {
                    _doubleArrays[hash] = value as double[];
                    return;
                }
                case 23: {
                    _stringArrays[hash] = value as string[];
                    return;
                }

                case 24: {
                    _vectors2Arrays[hash] = value as Vector2[];
                    return;
                }
                case 25: {
                    _vectors3Arrays[hash] = value as Vector3[];
                    return;
                }
                case 26: {
                    _vectors4Arrays[hash] = value as Vector4[];
                    return;
                }
                case 27: {
                    _vectors2IntArrays[hash] = value as Vector2Int[];
                    return;
                }
                case 28: {
                    _vectors3IntArrays[hash] = value as Vector3Int[];
                    return;
                }
                case 29: {
                    _quaternionArrays[hash] = value as Quaternion[];
                    return;
                }

                case 30: {
                    _layerMaskArrays[hash] = value as LayerMask[];
                    return;
                }
                case 31: {
                    _colorArrays[hash] = value as Color[];
                    return;
                }
                case 32: {
                    _curveArrays[hash] = value as AnimationCurve[];
                    return;
                }

                case 33: {
                    object[] array = value as object[];
                    _enumArrays[hash] = array == null ? null : array.Length > 0
                        ? Array.ConvertAll(array, e => new BlackboardValue<long> { value = Convert.ToInt64(e) })
                        : Array.Empty<BlackboardValue<long>>();
                    return;
                }
                case 34: {
                    var array = value as Object[];
                    _objectArrays[hash] = array == null ? null : array.Length > 0
                        ? Array.ConvertAll(array, e => new BlackboardValue<Object> { value = e })
                        : Array.Empty<BlackboardValue<Object>>();
                    return;
                }
                case 35: {
                    object[] array = value as object[];
                    _referenceArrays[hash] = array == null ? null : array.Length > 0
                        ? Array.ConvertAll(array, obj => new BlackboardReference { value = obj == null ? default : JsonUtility.FromJson(JsonUtility.ToJson(obj), obj.GetType()) })
                        : Array.Empty<BlackboardReference>();
                    return;
                }
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
            if (_strings.ContainsKey(hash)) {
                _strings.Remove(hash);
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

            if (_layerMasks.ContainsKey(hash)) {
                _layerMasks.Remove(hash);
                return;
            }
            if (_colors.ContainsKey(hash)) {
                _colors.Remove(hash);
                return;
            }
            if (_curves.ContainsKey(hash)) {
                _curves.Remove(hash);
                return;
            }

            if (_enums.ContainsKey(hash)) {
                _enums.Remove(hash);
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
            if (_stringArrays.ContainsKey(hash)) {
                _stringArrays.Remove(hash);
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

            if (_layerMaskArrays.ContainsKey(hash)) {
                _layerMaskArrays.Remove(hash);
                return;
            }
            if (_colorArrays.ContainsKey(hash)) {
                _colorArrays.Remove(hash);
                return;
            }
            if (_curveArrays.ContainsKey(hash)) {
                _curveArrays.Remove(hash);
                return;
            }

            if (_enumArrays.ContainsKey(hash)) {
                _enumArrays.Remove(hash);
                return;
            }
            if (_objectArrays.ContainsKey(hash)) {
                _objectArrays.Remove(hash);
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

        private static int GetMapIndex(Type type) {
            if (type == null) return -1;

            if (!type.IsArray) {
                if (type == typeof(bool)) return 0;
                if (type == typeof(int)) return 1;
                if (type == typeof(float)) return 2;
                if (type == typeof(double)) return 3;
                if (type == typeof(long)) return 4;
                if (type == typeof(string)) return 5;

                if (type == typeof(Vector2)) return 6;
                if (type == typeof(Vector3)) return 7;
                if (type == typeof(Vector4)) return 8;
                if (type == typeof(Vector2Int)) return 9;
                if (type == typeof(Vector3Int)) return 10;
                if (type == typeof(Quaternion)) return 11;

                if (type == typeof(LayerMask)) return 12;
                if (type == typeof(Color)) return 13;
                if (type == typeof(AnimationCurve)) return 14;

                if (type.IsEnum) return 15;
                if (typeof(Object).IsAssignableFrom(type)) return 16;
                if (type.IsSubclassOf(typeof(object)) || type.IsInterface) return 17;

                return -1;
            }

            type = type.GetElementType()!;

            if (type == typeof(bool)) return 18;
            if (type == typeof(int)) return 19;
            if (type == typeof(float)) return 20;
            if (type == typeof(double)) return 21;
            if (type == typeof(long)) return 22;
            if (type == typeof(string)) return 23;

            if (type == typeof(Vector2)) return 24;
            if (type == typeof(Vector3)) return 25;
            if (type == typeof(Vector4)) return 26;
            if (type == typeof(Vector2Int)) return 27;
            if (type == typeof(Vector3Int)) return 28;
            if (type == typeof(Quaternion)) return 29;

            if (type == typeof(LayerMask)) return 30;
            if (type == typeof(Color)) return 31;
            if (type == typeof(AnimationCurve)) return 32;

            if (type.IsEnum) return 33;
            if (typeof(Object).IsAssignableFrom(type)) return 34;
            if (type.IsSubclassOf(typeof(object)) || type.IsInterface) return 35;

            return -1;
        }
#endif
    }

}
