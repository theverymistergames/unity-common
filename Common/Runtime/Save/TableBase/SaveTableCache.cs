using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using MisterGames.Common.Save.Tables;
using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MisterGames.Common.Save.Storages {

#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    internal static class SaveTableCache {

        private static readonly object _lock = new();
        private static SynchronizationContext _mainThreadContext;
        private static Task<Cache> _buildTask;
        private const string Editor = "editor";
        private static Cache _cache;
        
        private sealed class Cache {
            public readonly Dictionary<(Type, Type), Type> tableMap = new();
            public readonly HashSet<Type> valueTypeSet = new();
        }
        
        public static Type GetBaseElementType(Type t) {
            return t.IsArray ? t :
                typeof(Object).IsAssignableFrom(t) ? typeof(Object) :
                t.IsClass || t.IsInterface ? typeof(object) :
                t.IsEnum ? typeof(Enum) :
                t;
        }
        
        public static bool TryGetTableType(Type keyType, Type valueType, out Type tableType) {
            return LoadCache().tableMap.TryGetValue((keyType, valueType), out tableType);
        }

        public static bool IsSupportedValueType(Type valueType) {
            var set = LoadCache().valueTypeSet;
            return set.Contains(valueType) || set.Contains(GetBaseElementType(valueType));
        }
        
        private static Cache LoadCache() {
            var cache = _cache;

#if UNITY_EDITOR
            return cache;
#endif

            if (cache == null)
            {
                var task = EnsureBuildStarted();
                cache = task.GetAwaiter().GetResult();
                PublishResult(cache);
            }
            
            return cache;
        }

#if UNITY_EDITOR
        static SaveTableCache() {
            InitializeForEditor();
        }
        
        private static void InitializeForEditor() {
            _cache = new Cache();
            
            var types = TypeCache.GetTypesDerivedFrom<ISaveTable>();
            
            for (int i = 0; i < types.Count; i++) {
                var type = types[i];

                if (!type.IsAbstract &&
                    Attribute.IsDefined(type, typeof(SerializableAttribute)) &&
                    type.GetCustomAttribute<SaveTableAttribute>(false) is { keyType: { } keyType, valueType: { } valueType }) 
                {
                    _cache.tableMap[(keyType, valueType)] = type;
                    _cache.valueTypeSet.Add(valueType);
                }
            }
        }
#else
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RuntimeInitialize() {
            InitializeForBuild();
        }
#endif
        
        private static void InitializeForBuild() {
            lock (_lock) {
                _mainThreadContext ??= SynchronizationContext.Current;
                EnsureBuildStartedInternal();
            }
        }

        private static Task<Cache> EnsureBuildStarted() {
            lock (_lock) {
                return EnsureBuildStartedInternal();
            }
        }

        private static Task<Cache> EnsureBuildStartedInternal() {
            if (_buildTask != null)
                return _buildTask;

            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a =>
                    !a.IsDynamic &&
                    !a.FullName.Contains(Editor, StringComparison.OrdinalIgnoreCase))
                .ToArray();

            _buildTask = Task.Run(() => BuildCacheFromAssemblies(assemblies, CancellationToken.None));

            _buildTask.ContinueWith(task => {
                if (task.IsCanceled)
                    return;

                if (task.IsFaulted) {
                    Debug.LogException(task.Exception);
                    return;
                }

                PublishOnMainThread(task.Result);
            });

            return _buildTask;
        }

        private static void PublishOnMainThread(Cache result) {
            if (_mainThreadContext == null) {
                PublishResult(result);
                return;
            }

            _mainThreadContext.Post(_ => {
                PublishResult(result);
            }, null);
        }

        private static void PublishResult(Cache result)
        {
            lock (_lock)
            {
                _cache ??= result;
            }
        }

        private static Cache BuildCacheFromAssemblies(
            Assembly[] assemblies,
            CancellationToken token
        ) {
            var result = new Cache();

            for (int i = 0; i < assemblies.Length; i++) {
                token.ThrowIfCancellationRequested();

                var assembly = assemblies[i];
                var types = GetLoadableTypes(assembly);

                for (int j = 0; j < types.Length; j++) {
                    var type = types[j];

                    if (type == null)
                        continue;

                    if (!type.IsAbstract &&
                        typeof(ISaveTable).IsAssignableFrom(type) &&
                        Attribute.IsDefined(type, typeof(SerializableAttribute)) &&
                        type.GetCustomAttribute<SaveTableAttribute>(false) is { keyType: { } keyType, valueType: { } valueType }) 
                    {
                        result.tableMap[(keyType, valueType)] = type;
                        result.valueTypeSet.Add(valueType);
                    }
                }
            }
            
            return result;
        }

        private static Type[] GetLoadableTypes(Assembly assembly) {
            try {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e) {
                return e.Types.Where(t => t != null).ToArray();
            }
        }
    }
    
}