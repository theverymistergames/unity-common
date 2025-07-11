using System;
using System.Collections.Generic;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Common.Service {
    
    public sealed class ServiceStorage : IServiceStorage {

        private readonly Dictionary<KeyIdType, object> _servicesWithIdMap = new();
        private readonly Dictionary<Type, object> _globalServicesMap = new();
        private readonly MultiValueDictionary<object, Type> _globalServiceToTypeMap = new();
        private readonly MultiValueDictionary<KeyIdInstance, Type> _serviceWithIdToTypeMap = new();

        public ServiceBuilder RegisterGlobal<T>(T service) where T : class {
            return RegisterGlobal(service, typeof(T));
        }

        public ServiceBuilder RegisterGlobal(object service, Type type) {
            if (service == null) {
                LogError($"trying to register global service of type [{type.Name}], " +
                         $"but passed service instance is null.");
                return CreateServiceBuilder(null);
            }

            if (_globalServicesMap.TryGetValue(type, out object existent)) {
                if (existent.Equals(service)) return CreateServiceBuilder(service);
                
                LogWarning($"trying to register global service [{service}] of type [{type.Name}], " +
                           $"but this type was already occupied by global service [{existent}]. " +
                           $"Old service will be replaced with the new service for this type.");
                
                UnregisterGlobal(existent);
            }
            else {
                _globalServiceToTypeMap.AddValue(service, type);
            }
            
            _globalServicesMap[type] = service;
            LogInfo($"global service [{service}] of type [{type.Name}] is registered.");
            
            return CreateServiceBuilder(service);
        }

        public void UnregisterGlobal(object service) {
            if (service == null) {
                LogError($"trying to unregister global service, " +
                         $"but passed service instance is null.");
                return;
            }
            
            int count = _globalServiceToTypeMap.GetCount(service);

            for (int i = 0; i < count; i++) {
                _globalServicesMap.Remove(_globalServiceToTypeMap.GetValue(service, i));
            }

            _globalServiceToTypeMap.RemoveValues(service);
            
            if (count > 0) LogInfo($"global service {service} of type [{service.GetType().Name}] is unregistered.");
        }
        
        public ServiceBuilder Register<T>(T service, int id) where T : class {
            return Register(service, typeof(T), id);
        }

        public ServiceBuilder Register(object service, Type type, int id) {
            if (service == null) {
                LogError($"trying to register service of type [{type.Name}] with id [{id}], " +
                         $"but passed service instance is null.");
                return CreateServiceBuilder(null, id);
            }
            
            var typeKey = CreateIdTypeKey(type, id);

            if (_servicesWithIdMap.TryGetValue(typeKey, out object existent)) {
                if (existent.Equals(service)) return CreateServiceBuilder(service, id);
                
                LogWarning($"trying to register service [{service}] of type [{type.Name}] with id [{id}], " +
                           $"but this id was already occupied by service [{existent}]. " +
                           $"Old service will be replaced with the new service for this id.");
                
                Unregister(existent, id);
            }
            else {
                _serviceWithIdToTypeMap.AddValue(CreateIdInstanceKey(service, id), type);
            }
            
            _servicesWithIdMap[typeKey] = service;
            LogInfo($"service [{service}] of type [{type.Name}] is registered with id [{id}].");
            
            return CreateServiceBuilder(service, id);
        }

        public void Unregister(object service, int id) {
            if (service == null) {
                LogError($"trying to unregister service with id [{id}], " +
                         $"but passed service instance is null.");
                return;
            }
            
            var instanceKey = CreateIdInstanceKey(service, id);
            int count = _serviceWithIdToTypeMap.GetCount(instanceKey);

            for (int i = 0; i < count; i++) {
                _servicesWithIdMap.Remove(CreateIdTypeKey(_serviceWithIdToTypeMap.GetValue(instanceKey, i), id));
            }

            _serviceWithIdToTypeMap.RemoveValues(instanceKey);
            
            if (count > 0) LogInfo($"service {service} of type [{service.GetType().Name}] with id [{id}] is unregistered.");
        }

        public T GetGlobalService<T>() where T : class {
            return _globalServicesMap.GetValueOrDefault(typeof(T)) as T;
        }

        public T GetService<T>(int id) where T : class {
            return _servicesWithIdMap.GetValueOrDefault(CreateIdTypeKey(typeof(T), id)) as T; 
        }

        public void Clear() {
            _servicesWithIdMap.Clear();
            _globalServicesMap.Clear();
            _serviceWithIdToTypeMap.Clear();
            _globalServiceToTypeMap.Clear();
        }

        private ServiceBuilder CreateServiceBuilder(object service, int? id = null) {
            return new ServiceBuilder(this, service, id);
        }

        private static KeyIdType CreateIdTypeKey(Type type, int id) {
            return new KeyIdType(id, type);
        }
        
        private static KeyIdInstance CreateIdInstanceKey(object instance, int id) {
            return new KeyIdInstance(id, instance);
        }
        
        private void LogInfo(string message) {
            Debug.Log($"<color=white>Services</color>: {message}");
        }
        
        private void LogWarning(string message) {
            Debug.LogWarning($"<color=white>Services</color>: {message}");
        }
        
        private void LogError(string message) {
            Debug.LogError($"<color=white>Services</color>: {message}");
        }
        
        private readonly struct KeyIdType : IEquatable<KeyIdType> {
            private readonly int _id;
            private readonly Type _type;
            
            public KeyIdType(int id, Type type) {
                _id = id;
                _type = type;
            }
            
            public bool Equals(KeyIdType other) => _id == other._id && _type == other._type;
            public override bool Equals(object obj) => obj is KeyIdType other && Equals(other);
            public override int GetHashCode() => HashCode.Combine(_id, _type);
            public static bool operator ==(KeyIdType left, KeyIdType right) => left.Equals(right);
            public static bool operator !=(KeyIdType left, KeyIdType right) => !left.Equals(right);
        }
        
        private readonly struct KeyIdInstance : IEquatable<KeyIdInstance> {
            private readonly int _id;
            private readonly object _instance;
            
            public KeyIdInstance(int id, object instance) {
                _id = id;
                _instance = instance;
            }
            
            public bool Equals(KeyIdInstance other) => _id == other._id && _instance.Equals(other._instance);
            public override bool Equals(object obj) => obj is KeyIdInstance other && Equals(other);
            public override int GetHashCode() => HashCode.Combine(_id, _instance);
            public static bool operator ==(KeyIdInstance left, KeyIdInstance right) => left.Equals(right);
            public static bool operator !=(KeyIdInstance left, KeyIdInstance right) => !left.Equals(right);
        }
    }
    
}