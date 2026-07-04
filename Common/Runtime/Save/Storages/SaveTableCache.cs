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
        private static Task<Dictionary<Type, Type>> _buildTask;
        private const string Editor = "editor";

        private static Dictionary<Type, Type> _tableTypesCache = new();
        
        public static Type GetBaseElementType(Type t) {
            return t.IsArray ? t :
                typeof(Object).IsAssignableFrom(t) ? typeof(Object) :
                t.IsClass || t.IsInterface ? typeof(object) :
                t.IsEnum ? typeof(Enum) :
                t;
        }
        
        public static bool TryGetTableType(Type elementType, out Type tableType)
        {
#if UNITY_EDITOR
            return _tableTypesCache.TryGetValue(elementType, out tableType);
#endif
            var cache = _tableTypesCache;
            
            if (cache == null)
            {
                var task = EnsureBuildStarted();
                cache = task.GetAwaiter().GetResult();
                PublishResult(cache);
            }

            return cache.TryGetValue(elementType, out tableType);
        }

#if UNITY_EDITOR
        static SaveTableCache() {
            InitializeForEditor();
        }
        
        private static void InitializeForEditor() {
            var types = TypeCache.GetTypesDerivedFrom<ISaveTable>();
            
            for (int i = 0; i < types.Count; i++) {
                var type = types[i];

                if (!type.IsAbstract &&
                    Attribute.IsDefined(type, typeof(SerializableAttribute)) &&
                    type.GetCustomAttribute<SaveTableAttribute>(false)?.elementType is { } elementType) 
                {
                    _tableTypesCache[elementType] = type;
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

        private static Task<Dictionary<Type, Type>> EnsureBuildStarted() {
            lock (_lock) {
                return EnsureBuildStartedInternal();
            }
        }

        private static Task<Dictionary<Type, Type>> EnsureBuildStartedInternal() {
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

        private static void PublishOnMainThread(Dictionary<Type, Type> result) {
            if (_mainThreadContext == null) {
                PublishResult(result);
                return;
            }

            _mainThreadContext.Post(_ => {
                PublishResult(result);
            }, null);
        }

        private static void PublishResult(Dictionary<Type, Type> result)
        {
            lock (_lock)
            {
                _tableTypesCache ??= result;
            }
        }

        private static Dictionary<Type, Type> BuildCacheFromAssemblies(
            Assembly[] assemblies,
            CancellationToken token
        ) {
            var result = new Dictionary<Type, Type>();

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
                        type.GetCustomAttribute<SaveTableAttribute>(false)?.elementType is { } elementType
                    ) {
                        result[elementType] = type;
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