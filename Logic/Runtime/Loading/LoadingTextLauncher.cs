using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Localization;
using MisterGames.Common.Maths;
using MisterGames.Common.Service;
using MisterGames.Dialogues.Components;
using MisterGames.Dialogues.Storage;
using MisterGames.Scenes.Core;
using UnityEngine;
using UnityEngine.Pool;

namespace MisterGames.Logic.Loading {
    
    public sealed class LoadingTextLauncher : MonoBehaviour, IArgumentResolver {
        
        [SerializeField] private DialogueLauncher _dialogueLauncher;
        [SerializeField] [Min(0f)] private float _loadingProgressSmoothing = 2f;
        [SerializeField] [Min(0f)] private float _finishDelay = 0.5f;

        public float LoadingProgress { get; private set; }

        private LoadingTextPreset _preset;
        private byte _loadingId;

        public async UniTask PrintLoadingText(IActor context, LoadingTextPreset preset, IActorAction loadAction, CancellationToken cancellationToken) {
            _preset = preset;
            
            byte id = _loadingId.IncrementUncheckedRef();
            var table = CreateDialogueTable(preset);
            var formatter = CreateFormatter(preset);
            var cs = new UniTaskCompletionSource();
            
            if (Services.TryGet(out ILocalizationService localizationService)) {
                localizationService.RegisterFormatter(formatter);
            }
            
            _dialogueLauncher.OnDialogueElementPrinted += OnDialogueElementPrinted;
            
            await _dialogueLauncher.LaunchDialogueAsync(table, cancellationToken);

            _dialogueLauncher.OnDialogueElementPrinted -= OnDialogueElementPrinted;

            await cs.Task;
            
            await UniTask.Delay(TimeSpan.FromSeconds(_finishDelay), cancellationToken: cancellationToken)
                .SuppressCancellationThrow();
            
            
            
            _loadingId.IncrementUncheckedRef();
            localizationService?.UnregisterFormatter(formatter);
            table.Dispose();
            formatter.Dispose();
            
            return;

            void OnDialogueElementPrinted(LocalizationKey key, int roleIndex) {
                if (preset.loadingProgressKey != key) return;
                
                NotifyLoadingProgress(id, key, cancellationToken).Forget();
                LaunchLoading(context, loadAction, cs, cancellationToken).Forget();
            }
        }

        private static async UniTask LaunchLoading(IActor context, IActorAction loadAction, UniTaskCompletionSource cs, CancellationToken cancellationToken) {
            if (loadAction != null) await loadAction.Apply(context, cancellationToken);
            cs.TrySetResult();
        }
        
        private async UniTask NotifyLoadingProgress(byte id, LocalizationKey key, CancellationToken cancellationToken) {
            LoadingProgress = 0f;
            
            while (!cancellationToken.IsCancellationRequested && id == _loadingId) {
                float oldProgress = LoadingProgress;
                float targetProgress = SceneLoader.GetLoadingProgress();
                
                LoadingProgress = LoadingProgress.SmoothExpNonZero(targetProgress, _loadingProgressSmoothing, Time.deltaTime);
                
                if (!oldProgress.IsNearlyEqual(LoadingProgress)) {
                    _dialogueLauncher.ReprintLast(key);    
                }
                
                await UniTask.Yield();
            }
        }

        void IArgumentResolver.Resolve(Locale locale, ref string value) {
            if (_preset.loadProgressCharsCount > 0) {
                var sb = new StringBuilder();
                int dec = Mathf.FloorToInt(LoadingProgress * _preset.loadProgressCharsCount);
                for (int i = 0; i < _preset.loadProgressCharsCount; i++) {
                    sb.Append(i <= dec ? _preset.loadProgressFullChar : _preset.loadProgressEmptyChar);
                }
                
                value = string.Format(value, $"{sb} {LoadingProgress * 100f:00}%");
                return;
            }
            
            value = string.Format(value, $"{LoadingProgress * 100f:00}%");
        }

        private static DialogueTable CreateDialogueTable(LoadingTextPreset preset) {
            return new DialogueTable(
                preset.dialogueId,
                roles: new[] { preset.roleId },
                branches: new[] { preset.branchId },
                elements: CreateDialogueElements(preset) 
            );
        }

        private Formatter CreateFormatter(LoadingTextPreset preset) {
            return new Formatter(preset, loadingProgressResolver: this);
        }
        
        private static IReadOnlyList<DialogueElement> CreateDialogueElements(LoadingTextPreset preset) {
            var list = new List<DialogueElement>();
            var buffer = ListPool<LocalizationKey>.Get();

            for (int i = 0; i < preset.blocks.Length; i++) {
                preset.blocks[i].GetValues(buffer);
            }

            for (int i = 0; i < buffer.Count; i++) {
                list.Add(new DialogueElement { branchId = preset.branchId, roleId = preset.roleId, key = buffer[i] });
            }
            
            ListPool<LocalizationKey>.Release(buffer);
            
            return list;
        }

        private sealed class Formatter : ILocalizationFormatter, IDisposable {

            private readonly Dictionary<LocalizationKey, IArgumentResolver> _argsMap;
            
            public Formatter(LoadingTextPreset preset, IArgumentResolver loadingProgressResolver) {
                _argsMap = DictionaryPool<LocalizationKey, IArgumentResolver>.Get();
                
                for (int i = 0; i < preset.args.Length; i++) {
                    ref var arg = ref preset.args[i];
                    _argsMap[arg.key] = arg.resolver;
                }
                
                _argsMap[preset.loadingProgressKey] = loadingProgressResolver;
            }

            public void Dispose() {
                DictionaryPool<LocalizationKey, IArgumentResolver>.Release(_argsMap);
            }

            public void Format(LocalizationKey key, Locale locale, ref string value) {
                if (_argsMap.TryGetValue(key, out var resolver)) {
                    resolver.Resolve(locale, ref value);
                }
            }
        }
    }
    
}