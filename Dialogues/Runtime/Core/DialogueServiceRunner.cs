using MisterGames.Common.Localization;
using MisterGames.Common.Service;
using UnityEngine;

namespace MisterGames.Dialogues.Core {
    
    public sealed class DialogueServiceRunner : MonoBehaviour {
        
        [SerializeField] private LocalizationSettings _localizationSettings;
        
        private readonly DialogueService _dialogueService = new();
        
        private void Awake() {
            _dialogueService.Initialize();
            Services.Register<IDialogueService>(_dialogueService);
        }

        private void OnDestroy() {
            Services.Unregister(_dialogueService);
        }
    }
    
}