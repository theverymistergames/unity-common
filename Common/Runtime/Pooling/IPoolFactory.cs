namespace MisterGames.Common.Pooling {

    public interface IPoolFactory<T> {

        T CreatePoolElement(T sample);
        void DestroyPoolElement(T element);

        void ActivatePoolElement(T element);
        void DeactivatePoolElement(T element);
    }

}
