using System;
using System.Collections.Generic;
using MisterGames.Common.Lists;
using MisterGames.Common.Strings;
using UnityEngine;

namespace MisterGames.Common.Data {

    public interface IBlackboardEditor {
        
        IReadOnlyList<BlackboardProperty> Properties { get; }
        
        bool ValidateType(Type type);

        BlackboardProperty AddProperty(string name, Type type, object value = default);

        bool SetPropertyName(string oldName, string newName, out string name);

        void SetPropertyValue(string name, object value);

        void SetPropertyIndex(string name, int newIndex);

        void RemoveProperty(string name);
        
    }
    
    [Serializable]
    public struct BlackboardProperty {
        public int hash;
        public string name;
        public string value;
        public string type;
    }

    [Serializable]
    public class BlackboardEvent {
        public event Action OnEmit = delegate {  };
        public void Emit() => OnEmit.Invoke();
    }

    [Serializable]
    public sealed class Blackboard : IBlackboardEditor {

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
        
        private static readonly Dictionary<Type, string> NameOverrides = new Dictionary<Type, string> {
            [typeof(bool)] = "Boolean",
            [typeof(float)] = "Float",
            [typeof(int)] = "Int",
            [typeof(string)] = "String",
        };

        [SerializeField] private List<BlackboardProperty> _properties = new List<BlackboardProperty>();
        IReadOnlyList<BlackboardProperty> IBlackboardEditor.Properties => _properties;
        
        private readonly Dictionary<int, bool> _bools = new Dictionary<int, bool>();
        private readonly Dictionary<int, float> _floats = new Dictionary<int, float>();
        private readonly Dictionary<int, int> _ints = new Dictionary<int, int>();
        private readonly Dictionary<int, string> _strings = new Dictionary<int, string>();
        private readonly Dictionary<int, Vector3> _vectors3 = new Dictionary<int, Vector3>();
        private readonly Dictionary<int, Vector2> _vectors2 = new Dictionary<int, Vector2>();
        private readonly Dictionary<int, GameObject> _gameObjects = new Dictionary<int, GameObject>();
        private readonly Dictionary<int, ScriptableObject> _scriptableObjects = new Dictionary<int, ScriptableObject>();
        private readonly Dictionary<int, BlackboardEvent> _events = new Dictionary<int, BlackboardEvent>();

        public static int StringToHash(string name) {
            return name.GetHashCode();
        }

        public static string GetTypeName(Type type) {
            return NameOverrides.ContainsKey(type) ? NameOverrides[type] : type.Name;
        }
        
        public static Type GetPropertyType(BlackboardProperty data) {
            return SerializedType.FromString(data.type);
        }

        public static T GetPropertyValue<T>(BlackboardProperty property) {
            var type = GetPropertyType(property);
            
            if (Is<bool>(type)) {
                bool value = JsonUtility.FromJson<Bool>(property.value).value;
                return As<T, bool>(value);
            }
                
            if (Is<float>(type)) {
                float value = JsonUtility.FromJson<Float>(property.value).value;
                return As<T, float>(value);
            }
                
            if (Is<int>(type)) {
                int value = JsonUtility.FromJson<Integer>(property.value).value;
                return As<T, int>(value);
            }
                
            if (Is<string>(type)) {
                string value = JsonUtility.FromJson<String>(property.value).value;
                return As<T, string>(value);
            }
                
            if (Is<Vector2>(type)) {
                var value = JsonUtility.FromJson<Vector2>(property.value);
                return As<T, Vector2>(value);
            }
                
            if (Is<Vector3>(type)) {
                var value = JsonUtility.FromJson<Vector3>(property.value);
                return As<T, Vector3>(value);
            }

            if (Is<ScriptableObject>(type)) {
                string name = property.value;
                var assets = ScriptableObjectsStorage.FindAssetsByName<ScriptableObject>(name);
                var value = assets.IsEmpty() ? null : assets[0];
                return As<T, ScriptableObject>(value);
            }
            
            if (Is<GameObject>(type)) return As<T, GameObject>(null);
            if (Is<BlackboardEvent>(type)) return As<T, BlackboardEvent>(null);

            return default;
        }

        bool IBlackboardEditor.ValidateType(Type type) {
            return ValidateType(type);
        }
        
        BlackboardProperty IBlackboardEditor.AddProperty(string name, Type type, object value) {
            if (!ValidateType(type)) return default;
            
            name = ValidateName(name);

            var property = new BlackboardProperty {
                hash = StringToHash(name),
                name = name,
                type = SerializeType(type),
                value = SerializeValue(type, value),
            };

            _properties.Add(property);

            return property;
        }

        bool IBlackboardEditor.SetPropertyName(string oldName, string newName, out string name) {
            int hash = StringToHash(oldName);
            for (int i = 0; i < _properties.Count; i++) {
                var property = _properties[i];
                if (property.hash != hash) continue;

                name = ValidateName(newName);
                property.hash = StringToHash(name);
                property.name = name;
                _properties[i] = property;
                
                return true;
            }

            name = null;
            return false;
        }

        void IBlackboardEditor.SetPropertyValue(string name, object value) {
            int hash = StringToHash(name);
            for (int i = 0; i < _properties.Count; i++) {
                var property = _properties[i];
                if (property.hash != hash) continue;

                property.value = SerializeValue(GetPropertyType(property), value);
                _properties[i] = property;
                return;
            }
        }

