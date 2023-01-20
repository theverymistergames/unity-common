using System;
using System.Collections.Generic;
using System.Linq;
using MisterGames.Common.Strings;
using UnityEngine;

namespace MisterGames.Common.Data {
    
    [Serializable]
    public struct BlackboardProperty {
        public int index;
        public string name;
        public string serializedValue;
        public string serializedType;
    }

    public sealed class BlackboardEvent {
        public event Action OnEmit = delegate {  };
        public void Emit() => OnEmit.Invoke();
    }

    [Serializable]
    public sealed class Blackboard  {

        public static readonly List<Type> SupportedTypes = new List<Type> {
            typeof(bool),
            typeof(float),
            typeof(int),
            typeof(string),
            typeof(Vector2),
            typeof(Vector3),
            typeof(ScriptableObject),
            typeof(GameObject),
            typeof(BlackboardEvent),
        };

        [SerializeField] private IntToBlackboardPropertyMap _propertiesMap;
        public Dictionary<int, BlackboardProperty> PropertiesMap => _propertiesMap;

        [Serializable] private sealed class IntToBlackboardPropertyMap : SerializedDictionary<int, BlackboardProperty> {}

        [Serializable] private struct Bool { public bool value; }
        [Serializable] private struct Float { public float value; }
        [Serializable] private struct Integer { public int value; }

        private readonly Dictionary<int, bool> _bools = new Dictionary<int, bool>();
        private readonly Dictionary<int, float> _floats = new Dictionary<int, float>();
        private readonly Dictionary<int, int> _ints = new Dictionary<int, int>();
        private readonly Dictionary<int, string> _strings = new Dictionary<int, string>();
        private readonly Dictionary<int, Vector3> _vectors3 = new Dictionary<int, Vector3>();
        private readonly Dictionary<int, Vector2> _vectors2 = new Dictionary<int, Vector2>();
        private readonly Dictionary<int, GameObject> _gameObjects = new Dictionary<int, GameObject>();
        private readonly Dictionary<int, ScriptableObject> _scriptableObjects = new Dictionary<int, ScriptableObject>();
        private readonly Dictionary<int, BlackboardEvent> _events = new Dictionary<int, BlackboardEvent>();

        private static readonly Dictionary<Type, string> NameOverrides = new Dictionary<Type, string> {
            [typeof(bool)] = "Boolean",
            [typeof(float)] = "Float",
            [typeof(int)] = "Int",
            [typeof(string)] = "String",
        };
        
        public static int StringToHash(string name) {
            return name.GetHashCode();
        }

        public static string GetTypeName(Type type) {
            return NameOverrides.TryGetValue(type, out string typeName) ? typeName : type.Name;
        }
        
        public static Type GetPropertyType(BlackboardProperty property) {
            return SerializedType.FromString(property.serializedType);
        }

        public static T GetPropertyValue<T>(BlackboardProperty property) {
            var type = GetPropertyType(property);

            if (Is<bool>(type)) {
                bool value = JsonUtility.FromJson<Bool>(property.serializedValue).value;
                return As<T, bool>(value);
            }
                
            if (Is<float>(type)) {
                float value = JsonUtility.FromJson<Float>(property.serializedValue).value;
                return As<T, float>(value);
            }
                
            if (Is<int>(type)) {
                int value = JsonUtility.FromJson<Integer>(property.serializedValue).value;
                return As<T, int>(value);
            }
                
            if (Is<string>(type)) {
                string value = property.serializedValue;
                return As<T, string>(value);
            }
                
            if (Is<Vector2>(type)) {
                var value = JsonUtility.FromJson<Vector2>(property.serializedValue);
                return As<T, Vector2>(value);
            }
                
            if (Is<Vector3>(type)) {
                var value = JsonUtility.FromJson<Vector3>(property.serializedValue);
                return As<T, Vector3>(value);
            }

            if (Is<ScriptableObject>(type)) {
                string name = property.serializedValue;
                var assets = ScriptableObjectsStorage.FindAssetsByName<ScriptableObject>(name);
                var value = assets.Length == 0 ? null : assets[0];
                return As<T, ScriptableObject>(value);
            }

            return default;
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
                serializedValue = SerializeValue(type, value),
                index = _propertiesMap.Count,
            };

