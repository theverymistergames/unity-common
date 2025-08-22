using System;

namespace MisterGames.Common.Service {
    
    public static class Services {

        public static T Get<T>() where T : class {
            return ServiceStorageRunner.ServiceStorage.GetGlobalService<T>();
        }

        public static T Get<T>(int id) where T : class {
            return ServiceStorageRunner.ServiceStorage.GetService<T>(id);
        }
        
        public static bool TryGet<T>(out T service) where T : class {
            if (ServiceStorageRunner.ServiceStorage.GetGlobalService<T>() is { } t) {
                service = t;
                return true;
            }
            
            service = null;
            return false;
        }
        
        public static bool TryGet<T>(int id, out T service) where T : class {
            if (ServiceStorageRunner.ServiceStorage.GetService<T>(id) is { } t) {
                service = t;
                return true;
            }
            
            service = null;
            return false;
        }
        
        public static ServiceBuilder Register<T>(T service) where T : class {
            return ServiceStorageRunner.ServiceStorage.RegisterGlobal(service);
        }

        public static ServiceBuilder Register(object service, Type type) {
            return ServiceStorageRunner.ServiceStorage.RegisterGlobal(service, type);
        }
        
        public static void Unregister(object service) {
            ServiceStorageRunner.ServiceStorage.UnregisterGlobal(service);
        }
        
        public static ServiceBuilder Register<T>(T service, int id) where T : class {
            return ServiceStorageRunner.ServiceStorage.Register(service, id);
        }
        
        public static ServiceBuilder Register(object service, Type type, int id) {
            return ServiceStorageRunner.ServiceStorage.Register(service, type, id);
        }

        public static void Unregister<T>(T service, int id) where T : class {
            ServiceStorageRunner.ServiceStorage.Unregister(service, id);
        }
    }
    
}