using System;

namespace MisterGames.Actors.Data
{
    
    [Serializable]
    public sealed class ActorPrefabData : IActorData
    {
        public ActorRoot prefab;
    }
    
}