using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Common.Async;
using MisterGames.Common.Lists;
using MisterGames.Common.Localization;
using MisterGames.Common.Pooling;
using MisterGames.Common.Service;
using MisterGames.Dialogues.Core;
using MisterGames.UI.Components;
using TMPro;
using UnityEngine;

namespace MisterGames.Dialogues.Components {
    
    public sealed class DialoguePrinter : MonoBehaviour, IActorComponent, IDialoguePrinter {
        
        [SerializeField] private PrintSettings _defaultSettings;
        [SerializeField] private RoleData[] _perRoleSettings;
        
        [Serializable]
        private struct RoleData {
            public int roleIndex;
            public PrintSettings printSettings;
        }
        
        [Serializable]
        private struct PrintSettings {
            [Header("Printer")]
            public UiTextPrinter textPrinter;
            public PrinterOutput printerOutput;
            public Transform replicaParent;
            public TMP_Text replicaTextPrefab;
            
            [Header("Text")]
            public HorizontalAlignmentOptions alignment;
            public VerticalAlignmentOptions vertical;
            public Vector4 margin;
        }

        private enum PrinterOutput {
            TextFieldPrefab,
            DefaultTextField,
        }
        
        private readonly List<TMP_Text> _allocatedTextFields = new();
        private readonly Dictionary<TMP_Text, (LocalizationKey key, UiTextPrinter printer)> _textPrintMap = new();
        private CancellationTokenSource _enableCts;
        private UiTextPrinter _lastPrinter;
        private TMP_Text _lastTextField;

        private void OnEnable() {
            AsyncExt.RecreateCts(ref _enableCts);

            if (Services.TryGet(out ILocalizationService localizationService)) {
                localizationService.OnLocaleChanged += OnLocaleChanged;
            }
        }

        private void OnDisable() {
            AsyncExt.DisposeCts(ref _enableCts);
            
            if (Services.TryGet(out ILocalizationService localizationService)) {
                localizationService.OnLocaleChanged -= OnLocaleChanged;
            }
        }

        private void OnLocaleChanged(Locale locale) {
            foreach (var (textField, (key, printer)) in _textPrintMap) {
                printer.SetText(textField, key.GetValue());
            }
        }

        public async UniTask PrintElement(LocalizationKey key, int roleIndex, CancellationToken cancellationToken) {
            cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _enableCts.Token).Token;
            
            bool hasCustomSettings = _perRoleSettings.TryFind(roleIndex, (r, i) => r.roleIndex == i, out var data);
            var customSettings = data.printSettings;

            var output = hasCustomSettings
                ? customSettings.printerOutput
                : _defaultSettings.printerOutput;

            var textPrinter = hasCustomSettings && customSettings.textPrinter != null
                ? customSettings.textPrinter
                : _defaultSettings.textPrinter;

            TMP_Text textField;
            
            switch (output) {
                case PrinterOutput.TextFieldPrefab:
                    var textPrefab = hasCustomSettings && customSettings.replicaTextPrefab != null 
                        ? customSettings.replicaTextPrefab
                        : _defaultSettings.replicaTextPrefab;
            
                    var textParent = hasCustomSettings && customSettings.replicaParent != null 
                        ? customSettings.replicaParent
                        : _defaultSettings.replicaParent;
            
                    textField = await CreateTextField(textPrefab, textParent);
                    if (cancellationToken.IsCancellationRequested) return;
                    
                    break;
                
                case PrinterOutput.DefaultTextField:
                    textField = textPrinter.DefaultTextField;
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            if (hasCustomSettings) {
                textField.margin = customSettings.margin;
                textField.horizontalAlignment = customSettings.alignment;
                textField.verticalAlignment = customSettings.vertical;
            }
            else {
                textField.margin = _defaultSettings.margin;
                textField.horizontalAlignment = _defaultSettings.alignment;
                textField.verticalAlignment = _defaultSettings.vertical;
            }

            _lastPrinter = textPrinter;
            _lastTextField = textField;

#if UNITY_EDITOR
            textField.name = $"textField_{textField.GetHashCode()}_{key.GetId()}";      
#endif

            _textPrintMap[textField] = (key, textPrinter);
            
            await textPrinter.PrintTextAsync(textField, key.GetValue(), cancellationToken);
        }

        public void CancelLastPrinting(bool clear = false) {
            if (_lastPrinter == null || _lastTextField == null) return;

            _lastPrinter.CancelPrinting(_lastTextField, clear);
        }

        public void FinishLastPrinting(float symbolDelay = -1) {
            if (_lastPrinter == null || _lastTextField == null) return;

            _lastPrinter.ForceFinishPrinting(_lastTextField, symbolDelay);
        }

        public void ClearAllText() {
            ReleaseAllTextFields();

            if (_defaultSettings.textPrinter != null) {
                _defaultSettings.textPrinter.CancelPrinting(_defaultSettings.textPrinter.DefaultTextField, clear: true);
            }

            for (int i = 0; i < _perRoleSettings.Length; i++) {
                ref var perRoleSetting = ref _perRoleSettings[i];
                ref var printSettings = ref perRoleSetting.printSettings;

                if (printSettings.textPrinter != null) {
                    printSettings.textPrinter.CancelPrinting(printSettings.textPrinter.DefaultTextField, clear: true);
                }
            }
        }

        private async UniTask<TMP_Text> CreateTextField(TMP_Text prefab, Transform parent) {
            var textField = await PrefabPool.Main.GetAsync(prefab, parent, active: false);
            _allocatedTextFields.Add(textField);

            var trf = textField.transform;
            trf.SetLocalPositionAndRotation(default, default);
            trf.localScale = Vector3.one;
            
            textField.SetText((string) null);
            textField.gameObject.SetActive(true);
            
            return textField;
        }

        private void ReleaseAllTextFields() {
            for (int i = 0; i < _allocatedTextFields.Count; i++) {
                PrefabPool.Main.Release(_allocatedTextFields[i]);
            }
            
            _allocatedTextFields.Clear();
        }
    }
    
}