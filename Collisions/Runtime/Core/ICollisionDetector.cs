using System;

namespace MisterGames.Collisions.Core {

    public interface ICollisionDetector {

        event Action OnContact;
        event Action OnLostContact;
        event Action OnTransformChanged;

        int Capacity { get; }

        CollisionInfo CollisionInfo { get; }

        ReadOnlySpan<CollisionInfo> FilterLastResults(CollisionFilter filter);

        void FetchResults();
    }

}
