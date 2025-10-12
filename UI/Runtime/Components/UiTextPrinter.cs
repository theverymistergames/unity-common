using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Async;
using MisterGames.Common.Localization;
using MisterGames.Common.Maths;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MisterGames.UI.Components {
    
    public sealed class UiTextPrinter : MonoBehaviour {

        [Header("Text")]
        [SerializeField] private TMP_Text _textField;
        [SerializeField] private LocalizationKey _localizationKey;
        [SerializeField] private LaunchMode _launchMode;

        [Header("Timings")]
        [SerializeField] private bool _useTimeScale;
        [SerializeField] [Min(0f)] private float _symbolDelayMin;
        [SerializeField] [Min(0f)] private float _symbolDelayMax;
        [SerializeField] [Min(0f)] private float _wordDelayMin;
        [SerializeField] [Min(0f)] private float _wordDelayMax;

        private enum LaunchMode {
            OnEnable,
            Manual,
        }

        private CancellationTokenSource _enableCts;
        private CancellationToken _destroyToken;
        
        private readonly Dictionary<int, byte> _operationIdMap = new();
        private readonly HashSet<int> _immediateFinishRequests = new();
        
        private void OnEnable() {
            _destroyToken = destroyCancellationToken;
            AsyncExt.RecreateCts(ref _enableCts);

            if (_launchMode == LaunchMode.OnEnable) {
                PrintTextAsync(_textField, _localizationKey.GetValue(), _enableCts.Token).Forget();
            }
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
            int length = content.Length;
            int pointer = 0;

            while (pointer < length && 
                   !cancellationToken.IsCancellationRequested && 
                   !_destroyToken.IsCancellationRequested &&
                   _operationIdMap.TryGetValue(hash, out currentId) && currentId == id) 
            {
                if (_immediateFinishRequests.Contains(hash)) {
                    textField.SetText(content);
                    _operationIdMap.Remove(hash);
                    _immediateFinishRequests.Remove(hash);
                    break;
                }
                
                char c = content[pointer++];
                sb.Append(c);

                textField.SetText(sb);
                
                if (pointer >= length) break;

                float delay = c is '\n' or ' '
                    ? Random.Range(_wordDelayMin, _wordDelayMax)
                    : Random.Range(_symbolDelayMin, _symbolDelayMax);
                
                if (delay <= 0f) continue;

                var delayType = _useTimeScale ? DelayType.DeltaTime : DelayType.UnscaledDeltaTime;
                
                await UniTask.Delay(TimeSpan.FromSeconds(delay), delayType, cancellationToken: cancellationToken)
                    .SuppressCancellationThrow();
            }

            if (cancellationToken.IsCancellationRequested ||
                destroyCancellationToken.IsCancellationRequested ||
                !_operationIdMap.TryGetValue(hash, out currentId) || currentId != id) 
            {
                return;
            }

            _operationIdMap.Remove(hash);
        }

        public void CancelPrinting(TMP_Text textField) {
            int hash = textField.GetHashCode();
            
            _operationIdMap.Remove(hash);
            _immediateFinishRequests.Add(hash);
        }

        public void FinishPrintingImmediately(TMP_Text textField) {
            int hash = textField.GetHashCode();
            
            if (_operationIdMap.ContainsKey(hash)) {
                _immediateFinishRequests.Add(hash);
                return;
            }

            _operationIdMap.Remove(hash);
            _immediateFinishRequests.Remove(hash);
        }

        public void ClearText(TMP_Text textField) {
            textField.SetText((string) null);
        }
    }
    
}