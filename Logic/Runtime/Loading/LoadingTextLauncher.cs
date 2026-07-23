using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Audio;
using MisterGames.Common.Inputs;
using MisterGames.Common.Labels;
using MisterGames.Common.Localization;
using MisterGames.Common.Maths;
using MisterGames.Common.Service;
using MisterGames.Dialogues.Components;
using MisterGames.Scenes.Core;
using MisterGames.UI.Data;
using UnityEngine;
using UnityEngine.Pool;

namespace MisterGames.Logic.Loading {
    
    public sealed class LoadingTextLauncher : MonoBehaviour, IArgumentResolver {
        
        [SerializeField] private DialoguePrinter _dialoguePrinter;
        [SerializeField] private UiSfxSettings _uiSfxSettings;
        
        private LoadingTextPreset _preset;
        private float _loadingProgress;
        private byte _loadingId;
        private int _dotsCount;

        public async UniTask PrintLoadingText(IActor context, LoadingTextPreset preset, IActorAction loadAction, CancellationToken cancellationToken) {
            _preset = preset;
            
            byte id = _loadingId.IncrementUncheckedRef();
            var formatter = new Formatter(preset, specialResolver: this);
            
            if (Services.TryGet(out ILocalizationService localizationService)) {
                localizationService.RegisterFormatter(formatter);
            }
            
            _dialoguePrinter.ClearAllText();
            
            await PrintMainElements(id, preset, cancellationToken);
            
            if (cancellationToken.IsCancellationRequested || id != _loadingId) {
                return;
            }

            if (preset.showProgress) {
                await _dialoguePrinter.PrintElement(preset.loadingProgressKey, 0, cancellationToken);
                if (cancellationToken.IsCancellationRequested || id != _loadingId) return;
            
                NotifyLoadingProgress(id, preset, cancellationToken).Forget();
            }
            
            if (loadAction != null) await loadAction.Apply(context, cancellationToken);
            
            if (cancellationToken.IsCancellationRequested || id != _loadingId) {
                return;
            }
            
            await UniTask.Delay(TimeSpan.FromSeconds(preset.afterLoadDelay), delayType: DelayType.UnscaledDeltaTime, cancellationToken: cancellationToken)
                .SuppressCancellationThrow();
            if (cancellationToken.IsCancellationRequested || id != _loadingId) {
                return;
            }
            
            await PrintElementsAfterLoading(id, preset, cancellationToken);
            if (cancellationToken.IsCancellationRequested || id != _loadingId) {
                return;
            }

            if (preset.awaitInput) {
                await _dialoguePrinter.PrintElement(preset.awaitInputKey, 0, cancellationToken);
                if (cancellationToken.IsCancellationRequested || id != _loadingId) return;
                
                AnimateDots(id, preset.awaitInputKey, cancellationToken).Forget();
                
                await AwaitAnyInput(id, cancellationToken);
                if (cancellationToken.IsCancellationRequested || id != _loadingId) return;

                if (AudioPool.Main is { } audioPool) {
                    audioPool.Play(
                        audioPool.ShuffleClips(preset.awaitedInputSounds.GetData()),
                        Vector3.zero,
                        _uiSfxSettings.volume.GetRandomInRange(),
                        pitch: _uiSfxSettings.pitch.GetRandomInRange(),
                        spatialBlend: 0f,
                        mixerGroup: _uiSfxSettings.mixerGroup,
                        options: _uiSfxSettings.affectedByTimeScale ? AudioOptions.AffectedByTimeScale : AudioOptions.None,
                        cancellationToken: CancellationToken.None
                    );
                }
            }
            
            await UniTask.Delay(TimeSpan.FromSeconds(preset.finishDelay), delayType: DelayType.UnscaledDeltaTime, cancellationToken: cancellationToken)
                .SuppressCancellationThrow();
            if (cancellationToken.IsCancellationRequested || id != _loadingId) {
                return;
            }
            
            _loadingId.IncrementUncheckedRef();
            localizationService?.UnregisterFormatter(formatter);
            formatter.Dispose();
        }

        public void ClearAllText() {
            _dialoguePrinter.ClearAllText();
        }
        
        private async UniTask PrintMainElements(byte id, LoadingTextPreset preset, CancellationToken cancellationToken) {
            var buffer = ListPool<LocalizationKey>.Get();
            
            for (int i = 0; i < preset.blocks?.Length; i++) {
                preset.blocks[i].GetValues(buffer);
            }

            for (int i = 0; i < buffer.Count && !cancellationToken.IsCancellationRequested && id == _loadingId; i++) {
                await _dialoguePrinter.PrintElement(buffer[i], 0, cancellationToken);
                if (cancellationToken.IsCancellationRequested || id != _loadingId) break;
                
                await UniTask.Delay(TimeSpan.FromSeconds(preset.printElementDelay), delayType: DelayType.UnscaledDeltaTime, cancellationToken: cancellationToken)
                    .SuppressCancellationThrow();
            }
            
            ListPool<LocalizationKey>.Release(buffer);
        }
        
