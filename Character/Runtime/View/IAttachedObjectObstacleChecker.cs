using UnityEngine;

namespace MisterGames.Character.View {

    public interface IAttachedObjectObstacleChecker {

        Vector3 AdjustObjectPosition(Transform root, Vector3 position, float radius, ref Vector3 obstacleOffset, float dt);
    }

}
