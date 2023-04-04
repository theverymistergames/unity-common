using MisterGames.Character.Configs;
using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.Character.Core2 {

    [CreateAssetMenu(fileName = nameof(CharacterSettings), menuName = "MisterGames/Character/" + nameof(CharacterSettings))]
    public sealed class CharacterSettings : ScriptableObject {

        [EmbeddedInspector] public CharacterMassSettings mass;
        [EmbeddedInspector] public CharacterViewSettings view;
        [EmbeddedInspector] public CharacterMotionSettings[] motionStates;

    }
}
