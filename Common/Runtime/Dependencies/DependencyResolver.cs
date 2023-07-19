using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using MisterGames.Common.Types;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MisterGames.Common.Dependencies {

    [Serializable]
    public sealed class DependencyResolver :
        IDependencyResolver,
        IDependencyContainer,
        IDependencyOverride
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

        public void Fetch(IDependency dependency) {
            _bucketsCount = 0;
            _dependenciesCount = 0;
            _unityObjectsCount = 0;
            _objectsCount = 0;

            _buckets ??= new List<DependencyBucket>();
            _dependencyMetas ??= new List<DependencyMeta>();
            _dependencyPointers ??= new List<DependencyPointer>();
            _unityObjects ??= new List<Object>();
            _objects ??= new List<object>();

            dependency?.OnSetupDependencies(this);

            for (int i = _dependencyMetas.Count - 1; i >= _dependenciesCount; i--) {
                _dependencyMetas.RemoveAt(i);
                _dependencyPointers.RemoveAt(i);
            }

            for (int i = _unityObjects.Count - 1; i >= _unityObjectsCount; i--) {
                _unityObjects.RemoveAt(i);
            }

            for (int i = _objects.Count - 1; i >= _objectsCount; i--) {
                _objects.RemoveAt(i);
            }

            for (int i = _buckets.Count - 1; i >= _bucketsCount; i--) {
                _buckets.RemoveAt(i);
            }
        }

        public void Fetch(IReadOnlyList<IDependency> dependencies) {
            _bucketsCount = 0;
            _dependenciesCount = 0;
            _unityObjectsCount = 0;
            _objectsCount = 0;

            _buckets ??= new List<DependencyBucket>();
            _dependencyMetas ??= new List<DependencyMeta>();
            _dependencyPointers ??= new List<DependencyPointer>();
            _unityObjects ??= new List<Object>();
            _objects ??= new List<object>();

            if (dependencies != null) {
                for (int i = 0; i < dependencies.Count; i++) {
                    dependencies[i].OnSetupDependencies(this);
                }
            }

            for (int i = _dependencyMetas.Count - 1; i >= _dependenciesCount; i--) {
                _dependencyMetas.RemoveAt(i);
                _dependencyPointers.RemoveAt(i);
            }

            for (int i = _unityObjects.Count - 1; i >= _unityObjectsCount; i--) {
                _unityObjects.RemoveAt(i);
            }

            for (int i = _objects.Count - 1; i >= _objectsCount; i--) {
                _objects.RemoveAt(i);
            }

            for (int i = _buckets.Count - 1; i >= _bucketsCount; i--) {
                _buckets.RemoveAt(i);
            }
        }

        public void Resolve(IDependency dependency) {
            _dependenciesCount = 0;
            dependency.OnResolveDependencies(this);
        }

        public void Resolve(IReadOnlyList<IDependency> dependencies) {
            _dependenciesCount = 0;
            for (int i = 0; i < dependencies.Count; i++) {
                dependencies[i].OnResolveDependencies(this);
            }
        }

        public bool TryResolve<T>(out T value) where T : class {
            switch (_mode) {
                case RuntimeResolveMode.Internal:
                    if (_typeOverrides.TryGetValue(typeof(T), out object v)) {
                        value = v is T t ? t : default;
                        return true;
                    }

                    value = default;
                    return false;

                case RuntimeResolveMode.Shared:
                    return _sharedDependencies.TryResolve(out value);

                default:
                    throw new NotSupportedException();
            }
        }

        public void OverrideDependenciesOfType<T>(T value) where T : class {
            switch (_mode) {
                case RuntimeResolveMode.Internal:
                    _typeOverrides[typeof(T)] = value;
                    break;

                case RuntimeResolveMode.Shared:
                    _sharedDependencies.OverrideDependenciesOfType(value);
                    break;

                default:
                    throw new NotSupportedException();
            }
        }

        public IDependencyContainer CreateBucket(object source) {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (!ValidateDependencyBucketSource(source)) return this;
#endif

            var bucket = new DependencyBucket {
                name = TypeNameFormatter.GetTypeName(source.GetType()),
                offset = _dependenciesCount,
                count = 0,
            };

            if (_buckets.Count > _bucketsCount) _buckets[_bucketsCount] = bucket;
            else _buckets.Add(bucket);

            _bucketsCount++;

            return this;
        }

        public IDependencyContainer Add<T>() where T : class {
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
                }

                pointer.list = 0;
                pointer.index = _unityObjectsCount++;
            }
            else if (type.IsSubclassOf(typeof(object)) || type.IsInterface) {
                for (int i = _objects.Count; i <= _objectsCount; i++) {
                    _objects.Add(default);
                }

                pointer.list = 1;
                pointer.index = _objectsCount++;
            }

            if (_dependencyPointers.Count > _dependenciesCount) {
                var existentPointer = _dependencyPointers[_dependenciesCount];
                if (existentPointer.list == pointer.list &&
                    GetElement<T>(existentPointer.list, existentPointer.index) is { } t
                ) {
                    InsertElement(t, pointer.list, pointer.index);
                    RemoveElement(existentPointer.list, existentPointer.index);
                }

                _dependencyPointers[_dependenciesCount] = pointer;
                _dependencyMetas[_dependenciesCount] = meta;
            }
            else {
                _dependencyPointers.Add(pointer);
                _dependencyMetas.Add(meta);
            }

            var bucket = _buckets[_bucketsCount - 1];
            bucket.count++;
            _buckets[_bucketsCount - 1] = bucket;

            _dependenciesCount++;

            return this;
        }

        public IDependencyResolver Resolve<T>(out T dependency) where T : class {
            int index = _dependenciesCount++;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (!ValidateResolvedDependency<T>(index)) {
                dependency = default;
                return this;
            }
#endif

            if (TryResolve<T>(out var overridenValue)) {
                dependency = overridenValue;
            }
            else {
                var pointer = _dependencyPointers[index];
                dependency = GetElement<T>(pointer.list, pointer.index);
            }

            return this;
        }

        private T GetElement<T>(int list, int index) where T : class {
            return list switch {
                0 => _unityObjects[index] is T t ? t : default,
                1 => _objects[index] is T t ? t : default,
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

            if (index > _dependencyMetas.Count - 1 || index > _dependencyPointers.Count - 1) {
                Debug.LogError($"Requested dependency of type {type.Name} is not found: " +
                               $"dependency index {index} exceeds total dependencies count: {_dependencyMetas.Count}.");
                return default;
            }

            var meta = _dependencyMetas[index];
            if (meta.type != type) {
                Debug.LogError($"Requested dependency of type {type.Name} is not found: " +
                               $"dependency type {type.Name} is not the same as added type {meta.type}.");
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
#endif
    }

}
