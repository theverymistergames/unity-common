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

        public SerializedDictionary<int, BlackboardProperty> PropertiesMap;

        public SerializedDictionary<int, bool> Bools;
        public SerializedDictionary<int, float> Floats;
        public SerializedDictionary<int, int> Ints;
        public SerializedDictionary<int, string> Strings;
        public SerializedDictionary<int, Vector2> Vectors2;
        public SerializedDictionary<int, Vector3> Vectors3;
        public SerializedDictionary<int, BlackboardEvent> BlackboardEvents;
        public SerializedDictionary<int, ScriptableObject> ScriptableObjects;
        public SerializedDictionary<int, GameObject> GameObjects;

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
            if (Bools.TryGetValue(hash, out bool value)) return value;

            Debug.LogWarning($"Blackboard: trying to get not existing bool value for hash {hash}");
            return default;
        }

        public float GetFloat(int hash) {
            if (Floats.TryGetValue(hash, out float value)) return value;

            Debug.LogWarning($"Blackboard: trying to get not existing float value for hash {hash}");
            return default;
        }

        public int GetInt(int hash) {
            if (Ints.TryGetValue(hash, out int value)) return value;

            Debug.LogWarning($"Blackboard: trying to get not existing int value for hash {hash}");
            return default;
        }

        public string GetString(int hash) {
            if (Strings.TryGetValue(hash, out string value)) return value;

            Debug.LogWarning($"Blackboard: trying to get not existing string value for hash {hash}");
            return default;
        }

        public Vector2 GetVector2(int hash) {
            if (Vectors2.TryGetValue(hash, out var value)) return value;

            Debug.LogWarning($"Blackboard: trying to get not existing Vector2 value for hash {hash}");
            return default;
        }

        public Vector3 GetVector3(int hash) {
            if (Vectors3.TryGetValue(hash, out var value)) return value;

            Debug.LogWarning($"Blackboard: trying to get not existing Vector3 value for hash {hash}");
            return default;
        }

        public BlackboardEvent GetBlackboardEvent(int hash) {
            if (BlackboardEvents.TryGetValue(hash, out var value)) return value;

            Debug.LogWarning($"Blackboard: trying to get not existing BlackboardEvent value for hash {hash}");
            return default;
        }

        public ScriptableObject GetScriptableObject(int hash) {
            if (ScriptableObjects.TryGetValue(hash, out var value)) return value;

            Debug.LogWarning($"Blackboard: trying to get not existing ScriptableObject value for hash {hash}");
            return default;
        }

        public GameObject GetGameObject(int hash) {
            if (GameObjects.TryGetValue(hash, out var value)) return value;

            Debug.LogWarning($"Blackboard: trying to get not existing GameObject value for hash {hash}");
            return default;
        }

        public void SetBool(int hash, bool value) {
            if (!Bools.ContainsKey(hash)) {
                Debug.LogWarning($"Blackboard: trying to set not existing bool value for hash {hash}");
                return;
            }

            Bools[hash] = value;
        }

        public void SetFloat(int hash, float value) {
            if (!Floats.ContainsKey(hash)) {
                Debug.LogWarning($"Blackboard: trying to set not existing float value for hash {hash}");
                return;
            }

            Floats[hash] = value;
        }

        public void SetInt(int hash, int value) {
            if (!Ints.ContainsKey(hash)) {
                Debug.LogWarning($"Blackboard: trying to set not existing int value for hash {hash}");
                return;
            }

            Ints[hash] = value;
        }

        public void SetString(int hash, string value) {
            if (!Strings.ContainsKey(hash)) {
                Debug.LogWarning($"Blackboard: trying to set not existing string value for hash {hash}");
                return;
            }

            Strings[hash] = value;
        }

        public void SetVector2(int hash, Vector2 value) {
            if (!Vectors2.ContainsKey(hash)) {
                Debug.LogWarning($"Blackboard: trying to set not existing Vector2 value for hash {hash}");
                return;
            }

            Vectors2[hash] = value;
        }

        public void SetVector3(int hash, Vector3 value) {
            if (!Vectors3.ContainsKey(hash)) {
                Debug.LogWarning($"Blackboard: trying to set not existing Vector3 value for hash {hash}");
                return;
            }

            Vectors3[hash] = value;
        }

        public void SetBlackboardEvent(int hash, BlackboardEvent value) {
            if (!BlackboardEvents.ContainsKey(hash)) {
                Debug.LogWarning($"Blackboard: trying to set not existing BlackboardEvent value for hash {hash}");
                return;
            }

            BlackboardEvents[hash] = value;
        }

        public void SetScriptableObject(int hash, ScriptableObject value) {
            if (!ScriptableObjects.ContainsKey(hash)) {
                Debug.LogWarning($"Blackboard: trying to set not existing ScriptableObject value for hash {hash}");
                return;
            }

            ScriptableObjects[hash] = value;
        }

        public void SetGameObject(int hash, GameObject value) {
            if (!GameObjects.ContainsKey(hash)) {
                Debug.LogWarning($"Blackboard: trying to set not existing GameObject value for hash {hash}");
                return;
            }

            GameObjects[hash] = value;
        }

        public bool TryAddProperty(string name, Type type, out BlackboardProperty property) {
            property = default;
            if (!ValidateType(type)) return false;
            
            name = ValidateName(name);
            int hash = StringToHash(name);
            if (PropertiesMap.ContainsKey(hash)) return false;

            property = new BlackboardProperty {
                name = name,
                serializedType = SerializedType.ToString(type),
                index = PropertiesMap.Count,
            };

            SetValue(type, hash, default);

            PropertiesMap[hash] = property;
            return true;
        }

        public bool TrySetPropertyName(int hash, string newName) {
            if (!PropertiesMap.TryGetValue(hash, out var property)) return false;


            newName = ValidateName(newName);
            int newHash = StringToHash(newName);
            if (PropertiesMap.ContainsKey(newHash)) return false;

            property.name = newName;

            PropertiesMap.Remove(hash);
            PropertiesMap[newHash] = property;

            var type = GetPropertyType(property);

            object value = GetValue(type, hash);
            RemoveValue(type, hash);
            SetValue(type, newHash, value);

            return true;
        }

        public bool TrySetPropertyValue(int hash, object value) {
            if (!PropertiesMap.TryGetValue(hash, out var property)) return false;

            SetValue(GetPropertyType(property), hash, value);
            return true;
        }

        public bool TrySetPropertyIndex(int hash, int newIndex) {
            if (!PropertiesMap.TryGetValue(hash, out var property)) return false;

            int oldIndex = property.index;

            property.index = newIndex;
            PropertiesMap[hash] = property;

            bool hasPropertyWithNewIndex = false;
            int newIndexPropertyHash = 0;

            foreach ((int h, var p) in PropertiesMap) {
                if (p.index != newIndex) continue;

                newIndexPropertyHash = h;
                hasPropertyWithNewIndex = true;
                break;
            }

            if (hasPropertyWithNewIndex) {
                var p = PropertiesMap[newIndexPropertyHash];
                p.index = oldIndex;
                PropertiesMap[newIndexPropertyHash] = p;
            }

            return true;
        }

        public bool RemoveProperty(int hash) {
            if (!PropertiesMap.TryGetValue(hash, out var property)) return false;

            PropertiesMap.Remove(hash);
            RemoveValue(GetPropertyType(property), hash);

            return true;
        }

        public Blackboard Clone() {
            return new Blackboard {
                PropertiesMap = new SerializedDictionary<int, BlackboardProperty>(PropertiesMap),
                Bools = new SerializedDictionary<int, bool>(Bools),
                Floats = new SerializedDictionary<int, float>(Floats),
                Ints = new SerializedDictionary<int, int>(Ints),
                Strings = new SerializedDictionary<int, string>(Strings),
                Vectors2 = new SerializedDictionary<int, Vector2>(Vectors2),
                Vectors3 = new SerializedDictionary<int, Vector3>(Vectors3),
                BlackboardEvents = new SerializedDictionary<int, BlackboardEvent>(BlackboardEvents),
                ScriptableObjects = new SerializedDictionary<int, ScriptableObject>(ScriptableObjects),
                GameObjects = new SerializedDictionary<int, GameObject>(GameObjects)
            };
        }

        private object GetValue(Type type, int hash) {
            if (type == typeof(bool)) {
                return Bools[hash];
            }

            if (type == typeof(float)) {
                return Floats[hash];
            }

            if (type == typeof(int)) {
                return Ints[hash];
            }

            if (type == typeof(string)) {
                return Strings[hash];
            }

            if (type == typeof(Vector2)) {
                return Vectors2[hash];
            }

            if (type == typeof(Vector3)) {
                return Vectors3[hash];
            }

            if (type == typeof(BlackboardEvent)) {
                return BlackboardEvents[hash];
            }

            if (type == typeof(ScriptableObject)) {
                return ScriptableObjects[hash];
            }

            if (type == typeof(GameObject)) {
                return GameObjects[hash];
            }

            return default;
        }

        private void SetValue(Type type, int hash, object value) {
            if (type == typeof(bool)) {
                Bools[hash] = value is bool b ? b : default;
                return;
            }

            if (type == typeof(float)) {
                Floats[hash] = value is float f ? f : default;
                return;
            }

            if (type == typeof(int)) {
                Ints[hash] = value is int i ? i : default;
                return;
            }

            if (type == typeof(string)) {
                Strings[hash] = value as string;
                return;
            }

            if (type == typeof(Vector2)) {
                Vectors2[hash] = value is Vector2 v2 ? v2 : default;
                return;
            }

            if (type == typeof(Vector3)) {
                Vectors3[hash] = value is Vector3 v3 ? v3 : default;
                return;
            }

            if (type == typeof(BlackboardEvent)) {
                BlackboardEvents[hash] = value as BlackboardEvent;
                return;
            }

            if (type == typeof(ScriptableObject)) {
                ScriptableObjects[hash] = value as ScriptableObject;
                return;
            }

            if (type == typeof(GameObject)) {
                GameObjects[hash] = value as GameObject;
            }
        }

        private void RemoveValue(Type type, int hash) {
            if (type == typeof(bool)) {
                Bools.Remove(hash);
                return;
            }

            if (type == typeof(float)) {
                Floats.Remove(hash);
                return;
            }

            if (type == typeof(int)) {
                Ints.Remove(hash);
                return;
            }

            if (type == typeof(string)) {
                Strings.Remove(hash);
                return;
            }

            if (type == typeof(Vector2)) {
                Vectors2.Remove(hash);
                return;
            }

            if (type == typeof(Vector3)) {
                Vectors3.Remove(hash);
                return;
            }

            if (type == typeof(BlackboardEvent)) {
                BlackboardEvents.Remove(hash);
                return;
            }

            if (type == typeof(ScriptableObject)) {
                ScriptableObjects.Remove(hash);
                return;
            }

            if (type == typeof(GameObject)) {
                GameObjects.Remove(hash);
            }
        }

        private string ValidateName(string name) {
            int hash = StringToHash(name);
            if (!PropertiesMap.ContainsKey(hash)) return name;

            int count = 1;
            string pattern = $@"{name} \([0-9]+\)";

            foreach (var property in PropertiesMap.Values) {
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
