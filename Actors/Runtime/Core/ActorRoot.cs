using System;
using System.Collections.Generic;
using System.Threading;
using MisterGames.Common.Async;
using MisterGames.Common.Attributes;
using MisterGames.Common.Data;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MisterGames.Actors {
    
    [DefaultExecutionOrder(-2000)]
    [DisallowMultipleComponent]
    public sealed class ActorRoot : MonoBehaviour, IActor {
        
        [EmbeddedInspector] [SerializeField] private ActorData _data;
        [SubclassSelector] [SerializeReference] private IActorData[] _dataOverrides;
        [SerializeField] [HideInInspector] private List<Transform> _childActors;
        [SerializeField] [HideInInspector] private List<Component> _actorComponents;
        [SerializeField] [HideInInspector] private bool _prewarmed;
        
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
        public CancellationToken EnableToken => _enableCts?.Token ?? new CancellationToken(canceled: true);
        public CancellationToken DestroyToken => _destroyCts?.Token ?? new CancellationToken(canceled: true);
        
        private Dictionary<Type, IActorData> _dataMap;
        private Dictionary<Type, DataOverride> _dataOverrideMap;
        private MultiValueDictionary<Type, object> _componentMap;
        
        private CancellationTokenSource _destroyCts;
        private CancellationTokenSource _enableCts;
        private Transform _transform;
        private GameObject _gameObject;
        private bool _isAwake;
        private bool _isDestroyed;
        private bool _actorComponentsFetched;
        private string _name;
        
        private void Awake() {
            AsyncExt.RecreateCts(ref _destroyCts);

            _name = name;
            _gameObject = gameObject;
            _transform = transform;
            
            _isAwake = true;
            _isDestroyed = false;
            
            FetchData();
            PrewarmActorComponents();
            FetchActorComponents();

            NotifySetData();
            NotifyAwake();
            NotifyPostAwake();
        }

        private void OnDestroy() {
            AsyncExt.DisposeCts(ref _destroyCts);
            
            _isAwake = false;
            _isDestroyed = true;
            
            NotifyDestroy();

            _dataMap?.Clear();
            _dataOverrideMap?.Clear();
            _componentMap?.Clear();
            _actorComponents?.Clear();
            _childActors?.Clear();
        }

        private void OnEnable() {
            AsyncExt.RecreateCts(ref _enableCts);
        }

        private void OnDisable() {
            AsyncExt.DisposeCts(ref _enableCts);
        }
        
        public bool TryGetData<T>(out T data) where T : class, IActorData {
#if UNITY_EDITOR
            if (!Application.isPlaying) FetchData();
#endif

            var type = typeof(T);

            if (_dataOverrideMap != null && 
                _dataOverrideMap.TryGetValue(type, out var dataOverride) && dataOverride.data is T to) {
                data = to;
                return true;
            }
            
            if (_dataMap != null && 
                _dataMap.TryGetValue(type, out var d) && d is T t) {
                data = t;
                return true;
            }

            data = null;
            return false;
        }
        
        public T GetData<T>() where T : class, IActorData {
            return TryGetData<T>(out var data) ? data : null;
        }

        public void SetData(IActorData data) {
#if UNITY_EDITOR
            if (!Application.isPlaying) FetchData();
#endif

            _dataMap ??= new Dictionary<Type, IActorData>();
            _dataMap[data.GetType()] = data;
            
            NotifySetData();
        }

        public void SetData(IReadOnlyList<IActorData> data) {
#if UNITY_EDITOR
            if (!Application.isPlaying) FetchData();
#endif
            
            _dataMap ??= new Dictionary<Type, IActorData>();
            
            for (int i = 0; i < data.Count; i++) {
                var d = data[i];
                _dataMap[d.GetType()] = d;
            }
            
            NotifySetData();
        }

        public void MuteData<T>() where T : class, IActorData {
#if UNITY_EDITOR
            if (!Application.isPlaying) FetchData();
#endif

            if (_dataMap?.Remove(typeof(T)) ?? false) {
                NotifySetData();
            }
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
            return TryGetComponent<T>(out var component) ? component : null;
        }

        public new bool TryGetComponent<T>(out T component) where T : class {
            if (_isDestroyed) {
                component = null;
                return false;
            }
            
            var type = typeof(T);
            if (_componentMap == null || !_componentMap.ContainsKey(type)) {
                FetchComponentsOfType<T>();
            }

            if (_componentMap!.TryGetFirstValue(type, out object c) && c is T componentT) {
                component = componentT;
                return true;
            }
            
            component = null;
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
            for (int i = 0, count = _actorComponents?.Count ?? 0; i < count && _isAwake; i++) {
                if (_actorComponents![i] is IActorComponent a) NotifyAwakeFor(a);
            }
        }
        
        private void NotifyPostAwake() {
            for (int i = 0, count = _actorComponents?.Count ?? 0; i < count && _isAwake; i++) {
                if (_actorComponents![i] is IActorComponent a) NotifyPostAwakeFor(a);
            }
        }
        
        private void NotifyDestroy() {
            for (int i = 0, count = _actorComponents?.Count ?? 0; i < count; i++) {
                if (_actorComponents![i] is IActorComponent a) NotifyDestroyFor(a);
            }
        }
        
        private void NotifySetData(object excludedObject = null) {
            for (int i = 0, count = _actorComponents?.Count ?? 0; i < count && _isAwake; i++) {
                if (_actorComponents![i] is IActorComponent a && a != excludedObject) NotifySetDataFor(a);
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
        
        private void NotifyPostAwakeFor(IActorComponent component) {
            if (component is not Component c || BelongsToOneOfChildActors(c.transform)) {
                return;
            }

            component.OnPostAwake(this);
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

        public void PrewarmActorComponents(bool forceUpdate = false) {
#if UNITY_EDITOR
            forceUpdate = true;
#endif
            
            FetchChildActorRoots(forceUpdate);
            CollectActorComponents(forceUpdate);

            _prewarmed = true;

#if UNITY_EDITOR
            if (!Application.isPlaying) EditorUtility.SetDirty(this);
#endif
        }

        private void FetchChildActorRoots(bool forceUpdate = false) {
            if (_prewarmed && !forceUpdate) return;
            
            var self = transform;
            _childActors ??= new List<Transform>();
            _childActors.Clear();
            
            var childActors = GetComponentsInChildren<ActorRoot>(includeInactive: true);
            for (int i = 0; i < childActors.Length; i++) {
                var childActor = childActors[i].transform;
                if (childActor == self) continue;

                _childActors.Add(childActor);
            }
        }

        private void CollectActorComponents(bool forceUpdate = false) {
            if (_prewarmed && !forceUpdate) return;
            
            _actorComponents ??= new List<Component>();
            _actorComponents.Clear();

            var components = GetComponentsInChildren<IActorComponent>(includeInactive: true);
            for (int i = 0; i < components.Length; i++) {
                _actorComponents.Add((Component) components[i]);
            }
        }

        private void FetchActorComponents() {
            if (_actorComponentsFetched) return;

            int count = _actorComponents?.Count ?? 0;
            _componentMap ??= new MultiValueDictionary<Type, object>(count);
                
            for (int i = 0; i < count; i++) {
                var c = _actorComponents![i];
                if (c != null) _componentMap.AddValue(c.GetType(), c);
            }
            
            _actorComponentsFetched = true;
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
        
        private void FetchData() {
            if (_isDestroyed) return;
            
            if (_data != null) {
                var dataArray = _data.GetDataArray();
                _dataMap ??= new Dictionary<Type, IActorData>(dataArray.Count);
                for (int i = 0; i < dataArray.Count; i++) {
                    if (dataArray[i] is { } data) _dataMap[data.GetType()] = data;
                }    
            }

            if (_dataOverrides != null) {
                _dataMap ??= new Dictionary<Type, IActorData>(_dataOverrides.Length);
                for (int i = 0; i < _dataOverrides.Length; i++) {
                    if (_dataOverrides[i] is { } data) _dataMap[data.GetType()] = data;
                }
            }
            
            _dataMap ??= new Dictionary<Type, IActorData>();
        }
        
        public override string ToString() {
            return _isDestroyed ? $"Actor({_name} [destroyed])" : $"Actor({gameObject.scene.name}/{name})";
        }
    }
    
}