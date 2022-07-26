namespace MisterGames.Blueprints.Core {
    
    public interface IBlueprintEnter {
        void Enter(int port);
    }

    public interface IBlueprintGetter<out T> {
        T Get(int port);
    }

    internal interface IBlueprintGetter {
        T Get<T>(int port);
    }

}