            _propertiesMap[hash] = property;
            return true;
        }

        public bool TrySetPropertyName(string oldName, string newName) {
            int oldHash = StringToHash(oldName);
            if (!_propertiesMap.TryGetValue(oldHash, out var property)) return false;

            newName = ValidateName(newName);
            int newHash = StringToHash(newName);
            if (_propertiesMap.ContainsKey(newHash)) return false;

            property.name = newName;

            _propertiesMap.Remove(oldHash);
            _propertiesMap[newHash] = property;

            return true;
        }

        public bool TrySetPropertyValue(string name, object value) {
            int hash = StringToHash(name);
            if (!_propertiesMap.TryGetValue(hash, out var property)) return false;

            property.serializedValue = SerializeValue(GetPropertyType(property), value);
            _propertiesMap[hash] = property;

            return true;
        }

        public bool TrySetPropertyIndex(string name, int newIndex) {
            int hash = StringToHash(name);
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

        public void RemoveProperty(string name) {
            int hash = StringToHash(name);
            if (!_propertiesMap.ContainsKey(hash)) return;

            _propertiesMap.Remove(hash);
        }

        public void Compile() {
            foreach ((int hash, var property) in _propertiesMap) {
                InitProperty(hash, property);
            }
        }
        
        public T Get<T>(int hash) {
            var type = typeof(T);

            if (Is<bool>(type) && _bools.TryGetValue(hash, out bool b)) return As<T, bool>(b);
            if (Is<float>(type) && _floats.TryGetValue(hash, out float f)) return As<T, float>(f);
            if (Is<int>(type) && _ints.TryGetValue(hash, out int i)) return As<T, int>(i);
            if (Is<string>(type) && _strings.TryGetValue(hash, out string s)) return As<T, string>(s);
            if (Is<Vector2>(type) && _vectors2.TryGetValue(hash, out var v2)) return As<T, Vector2>(v2);
            if (Is<Vector3>(type) && _vectors3.TryGetValue(hash, out var v3)) return As<T, Vector3>(v3);
            if (Is<ScriptableObject>(type) && _scriptableObjects.TryGetValue(hash, out var so)) return As<T, ScriptableObject>(so);
            if (Is<GameObject>(type) && _gameObjects.TryGetValue(hash, out var go)) return As<T, GameObject>(go);
            if (Is<BlackboardEvent>(type) && _events.TryGetValue(hash, out var evt)) return As<T, BlackboardEvent>(evt);

            Debug.LogError($"Blackboard: trying to get not existing value of type {type.Name}");
            return default;
        }
        
        public void Set<T>(int hash, T value) {
            var type = typeof(T);

            if (Is<bool>(type) && _bools.ContainsKey(hash)) {
                _bools[hash] = As<bool, T>(value);
                return;
            }

            if (Is<float>(type) && _floats.ContainsKey(hash)) {
                _floats[hash] = As<float, T>(value);
                return;
            }
            if (Is<int>(type) && _ints.ContainsKey(hash)) {
                _ints[hash] = As<int, T>(value);
                return;
            }
            
            if (value is string s && _strings.ContainsKey(hash)) {
                _strings[hash] = s;
                return;
            }
            
            if (Is<Vector2>(type) && _vectors2.ContainsKey(hash)) {
                _vectors2[hash] = As<Vector2, T>(value);
                return;
            }
            
            if (Is<Vector3>(type) && _vectors3.ContainsKey(hash)) {
                _vectors3[hash] = As<Vector3, T>(value);
                return;
            }
            
            if (value is ScriptableObject so && _scriptableObjects.ContainsKey(hash)) {
                _scriptableObjects[hash] = so;
                return;
            }
            
            if (value is GameObject go && _gameObjects.ContainsKey(hash)) {
                _gameObjects[hash] = go;
                return;
            }
            
            if (value is BlackboardEvent && _events.ContainsKey(hash)) {
                Debug.LogWarning($"Blackboard: cannot set new value to {nameof(BlackboardEvent)}");
                return;
            }
            
            Debug.LogError($"Blackboard: trying to set not existing value {value} of type {type.Name}");
        }

        public bool Contains<T>(int hash) {
            var type = typeof(T);
            
            if (Is<bool>(type)) return _bools.ContainsKey(hash);
            if (Is<float>(type)) return _floats.ContainsKey(hash);
            if (Is<int>(type)) return _ints.ContainsKey(hash);
            if (Is<string>(type)) return _strings.ContainsKey(hash);
            if (Is<Vector2>(type)) return _vectors2.ContainsKey(hash);
            if (Is<Vector3>(type)) return _vectors3.ContainsKey(hash);
            if (Is<ScriptableObject>(type)) return _scriptableObjects.ContainsKey(hash);
            if (Is<GameObject>(type)) return _gameObjects.ContainsKey(hash);
            if (Is<BlackboardEvent>(type)) return _events.ContainsKey(hash);
            
            return false;
        }

        private void InitProperty(int hash, BlackboardProperty property) {
            var type = GetPropertyType(property);

            if (Is<bool>(type)) {
                _bools[hash] = GetPropertyValue<bool>(property);
                return;
            }
            
            if (Is<float>(type)) {
                _floats[hash] = GetPropertyValue<float>(property);
                return;
            }
            
            if (Is<int>(type)) {
                _ints[hash] = GetPropertyValue<int>(property);
                return;
            }
            
            if (Is<string>(type)) {
                _strings[hash] = GetPropertyValue<string>(property);
                return;
            }
            
            if (Is<Vector2>(type)) {
                _vectors2[hash] = GetPropertyValue<Vector2>(property);
                return;
            }
            
            if (Is<Vector3>(type)) {
                _vectors3[hash] = GetPropertyValue<Vector3>(property);
                return;
            }

            if (Is<ScriptableObject>(type)) {
                _scriptableObjects[hash] = GetPropertyValue<ScriptableObject>(property);
                return;
            }
            
            if (Is<GameObject>(type)) {
                _gameObjects[hash] = null;
                return;
            }
            
            if (Is<BlackboardEvent>(type)) {
                _events[hash] = new BlackboardEvent();
                return;
            }
        }

        private static string SerializeValue(Type type, object value) {
            if (Is<bool>(type)) return JsonUtility.ToJson(new Bool { value = As<bool, object>(value) });
            if (Is<float>(type)) return JsonUtility.ToJson(new Float { value = As<float, object>(value) });
            if (Is<int>(type)) return JsonUtility.ToJson(new Integer { value = As<int, object>(value) });
            if (Is<string>(type)) return As<string, object>(value);
            if (Is<Vector2>(type)) return JsonUtility.ToJson(As<Vector2, object>(value));
            if (Is<Vector3>(type)) return JsonUtility.ToJson(As<Vector3, object>(value));

            if (Is<ScriptableObject>(type)) return value == null ? "" : As<ScriptableObject, object>(value).name;

            if (Is<BlackboardEvent>(type)) return "";
            if (Is<GameObject>(type)) return "";
            
            return JsonUtility.ToJson(value);
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

        public static bool ValidateType(Type type) {
            if (SupportedTypes.Contains(type)) return true;
            Debug.LogError($"Blackboard does not support type {type.Name}");
            return false;
        }

        private static R As<R, V>(V value) {
            if (value == null) return default;
            return value is R r ? r : default;
        }

        private static bool Is<T>(Type type) {
            return typeof(T) == type;
        }
    }

}