        void IBlackboardEditor.SetPropertyIndex(string name, int newIndex) {
            int count = _properties.Count;
            if (newIndex >= count) return;
            
            int hash = StringToHash(name);
            int oldIndex = -1;
            
            for (int i = 0; i < count; i++) {
                var swapProperty = _properties[i];
                if (swapProperty.hash != hash) continue;
                
                if (i == newIndex) return;
                oldIndex = i;
                break;
            }
            
            if (oldIndex < 0) return;
            (_properties[newIndex], _properties[oldIndex]) = (_properties[oldIndex], _properties[newIndex]);
        }

        void IBlackboardEditor.RemoveProperty(string name) {
            int hash = StringToHash(name);
            for (int i = 0; i < _properties.Count; i++) {
                if (_properties[i].hash != hash) continue;
                _properties.RemoveAt(i);
                return;
            }
        }

        public void Init() {
            for (int i = 0; i < _properties.Count; i++) {
                InitProperty(_properties[i]);
            }
        }
        
        public T Get<T>(int hash) {
            if (!ValidateType<T>()) return default;
            var type = typeof(T);

            if (Is<bool>(type) && _bools.ContainsKey(hash)) return As<T, bool>(_bools[hash]);
            if (Is<float>(type) && _floats.ContainsKey(hash)) return As<T, float>(_floats[hash]);
            if (Is<int>(type) && _ints.ContainsKey(hash)) return As<T, int>(_ints[hash]);
            if (Is<string>(type) && _strings.ContainsKey(hash)) return As<T, string>(_strings[hash]);
            if (Is<Vector2>(type) && _vectors2.ContainsKey(hash)) return As<T, Vector2>(_vectors2[hash]);
            if (Is<Vector3>(type) && _vectors3.ContainsKey(hash)) return As<T, Vector3>(_vectors3[hash]);
            if (Is<ScriptableObject>(type) && _scriptableObjects.ContainsKey(hash)) return As<T, ScriptableObject>(_scriptableObjects[hash]);
            if (Is<GameObject>(type) && _gameObjects.ContainsKey(hash)) return As<T, GameObject>(_gameObjects[hash]);
            if (Is<BlackboardEvent>(type) && _events.ContainsKey(hash)) return As<T, BlackboardEvent>(_events[hash]);

            Debug.LogError($"Blackboard: trying to get not existing value of type {type.Name}");
            return default;
        }
        
        public void Set<T>(int hash, T value) {
            if (!ValidateType<T>()) return;
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
            
            if (Is<string>(type) && _strings.ContainsKey(hash)) {
                _strings[hash] = As<string, T>(value);
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
            
            if (Is<ScriptableObject>(type) && _scriptableObjects.ContainsKey(hash)) {
                _scriptableObjects[hash] = As<ScriptableObject, T>(value);
                return;
            }
            
            if (Is<GameObject>(type) && _gameObjects.ContainsKey(hash)) {
                _gameObjects[hash] = As<GameObject, T>(value);
                return;
            }
            
            if (Is<BlackboardEvent>(type) && _events.ContainsKey(hash)) {
                return;
            }
            
            Debug.LogError($"Blackboard: trying to set not existing value {value} of type {type.Name}");
        }

        public bool Contains<T>(int hash) {
            if (!ValidateType<T>()) return false;
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

        private void InitProperty(BlackboardProperty property) {
            int hash = property.hash;
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

        private static string SerializeType(Type type) {
            return SerializedType.ToString(type);
        }
        
        private static string SerializeValue(Type type, object value) {
            if (Is<bool>(type)) return JsonUtility.ToJson(new Bool { value = As<bool, object>(value) });
            if (Is<float>(type)) return JsonUtility.ToJson(new Float { value = As<float, object>(value) });
            if (Is<int>(type)) return JsonUtility.ToJson(new Integer { value = As<int, object>(value) });
            if (Is<string>(type)) return JsonUtility.ToJson(new String { value = As<string, object>(value) });
            if (Is<Vector2>(type)) return JsonUtility.ToJson(As<Vector2, object>(value));
            if (Is<Vector3>(type)) return JsonUtility.ToJson(As<Vector3, object>(value));

            if (Is<ScriptableObject>(type)) return value == null ? "" : As<ScriptableObject, object>(value).name;

            if (Is<BlackboardEvent>(type)) return "";
            if (Is<GameObject>(type)) return "";
            
            return JsonUtility.ToJson(value);
        }

        private string ValidateName(string name) {
            int hash = StringToHash(name);
            for (int i = 0; i < _properties.Count; i++) {
                var property = _properties[i];
                if (property.hash != hash) continue;

                int count = 1;
                string pattern = $@"{name} \([0-9]+\)";
                
                for (int j = 0; j < _properties.Count; j++) {
                    var p = _properties[j];
                    if (p.name.IsValidForPattern(pattern)) count++;
                }

                return $"{name} ({count})";
            }
            return name;
        }

        private static bool ValidateType<T>() {
            return ValidateType(typeof(T));
        }
        
        private static bool ValidateType(Type type) {
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

        private struct Bool { public bool value; }
        private struct Float { public float value; }
        private struct Integer { public int value; }
        private struct String { public string value; }

    }

}