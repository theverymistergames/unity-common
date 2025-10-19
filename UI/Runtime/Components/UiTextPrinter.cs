using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Async;
using MisterGames.Common.Attributes;
using MisterGames.Common.Localization;
using MisterGames.Common.Maths;
using MisterGames.Common.Tick;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MisterGames.UI.Components {
    
    public sealed class UiTextPrinter : MonoBehaviour {

        [Header("Text")]
        [SerializeField] private LaunchMode _launchMode;
        [VisibleIf(nameof(_launchMode), 0)]
        [SerializeField] private TMP_Text _textField;
        [VisibleIf(nameof(_launchMode), 0)]
        [SerializeField] private LocalizationKey _localizationKey;

        [Header("Printing")]
        [SerializeField] private bool _useTimeScale = true;
        [SerializeField] [Min(0f)] private float _symbolDelayMin = 0.1f;
        [SerializeField] [Min(0f)] private float _symbolDelayMax = 0.1f;
        [SerializeField] [Min(0f)] private float _forceFinishSymbolDelay = 0.01f;

        [Header("Special Symbols")]
        [SerializeField] private SpecialSymbolData[] _specialSymbols;

        [Serializable]
        private struct SpecialSymbolData {
            public SymbolMask symbolMask;
            [Min(0f)] public float delayMin;
            [Min(0f)] public float delayMax;
        }

        [Flags]
        private enum SymbolMask {
            None = 0,
            Space = 1,
            NewLine = 2,
            Comma = 4,
            Period = 8,
            QuestionMark = 16,
            ExclamationMark = 32,
            Semicolon = 64,
            Colon = 128,
            Ellipsis = 256,
        }
        
        private enum LaunchMode {
            OnEnable,
            Manual,
        }

        private const string TransparentTagOpen = "<color=#00000000>";
        private const string TransparentTagClose = "</color>";

        private CancellationTokenSource _destroyCts;
        private CancellationTokenSource _enableCts;

        private readonly Dictionary<int, byte> _operationIdMap = new();
        private readonly Dictionary<int, float> _immediateFinishRequestsMap = new();

        private void Awake() {
            AsyncExt.RecreateCts(ref _destroyCts);
        }

        private void OnDestroy() {
            AsyncExt.DisposeCts(ref _destroyCts);
        }

        private void OnEnable() {
            AsyncExt.RecreateCts(ref _enableCts);

            if (_launchMode == LaunchMode.OnEnable) {
                PrintDefaultAsync(_enableCts.Token).Forget();
            }
        }

        private void OnDisable() {
            AsyncExt.DisposeCts(ref _enableCts);
        }

        public UniTask PrintDefaultAsync(CancellationToken cancellationToken) {
            return PrintTextAsync(_textField, _localizationKey.GetValue(), cancellationToken);
        }

        public void CancelPrintingDefault(bool clear = false) {
            CancelPrinting(_textField, clear);
        }

        public void ForceFinishPrintingDefault(float symbolDelay = -1f) {
            ForceFinishPrinting(_textField, symbolDelay);
        }
        
        public async UniTask PrintTextAsync(
            TMP_Text textField,
            string content,
            CancellationToken cancellationToken) 
        {
            int hash = textField.GetHashCode();
            byte id = _operationIdMap.GetValueOrDefault(hash);
            byte currentId;

            _operationIdMap[hash] = id.IncrementUncheckedRef();

            var sb = new StringBuilder();
            sb.Append(TransparentTagOpen);
            sb.Append(content);
            sb.Append(TransparentTagClose);
            
            int length = content.Length;
            int pointer = 0;
            int caret = 0;
            
            bool useTimeScale = _useTimeScale;
            float delayAccum = 0f;
            char prev = '\0';
            
            while (pointer < length && 
                   !cancellationToken.IsCancellationRequested && 
                   !_destroyCts.IsCancellationRequested &&
                   _operationIdMap.TryGetValue(hash, out currentId) && currentId == id) 
            {
                float symbolDelay = -1f;
                bool isForceFinish = _immediateFinishRequestsMap.TryGetValue(hash, out float finishDelay);
                
                if (isForceFinish) {
                    if (finishDelay <= 0f) {
                        textField.SetText(content);
                        _operationIdMap.Remove(hash);
                        _immediateFinishRequestsMap.Remove(hash);
                        break;   
                    }
                    
                    symbolDelay = finishDelay;
                }
                
                char c = GetNextCharSkippingTags(content, ref pointer, length);

                sb.Remove(caret, TransparentTagOpen.Length);
                caret = pointer;
                sb.Insert(caret, TransparentTagOpen);
                
                textField.SetText(sb);
                
                if (symbolDelay < 0f) {
                    int ptr = pointer;
                    symbolDelay = GetSymbolDelay(prev, c, GetNextCharSkippingTags(content, ref ptr, length));
                }
                
                prev = c;
                delayAccum += symbolDelay;
                
                float startTime = useTimeScale ? TimeSources.scaledTime : Time.time;
                finishDelay = isForceFinish ? finishDelay : -1f;
                
                while ((useTimeScale ? TimeSources.scaledTime : Time.time) - startTime < delayAccum &&
                       !cancellationToken.IsCancellationRequested &&
                       !_destroyCts.IsCancellationRequested &&
                       _operationIdMap.TryGetValue(hash, out currentId) && currentId == id && 
                       finishDelay.IsNearlyEqual(_immediateFinishRequestsMap.GetValueOrDefault(hash, -1f)))
                {
                    await UniTask.Yield();
                }

                float newFinishDelay = _immediateFinishRequestsMap.GetValueOrDefault(hash, -1f);

                if (finishDelay.IsNearlyEqual(newFinishDelay)) {
                    delayAccum -= (useTimeScale ? TimeSources.scaledTime : Time.time) - startTime;
                    continue;
                }

                delayAccum = 0f;
            }

            if (cancellationToken.IsCancellationRequested ||
                destroyCancellationToken.IsCancellationRequested ||
                !_operationIdMap.TryGetValue(hash, out currentId) || currentId != id) 
            {
                return;
            }

            _operationIdMap.Remove(hash);
            _immediateFinishRequestsMap.Remove(hash);
        }

        public void CancelPrinting(TMP_Text textField, bool clear = false) {
            int hash = textField.GetHashCode();
            
            _operationIdMap.Remove(hash);
            _immediateFinishRequestsMap.Remove(hash);
            
            if (clear) textField.SetText((string) null);
        }

        public void ForceFinishPrinting(TMP_Text textField, float symbolDelay = -1f) {
            int hash = textField.GetHashCode();
            
            if (_operationIdMap.ContainsKey(hash)) {
                if (symbolDelay < 0f) symbolDelay = _forceFinishSymbolDelay;
                _immediateFinishRequestsMap[hash] = symbolDelay;
                return;
            }

            _operationIdMap.Remove(hash);
            _immediateFinishRequestsMap.Remove(hash);
        }

        private static char GetNextCharSkippingTags(string content, ref int pointer, int length) {
            char c = pointer < length ? content[pointer++] : '\0';
            
            while (pointer - 1 < length && c == '<') {
                while (pointer < length && content[pointer++] != '>') {
                    // Skip all tags in a row
                }
                
                if (pointer < length) c = content[pointer++];
            }

            return c;
        }
        
        private float GetSymbolDelay(char prev, char curr, char next) {
            for (int i = 0; i < _specialSymbols.Length; i++) {
                var data = _specialSymbols[i];
                
                if ((data.symbolMask & SymbolMask.Space) != 0 && curr == ' ' && next != ' ' || 
                    (data.symbolMask & SymbolMask.NewLine) != 0 && curr == '\n' && next != '\n' || 
                    (data.symbolMask & SymbolMask.Comma) != 0 && curr == ',' && next != ',' ||
                    (data.symbolMask & SymbolMask.Period) != 0 && prev != '.' && curr == '.' && next != '.' ||
                    (data.symbolMask & SymbolMask.QuestionMark) != 0 && curr == '?' && next != '?' ||
                    (data.symbolMask & SymbolMask.ExclamationMark) != 0 && curr == '!' && next != '!' ||
                    (data.symbolMask & SymbolMask.Semicolon) != 0 && curr == ';' && next != ';' ||
                    (data.symbolMask & SymbolMask.Colon) != 0 && curr == ':' && next != ':' ||
                    (data.symbolMask & SymbolMask.Ellipsis) != 0 && (curr == '…' && next != '…' || prev == '.' && curr == '.' && next != '.')) 
                {
                    return Random.Range(data.delayMin, data.delayMax);
                }
            }

            return Random.Range(_symbolDelayMin, _symbolDelayMax);
        }
    }
    
}