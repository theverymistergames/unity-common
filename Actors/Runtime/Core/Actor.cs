using System;
using System.Collections.Generic;
using MisterGames.Common.Attributes;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Actors
{
    
    [DefaultExecutionOrder(-2000)]
    public sealed class Actor : MonoBehaviour, IActor {
        
        [EmbeddedInspector] [SerializeField] private ActorData _data;
        [SubclassSelector] [SerializeReference] private IActorData[] _dataOverrides;
        
        private readonly struct DataOverride {
            
            public readonly int sourceHash;
            public readonly IActorData data;

            public DataOverride(int sourceHash, IActorData data) {
                this.sourceHash = sourceHash;
                this.data = data;
            }
        }

        public IActor ParentActor { get; set; }
        public Transform Transform => _isAwake ? _transform : !_isDestroyed ? transform : null;
        public GameObject GameObject => _isAwake ? _gameObject : !_isDestroyed ? gameObject : null;
        public ActorData DataSO => _data;
        
        private Dictionary<Type, IActorData> _dataMap;
        private Dictionary<Type, DataOverride> _dataOverrideMap;
        private MultiValueDictionary<Type, object> _componentMap;
        private List<IActorComponent> _actorComponents;
        private List<Transform> _childActors;
        
        private Transform _transform;
        private GameObject _gameObject;
        private bool _isAwake;
        private bool _isDestroyed;
        
        private void Awake() {
            _gameObject = gameObject;
            _transform = transform;
            
            _isAwake = true;
            _isDestroyed = false;
            
            FetchData();
            FetchChildActors();
            FetchActorComponents();

            NotifySetData();
            NotifyAwake();
        }

        private void OnDestroy() {
            _isAwake = false;
            _isDestroyed = true;
            
            NotifyDestroy();

            _dataMap?.Clear();
            _dataOverrideMap?.Clear();
            _componentMap?.Clear();
            _actorComponents?.Clear();
            _childActors?.Clear();
            
#if UNITY_EDITOR
            _addedAtRuntimeDataTypes.Clear();
            _prefabDataTypes.Clear();
            _removeDataTypesCache.Clear();
#endif
        }

#if UNITY_EDITOR
        private void OnEnable() {
            RebindData();
        }

        private void OnDisable() {
            UnbindData();
        }
#endif
        
        public bool TryGetData<T>(out T data) where T : class, IActorData {
#if UNITY_EDITOR
            if (!Application.isPlaying) FetchData();
#endif

            var type = typeof(T);

            if (_dataOverrideMap != null && 
                _dataOverrideMap.TryGetValue(type, out var dataOverride) && dataOverride.data is T to
            ) {
                data = to;
                return true;
            }
            
            if (_dataMap != null && _dataMap.TryGetValue(type, out var d) && d is T t) {
                data = t;
                return true;
            }

            data = default;
            return false;
        }
        
        public T GetData<T>() where T : class, IActorData {
            return TryGetData<T>(out var data) ? data : default;
        }

        public void SetData(IActorData data) {
#if UNITY_EDITOR
            if (!Application.isPlaying) FetchData();
#endif
            
            var type = data.GetType();
            
#if UNITY_EDITOR
            if (!_dataMap.ContainsKey(type)) _addedAtRuntimeDataTypes.Add(type);
#endif
            
            _dataMap ??= new Dictionary<Type, IActorData>();
            _dataMap[type] = data;
            
            NotifySetData();
        }

        public void SetData(IReadOnlyList<IActorData> data) {
#if UNITY_EDITOR
            if (!Application.isPlaying) FetchData();
#endif
            
            _dataMap ??= new Dictionary<Type, IActorData>();
            
            for (int i = 0; i < data.Count; i++) {
                var d = data[i];
                var type = d.GetType();
            
#if UNITY_EDITOR
                if (!_dataMap.ContainsKey(type)) _addedAtRuntimeDataTypes.Add(type);
#endif
                
                _dataMap[type] = d;

            }
            
            NotifySetData();
        }

        public bool RemoveData<T>() where T : class, IActorData {
#if UNITY_EDITOR
            if (!Application.isPlaying) FetchData();
#endif
            
#if UNITY_EDITOR
            _addedAtRuntimeDataTypes.Add(typeof(T));
#endif

            if (_dataMap?.Remove(typeof(T)) ?? false) {
                NotifySetData();
                return true;
            }
            
            return false;
        }

        public void SetDataOverride(object source, IActorData data) {
#if UNITY_EDITOR
            if (!Application.isPlaying) FetchData();
#endif
            
            _dataOverrideMap ??= new Dictionary<Type, DataOverride>();
            _dataOverrideMap[data.GetType()] = new DataOverride(source.GetHashCode(), data);
            
            NotifySetData(excludedObject: source);
        }

        public void SetDataOverrides(object source, IReadOnlyList<IActorData> data) {
#if UNITY_EDITOR
            if (!Application.isPlaying) FetchData();
#endif
            
            _dataOverrideMap ??= new Dictionary<Type, DataOverride>();
            
            for (int i = 0; i < data.Count; i++) {
                var d = data[i];
                _dataOverrideMap[d.GetType()] = new DataOverride(source.GetHashCode(), d);
            }
            
            NotifySetData(excludedObject: source);
        }

        public void RemoveDataOverride(object source, IActorData data) {
            var type = data.GetType();
            
            if (_dataOverrideMap == null ||
                !_dataOverrideMap.TryGetValue(type, out var dataOverride) ||
                dataOverride.sourceHash != source.GetHashCode()
            ) {
                return;
            }

            _dataOverrideMap.Remove(type);
            
            NotifySetData(excludedObject: source);
        }

        public void RemoveDataOverrides(object source, IReadOnlyList<IActorData> data) {
            if (_dataOverrideMap == null) return;

            for (int i = 0; i < data.Count; i++) {
                var d = data[i];
                var type = d.GetType();
                
                if (!_dataOverrideMap.TryGetValue(type, out var dataOverride) ||
                    dataOverride.sourceHash != source.GetHashCode()
                ) {
                    continue;
                }
                
                _dataOverrideMap.Remove(type);
            }
            
            NotifySetData(excludedObject: source);
        }

        public new T GetComponent<T>() where T : class {
            return TryGetComponent<T>(out var component) ? component : default;
        }

        public new bool TryGetComponent<T>(out T component) where T : class {
            if (_isDestroyed) {
                component = default;
                return false;
            }
            
            var type = typeof(T);

            if (type == typeof(IActorComponent)) {
                FetchActorComponents();
                component = _actorComponents.Count > 0 ? _actorComponents[0] as T : default;
                return component != null;
            }
            
            if (_componentMap == null || !_componentMap.ContainsKey(type)) {
                FetchComponentsOfType<T>();
            }

            if (_componentMap!.TryGetFirstValue(type, out object c) && c is T componentT) {
                component = componentT;
                return true;
            }
            
            component = default;
            return false;
        }

        public T AddComponent<T>() where T : Component {
            if (_isDestroyed) return null;
            
            var c = gameObject.AddComponent<T>();

            if (c is IActorComponent a) {
                NotifySetDataFor(a);
                NotifyAwakeFor(a);
            }
            
            FetchComponentsOfType<T>();
            
            return c;
        }

        public T AddComponent<T>(T component) where T : Component {
            if (_isDestroyed) return null;
            
            if (component is IActorComponent a) {
                NotifySetDataFor(a);
                NotifyAwakeFor(a);
            }
            
            FetchComponentsOfType<T>();
            
            return component;
        }

        public T GetOrAddComponent<T>() where T : Component {
            return TryGetComponent<T>(out var t) ? t : AddComponent<T>();
        }

        public new ComponentCollection<T> GetComponents<T>() where T : class {
            var type = typeof(T);
            if (type == typeof(IActorComponent)) return new ComponentCollection<T>(_actorComponents);
            
            if (!_componentMap.ContainsKey(typeof(T))) FetchComponentsOfType<T>();
            return new ComponentCollection<T>(_componentMap);
        }
        
        public new void GetComponents<T>(List<T> dest) where T : class {
            dest.Clear();
            var components = GetComponents<T>();

            foreach (var component in components) {
                dest.Add(component);
            }
        }

        public void DestroyActor(float time = 0f) {
            if (_isDestroyed) return;
            
            if (time > 0f) Destroy(gameObject, time);
            else Destroy(gameObject);
        }

        private void NotifyAwake() {
            for (int i = 0, count = _actorComponents.Count; i < count && _isAwake; i++) {
                NotifyAwakeFor(_actorComponents[i]);
            }
        }
        
        private void NotifyDestroy() {
            for (int i = 0, count = _actorComponents.Count; i < count; i++) {
                NotifyDestroyFor(_actorComponents[i]);
            }
        }
        
        private void NotifySetData(object excludedObject = null)
        {
            for (int i = 0, count = _actorComponents?.Count ?? 0; i < count && _isAwake; i++) {
                if (_actorComponents![i] is {} c && c != excludedObject) NotifySetDataFor(c);
            }
        }
        
        private void NotifySetDataFor(IActorComponent component) {
            if (component is not Component c || BelongsToOneOfChildActors(c.transform)) {
                return;
            }
            
            component.OnSetData(this);
        }
        
        private void NotifyAwakeFor(IActorComponent component) {
            if (component is not Component c || BelongsToOneOfChildActors(c.transform)) {
                return;
            }

            component.OnAwake(this);
        }

        private void NotifyDestroyFor(IActorComponent component) {
            if (component is not Component c || c == null || BelongsToOneOfChildActors(c.transform)) {
                return;
            }
            
            component.OnDestroyed(this);
        }

        private bool BelongsToOneOfChildActors(Transform t) {
            for (int i = 0; i < _childActors.Count; i++) {
                if (_childActors[i] == null) continue;
                if (t.IsChildOf(_childActors[i])) return true;
            }

            return false;
        }
        
        private void FetchChildActors() {
            if (_isDestroyed) return;
            
            _childActors ??= new List<Transform>();
            _childActors.Clear();
            
            var childActors = GetComponentsInChildren<Actor>();
            for (int i = 0; i < childActors.Length; i++)
            {
                var childActor = childActors[i].transform;
                if (childActor == _transform) continue;

                _childActors.Add(childActor);
            }
        }

        private void FetchActorComponents() {
            if (_isDestroyed) return;
            
            _actorComponents ??= new List<IActorComponent>(GetComponentsInChildren<IActorComponent>(includeInactive: true));
        }
        
        private void FetchComponentsOfType<T>() where T : class {
            if (_isDestroyed) return;
            
            var type = typeof(T);
            var results = GetComponentsInChildren<T>(includeInactive: true);
            _componentMap ??= new MultiValueDictionary<Type, object>(results.Length);
            
            for (int i = 0; i < results.Length; i++) {
                _componentMap.AddValue(type, results[i]);
            }
        }
        
        private void FetchData()
        {
            if (_isDestroyed) return;
            
#if UNITY_EDITOR
            _prefabDataTypes.Clear();
            _removeDataTypesCache.Clear();
#endif

            if (_data != null) {
                var prefabData = _data.Data;
                int count = prefabData.Count;
                
                _dataMap ??= new Dictionary<Type, IActorData>();
                
                for (int i = 0; i < count; i++) {
                    if (prefabData[i] is not { } data) continue;
                
                    _dataMap[data.GetType()] = data;
                
#if UNITY_EDITOR
                    _prefabDataTypes.Add(data.GetType());
#endif
                }    
            }

            if (_dataOverrides != null) {
                _dataMap ??= new Dictionary<Type, IActorData>(_dataOverrides.Length);
                
                for (int i = 0; i < _dataOverrides.Length; i++) {
                    if (_dataOverrides[i] is not { } data) continue;
                
                    _dataMap[data.GetType()] = data;
                
#if UNITY_EDITOR
                    _prefabDataTypes.Add(data.GetType());
#endif              
                }
            }
            
            _dataMap ??= new Dictionary<Type, IActorData>();
            
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
            if (Application.isPlaying) NotifySetData();
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