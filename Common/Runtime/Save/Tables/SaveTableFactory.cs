using System;
using System.Collections.Generic;
using System.Reflection;
using Object = UnityEngine.Object;

namespace MisterGames.Common.Save.Tables {
    
    public sealed class SaveTableFactory : ISaveTableFactory {
        
        private static readonly Dictionary<Type, Type> _tableTypes = new();
        private readonly Dictionary<Type, ISaveTable> _tables = new();

        public IEnumerable<ISaveTable> Tables => _tables.Values;
        
        public ISaveTable Get<T>() {
            return Get(typeof(T));
        }
        
        public ISaveTable Get(Type elementType) {
            return _tables.GetValueOrDefault(GetBaseElementType(elementType));
        }

        public void Set<T>(ISaveTable value) {
            Set(typeof(T), value);
        }

        public void Set(Type elementType, ISaveTable value) {
            _tables[GetBaseElementType(elementType)] = value;
        }

        public ISaveTable GetOrCreate<T>() {
            return GetOrCreate(typeof(T));
        }
        
        public ISaveTable GetOrCreate(Type elementType) {
            if (_tables.TryGetValue(elementType, out var repo) && repo != null) {
                return repo;
            }
            
            var baseType = GetBaseElementType(elementType);
            if (_tables.TryGetValue(baseType, out repo) && repo != null) {
                return repo;
            }
            
            Prewarm();

            if (_tableTypes.TryGetValue(elementType, out var repositoryType)) {
                repo = Activator.CreateInstance(repositoryType) as ISaveTable;
                _tables[baseType] = repo;
            }
            else if (_tableTypes.TryGetValue(baseType, out repositoryType)) {
                repo = Activator.CreateInstance(repositoryType) as ISaveTable;
                _tables[baseType] = repo;
            }

            return repo;
        }

        public void Clear() {
            _tables.Clear();
        }

        public void Prewarm() {
            if (_tableTypes.Count > 0) return;
            
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            
            for (int a = 0; a < assemblies.Length; a++) {
                var assembly = assemblies[a];
                var types = assembly.GetTypes();

                for (int t = 0; t < types.Length; t++) {
                    var type = types[t];
                    
                    if (type.IsAbstract || type.IsValueType ||
                        !typeof(ISaveTable).IsAssignableFrom(type) ||
                        !Attribute.IsDefined(type, typeof(SerializableAttribute)) ||
                        !TryGetElementType(type, out var elementType)
                    ) {
                        continue;
                    }

                    _tableTypes[elementType] = type;
                }
            }
        }

        private static Type GetBaseElementType(Type t) {
            return t.IsArray ? t : 
                typeof(Object).IsAssignableFrom(t) ? typeof(Object) : 
                t.IsClass || t.IsInterface ? typeof(object) :
                t.IsEnum ? typeof(Enum) :
                t;
        }

        private static bool TryGetElementType(Type type, out Type elementType) {
            elementType = type.GetCustomAttribute<SaveTableAttribute>(false)?.elementType;
            return elementType != null;
        }
    }
    
}