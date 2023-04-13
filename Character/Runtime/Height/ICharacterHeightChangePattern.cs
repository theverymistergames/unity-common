using UnityEngine;

namespace MisterGames.Character.Height {

    public interface ICharacterHeightChangePattern {
        float MapHeight(float height);
        Quaternion MapHeadRotationOffset(float height);
        Vector3 MapHeadPositionOffset(float height);
    }
}
