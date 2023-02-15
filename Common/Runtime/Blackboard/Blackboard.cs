using System;
using System.Collections.Generic;
using MisterGames.Common.Easing;
using MisterGames.Common.Strings;
using UnityEngine;

namespace MisterGames.Common.Data {

    [Serializable]
    public sealed class Blackboard {

        [SerializeField] private SerializedDictionary<int, bool> _bools = new SerializedDictionary<int, bool>();
        [SerializeField] private SerializedDictionary<int, float> _floats = new SerializedDictionary<int, float>();
        [SerializeField] private SerializedDictionary<int, int> _ints = new SerializedDictionary<int, int>();
        [SerializeField] private SerializedDictionary<int, string> _strings = new SerializedDictionary<int, string>();
        [SerializeField] private SerializedDictionary<int, Vector2> _vectors2 = new SerializedDictionary<int, Vector2>();
        [SerializeField] private SerializedDictionary<int, Vector3> _vectors3 = new SerializedDictionary<int, Vector3>();
        [SerializeField] private SerializedDictionary<int, EasingCurve> _curves = new SerializedDictionary<int, EasingCurve>();
        [SerializeField] private SerializedDictionary<int, ScriptableObject> _scriptableObjects = new SerializedDictionary<int, ScriptableObject>();
        [SerializeField] private SerializedDictionary<int, GameObject> _gameObjects = new SerializedDictionary<int, GameObject>();

        public Blackboard() { }

        public Blackboard(Blackboard source) {
            _bools = new SerializedDictionary<int, bool>(source._bools);
            _floats = new SerializedDictionary<int, float>(source._floats);
            _ints = new SerializedDictionary<int, int>(source._ints);
            _strings = new SerializedDictionary<int, string>(source._strings);
            _vectors2 = new SerializedDictionary<int, Vector2>(source._vectors2);
            _vectors3 = new SerializedDictionary<int, Vector3>(source._vectors3);
            _curves = new SerializedDictionary<int, EasingCurve>(source._curves);
            _scriptableObjects = new SerializedDictionary<int, ScriptableObject>(source._scriptableObjects);
            _gameObjects = new SerializedDictionary<int, GameObject>(source._gameObjects);

#if UNITY_EDITOR
            _properties = new List<BlackboardProperty>(source._properties);
#endif
        }

        public bool GetBool(int hash) {
            if (_bools.TryGetValue(hash, out bool value)) return value;

            Debug.LogWarning($"Blackboard: trying to get not existing bool value for hash {hash}");
            return default;
        }

        public float GetFloat(int hash) {
            if (_floats.TryGetValue(hash, out float value)) return value;

            Debug.LogWarning($"Blackboard: trying to get not existing float value for hash {hash}");
            return default;
        }

        public int GetInt(int hash) {
            if (_ints.TryGetValue(hash, out int value)) return value;

            Debug.LogWarning($"Blackboard: trying to get not existing int value for hash {hash}");
            return default;
        }

        public string GetString(int hash) {
            if (_strings.TryGetValue(hash, out string value)) return value;

            Debug.LogWarning($"Blackboard: trying to get not existing string value for hash {hash}");
            return default;
        }

        public Vector2 GetVector2(int hash) {
            if (_vectors2.TryGetValue(hash, out var value)) return value;

            Debug.LogWarning($"Blackboard: trying to get not existing Vector2 value for hash {hash}");
            return default;
        }

        public Vector3 GetVector3(int hash) {
            if (_vectors3.TryGetValue(hash, out var value)) return value;

            Debug.LogWarning($"Blackboard: trying to get not existing Vector3 value for hash {hash}");
            return default;
        }

        public EasingCurve GetCurve(int hash) {
            if (_curves.TryGetValue(hash, out var value)) return value;

            Debug.LogWarning($"Blackboard: trying to get not existing EasingAnimationCurve value for hash {hash}");
            return default;
        }

        public ScriptableObject GetScriptableObject(int hash) {
            if (_scriptableObjects.TryGetValue(hash, out var value)) return value;

            Debug.LogWarning($"Blackboard: trying to get not existing ScriptableObject value for hash {hash}");
            return default;
        }

        public GameObject GetGameObject(int hash) {
            if (_gameObjects.TryGetValue(hash, out var value)) return value;

            Debug.LogWarning($"Blackboard: trying to get not existing GameObject value for hash {hash}");
            return default;
        }

