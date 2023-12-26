using MisterGames.Character.Core;
using UnityEngine;

namespace MisterGames.Character.Capsule {

    public delegate void HeightChangeCallback(float newHeight, float oldHeight);

    public interface ICharacterCapsulePipeline : ICharacterPipeline {

        /// <summary>
        /// Notifies when height is being changed.
        /// </summary>
        event HeightChangeCallback OnHeightChange;

        /// <summary>
        /// Current character capsule height.
        /// </summary>
        float Height { get; set; }

        /// <summary>
        /// Current character capsule radius.
        /// </summary>
        float Radius { get; set; }

        /// <summary>
        /// Same as height and radius.
        /// </summary>
        CharacterCapsuleSize CapsuleSize { get; set; }

        /// <summary>
        /// Top point of the character capsule collider.
        /// </summary>
        Vector3 ColliderTop { get; }

        /// <summary>
        /// Center point of the character capsule collider.
        /// </summary>
        Vector3 ColliderCenter { get; }

        /// <summary>
        /// Bottom point of the character capsule collider.
        /// </summary>
        Vector3 ColliderBottom { get; }
    }

}
