using System.Collections.Generic;
using UnityEngine;

namespace MisterGames.Common.Data {

    public sealed class RuntimeBlackboard {

        private readonly Dictionary<int, bool> _bools = new Dictionary<int, bool>();
        private readonly Dictionary<int, float> _floats = new Dictionary<int, float>();
        private readonly Dictionary<int, int> _ints = new Dictionary<int, int>();
        private readonly Dictionary<int, string> _strings = new Dictionary<int, string>();
        private readonly Dictionary<int, Vector3> _vectors3 = new Dictionary<int, Vector3>();
        private readonly Dictionary<int, Vector2> _vectors2 = new Dictionary<int, Vector2>();
        private readonly Dictionary<int, GameObject> _gameObjects = new Dictionary<int, GameObject>();
        private readonly Dictionary<int, ScriptableObject> _scriptableObjects = new Dictionary<int, ScriptableObject>();
        private readonly Dictionary<int, BlackboardEvent> _blackboardEvents = new Dictionary<int, BlackboardEvent>();

        public bool GetBool(int hash) {
            if (_bools.TryGetValue(hash, out bool value)) return value;

            Debug.LogWarning($"Blackboard: trying to get not existing bool value for hash {hash} ]");
            return default;
        }

        public float GetFloat(int hash) {
            if (_floats.TryGetValue(hash, out float value)) return value;

            Debug.LogWarning($"Blackboard: trying to get not existing float value for hash {hash} ]");
            return default;
        }

        public int GetInt(int hash) {
            if (_ints.TryGetValue(hash, out int value)) return value;

            Debug.LogWarning($"Blackboard: trying to get not existing int value for hash {hash} ]");
            return default;
        }

        public string GetString(int hash) {
            if (_strings.TryGetValue(hash, out string value)) return value;

            Debug.LogWarning($"Blackboard: trying to get not existing string value for hash {hash} ]");
            return default;
        }

        public Vector2 GetVector2(int hash) {
            if (_vectors2.TryGetValue(hash, out var value)) return value;

            Debug.LogWarning($"Blackboard: trying to get not existing Vector2 value for hash {hash} ]");
            return default;
        }

        public Vector3 GetVector3(int hash) {
            if (_vectors3.TryGetValue(hash, out var value)) return value;

            Debug.LogWarning($"Blackboard: trying to get not existing Vector3 value for hash {hash} ]");
            return default;
        }

        public ScriptableObject GetScriptableObject(int hash) {
            if (_scriptableObjects.TryGetValue(hash, out var value)) return value;

            Debug.LogWarning($"Blackboard: trying to get not existing ScriptableObject value for hash {hash} ]");
            return default;
        }

        public GameObject GetGameObject(int hash) {
            if (_gameObjects.TryGetValue(hash, out var value)) return value;

            Debug.LogWarning($"Blackboard: trying to get not existing GameObject value for hash {hash} ]");
            return default;
        }

        public BlackboardEvent GetBlackboardEvent(int hash) {
            if (_blackboardEvents.TryGetValue(hash, out var value)) return value;

            Debug.LogWarning($"Blackboard: trying to get not existing BlackboardEvent value for hash {hash}]");
            return default;
        }

        public void SetBool(int hash, bool value) {
            _bools[hash] = value;
        }

        public void SetFloat(int hash, float value) {
            _floats[hash] = value;
        }

        public void SetInt(int hash, int value) {
            _ints[hash] = value;
        }

        public void SetString(int hash, string value) {
            _strings[hash] = value;
        }

        public void SetVector2(int hash, Vector2 value) {
            _vectors2[hash] = value;
        }

        public void SetVector3(int hash, Vector3 value) {
            _vectors3[hash] = value;
        }

        public void SetScriptableObject(int hash, ScriptableObject value) {
            _scriptableObjects[hash] = value;
        }

        public void SetGameObject(int hash, GameObject value) {
            _gameObjects[hash] = value;
        }

        public void SetBlackboardEvent(int hash, BlackboardEvent value) {
            _blackboardEvents[hash] = value;
        }

        public bool HasBool(int hash) {
            return _bools.ContainsKey(hash);
        }

        public bool HasFloat(int hash) {
            return _floats.ContainsKey(hash);
        }

        public bool HasInt(int hash) {
            return _ints.ContainsKey(hash);
        }

        public bool HasString(int hash) {
            return _strings.ContainsKey(hash);
        }

        public bool HasVector2(int hash) {
            return _vectors2.ContainsKey(hash);
        }

        public bool HasVector3(int hash) {
            return _vectors3.ContainsKey(hash);
        }

        public bool HasScriptableObject(int hash) {
            return _scriptableObjects.ContainsKey(hash);
        }

        public bool HasGameObject(int hash) {
            return _gameObjects.ContainsKey(hash);
        }

        public bool HasBlackboardEvent(int hash) {
            return _blackboardEvents.ContainsKey(hash);
        }
    }

}
