using System;
using System.Collections.Generic;
using MisterGames.Common.Strings;
using UnityEngine;

namespace MisterGames.Common.Data {

    [Serializable]
    public sealed class Blackboard {

        public static readonly List<Type> SupportedTypes = new List<Type> {
            typeof(bool),
            typeof(float),
            typeof(int),
            typeof(string),
            typeof(Vector2),
            typeof(Vector3),
            typeof(BlackboardEvent),
            typeof(ScriptableObject),
            typeof(GameObject),
        };

        private static readonly Dictionary<Type, string> NameOverrides = new Dictionary<Type, string> {
            [typeof(bool)] = "Boolean",
            [typeof(float)] = "Float",
            [typeof(int)] = "Int",
            [typeof(string)] = "String",
        };

        [SerializeField] private SerializedDictionary<int, BlackboardProperty> _propertiesMap;

        [SerializeField] private SerializedDictionary<int, bool> _bools;
        [SerializeField] private SerializedDictionary<int, float> _floats;
        [SerializeField] private SerializedDictionary<int, int> _ints;
        [SerializeField] private SerializedDictionary<int, string> _strings;
        [SerializeField] private SerializedDictionary<int, Vector2> _vectors2;
        [SerializeField] private SerializedDictionary<int, Vector3> _vectors3;
        [SerializeField] private SerializedDictionary<int, BlackboardEvent> _blackboardEvents;
        [SerializeField] private SerializedDictionary<int, ScriptableObject> _scriptableObjects;
        [SerializeField] private SerializedDictionary<int, GameObject> _gameObjects;

        public Dictionary<int, BlackboardProperty> PropertiesMap => _propertiesMap;

        public static int StringToHash(string name) {
            return name.GetHashCode();
        }

        public static string GetTypeName(Type type) {
            return NameOverrides.TryGetValue(type, out string typeName) ? typeName : type.Name;
        }
        
        public static Type GetPropertyType(BlackboardProperty property) {
            return SerializedType.FromString(property.serializedType);
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

        public BlackboardEvent GetBlackboardEvent(int hash) {
            if (_blackboardEvents.TryGetValue(hash, out var value)) return value;

            Debug.LogWarning($"Blackboard: trying to get not existing BlackboardEvent value for hash {hash}");
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

        public void SetBlackboardEvent(int hash, BlackboardEvent value) {
            if (!_blackboardEvents.ContainsKey(hash)) {
                Debug.LogWarning($"Blackboard: trying to set not existing BlackboardEvent value for hash {hash}");
                return;
            }

            _blackboardEvents[hash] = value;
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

        public bool TryAddProperty(string name, Type type, object value, out BlackboardProperty property) {
            property = default;
            if (!ValidateType(type)) return false;
            
            name = ValidateName(name);
            int hash = StringToHash(name);
            if (_propertiesMap.ContainsKey(hash)) return false;

            property = new BlackboardProperty {
                name = name,
                serializedType = SerializedType.ToString(type),
                index = _propertiesMap.Count,
            };

            SetValueEntry(type, hash, value);

            _propertiesMap[hash] = property;
            return true;
        }

        public bool TrySetPropertyName(int hash, string newName) {
            if (!_propertiesMap.TryGetValue(hash, out var property)) return false;

            newName = ValidateName(newName);
            int newHash = StringToHash(newName);
            if (_propertiesMap.ContainsKey(newHash)) return false;

            property.name = newName;

            _propertiesMap.Remove(hash);
            _propertiesMap[newHash] = property;

            return true;
        }

        public bool TrySetPropertyValue(int hash, object value) {
            if (!_propertiesMap.TryGetValue(hash, out var property)) return false;

            SetValueEntry(GetPropertyType(property), hash, value);
            return true;
        }

        public bool TrySetPropertyIndex(int hash, int newIndex) {
            if (!_propertiesMap.TryGetValue(hash, out var property)) return false;

            int oldIndex = property.index;

            property.index = newIndex;
            _propertiesMap[hash] = property;

            bool hasPropertyWithNewIndex = false;
            int newIndexPropertyHash = 0;

            foreach ((int h, var p) in _propertiesMap) {
                if (p.index != newIndex) continue;

                newIndexPropertyHash = h;
                hasPropertyWithNewIndex = true;
                break;
            }

            if (hasPropertyWithNewIndex) {
                var p = _propertiesMap[newIndexPropertyHash];
                p.index = oldIndex;
                _propertiesMap[newIndexPropertyHash] = p;
            }

            return true;
        }

        public bool RemoveProperty(int hash) {
            if (!_propertiesMap.TryGetValue(hash, out var property)) return false;

            _propertiesMap.Remove(hash);
            RemoveValueEntry(GetPropertyType(property), hash);

            return true;
        }

        public Blackboard Clone() {
            return new Blackboard {
                _propertiesMap = new SerializedDictionary<int, BlackboardProperty>(_propertiesMap),
                _bools = new SerializedDictionary<int, bool>(_bools),
                _floats = new SerializedDictionary<int, float>(_floats),
                _ints = new SerializedDictionary<int, int>(_ints),
                _strings = new SerializedDictionary<int, string>(_strings),
                _vectors2 = new SerializedDictionary<int, Vector2>(_vectors2),
                _vectors3 = new SerializedDictionary<int, Vector3>(_vectors3),
                _blackboardEvents = new SerializedDictionary<int, BlackboardEvent>(_blackboardEvents),
                _scriptableObjects = new SerializedDictionary<int, ScriptableObject>(_scriptableObjects),
                _gameObjects = new SerializedDictionary<int, GameObject>(_gameObjects)
            };
        }

        private void SetValueEntry(Type type, int hash, object value) {
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

            if (type == typeof(BlackboardEvent)) {
                _blackboardEvents[hash] = value as BlackboardEvent;
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

        private void RemoveValueEntry(Type type, int hash) {
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

            if (type == typeof(BlackboardEvent)) {
                _blackboardEvents.Remove(hash);
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
            if (!_propertiesMap.ContainsKey(hash)) return name;

            int count = 1;
            string pattern = $@"{name} \([0-9]+\)";

            foreach (var property in _propertiesMap.Values) {
                if (property.name.IsValidForPattern(pattern)) count++;
            }

            return $"{name} ({count})";
        }

        private static bool ValidateType(Type type) {
            if (SupportedTypes.Contains(type)) return true;

            Debug.LogError($"Blackboard does not support type {type.Name}");
            return false;
        }
    }

}
