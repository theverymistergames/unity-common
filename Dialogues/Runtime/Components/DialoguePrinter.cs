using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Async;
using MisterGames.Common.Localization;
using MisterGames.Common.Pooling;
using MisterGames.Common.Service;
using MisterGames.Dialogues.Core;
using MisterGames.UI.Components;
using TMPro;
using UnityEngine;

namespace MisterGames.Dialogues.Components {
    
    public sealed class DialoguePrinter : MonoBehaviour, IDialoguePrinter {
        
        [Header("Printing")]
        [SerializeField] private UiTextPrinter _textPrinter;
        [SerializeField] private Transform _replicaParent;
        [SerializeField] private TMP_Text _replicaTextPrefab;
        
        private readonly List<TMP_Text> _allocatedTextFields = new();
        private CancellationTokenSource _enableCts;

        private void OnEnable() {
            AsyncExt.RecreateCts(ref _enableCts);
            
            Services.Get<IDialogueService>()?.RegisterPrinter(this);
        }

        private void OnDisable() {
            AsyncExt.DisposeCts(ref _enableCts);
            
            Services.Get<IDialogueService>()?.UnregisterPrinter(this);
        }

        public async UniTask PrintElement(LocalizationKey key, CancellationToken cancellationToken) {
            cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _enableCts.Token).Token;
            
            var textField = await CreateTextField(_replicaParent);
            if (cancellationToken.IsCancellationRequested) return;

            await _textPrinter.PrintTextAsync(textField, key.GetValue(), cancellationToken);
        }

        public void CancelCurrentElementPrinting(DialogueCancelMode mode) {
            if (_allocatedTextFields.Count == 0) return;

            var textField = _allocatedTextFields[^1];
            
            switch (mode) {
                case DialogueCancelMode.Clear:
                    _textPrinter.ClearText(textField);
                    break;
                
                case DialogueCancelMode.Stop:
                    _textPrinter.CancelPrinting(textField);
                    break;
                
                case DialogueCancelMode.PrintToEnd:
                    _textPrinter.FinishPrintingImmediately(textField);
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }
        }

        public void ClearAllText() {
            ReleaseAllTextFields();
        }

        private async UniTask<TMP_Text> CreateTextField(Transform parent) {
            var textField = await PrefabPool.Main.GetAsync(_replicaTextPrefab, parent, active: false);
            _allocatedTextFields.Add(textField);
            
            textField.SetText((string) null);
            textField.gameObject.SetActive(true);
            
            return textField;
        }

        private void ReleaseTextField(TMP_Text textField) {
            PrefabPool.Main.Release(textField);
            _allocatedTextFields.Remove(textField);
        }
        
        private void ReleaseAllTextFields() {
            for (int i = 0; i < _allocatedTextFields.Count; i++) {
                ReleaseTextField(_allocatedTextFields[i]);
            }
            
            _allocatedTextFields.Clear();
        }
    }
    
}