using System.Collections.Generic;
using UnityEngine;

namespace MisterGames.Common.Data {

    public sealed class RuntimeBlackboard {

        internal readonly Dictionary<int, bool> _bools = new Dictionary<int, bool>();
        internal readonly Dictionary<int, float> _floats = new Dictionary<int, float>();
        internal readonly Dictionary<int, int> _ints = new Dictionary<int, int>();
        internal readonly Dictionary<int, string> _strings = new Dictionary<int, string>();
        internal readonly Dictionary<int, Vector3> _vectors3 = new Dictionary<int, Vector3>();
        internal readonly Dictionary<int, Vector2> _vectors2 = new Dictionary<int, Vector2>();
        internal readonly Dictionary<int, GameObject> _gameObjects = new Dictionary<int, GameObject>();
        internal readonly Dictionary<int, ScriptableObject> _scriptableObjects = new Dictionary<int, ScriptableObject>();
        internal readonly Dictionary<int, BlackboardEvent> _events = new Dictionary<int, BlackboardEvent>();

        public T Get<T>(int hash) {
            var type = typeof(T);

            if (Blackboard.Is<bool>(type) && _bools.TryGetValue(hash, out bool b)) return Blackboard.As<T, bool>(b);
            if (Blackboard.Is<float>(type) && _floats.TryGetValue(hash, out float f)) return Blackboard.As<T, float>(f);
            if (Blackboard.Is<int>(type) && _ints.TryGetValue(hash, out int i)) return Blackboard.As<T, int>(i);
            if (Blackboard.Is<string>(type) && _strings.TryGetValue(hash, out string s)) return Blackboard.As<T, string>(s);
            if (Blackboard.Is<Vector2>(type) && _vectors2.TryGetValue(hash, out var v2)) return Blackboard.As<T, Vector2>(v2);
            if (Blackboard.Is<Vector3>(type) && _vectors3.TryGetValue(hash, out var v3)) return Blackboard.As<T, Vector3>(v3);
            if (Blackboard.Is<ScriptableObject>(type) && _scriptableObjects.TryGetValue(hash, out var so)) return Blackboard.As<T, ScriptableObject>(so);
            if (Blackboard.Is<GameObject>(type) && _gameObjects.TryGetValue(hash, out var go)) return Blackboard.As<T, GameObject>(go);
            if (Blackboard.Is<BlackboardEvent>(type) && _events.TryGetValue(hash, out var evt)) return Blackboard.As<T, BlackboardEvent>(evt);

            Debug.LogError($"Blackboard: trying to get not existing value of type {type.Name}");
            return default;
        }

        public void Set<T>(int hash, T value) {
            var type = typeof(T);

            if (Blackboard.Is<bool>(type) && _bools.ContainsKey(hash)) {
                _bools[hash] = Blackboard.As<bool, T>(value);
                return;
            }

            if (Blackboard.Is<float>(type) && _floats.ContainsKey(hash)) {
                _floats[hash] = Blackboard.As<float, T>(value);
                return;
            }
            if (Blackboard.Is<int>(type) && _ints.ContainsKey(hash)) {
                _ints[hash] = Blackboard.As<int, T>(value);
                return;
            }

            if (value is string s && _strings.ContainsKey(hash)) {
                _strings[hash] = s;
                return;
            }

            if (Blackboard.Is<Vector2>(type) && _vectors2.ContainsKey(hash)) {
                _vectors2[hash] = Blackboard.As<Vector2, T>(value);
                return;
            }

            if (Blackboard.Is<Vector3>(type) && _vectors3.ContainsKey(hash)) {
                _vectors3[hash] = Blackboard.As<Vector3, T>(value);
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

            if (Blackboard.Is<bool>(type)) return _bools.ContainsKey(hash);
            if (Blackboard.Is<float>(type)) return _floats.ContainsKey(hash);
            if (Blackboard.Is<int>(type)) return _ints.ContainsKey(hash);
            if (Blackboard.Is<string>(type)) return _strings.ContainsKey(hash);
            if (Blackboard.Is<Vector2>(type)) return _vectors2.ContainsKey(hash);
            if (Blackboard.Is<Vector3>(type)) return _vectors3.ContainsKey(hash);
            if (Blackboard.Is<ScriptableObject>(type)) return _scriptableObjects.ContainsKey(hash);
            if (Blackboard.Is<GameObject>(type)) return _gameObjects.ContainsKey(hash);
            if (Blackboard.Is<BlackboardEvent>(type)) return _events.ContainsKey(hash);

            return false;
        }

    }
}
