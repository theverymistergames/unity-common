using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Async;
using MisterGames.Common.Localization;
using MisterGames.Common.Maths;
using MisterGames.Common.Service;
using MisterGames.Dialogues.Components;
using UnityEngine;
using UnityEngine.Pool;

namespace MisterGames.Logic.UI {
    
    public sealed class PlainTextLauncher : MonoBehaviour {
        
        [Header("Print")]
        [SerializeField] private DialoguePrinter _dialoguePrinter;
        [SerializeField] [Min(0f)] private float _printElementDelayDefault = 0.1f;
        [SerializeField] [Min(0f)] private float _printElementDelayFast = 0.05f;
        [SerializeField] [Min(-1f)] private float _skipSymbolDelay = -1f;
        [SerializeField] private bool _useUnscaledTime = true;

        [Flags]
        public enum PrintOptions {
            None = 0,
            FastPrint = 1,
        }

        private CancellationTokenSource _skipCts;
        private byte _printId;

        public async UniTask PrintPlainText(PlainTextPreset preset, PrintOptions options, CancellationToken cancellationToken) {
            byte id = _printId.IncrementUncheckedRef();
            var formatter = new Formatter(preset);
            
            if (Services.TryGet(out ILocalizationService localizationService)) {
                localizationService.RegisterFormatter(formatter);
            }
            
            _dialoguePrinter.ClearAllText();
            
            float printElementDelay = _printElementDelayDefault;

            if ((options & PrintOptions.FastPrint) != 0) {
                printElementDelay = _printElementDelayFast;
            }
            
            await PrintMainElements(id, preset, printElementDelay, cancellationToken);

            if (cancellationToken.IsCancellationRequested || id != _printId) {
                return;
            }
            
            _printId.IncrementUncheckedRef();
            localizationService?.UnregisterFormatter(formatter);
            formatter.Dispose();
        }

        public void ClearAllText() {
            _dialoguePrinter.ClearAllText();
        }

        public void NotifySkip() {
            AsyncExt.DisposeCts(ref _skipCts);
            _dialoguePrinter.FinishLastPrinting(_skipSymbolDelay);
        }
        
        private async UniTask PrintMainElements(byte id, PlainTextPreset preset, float printDelay, CancellationToken cancellationToken) {
            var buffer = ListPool<LocalizationKey>.Get();
            
            AsyncExt.RecreateCts(ref _skipCts);
            var skipToken = CancellationTokenSource.CreateLinkedTokenSource(_skipCts.Token, cancellationToken).Token;
            
            for (int i = 0; i < preset.blocks?.Length; i++) {
                buffer.Clear();
                preset.blocks[i].GetValues(buffer);
                
                for (int j = 0; j < buffer.Count && !cancellationToken.IsCancellationRequested && id == _printId; j++) {
                    await _dialoguePrinter.PrintElement(buffer[j], 0, cancellationToken);
                    if (cancellationToken.IsCancellationRequested || id != _printId) break;
                
                    await UniTask.Delay(TimeSpan.FromSeconds(printDelay), ignoreTimeScale: _useUnscaledTime, cancellationToken: cancellationToken)
                        .SuppressCancellationThrow();
                }

                if (cancellationToken.IsCancellationRequested || id != _printId) break;
                
                if (preset.waitSkipInputAfterBlock) {
                    if (_skipCts == null) {
                        AsyncExt.RecreateCts(ref _skipCts);
                        skipToken = CancellationTokenSource.CreateLinkedTokenSource(_skipCts.Token, cancellationToken).Token;
                    }

                    await WaitSkipInput(skipToken, cancellationToken);
                }
            }

            ListPool<LocalizationKey>.Release(buffer);
        }
        
        private static async UniTask WaitSkipInput(CancellationToken skipToken, CancellationToken cancellationToken) {
            while (!cancellationToken.IsCancellationRequested && !skipToken.IsCancellationRequested) {
                await UniTask.Yield();
            }
        }
        
        private sealed class Formatter : ILocalizationFormatter, IDisposable {

            private readonly Dictionary<LocalizationKey, IArgumentResolver> _argsMap;
            
            public Formatter(PlainTextPreset preset) {
                _argsMap = DictionaryPool<LocalizationKey, IArgumentResolver>.Get();
                
                for (int i = 0; i < preset.args.Length; i++) {
                    ref var arg = ref preset.args[i];
                    for (int j = 0; j < arg.keys.Length; j++) {
                        _argsMap[arg.keys[j]] = arg.resolver;
                    }
                }
            }

            public void Dispose() {
                DictionaryPool<LocalizationKey, IArgumentResolver>.Release(_argsMap);
            }

            void ILocalizationFormatter.Format(LocalizationKey key, Locale locale, ref string value) {
                if (_argsMap.TryGetValue(key, out var resolver)) {
                    resolver.Resolve(key, locale, ref value);
                }
            }
        }
    }
    
}