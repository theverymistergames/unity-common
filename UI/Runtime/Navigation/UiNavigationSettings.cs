using MisterGames.Input.Actions;
using UnityEngine;

namespace MisterGames.UI.Navigation {

    [CreateAssetMenu(fileName = nameof(UiNavigationSettings), menuName = "MisterGames/UI/" + nameof(UiNavigationSettings))]
    public sealed class UiNavigationSettings : ScriptableObject {

        public InputActionRef cancelInput;

    }
    
}