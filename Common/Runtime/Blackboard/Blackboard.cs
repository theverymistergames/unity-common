using System;
using System.Collections.Generic;
using MisterGames.Common.Strings;
using UnityEditor;
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
        [SerializeField] private SerializedDictionary<int, ReferenceContainer> _references = new SerializedDictionary<int, ReferenceContainer>();

        [Serializable]
        private struct ReferenceContainer {
            [SerializeReference] public object data;
        }

        public Blackboard() { }

        public Blackboard(Blackboard source) {
            _bools = new SerializedDictionary<int, bool>(source._bools);
            _floats = new SerializedDictionary<int, float>(source._floats);
            _ints = new SerializedDictionary<int, int>(source._ints);
            _strings = new SerializedDictionary<int, string>(source._strings);
            _vectors2 = new SerializedDictionary<int, Vector2>(source._vectors2);
            _vectors3 = new SerializedDictionary<int, Vector3>(source._vectors3);
            _objects = new SerializedDictionary<int, Object>(source._objects);
            _references = new SerializedDictionary<int, ReferenceContainer>(source._references);

#if UNITY_EDITOR
            _properties = new List<BlackboardProperty>(source._properties);
#endif
        }

        public T Get<T>(int hash) {
            var type = typeof(T);

            if (type == typeof(bool)) {
                return _bools[hash] is T t ? t : default;
            }

            if (type == typeof(float)) {
                return _floats[hash] is T t ? t : default;
            }

            if (type == typeof(int)) {
                return _ints[hash] is T t ? t : default;
            }

            if (type == typeof(string)) {
                return _strings[hash] is T t ? t : default;
            }

            if (type == typeof(Vector2)) {
                return _vectors2[hash] is T t ? t : default;
            }

            if (type == typeof(Vector3)) {
                return _vectors3[hash] is T t ? t : default;
            }

            if (typeof(Object).IsAssignableFrom(type)) {
                return _objects.TryGetValue(hash, out var obj) && obj is T t ? t : default;
            }

            if (typeof(object).IsAssignableFrom(type)) {
                return _references.TryGetValue(hash, out var container) && container.data is T t ? t : default;
            }

            Debug.LogError($"Trying to get blackboard property of unsupported type {type.Name} for hash {hash}");
            return default;
        }

