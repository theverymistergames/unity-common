using System;
using UnityEngine;

namespace MisterGames.Collisions.Core {

    public interface ICollisionDetector {

        event Action OnContact;
        event Action OnLostContact;
        event Action OnTransformChanged;

        Vector3 OriginOffset { get; set; }
        float Distance { get; set; }
        int Capacity { get; }
        bool HasContact { get; }
        CollisionInfo CollisionInfo { get; }

        ReadOnlySpan<CollisionInfo> FilterLastResults(CollisionFilter filter);
    }

}
