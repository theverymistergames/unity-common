using Cysharp.Threading.Tasks;

namespace MisterGames.Common.Pooling {
  
    public interface IObjectPoolAsync<T> where T : class { 
      
        int CountAll { get; }
        int CountActive { get; }
        int CountInactive { get; }

        T Get(T sample);

        UniTask<T> GetAsync(T sample);

        void Release(T element);

        void Clear();
    }
    
}