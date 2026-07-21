using System;
using System.Collections.Generic;
using MisterGames.Common.Localization;
using MisterGames.Dialogues.Core;
using UnityEngine.Pool;

namespace MisterGames.Dialogues.Storage {
    
    public sealed class DialogueTable : IDialogueTable, IDisposable {

        public LocalizationKey DialogueId { get; }
        
        public IReadOnlyList<LocalizationKey> Roles => _roles;
        public IReadOnlyList<LocalizationKey> Branches => _branches;
        public IReadOnlyList<DialogueElement> Elements => _elements;

        private readonly List<LocalizationKey> _roles;
        private readonly List<LocalizationKey> _branches;
        private readonly List<DialogueElement> _elements;
        private readonly Dictionary<LocalizationKey, int> _elementsIndexMap;
        
        public DialogueTable(IDialogueTableStorage storage) {
            DialogueId = storage.DialogueId;
            _roles = CreateRoles(storage);
            _branches = CreateBranches(storage);
            _elements = CreateElements(storage);
            _elementsIndexMap = CreateElementsIndexMap(_elements);
        }

        public DialogueTable(
            LocalizationKey dialogueId,
            IReadOnlyList<LocalizationKey> roles,
            IReadOnlyList<LocalizationKey> branches,
            IReadOnlyList<DialogueElement> elements) 
        {
            DialogueId = dialogueId;
            
            _roles = ListPool<LocalizationKey>.Get();
            _branches = ListPool<LocalizationKey>.Get();
            _elements = ListPool<DialogueElement>.Get();
            
            _roles.AddRange(roles);
            _branches.AddRange(branches);
            _elements.AddRange(elements);
            _elementsIndexMap = CreateElementsIndexMap(_elements);
        }

        public void Dispose() {
            ListPool<LocalizationKey>.Release(_roles);
            ListPool<LocalizationKey>.Release(_branches);
            ListPool<DialogueElement>.Release(_elements);
            DictionaryPool<LocalizationKey, int>.Release(_elementsIndexMap);
        }

        public DialogueElement GetElementData(LocalizationKey elementKey) {
            return _elementsIndexMap.TryGetValue(elementKey, out int index) ? _elements[index] : default;
        }

        private static List<LocalizationKey> CreateRoles(IDialogueTableStorage storage) {
            int rolesCount = storage.RolesCount;
            var roles = ListPool<LocalizationKey>.Get();
            
            for (int i = 0; i < rolesCount; i++) {
                roles.Add(storage.GetRoleAt(i));
            }

            return roles;
        }
        
        private static List<LocalizationKey> CreateBranches(IDialogueTableStorage storage) {
            int branchesCount = storage.BranchesCount;
            var branches = ListPool<LocalizationKey>.Get();
            
            for (int i = 0; i < branchesCount; i++) {
                branches.Add(storage.GetBranchAt(i));
            }

            return branches;
        }

        private static List<DialogueElement> CreateElements(IDialogueTableStorage storage) {
            int elementsCount = storage.ElementsCount;
            var elements = ListPool<DialogueElement>.Get();
            
            for (int i = 0; i < elementsCount; i++) {
                elements.Add(storage.GetElementAt(i));
            }
            
            return elements;
        }

        private static Dictionary<LocalizationKey, int> CreateElementsIndexMap(IReadOnlyList<DialogueElement> elements) {
            var indexMap = DictionaryPool<LocalizationKey, int>.Get();
            
            for (int i = 0; i < elements.Count; i++) {
                indexMap[elements[i].key] = i;
            }
            
            return indexMap;
        }
    }
    
}