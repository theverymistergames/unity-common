namespace MisterGames.Actors {
    
    public interface IActorComponent {
        void OnAwakeActor(IActor actor) { }
        void OnDestroyActor(IActor actor) { }
        
        void OnActorDataUpdated(IActor actor) { }
    }
    
}
