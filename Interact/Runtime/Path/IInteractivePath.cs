using UnityEngine;

namespace MisterGames.Interact.Path {

    public interface IInteractivePath {

        float GetStart(Vector3 position);
        float MoveAlongPath(float t, Vector3 motion);

        void Evaluate(float t, out Vector3 position, out Vector3 forward, out Quaternion orientation);
    }

}
