using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using MisterGames.Common.Types;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MisterGames.Common.Dependencies {

    [Serializable]
    public sealed class DependencyResolver : IDependencyResolver, IDependencyContainer, IDependencyOverride {

        [SerializeField] private RuntimeResolveMode _mode;
        [SerializeField] private RuntimeDependencyResolver _sharedDependencies;

        [SerializeField] private List<DependencyMeta> _dependencyMetaList;
        [SerializeField] private List<Object> _unityObjects;
        [SerializeReference] private List<object> _objects;

        private enum RuntimeResolveMode {
            Internal,
            Shared,
        }

        private readonly Dictionary<Type, object> _dependenciesByType = new Dictionary<Type, object>();

        private int _iteratorMeta;
        private int _iteratorUnityObjects;
        private int _iteratorObjects;

        public void Fetch(IDependency dependency) {
            _iteratorMeta = 0;
            _iteratorUnityObjects = 0;
            _iteratorObjects = 0;

            _dependencyMetaList ??= new List<DependencyMeta>();
            _unityObjects ??= new List<Object>();
            _objects ??= new List<object>();

            dependency.OnAddDependencies(this);

            for (int i = _dependencyMetaList.Count - 1; i >= _iteratorMeta; i--) {
                _dependencyMetaList.RemoveAt(i);
            }

            for (int i = _unityObjects.Count - 1; i >= _iteratorUnityObjects; i--) {
                _unityObjects.RemoveAt(i);
            }

            for (int i = _objects.Count - 1; i >= _iteratorObjects; i--) {
                _objects.RemoveAt(i);
            }
        }

        public void Fetch(IReadOnlyList<IDependency> dependencies) {
            _iteratorMeta = 0;
            _iteratorUnityObjects = 0;
            _iteratorObjects = 0;

            _dependencyMetaList ??= new List<DependencyMeta>();
            _unityObjects ??= new List<Object>();
            _objects ??= new List<object>();

            for (int i = 0; i < dependencies.Count; i++) {
                dependencies[i].OnAddDependencies(this);
            }

            for (int i = _dependencyMetaList.Count - 1; i >= _iteratorMeta; i--) {
                _dependencyMetaList.RemoveAt(i);
            }

            for (int i = _unityObjects.Count - 1; i >= _iteratorUnityObjects; i--) {
                _unityObjects.RemoveAt(i);
            }

            for (int i = _objects.Count - 1; i >= _iteratorObjects; i--) {
                _objects.RemoveAt(i);
            }
        }

        public void Resolve(IDependency dependency) {
            _iteratorMeta = 0;

            dependency.OnResolveDependencies(this);
        }

        public void Resolve(IReadOnlyList<IDependency> dependencies) {
            _iteratorMeta = 0;

            for (int i = 0; i < dependencies.Count; i++) {
                dependencies[i].OnResolveDependencies(this);
            }
        }

        public bool TryResolveDependencyOverride<T>(out T value) {
            switch (_mode) {
                case RuntimeResolveMode.Internal:
                    if (_dependenciesByType.TryGetValue(typeof(T), out object v)) {
                        value = v is T t ? t : default;
                        return true;
                    }

                    value = default;
                    return false;

                case RuntimeResolveMode.Shared:
                    return _sharedDependencies.TryResolveDependencyOverride(out value);

                default:
                    throw new NotSupportedException();
            }
        }

        public void SetDependenciesOfType<T>(T value) where T : class {
            switch (_mode) {
                case RuntimeResolveMode.Internal:
                    _dependenciesByType[typeof(T)] = value;
                    break;

                case RuntimeResolveMode.Shared:
                    _sharedDependencies.SetDependenciesOfType(value);
                    break;

                default:
                    throw new NotSupportedException();
            }
        }

        public void AddDependency<T>(object source) {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (!ValidateNewDependency(source, typeof(T))) return;
#endif

            var type = typeof(T);
            var meta = new DependencyMeta {
                name = TypeNameFormatter.GetTypeName(type),
                category = TypeNameFormatter.GetTypeName(source.GetType()),
                type = new SerializedType(type)
            };

            if (typeof(Object).IsAssignableFrom(type)) {
                if (_iteratorUnityObjects >= _unityObjects.Count) {
                    for (int i = _unityObjects.Count; i <= _iteratorUnityObjects; i++) {
                        _unityObjects.Add(default);
                    }
                }

                meta.listIndex = 0;
                meta.elementIndex = _iteratorUnityObjects++;
            }
            else if (type.IsSubclassOf(typeof(object)) || type.IsInterface) {
                if (_iteratorObjects >= _objects.Count) {
                    for (int i = _objects.Count; i <= _iteratorObjects; i++) {
                        _objects.Add(default);
                    }
                }

                meta.listIndex = 1;
                meta.elementIndex = _iteratorObjects++;
            }
            else return;

            if (_iteratorMeta < _dependencyMetaList.Count) {
                var existingMeta = _dependencyMetaList[_iteratorMeta];

                if (existingMeta.name == meta.name &&
                    existingMeta.category == meta.category &&
                    existingMeta.listIndex == meta.listIndex
                ) {
                    switch (existingMeta.listIndex) {
                        case 0:
                            if (existingMeta.elementIndex < _unityObjects.Count) {
                                var value = _unityObjects[existingMeta.elementIndex];
                                if (value is T) _unityObjects[meta.elementIndex] = value;
                            }
                            break;

                        case 1:
                            if (existingMeta.elementIndex < _objects.Count) {
                                object value = _objects[existingMeta.elementIndex];
                                if (value is T) _objects[meta.elementIndex] = value;
                            }
                            break;
                    }
                }
            }
            else {
                for (int i = _dependencyMetaList.Count; i <= _iteratorMeta; i++) {
                    _dependencyMetaList.Add(default);
                }
            }

            _dependencyMetaList[_iteratorMeta++] = meta;
        }

        public T ResolveDependency<T>() {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (!ValidateRequestedDependencyMetaByIndex<T>(_iteratorMeta)) return default;
#endif

            if (TryResolveDependencyOverride<T>(out var overridenValue)) {
                _iteratorMeta++;
                return overridenValue;
            }

            var meta = _dependencyMetaList[_iteratorMeta++];

            int listIndex = meta.listIndex;
            int elementIndex = meta.elementIndex;

            return listIndex switch {
                0 => elementIndex < _unityObjects.Count && _unityObjects[elementIndex] is T t ? t : default,
                1 => elementIndex < _objects.Count && _objects[elementIndex] is T t ? t : default,
                _ => default,
            };
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        [AssertionMethod]
        private bool ValidateNewDependency(object source, Type type) {
            if (typeof(Object).IsAssignableFrom(type)) return true;
            if (type.IsSubclassOf(typeof(object)) || type.IsInterface) return true;

            Debug.LogError($"New dependency {source.GetType().Name}/{type.Name} of type {type.Name} cannot be added: " +
                           $"dependency has unsupported type {type}.");
            return false;
        }

        [AssertionMethod]
        private bool ValidateRequestedDependencyMetaByIndex<T>(int index) {
            var type = typeof(T);

            if (index > _dependencyMetaList.Count - 1) {
                Debug.LogError($"Requested dependency of type {type.Name} is not found: " +
                               $"dependency index {index} exceeds total dependencies count: {_dependencyMetaList.Count}.");
                return default;
            }

            var meta = _dependencyMetaList[index];

            if (meta.type != type) {
                Debug.LogError($"Requested dependency of type {type.Name} is not found: " +
                               $"dependency type {type.Name} is not the same as added type {meta.type}.");
                return false;
            }

            switch (meta.listIndex) {
                case 0:
                    if (meta.elementIndex > _unityObjects.Count - 1) {
                        Debug.LogError($"Requested dependency of type {type.Name} is not found: " +
                                       $"dependency has incorrect element index {meta.elementIndex}.");
                        return false;
                    }

                    return true;

                case 1:
                    if (meta.elementIndex > _objects.Count - 1) {
                        Debug.LogError($"Requested dependency of type {type.Name} is not found: " +
                                       $"dependency has incorrect element index {meta.elementIndex}.");
                        return false;
                    }

                    return true;
            }

            Debug.LogError($"Dependency of type {type.Name} is not found: " +
                           $"dependency has incorrect list index {meta.listIndex}.");
            return false;
        }
#endif
    }

}
