using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Assertions;

namespace MisterGames.Common.Data {

    public class ObjectDataMap<T> {

        public int Count => _keys.Count;
        public T this[int index] => _data[_keys[index]];
        
        private readonly List<int> _keys = new List<int>();
        private readonly Dictionary<int, T> _data = new Dictionary<int, T>();
        
        public void Register(Object source, T defaultData) {
            int id = GetId(source);
            if (_data.ContainsKey(id)) return;
            
            _keys.Add(id);
            _data.Add(id, defaultData);
        }

        public void Unregister(Object source) {
            int id = GetId(source);
            _data.Remove(id);
            _keys.Remove(id);
        }
        
        public void Clear() {
            _data.Clear();
            _keys.Clear();
        }
        
        public T Get(Object source) {
            int id = GetId(source);
            AssertIsValidId(id);
            return _data[id];
        }
        
        public void Set(Object source, T newData) {
            int id = GetId(source);
            AssertIsValidId(id);
            _data[id] = newData;
        }

        private static int GetId(Object source) {
            return source.GetInstanceID();
        }
        
        [AssertionMethod]
        private void AssertIsValidId(int id) {
            Assert.IsTrue(_data.ContainsKey(id), "Not bound source trying to interact with data");
        }

    }

}