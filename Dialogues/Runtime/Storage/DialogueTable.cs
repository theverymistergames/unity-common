using System;
using System.Buffers;
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

        private readonly LocalizationKey[] _roles;
        private readonly LocalizationKey[] _branches;
        private readonly DialogueElement[] _elements;
        private readonly Dictionary<LocalizationKey, int> _elementsIndexMap;
        
        public DialogueTable(IDialogueTableStorage storage) {
            DialogueId = storage.DialogueId;
            _roles = CreateRoles(storage);
            _branches = CreateBranches(storage);
            _elements = CreateElements(storage, out _elementsIndexMap);
        }

        public void Dispose() {
            ArrayPool<LocalizationKey>.Shared.Return(_roles);
            ArrayPool<LocalizationKey>.Shared.Return(_branches);
            ArrayPool<DialogueElement>.Shared.Return(_elements);
            DictionaryPool<LocalizationKey, int>.Release(_elementsIndexMap);
        }

        public DialogueElement GetElementData(LocalizationKey elementKey) {
            return _elementsIndexMap.TryGetValue(elementKey, out int index) ? _elements[index] : default;
        }

        private static LocalizationKey[] CreateRoles(IDialogueTableStorage storage) {
            int rolesCount = storage.RolesCount;
            var roles = ArrayPool<LocalizationKey>.Shared.Rent(rolesCount);
            
            for (int i = 0; i < rolesCount; i++) {
                roles[i] = storage.GetRoleAt(i);
            }

            return roles;
        }
        
        private static LocalizationKey[] CreateBranches(IDialogueTableStorage storage) {
            int branchesCount = storage.BranchesCount;
            var branches = ArrayPool<LocalizationKey>.Shared.Rent(branchesCount);
            
            for (int i = 0; i < branchesCount; i++) {
                branches[i] = storage.GetBranchAt(i);
            }

            return branches;
        }
        
        private static DialogueElement[] CreateElements(IDialogueTableStorage storage, out Dictionary<LocalizationKey, int> indexMap) {
            int elementsCount = storage.ElementsCount;
            var elements = ArrayPool<DialogueElement>.Shared.Rent(elementsCount);
            
            indexMap = DictionaryPool<LocalizationKey, int>.Get();
            
            for (int i = 0; i < elementsCount; i++) {
                var e = storage.GetElementAt(i);
                
                elements[i] = e;
                indexMap[e.key] = i;
            }
            
            return elements;
        }
    }
    
}