        private async UniTask PrintElementsAfterLoading(byte id, LoadingTextPreset preset, CancellationToken cancellationToken) {
            var buffer = ListPool<LocalizationKey>.Get();
            
            for (int i = 0; i < preset.afterLoading?.Length; i++) {
                preset.afterLoading[i].GetValues(buffer);
            }

            for (int i = 0; i < buffer.Count && !cancellationToken.IsCancellationRequested && id == _loadingId; i++) {
                await _dialoguePrinter.PrintElement(buffer[i], 0, cancellationToken);
            }
            
            ListPool<LocalizationKey>.Release(buffer);
        }

        private async UniTask AwaitAnyInput(byte id, CancellationToken cancellationToken) {
            while (!cancellationToken.IsCancellationRequested && 
                   Services.TryGet(out IDeviceService deviceService) && 
                   !deviceService.AnyKeyPressedThisFrame && 
                   _loadingId == id) 
            {
                await UniTask.Yield();
            }
        }

        private async UniTask AnimateDots(byte id, LocalizationKey key, CancellationToken cancellationToken) {
            _dotsCount = 0;
            
            while (!cancellationToken.IsCancellationRequested && id == _loadingId) {
                for (int i = 0; i <= _preset.dotsCount && !cancellationToken.IsCancellationRequested && id == _loadingId; i++) {
                    _dotsCount = i;
                    _dialoguePrinter.ReprintLast(key);
                    
                    await UniTask.Delay(TimeSpan.FromSeconds(_preset.dotPrintDelay), delayType: DelayType.UnscaledDeltaTime, cancellationToken: cancellationToken)
                        .SuppressCancellationThrow();
                }
                
                if (cancellationToken.IsCancellationRequested || id != _loadingId) break;
                
                await UniTask.Delay(TimeSpan.FromSeconds(_preset.dotPrintRestartDelay), delayType: DelayType.UnscaledDeltaTime, cancellationToken: cancellationToken)
                    .SuppressCancellationThrow();
                
                await UniTask.Yield();
            }
        }
        
        private async UniTask NotifyLoadingProgress(byte id, LoadingTextPreset preset, CancellationToken cancellationToken) {
            _loadingProgress = 0f;
            
            while (!cancellationToken.IsCancellationRequested && id == _loadingId) {
                float oldProgress = _loadingProgress;
                float targetProgress = SceneLoader.GetLoadingProgress();
                
                _loadingProgress = _loadingProgress.SmoothExpNonZero(targetProgress, preset.progressSmoothing, Time.unscaledDeltaTime);
                
                if (!oldProgress.IsNearlyEqual(_loadingProgress)) {
                    _dialoguePrinter.ReprintLast(preset.loadingProgressKey);    
                }
                
                await UniTask.Yield();
            }
        }

        void IArgumentResolver.Resolve(LocalizationKey key, Locale locale, ref string value) {
            if (key == _preset.loadingProgressKey) {
                if (_preset.loadProgressCharsCount > 0) {
                    var sb = new StringBuilder();
                    int dec = Mathf.FloorToInt(_loadingProgress * _preset.loadProgressCharsCount);
                    for (int i = 0; i < _preset.loadProgressCharsCount; i++) {
                        sb.Append(i <= dec ? _preset.loadProgressFullChar : _preset.loadProgressEmptyChar);
                    }
                
                    value = string.Format(value, $"{sb} {_loadingProgress * 100f:00}%");
                    return;
                }
            
                value = string.Format(value, $"{_loadingProgress * 100f:00}%");
                return;
            }

            if (key == _preset.awaitInputKey) {
                value = string.Format(value, new string(_preset.dotChar, _dotsCount));
            }
        }

        private sealed class Formatter : ILocalizationFormatter, IDisposable {

            private readonly Dictionary<LocalizationKey, IArgumentResolver> _argsMap;
            
            public Formatter(LoadingTextPreset preset, IArgumentResolver specialResolver) {
                _argsMap = DictionaryPool<LocalizationKey, IArgumentResolver>.Get();
                
                for (int i = 0; i < preset.args.Length; i++) {
                    ref var arg = ref preset.args[i];
                    _argsMap[arg.key] = arg.resolver;
                }
                
                _argsMap[preset.loadingProgressKey] = specialResolver;
                _argsMap[preset.awaitInputKey] = specialResolver;
            }

            public void Dispose() {
                DictionaryPool<LocalizationKey, IArgumentResolver>.Release(_argsMap);
            }

            public void Format(LocalizationKey key, Locale locale, ref string value) {
                if (_argsMap.TryGetValue(key, out var resolver)) {
                    resolver.Resolve(key, locale, ref value);
                }
            }
        }
    }
    
}