        public void SetBool(int hash, bool value) {
            if (!_bools.ContainsKey(hash)) {
                Debug.LogWarning($"Blackboard: trying to set not existing bool value for hash {hash}");
                return;
            }

            _bools[hash] = value;
        }

        public void SetFloat(int hash, float value) {
            if (!_floats.ContainsKey(hash)) {
                Debug.LogWarning($"Blackboard: trying to set not existing float value for hash {hash}");
                return;
            }

            _floats[hash] = value;
        }

        public void SetInt(int hash, int value) {
            if (!_ints.ContainsKey(hash)) {
                Debug.LogWarning($"Blackboard: trying to set not existing int value for hash {hash}");
                return;
            }

            _ints[hash] = value;
        }

        public void SetString(int hash, string value) {
            if (!_strings.ContainsKey(hash)) {
                Debug.LogWarning($"Blackboard: trying to set not existing string value for hash {hash}");
                return;
            }

            _strings[hash] = value;
        }

        public void SetVector2(int hash, Vector2 value) {
            if (!_vectors2.ContainsKey(hash)) {
                Debug.LogWarning($"Blackboard: trying to set not existing Vector2 value for hash {hash}");
                return;
            }

            _vectors2[hash] = value;
        }

        public void SetVector3(int hash, Vector3 value) {
            if (!_vectors3.ContainsKey(hash)) {
                Debug.LogWarning($"Blackboard: trying to set not existing Vector3 value for hash {hash}");
                return;
            }

            _vectors3[hash] = value;
        }

        public void SetCurve(int hash, EasingCurve value) {
            if (!_curves.ContainsKey(hash)) {
                Debug.LogWarning($"Blackboard: trying to set not existing EasingAnimationCurve value for hash {hash}");
                return;
            }

            _curves[hash] = value;
        }

        public void SetScriptableObject(int hash, ScriptableObject value) {
            if (!_scriptableObjects.ContainsKey(hash)) {
                Debug.LogWarning($"Blackboard: trying to set not existing ScriptableObject value for hash {hash}");
                return;
            }

            _scriptableObjects[hash] = value;
        }

        public void SetGameObject(int hash, GameObject value) {
            if (!_gameObjects.ContainsKey(hash)) {
                Debug.LogWarning($"Blackboard: trying to set not existing GameObject value for hash {hash}");
                return;
            }

            _gameObjects[hash] = value;
        }

#if UNITY_EDITOR
        [SerializeField] private List<BlackboardProperty> _properties = new List<BlackboardProperty>();
        [SerializeField] private Blackboard _overridenBlackboard;

        public IReadOnlyList<BlackboardProperty> Properties => _properties;
        public Blackboard OverridenBlackboard => _overridenBlackboard;

        public static readonly List<Type> SupportedTypes = new List<Type> {
            typeof(bool),
            typeof(float),
            typeof(int),
            typeof(string),
            typeof(Vector2),
            typeof(Vector3),
            typeof(EasingCurve),
            typeof(ScriptableObject),
            typeof(GameObject),
        };

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

        public bool TryAddProperty(string name, Type type, object value, out BlackboardProperty property) {
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

            SetValue(type, hash, value);

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
                _curves.ContainsKey(hash) ||
                _scriptableObjects.ContainsKey(hash) ||
                _gameObjects.ContainsKey(hash);
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

            if (type == typeof(EasingCurve)) {
                return _curves[hash];
            }

            if (type == typeof(ScriptableObject)) {
                return _scriptableObjects[hash];
            }

            if (type == typeof(GameObject)) {
                return _gameObjects[hash];
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
                _strings[hash] = value as string ?? string.Empty;
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

            if (type == typeof(EasingCurve)) {
                _curves[hash] = value as EasingCurve ?? new EasingCurve();
                return;
            }

            if (type == typeof(ScriptableObject)) {
                _scriptableObjects[hash] = value as ScriptableObject;
                return;
            }

            if (type == typeof(GameObject)) {
                _gameObjects[hash] = value as GameObject;
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

            if (type == typeof(EasingCurve)) {
                _curves.Remove(hash);
                return;
            }

            if (type == typeof(ScriptableObject)) {
                _scriptableObjects.Remove(hash);
                return;
            }

            if (type == typeof(GameObject)) {
                _gameObjects.Remove(hash);
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
            if (SupportedTypes.Contains(type)) return true;

            Debug.LogError($"Blackboard does not support type {type.Name}");
            return false;
        }
#endif
    }

}
