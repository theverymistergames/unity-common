using System;

namespace MisterGames.Collisions.Core {

    public interface ICollisionDetector {
        event Action OnContact;
        event Action OnLostContact;
        event Action OnTransformChanged;

        CollisionInfo CollisionInfo { get; }

        void FilterLastResults(CollisionFilter filter, out CollisionInfo info);
        void FetchResults();
    }

}
