using UnityEngine;

namespace MisterGames.Interact.Path {

    public interface IInteractivePath {

        float GetNearestPathPoint(Vector3 position);
        float MoveAlongPath(float t, Vector3 motion);

        void Evaluate(float t, out Vector3 position, out Vector3 tangent, out Vector3 normal);
    }

}
