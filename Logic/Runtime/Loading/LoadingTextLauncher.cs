using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Localization;
using MisterGames.Common.Service;
using MisterGames.Dialogues.Components;
using MisterGames.Dialogues.Storage;
using UnityEngine;
using UnityEngine.Pool;

namespace MisterGames.Logic.Loading {
    
    public sealed class LoadingTextLauncher : MonoBehaviour {
        
        [SerializeField] private DialogueLauncher _dialogueLauncher;

        public async UniTask PrintLoadingText(LoadingTextPreset preset, CancellationToken cancellationToken) {
            var table = CreateDialogueTable(preset);
            var formatter = CreateFormatter(preset);

            if (Services.TryGet(out ILocalizationService localizationService)) {
                localizationService.RegisterFormatter(formatter);
            }
            
            await _dialogueLauncher.LaunchDialogueAsync(table, cancellationToken);

            localizationService?.UnregisterFormatter(formatter);
            
            table.Dispose();
            formatter.Dispose();
        }

        private static DialogueTable CreateDialogueTable(LoadingTextPreset preset) {
            return new DialogueTable(
                preset.dialogueId,
                roles: new[] { preset.roleId },
                branches: new[] { preset.branchId },
                elements: CreateDialogueElements(preset) 
            );
        }

        private static Formatter CreateFormatter(LoadingTextPreset preset) {
            return new Formatter(preset);
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
            
            public Formatter(LoadingTextPreset preset) {
                _argsMap = DictionaryPool<LocalizationKey, IArgumentResolver>.Get();
                
                for (int i = 0; i < preset.args.Length; i++) {
                    ref var arg = ref preset.args[i];
                    _argsMap[arg.key] = arg.resolver;
                }
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