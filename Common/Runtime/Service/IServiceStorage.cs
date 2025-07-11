namespace MisterGames.Common.Service {
    
    public interface IServiceStorage {

        ServiceBuilder<T> RegisterGlobal<T>(T service) where T : class;
        void UnregisterGlobal<T>(T service) where T : class;

        ServiceBuilder<T> Register<T>(T service, int id) where T : class;
        void Unregister<T>(T service, int id) where T : class;

        T GetGlobalService<T>() where T : class;
        T GetService<T>(int id) where T : class;
    }
    
}