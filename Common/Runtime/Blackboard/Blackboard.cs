using System;
using System.Collections.Generic;
using MisterGames.Common.Strings;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MisterGames.Common.Data {

    [Serializable]
    public sealed class Blackboard {

        [SerializeField] private SerializedDictionary<int, bool> _bools = new SerializedDictionary<int, bool>();
        [SerializeField] private SerializedDictionary<int, float> _floats = new SerializedDictionary<int, float>();
        [SerializeField] private SerializedDictionary<int, int> _ints = new SerializedDictionary<int, int>();
        [SerializeField] private SerializedDictionary<int, string> _strings = new SerializedDictionary<int, string>();
        [SerializeField] private SerializedDictionary<int, Vector2> _vectors2 = new SerializedDictionary<int, Vector2>();
        [SerializeField] private SerializedDictionary<int, Vector3> _vectors3 = new SerializedDictionary<int, Vector3>();
        [SerializeField] private SerializedDictionary<int, Object> _objects = new SerializedDictionary<int, Object>();
        [SerializeField] private SerializedDictionary<int, BlackboardReference> _references = new SerializedDictionary<int, BlackboardReference>();

        public Blackboard() { }

        public Blackboard(Blackboard source) {
            _bools = new SerializedDictionary<int, bool>(source._bools);
            _floats = new SerializedDictionary<int, float>(source._floats);
            _ints = new SerializedDictionary<int, int>(source._ints);
            _strings = new SerializedDictionary<int, string>(source._strings);
            _vectors2 = new SerializedDictionary<int, Vector2>(source._vectors2);
            _vectors3 = new SerializedDictionary<int, Vector3>(source._vectors3);
            _objects = new SerializedDictionary<int, Object>(source._objects);
            _references = new SerializedDictionary<int, BlackboardReference>(source._references);

#if UNITY_EDITOR
            _properties = new List<BlackboardProperty>(source._properties);
#endif
        }

        public T Get<T>(int hash) {
            var type = typeof(T);

            if (type.IsValueType) {
                if (type == typeof(bool)) {
                    return _bools[hash] is T t ? t : default;
                }

                if (type == typeof(float)) {
                    return _floats[hash] is T t ? t : default;
                }

                if (type == typeof(int)) {
                    return _ints[hash] is T t ? t : default;
                }

                if (type == typeof(Vector2)) {
                    return _vectors2[hash] is T t ? t : default;
                }

                if (type == typeof(Vector3)) {
                    return _vectors3[hash] is T t ? t : default;
                }

                if (type.IsEnum) {
                    var enumUnderlyingType = type.GetEnumUnderlyingType();

                    if (enumUnderlyingType == typeof(int)) {
                        return Enum.ToObject(type, _ints[hash]) is T t ? t : default;
                    }

                    return default;
                }

                return default;
            }

            if (type == typeof(string)) {
                return _strings[hash] is T t ? t : default;
            }

            if (typeof(Object).IsAssignableFrom(type)) {
                return _objects.TryGetValue(hash, out var obj) && obj is T t ? t : default;
            }

            if (type.IsSubclassOf(typeof(object))) {
                return _references.TryGetValue(hash, out var reference) && reference.data is T t ? t : default;
            }

            return default;
        }

#if UNITY_EDITOR
        [SerializeField] private List<BlackboardProperty> _properties = new List<BlackboardProperty>();
        [SerializeField] private Blackboard _overridenBlackboard;

        public IReadOnlyList<BlackboardProperty> Properties => _properties;
        public Blackboard OverridenBlackboard => _overridenBlackboard;

        public static readonly Type[] RootSearchFolderTypes = new[] {
            typeof(GameObject),
            typeof(bool),
            typeof(float),
            typeof(int),
            typeof(string),
            typeof(Vector2),
            typeof(Vector3),
        };

        private static readonly HashSet<Type> SupportedValueTypes = new HashSet<Type> {
            typeof(bool),
            typeof(float),
            typeof(int),
            typeof(string),
            typeof(Vector2),
            typeof(Vector3)
        };

        private const string EDITOR = "editor";
        public static bool IsSupportedType(Type type) {
            return
                type.IsVisible && (type.IsPublic || type.IsNestedPublic) && !type.IsGenericType &&
                type.FullName is not null && !type.FullName.Contains(EDITOR, StringComparison.OrdinalIgnoreCase) &&
                (
                    type.IsValueType && (type.IsEnum && type.GetEnumUnderlyingType() == typeof(int) || SupportedValueTypes.Contains(type)) ||
                    !type.IsValueType && (typeof(Object).IsAssignableFrom(type) || Attribute.IsDefined(type, typeof(SerializableAttribute)))
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

        public bool TryResetPropertyValue(int hash) {
            if (!TryGetProperty(hash, out var property) || property.type == null) return false;

            object value = _overridenBlackboard != null && _overridenBlackboard.TryGetPropertyValue(hash, out object v)
                ? property.type == v?.GetType() ? v : default
                : default;

            SetValue(property.type, hash, value);
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
                _floats.ContainsKey(hash) ||
                _ints.ContainsKey(hash) ||
                _strings.ContainsKey(hash) ||
                _vectors2.ContainsKey(hash) ||
                _vectors3.ContainsKey(hash) ||
                _objects.ContainsKey(hash) ||
                _references.ContainsKey(hash);
        }

        private object GetValue(Type type, int hash) {
            if (type == null) return default;

            if (type.IsValueType) {
                if (type == typeof(bool)) {
                    return _bools[hash];
                }

                if (type == typeof(float)) {
                    return _floats[hash];
                }

                if (type == typeof(int)) {
                    return _ints[hash];
                }

                if (type == typeof(Vector2)) {
                    return _vectors2[hash];
                }

                if (type == typeof(Vector3)) {
                    return _vectors3[hash];
                }

                if (type.IsEnum) {
                    var enumUnderlyingType = type.GetEnumUnderlyingType();

                    if (enumUnderlyingType == typeof(int)) {
                        return Enum.ToObject(type, _ints[hash]);
                    }

                    return default;
                }

                return default;
            }

            if (type == typeof(string)) {
                return _strings[hash];
            }

            if (typeof(Object).IsAssignableFrom(type)) {
                return _objects[hash];
            }

            if (type.IsSubclassOf(typeof(object))) {
                return _references[hash].data;
            }

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

                if (type == typeof(int)) {
                    _ints[hash] = value is int i ? i : default;
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

                if (type.IsEnum) {
                    var enumUnderlyingType = type.GetEnumUnderlyingType();

                    if (enumUnderlyingType == typeof(int)) {
                        _ints[hash] = value == null ? 0 : (int) Enum.ToObject(type, value);
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

            if (typeof(Object).IsAssignableFrom(type)) {
                _objects[hash] = value as Object;
                return;
            }

            if (type.IsSubclassOf(typeof(object))) {
                value = JsonUtility.FromJson(JsonUtility.ToJson(value), type);
                _references[hash] = new BlackboardReference { data = value ?? Activator.CreateInstance(type) };
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
                    _floats.Remove(hash);
                    return;
                }

                if (type == typeof(int)) {
                    _ints.Remove(hash);
                    return;
                }

                if (type == typeof(string)) {
                    _strings.Remove(hash);
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

                if (type.IsEnum) {
                    var enumUnderlyingType = type.GetEnumUnderlyingType();

                    if (enumUnderlyingType == typeof(int)) {
                        _ints.Remove(hash);
                        return;
                    }

                    return;
                }
            }

            if (typeof(Object).IsAssignableFrom(type)) {
                _objects.Remove(hash);
                return;
            }

            if (type.IsSubclassOf(typeof(object))) {
                _references.Remove(hash);
                return;
            }
        }

        private void RemoveValueByHash(int hash) {
            if (_bools.ContainsKey(hash)) {
                _bools.Remove(hash);
                return;
            }

            if (_floats.ContainsKey(hash)) {
                _floats.Remove(hash);
                return;
            }

            if (_ints.ContainsKey(hash)) {
                _ints.Remove(hash);
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
