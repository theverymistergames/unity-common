using UnityEngine;

namespace MisterGames.Character.Animations {

    public interface ICharacterAnimationPattern {

        Vector3 MapBodyPositionOffset(float progress);
        Quaternion MapBodyRotationOffset(float progress);

        Vector3 MapHeadPositionOffset(float progress);
        Quaternion MapHeadRotationOffset(float progress);
    }

}