#if UNITY_EDITOR
        [CustomPropertyDrawer(typeof(ReferenceContainer))]
        private sealed class ReferenceContainerPropertyDrawer : PropertyDrawer {
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
                EditorGUI.PropertyField(position, property.FindPropertyRelative("data"), label);
            }
            public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
                return EditorGUI.GetPropertyHeight(property.FindPropertyRelative("data"));
            }
        }

        [SerializeField] private List<BlackboardProperty> _properties = new List<BlackboardProperty>();
        [SerializeField] private Blackboard _overridenBlackboard;

        public IReadOnlyList<BlackboardProperty> Properties => _properties;
        public Blackboard OverridenBlackboard => _overridenBlackboard;

        public static readonly Type[] SupportedConcreteTypes = new[] {
            typeof(GameObject),
            typeof(bool),
            typeof(float),
            typeof(int),
            typeof(string),
            typeof(Vector2),
            typeof(Vector3),
        };

        public static readonly Type[] SupportedDerivedTypes = new[] {
            typeof(ScriptableObject),
            typeof(Component),
            //typeof(object),
        };

        public static bool IsSupportedType(Type t) {
            for (int i = 0; i < SupportedConcreteTypes.Length; i++) {
                if (t == SupportedConcreteTypes[i]) return true;
            }

            for (int i = 0; i < SupportedDerivedTypes.Length; i++) {
                if (SupportedDerivedTypes[i].IsAssignableFrom(t)) return IsSupportedDerivedType(t);
            }

            return false;
        }

        private const string EDITOR = "editor";
        public static bool IsSupportedDerivedType(Type t) {
            return
                t.IsVisible && (t.IsPublic || t.IsNestedPublic) && !t.IsGenericType &&
                t.FullName is not null && !t.FullName.Contains(EDITOR, StringComparison.OrdinalIgnoreCase) &&
                (typeof(Object).IsAssignableFrom(t) || Attribute.IsDefined(t, typeof(SerializableAttribute)));
        }

        public static int StringToHash(string name) {
            return name.GetHashCode();
        }

        public void OverrideBlackboard(Blackboard blackboard, bool removeNotExistentProperties = true) {
            _overridenBlackboard = blackboard;

            if (removeNotExistentProperties) {
                for (int i = 0; i < _properties.Count; i++) {
                    var property = _properties[i];
                    if (!blackboard.HasProperty(property.hash)) RemoveProperty(property.hash);
                }
            }

            for (int i = 0; i < blackboard.Properties.Count; i++) {
                var property = blackboard.Properties[i];

                if (!HasProperty(property.hash)) {
                    _properties.Add(property);
                    SetValue(property.type, property.hash, blackboard.GetValue(property.type, property.hash));
                    continue;
                }

                TrySetPropertyIndex(property.hash, i);
            }
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
            if (!TryGetProperty(hash, out var property)) {
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
            if (!TryGetProperty(hash, out var property)) return false;

            if (value.GetType() != property.type) return false;

            SetValue(property.type, hash, value);
            return true;
        }

        public bool TrySetPropertyValueAtIndex(int index, object value) {
            if (index < 0 || index > _properties.Count - 1) return false;

            var property = _properties[index];
            if (value.GetType() != property.type) return false;

            SetValue(property.type, property.hash, value);
            return true;
        }

        public bool TrySetPropertyName(int hash, string newName) {
            if (!TryGetProperty(hash, out int index, out var property)) return false;

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
            if (!TryGetProperty(hash, out int oldIndex, out var property)) return false;

            if (newIndex >= _properties.Count) {
                _properties.RemoveAt(oldIndex);
                _properties.Add(property);
                return true;
            }

            _properties[oldIndex] = _properties[newIndex];
            _properties[newIndex] = property;

            return true;
        }

        public bool HasProperty(int hash) {
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

        public void RemoveProperty(int hash) {
            if (!TryGetProperty(hash, out int index, out var property)) return;

            _properties.RemoveAt(index);
            RemoveValue(property.type, hash);
        }

        public void RemovePropertyAt(int index) {
            if (index < 0 || index > _properties.Count - 1) return;
            _properties.RemoveAt(index);
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

        private object GetValue(Type type, int hash) {
            if (type == typeof(bool)) {
                return _bools[hash];
            }

            if (type == typeof(float)) {
                return _floats[hash];
            }

            if (type == typeof(int)) {
                return _ints[hash];
            }

            if (type == typeof(string)) {
                return _strings[hash];
            }

            if (type == typeof(Vector2)) {
                return _vectors2[hash];
            }

            if (type == typeof(Vector3)) {
                return _vectors3[hash];
            }

            if (typeof(Object).IsAssignableFrom(type)) {
                return _objects[hash];
            }

            if (typeof(object).IsAssignableFrom(type)) {
                return _references[hash].data;
            }

            return default;
        }

        private void SetValue(Type type, int hash, object value) {
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

            if (type == typeof(string)) {
                _strings[hash] = value as string;
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

            if (typeof(Object).IsAssignableFrom(type)) {
                _objects[hash] = value as Object;
                return;
            }

            if (typeof(object).IsAssignableFrom(type)) {
                _references[hash] = new ReferenceContainer { data = value };
                return;
            }
        }

        private void RemoveValue(Type type, int hash) {
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

            if (typeof(Object).IsAssignableFrom(type)) {
                _objects.Remove(hash);
                return;
            }

            if (typeof(object).IsAssignableFrom(type)) {
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
