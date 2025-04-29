using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Async;
using TMPro;
using UnityEngine;

namespace MisterGames.UI.Components {
    
    public sealed class UiTextAnimation : MonoBehaviour {
        
        [SerializeField] private TMP_Text _text;
        [SerializeField] private string _animatedText;
        [SerializeField] private StateData[] _states;

        private enum State {
            DoNothing,
            Print,
            Clear,
            Loop,
        }

        [Serializable]
        private struct StateData {
            public State state;
            [Min(0f)] public float charPrintPeriod;
            [Min(0f)] public float finishDelay;
        }

        private CancellationTokenSource _enableCts;
        private char[] _chars;

        private void Awake() {
            _chars = _animatedText.ToCharArray();
        }

        private void OnEnable() {
            AsyncExt.RecreateCts(ref _enableCts);
            StartAnimation(_enableCts.Token).Forget();
        }

        private void OnDisable() {
            AsyncExt.DisposeCts(ref _enableCts);
        }

        private async UniTask StartAnimation(CancellationToken cancellationToken) {
            int stateIndex = 0;
            
            while (!cancellationToken.IsCancellationRequested) {
                var state = _states[stateIndex++];

                switch (state.state) {
                    case State.DoNothing:
                        break;
                    
                    case State.Print:
                        await Print(state.charPrintPeriod, cancellationToken);
                        break;
                    
                    case State.Clear:
                        _text.SetText(Array.Empty<char>());
                        break;
                    
                    case State.Loop:
                        stateIndex = 0;
                        break;
                    
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (state.finishDelay > 0f) {
                    await UniTask.Delay(TimeSpan.FromSeconds(state.finishDelay), cancellationToken: cancellationToken)
                        .SuppressCancellationThrow();    
                }
            }
        }

        private async UniTask Print(float charPrintPeriod, CancellationToken cancellationToken) {
            int length = _chars.Length;
            
            for (int i = 0; i < length && !cancellationToken.IsCancellationRequested; i++) {
                _text.SetText(_chars, 0, i + 1);

                if (charPrintPeriod > 0f) {
                    await UniTask.Delay(TimeSpan.FromSeconds(charPrintPeriod), cancellationToken: cancellationToken)
                        .SuppressCancellationThrow();   
                }
            }
        }

#if UNITY_EDITOR
        private void Reset() {
            _text = GetComponent<TMP_Text>();
        }

        private void OnValidate() {
            if (_animatedText != null) _chars = _animatedText.ToCharArray();
        }
#endif
    }
    
}