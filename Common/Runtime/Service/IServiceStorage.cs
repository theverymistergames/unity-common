using System;

namespace MisterGames.Common.Service {
    
    public interface IServiceStorage {

        ServiceBuilder RegisterGlobal<T>(T service) where T : class;
        ServiceBuilder RegisterGlobal(object service, Type type);
        void UnregisterGlobal(object service);

        ServiceBuilder Register<T>(T service, int id) where T : class;
        ServiceBuilder Register(object service, Type type, int id);
        void Unregister(object service, int id);

        T GetGlobalService<T>() where T : class;
        T GetService<T>(int id) where T : class;
    }
    
}