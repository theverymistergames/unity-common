using UnityEngine;

namespace MisterGames.Interact.Path {

    public interface IInteractivePath {
        void Evaluate(float t, out Vector3 position, out Vector3 tangent, out Vector3 normal);
        float MoveAlongPath(float t, Vector3 motion);
    }

}
