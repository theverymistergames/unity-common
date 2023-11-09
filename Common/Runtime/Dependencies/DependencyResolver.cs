using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using MisterGames.Common.Types;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MisterGames.Common.Dependencies {
    [Serializable]
    public sealed class DependencyResolver :
        IDependencyResolver,
        IDependencyContainer,
        IDependencyBucket,
        IDependencySetter
    {
        [SerializeField] private RuntimeResolveMode _mode;
        [SerializeField] private RuntimeDependencyResolver _sharedDependencies;

        [SerializeField] private List<DependencyBucket> _buckets;
        [SerializeField] private List<DependencyPointer> _dependencyPointers;
        [SerializeField] private List<DependencyMeta> _dependencyMetas;

        [SerializeReference] private List<object> _objects;
        [SerializeField] private List<Object> _unityObjects;

        private readonly Dictionary<Type, object> _typeOverrides = new Dictionary<Type, object>();

        private int _bucketsCount;
        private int _dependenciesCount;
        private int _unityObjectsCount;
        private int _objectsCount;

        private bool _changeFlag;

        private enum RuntimeResolveMode {
            Internal,
            Shared,
        }

        [Serializable]
        private struct DependencyMeta {
            public SerializedType type;
        }

        [Serializable]
        private struct DependencyPointer {
            public int list;
            public int index;
        }

        [Serializable]
        private struct DependencyBucket {
            public string name;
            public int offset;
            public int count;
        }

        public void Resolve(IDependency dependency, bool additive = false) {
            if (!additive) _dependenciesCount = 0;
            dependency?.OnResolveDependencies(this);
        }

        public void Resolve(IReadOnlyList<IDependency> dependencies, bool additive = false) {
            if (!additive) _dependenciesCount = 0;
            if (dependencies == null) return;

            for (int i = 0; i < dependencies.Count; i++) {
                dependencies[i]?.OnResolveDependencies(this);
            }
        }

        public IDependencyBucket CreateBucket(object source) {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (!ValidateDependencyBucketSource(source)) return this;
#endif

            var bucket = new DependencyBucket {
                name = TypeNameFormatter.GetShortTypeName(source.GetType()),
                offset = _dependenciesCount,
                count = 0,
            };

            if (_buckets.Count > _bucketsCount) {
                var existentBucket = _buckets[_bucketsCount];
                _buckets[_bucketsCount] = bucket;

                bool hasSameName = string.IsNullOrWhiteSpace(existentBucket.name)
                    ? string.IsNullOrWhiteSpace(bucket.name)
                    : existentBucket.name == bucket.name;

                if (!hasSameName || existentBucket.offset != bucket.offset) _changeFlag = true;
            }
            else {
                _buckets.Add(bucket);
                _changeFlag = true;
            }

            _bucketsCount++;

            return this;
        }

        public IDependencyBucket Add<T>() where T : class {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (!ValidateDependencyBucketCreated()) return this;
            if (!ValidateDependencyType(typeof(T))) return this;
#endif

            var type = typeof(T);
            var meta = new DependencyMeta { type = new SerializedType(type) };
            var pointer = new DependencyPointer();

            if (typeof(Object).IsAssignableFrom(type)) {
                for (int i = _unityObjects.Count; i <= _unityObjectsCount; i++) {
                    _unityObjects.Add(default);
                    _changeFlag = true;
                }

                pointer.list = 0;
                pointer.index = _unityObjectsCount++;
            }
            else if (type.IsSubclassOf(typeof(object)) || type.IsInterface) {
                for (int i = _objects.Count; i <= _objectsCount; i++) {
                    _objects.Add(default);
                    _changeFlag = true;
                }

                pointer.list = 1;
                pointer.index = _objectsCount++;
            }

            bool hasSameTypeAsExistent = false;

            if (_dependencyMetas.Count > _dependenciesCount) {
                if (_dependencyMetas[_dependenciesCount].type == meta.type) hasSameTypeAsExistent = true;
                else _changeFlag = true;

                _dependencyMetas[_dependenciesCount] = meta;
            }
            else {
                _dependencyMetas.Add(meta);
                _changeFlag = true;
            }

            if (_dependencyPointers.Count > _dependenciesCount) {
                var existentPointer = _dependencyPointers[_dependenciesCount];

                if (existentPointer.list != pointer.list || existentPointer.index != pointer.index || !hasSameTypeAsExistent) {
                    _changeFlag = true;
                }

                if (existentPointer.list == pointer.list &&
                    GetElement<T>(existentPointer.list, existentPointer.index) is { } t
                ) {
                    InsertElement(t, pointer.list, pointer.index);
                    RemoveElement(existentPointer.list, existentPointer.index);
                }

                _dependencyPointers[_dependenciesCount] = pointer;
            }
            else {
                _dependencyPointers.Add(pointer);
                _changeFlag = true;
            }

            var bucket = _buckets[_bucketsCount - 1];
            bucket.count++;
            _buckets[_bucketsCount - 1] = bucket;

            _dependenciesCount++;

            return this;
        }

        public T Resolve<T>() where T : class {
            int index = _dependenciesCount++;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (!ValidateResolvedDependency<T>(index)) return default;
#endif

            var pointer = _dependencyPointers[index];
            var value = GetElement<T>(pointer.list, pointer.index);
            if (value is not null) return value;

            switch (_mode) {
                case RuntimeResolveMode.Internal:
                    if (_typeOverrides.TryGetValue(typeof(T), out object v)) return v as T;
                    return default;

                case RuntimeResolveMode.Shared:
                    return _sharedDependencies.Resolve<T>();

                default:
                    throw new NotSupportedException();
            }
        }

        public void SetValue<T>(T value) where T : class {
            switch (_mode) {
                case RuntimeResolveMode.Internal:
                    _typeOverrides[typeof(T)] = value;
                    break;

                case RuntimeResolveMode.Shared:
                    _sharedDependencies.SetValue(value);
                    break;

                default:
                    throw new NotSupportedException();
            }
        }

        private T GetElement<T>(int list, int index) where T : class {
            return list switch {
                0 => _unityObjects[index] as T,
                1 => _objects[index] as T,
                _ => throw new NotSupportedException(),
            };
        }

        private void InsertElement<T>(T element, int list, int index) where T : class {
            switch (list) {
                case 0:
                    if (element is Object u) _unityObjects[index] = u;
                    break;

                case 1:
                    if (element is object o) _objects[index] = o;
                    break;

                default:
                    throw new NotSupportedException();
            }
        }

        private void RemoveElement(int list, int index) {
            switch (list) {
                case 0:
                    _unityObjects.RemoveAt(index);
                    break;

                case 1:
                    _objects.RemoveAt(index);
                    break;

                default:
                    throw new NotSupportedException();
            }
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public void PrepareFetch() {
            _changeFlag = false;

            _bucketsCount = 0;
            _dependenciesCount = 0;
            _unityObjectsCount = 0;
            _objectsCount = 0;

            _buckets ??= new List<DependencyBucket>();
            _dependencyMetas ??= new List<DependencyMeta>();
            _dependencyPointers ??= new List<DependencyPointer>();
            _unityObjects ??= new List<Object>();
            _objects ??= new List<object>();
        }

        public bool CommitFetch() {
            for (int i = _dependencyMetas.Count - 1; i >= _dependenciesCount; i--) {
                _changeFlag = true;
                _dependencyMetas.RemoveAt(i);
                _dependencyPointers.RemoveAt(i);
            }

            for (int i = _unityObjects.Count - 1; i >= _unityObjectsCount; i--) {
                _changeFlag = true;
                _unityObjects.RemoveAt(i);
            }

            for (int i = _objects.Count - 1; i >= _objectsCount; i--) {
                _changeFlag = true;
                _objects.RemoveAt(i);
            }

            for (int i = _buckets.Count - 1; i >= _bucketsCount; i--) {
                _changeFlag = true;
                _buckets.RemoveAt(i);
            }

            return _changeFlag;
        }

        public void Fetch(IDependency dependency) {
            dependency?.OnSetupDependencies(this);
        }

        public void Fetch(IReadOnlyList<IDependency> dependencies) {
            if (dependencies == null) return;

            for (int i = 0; i < dependencies.Count; i++) {
                dependencies[i]?.OnSetupDependencies(this);
            }
        }

        [AssertionMethod]
        private bool ValidateDependencyBucketSource(object source) {
            if (source != null) return true;

            Debug.LogError($"Cannot register dependency: source is null.");
            return false;
        }

        [AssertionMethod]
        private bool ValidateDependencyBucketCreated() {
            if (_buckets.Count > 0) return true;

            Debug.LogError($"Cannot add dependency: no bucket was created.");
            return false;
        }

        [AssertionMethod]
        private bool ValidateDependencyType(Type type) {
            if (typeof(Object).IsAssignableFrom(type)) return true;
            if (type.IsSubclassOf(typeof(object)) || type.IsInterface) return true;

            Debug.LogError($"Dependency of type {type.Name} cannot be added: " +
                           $"type is not supported.");
            return false;
        }

        [AssertionMethod]
        private bool ValidateResolvedDependency<T>(int index) where T : class {
            var type = typeof(T);

            if (index > _dependencyMetas.Count - 1) {
                Debug.LogError($"Requested dependency of type {type.Name} is not found: " +
                               $"dependency index {index} exceeds total dependencies count: {_dependencyMetas.Count}.");
                return default;
            }

            var meta = _dependencyMetas[index];
            if (!type.IsAssignableFrom(meta.type.ToType())) {
                Debug.LogError($"Requested dependency of type {type.Name} is not found: " +
                               $"dependency type {type.Name} is not assignable from added type {meta.type}.");
                return false;
            }

            var pointer = _dependencyPointers[index];
            switch (pointer.list) {
                case 0:
                    if (pointer.index > _unityObjects.Count - 1) {
                        Debug.LogError($"Requested dependency of type {type.Name} is not found: " +
                                       $"dependency has incorrect element index {pointer.index}.");
                        return false;
                    }

                    return true;

                case 1:
                    if (pointer.index > _objects.Count - 1) {
                        Debug.LogError($"Requested dependency of type {type.Name} is not found: " +
                                       $"dependency has incorrect element index {pointer.index}.");
                        return false;
                    }

                    return true;

                default:
                    Debug.LogError($"Requested dependency of type {type.Name} is not found: " +
                                   $"dependency has incorrect list index {pointer.list}.");
                    return false;
            }
        }

        public override string ToString() {
            var sb = new StringBuilder();

            sb.AppendLine($"{nameof(DependencyResolver)}: ");

            sb.AppendLine($"- Mode: {_mode}");

            sb.AppendLine($"- Shared Dependencies: {_sharedDependencies}");

            sb.AppendLine($"- Buckets:");
            for (int i = 0; i < _buckets.Count; i++) {
                var b = _buckets[i];
                sb.AppendLine($"--- Bucket #{i} {b.name}: start index {b.offset}, count {b.count}");
            }

            sb.AppendLine($"- Dependency Pointers:");
            for (int i = 0; i < _dependencyPointers.Count; i++) {
                var p = _dependencyPointers[i];
                sb.AppendLine($"--- Pointer #{i}: list {p.list}, index {p.index}");
            }

            sb.AppendLine($"- Dependency Metas:");
            for (int i = 0; i < _dependencyMetas.Count; i++) {
                var m = _dependencyMetas[i];
                sb.AppendLine($"--- Meta #{i}: {m.type}");
            }

            sb.AppendLine($"- Unity Objects:");
            for (int i = 0; i < _unityObjects.Count; i++) {
                var o = _unityObjects[i];
                sb.AppendLine($"--- Unity Object #{i}: {o}");
            }

            sb.AppendLine($"- Objects:");
            for (int i = 0; i < _objects.Count; i++) {
                object o = _objects[i];
                sb.AppendLine($"--- Object #{i}: {o}");
            }

            sb.AppendLine($"- Type Overrides:");
            int j = 0;
            foreach (var kvp in _typeOverrides) {
                sb.AppendLine($"--- Override #{j++}: type {kvp.Key.Name}, value {kvp.Value}");
            }

            return sb.ToString();
        }
#endif
    }

}
