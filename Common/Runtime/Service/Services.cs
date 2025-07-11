namespace MisterGames.Common.Service {
    
    public static class Services {

        public static T Get<T>() where T : class {
            return ServiceStorageRunner.ServiceStorage.GetGlobalService<T>();
        }

        public static T Get<T>(int id) where T : class {
            return ServiceStorageRunner.ServiceStorage.GetService<T>(id);
        }
        
        public static ServiceBuilder<T> Register<T>(T service) where T : class {
            return ServiceStorageRunner.ServiceStorage.RegisterGlobal(service);
        }

        public static void Unregister<T>(T service) where T : class {
            ServiceStorageRunner.ServiceStorage.UnregisterGlobal(service);
        }
        
        public static ServiceBuilder<T> Register<T>(T service, int id) where T : class {
            return ServiceStorageRunner.ServiceStorage.Register(service, id);
        }

        public static void Unregister<T>(T service, int id) where T : class {
            ServiceStorageRunner.ServiceStorage.Unregister(service, id);
        }
    }
    
}