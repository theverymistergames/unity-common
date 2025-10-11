using System.Collections.Generic;
using MisterGames.Common.Localization;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Dialogues.Storage {
    
    [CreateAssetMenu(fileName = nameof(DialogueStorage), menuName = "MisterGames/Dialogues/" + nameof(DialogueStorage))]
    public sealed class DialogueStorage : ScriptableObject, IDialogueStorage {

        [SerializeField] private LocalizationKey _dialogueId;
        [HideLocalizationTable]
        [SerializeField] private List<LocalizationKey> _roles = new();
        [HideLocalizationTable]
        [SerializeField] private List<LocalizationKey> _branches = new();
        [SerializeField] private List<DialogueElement> _elements = new();

        public LocalizationKey DialogueId => _dialogueId;
        public int RolesCount => _roles.Count;
        public int BranchesCount => _branches.Count;
        public int ElementsCount => _elements.Count;
        
        public void SetDialogueId(LocalizationKey id) {
            _dialogueId = id;
            SetDirtyIfEditor();
        }

        public LocalizationKey GetRoleAt(int index) {
            return _roles[index];
        }
        
        public void AddRole(LocalizationKey role) {
            _roles.Add(role);
            SetDirtyIfEditor();
        }

        public LocalizationKey GetBranchAt(int index) {
            return _branches[index];
        }

        public void AddBranch(LocalizationKey branch) {
            _branches.Add(branch);
            SetDirtyIfEditor();
        }
        
        public DialogueElement GetElementAt(int index) {
            return _elements[index];
        }

        public void AddElement(DialogueElement dialogueElement) {
            _elements.Add(dialogueElement);
            SetDirtyIfEditor();
        }

        public void RemoveElementAt(int index) {
            _elements.RemoveAt(index);
            SetDirtyIfEditor();
        }

        public void ClearAll() {
            _dialogueId = default;
            
            _roles.Clear();
            _branches.Clear();
            _elements.Clear();
            
            SetDirtyIfEditor();
        }

        private void SetDirtyIfEditor() {
#if UNITY_EDITOR
            if (!Application.isPlaying) EditorUtility.SetDirty(this);
#endif
        }
    }
    
}