using System;
using System.Collections.Generic;
using MisterGames.Common.Attributes;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Actors
{
    
    [DefaultExecutionOrder(-1000)]
    public sealed class Actor : MonoBehaviour, IActor {
        
        [EmbeddedInspector] [SerializeField] private ActorData _data;
        
        public Transform Transform { get; private set; }
        public GameObject GameObject { get; private set; }

        private Dictionary<Type, IActorData> _dataMap;
        private MultiValueDictionary<Type, object> _genericComponentMap;
        private List<IActorComponent> _actorComponents;
        private bool _isAwake;
        
        private void Awake() {
            GameObject = gameObject;
            Transform = transform;
            
            _isAwake = true;
            FetchData();
            FetchActorComponents();
            
            NotifyAwake();
            NotifyDataUpdated();
        }

        private void OnDestroy() {
            _isAwake = false;
            NotifyDestroy();

            _dataMap?.Clear();
            _genericComponentMap?.Clear();
            _actorComponents?.Clear();
            
#if UNITY_EDITOR
            _addedAtRuntimeDataTypes.Clear();
            _prefabDataTypes.Clear();
            _removeDataTypesCache.Clear();
#endif
        }

        private void OnEnable() {
#if UNITY_EDITOR
            RebindData();
#endif
        }

        private void OnDisable() {
#if UNITY_EDITOR
            UnbindData();
#endif      
        }
        
        public T GetData<T>() where T : class, IActorData {
            return _dataMap.GetValueOrDefault(typeof(T)) as T;
        }

        public void SetData<T>(T data) where T : class, IActorData {
#if UNITY_EDITOR
            if (!_dataMap.ContainsKey(typeof(T))) _addedAtRuntimeDataTypes.Add(typeof(T));
#endif
            
            _dataMap[typeof(T)] = data;
            NotifyDataUpdated();
        }

        public bool RemoveData<T>() where T : class, IActorData {
#if UNITY_EDITOR
            _addedAtRuntimeDataTypes.Add(typeof(T));
#endif

            if (_dataMap.Remove(typeof(T))) {
                NotifyDataUpdated();
                return true;
            }
            
            return false;
        }

        public new T GetComponent<T>() where T : class {
            return TryGetComponent<T>(out var component) ? component : default;
        }

        public new bool TryGetComponent<T>(out T component) where T : class {
            var type = typeof(T);

            if (type == typeof(IActorComponent)) {
                FetchActorComponents();
                component = _actorComponents.Count > 0 ? _actorComponents[0] as T : default;
                return component != null;
            }
            
            if (_genericComponentMap == null || !_genericComponentMap.ContainsKey(type)) {
                FetchComponentsOfType<T>();
            }

            if (_genericComponentMap!.TryGetFirstValue(type, out object c) && c is T componentT) {
                component = componentT;
                return true;
            }
            
            component = default;
            return false;
        }

        public new ComponentCollection<T> GetComponents<T>() where T : class {
            var type = typeof(T);
            if (type == typeof(IActorComponent)) return new ComponentCollection<T>(_actorComponents);
            
            if (!_genericComponentMap.ContainsKey(typeof(T))) FetchComponentsOfType<T>();
            return new ComponentCollection<T>(_genericComponentMap);
        }

        public void DestroyActor(float time = 0f) {
            if (time > 0f) Destroy(gameObject, time);
            else Destroy(gameObject);
        }

        private void NotifyAwake() {
            for (int i = 0, count = _actorComponents.Count; i < count && _isAwake; i++) {
                _actorComponents[i]?.OnAwakeActor(this);
            }
        }
        
        private void NotifyDestroy() {
            for (int i = 0, count = _actorComponents.Count; i < count; i++) {
                _actorComponents[i]?.OnDestroyActor(this);
            }
        }
        
        private void NotifyDataUpdated()
        {
            for (int i = 0, count = _actorComponents.Count; i < count && _isAwake; i++) {
                _actorComponents[i]?.OnActorDataUpdated(this);
            }
        }

        private void FetchActorComponents() {
            _actorComponents ??= new List<IActorComponent>(GetComponentsInChildren<IActorComponent>(includeInactive: true));
        }
        
        private void FetchComponentsOfType<T>() where T : class {
            var type = typeof(T);
            var results = GetComponentsInChildren<T>(includeInactive: true);
            _genericComponentMap ??= new MultiValueDictionary<Type, object>(results.Length);
            
            for (int i = 0; i < results.Length; i++) {
                _genericComponentMap.AddValue(type, results[i]);
            }
        }
        
        private void FetchData()
        {
            if (_data == null)
            {
                _dataMap ??= new Dictionary<Type, IActorData>();
                return;
            }

            var prefabData = _data.Data;
            int count = prefabData.Count;
            
            _dataMap ??= new Dictionary<Type, IActorData>(count);

#if UNITY_EDITOR
            _prefabDataTypes.Clear();
            _removeDataTypesCache.Clear();
#endif
            
            for (int i = 0; i < count; i++) {
                if (prefabData[i] is not { } data) continue;
                
                _dataMap[data.GetType()] = data;
                
#if UNITY_EDITOR
                _prefabDataTypes.Add(data.GetType());
#endif
            }
            
#if UNITY_EDITOR
            foreach (var data in _dataMap.Values) {
                if (!_prefabDataTypes.Contains(data.GetType()) && 
                    !_addedAtRuntimeDataTypes.Contains(data.GetType())
                ) {
                    _removeDataTypesCache.Add(data.GetType());
                }
            }

            for (int i = 0; i < _removeDataTypesCache.Count; i++) {
                _dataMap.Remove(_removeDataTypesCache[i]);
            }
            
            _prefabDataTypes.Clear();
            _removeDataTypesCache.Clear();
#endif
        }
        
#if UNITY_EDITOR
        private ActorData _dataCache;
        private readonly HashSet<Type> _addedAtRuntimeDataTypes = new();
        private readonly HashSet<Type> _prefabDataTypes = new();
        private readonly List<Type> _removeDataTypesCache = new();
        
        private void OnValidate() {
            if (Application.isPlaying) RebindData();
        }

        private void OnValidateData() {
            FetchData();
            NotifyDataUpdated();
        }
        
        private void RebindData() {
            if (_dataCache != null) _dataCache.OnValidateCalled -= OnValidateData;
            if (_data != null) _data.OnValidateCalled += OnValidateData;
            _dataCache = _data;
        }

        private void UnbindData() {
            if (_dataCache != null) _dataCache.OnValidateCalled -= OnValidateData;
            if (_data != null) _data.OnValidateCalled -= OnValidateData;
            _dataCache = null;
        }
#endif
    }
    
}