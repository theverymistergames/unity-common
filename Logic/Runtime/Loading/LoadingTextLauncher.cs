using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Lists;
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

            for (int i = 0; i < preset.groups.Length; i++) {
                var group = preset.groups[i];
                if (group.variants?.Length <= 0) continue;
                
                var variant = group.variants.GetRandom();
                list.Add(new DialogueElement { branchId = preset.branchId, roleId = preset.roleId, key = variant });
            }
            
            return list;
        }

        private sealed class Formatter : ILocalizationFormatter, IDisposable {

            private readonly Dictionary<LocalizationKey, LocalizationKey> _singleFormatsMap;
            private readonly Dictionary<LocalizationKey, (LocalizationKey, LocalizationKey)> _doubleFormatsMap;
            private readonly Dictionary<LocalizationKey, List<LocalizationKey>> _arrayFormatsMap;
            private readonly Dictionary<LocalizationKey, LocalizationKey[]> _localizedVariablesMap;
            private readonly Dictionary<LocalizationKey, string[]> _stringVariablesMap;
            
            public Formatter(LoadingTextPreset preset) {
                _singleFormatsMap = DictionaryPool<LocalizationKey, LocalizationKey>.Get();
                _doubleFormatsMap = DictionaryPool<LocalizationKey, (LocalizationKey, LocalizationKey)>.Get();
                _arrayFormatsMap = DictionaryPool<LocalizationKey, List<LocalizationKey>>.Get();
                _localizedVariablesMap = DictionaryPool<LocalizationKey, LocalizationKey[]>.Get();
                _stringVariablesMap = DictionaryPool<LocalizationKey, string[]>.Get();
                
                for (int i = 0; i < preset.groups.Length; i++) {
                    ref var group = ref preset.groups[i];
                    int variantCount = group.variants?.Length ?? 0;
                    int variablesCount = group.variables?.Length ?? 0;
                    
                    if (variantCount <= 0 || variablesCount <= 0) continue;

                    if (variablesCount == 1) {
                        for (int j = 0; j < variantCount; j++) {
                            _singleFormatsMap[group.variants![j]] = group.variables![0];
                        }
                        continue;
                    }
                    
                    if (variablesCount == 2) {
                        for (int j = 0; j < variantCount; j++) {
                            _doubleFormatsMap[group.variants![j]] = (group.variables![0], group.variables[1]);
                        }
                        continue;
                    }
                    
                    var formatArray = ListPool<LocalizationKey>.Get();
                    formatArray.Capacity = variantCount;
                    
                    for (int j = 0; j < variablesCount; j++) {
                        formatArray.Add(group.variables![j]);
                    }

                    for (int j = 0; j < variantCount; j++) {
                        _arrayFormatsMap[group.variants![j]] = formatArray;
                    }
                }
                
                for (int i = 0; i < preset.stringVariables.Length; i++) {
                    ref var variable = ref preset.stringVariables[i];
                    
                    _stringVariablesMap[variable.key] = variable.values;
                }
                
                for (int i = 0; i < preset.localizedVariables.Length; i++) {
                    ref var variable = ref preset.localizedVariables[i];
                    _localizedVariablesMap[variable.key] = variable.values;
                }
            }

            public void Dispose() {
                foreach (var list in _arrayFormatsMap.Values) {
                    ListPool<LocalizationKey>.Release(list);
                }
                
                DictionaryPool<LocalizationKey, LocalizationKey>.Release(_singleFormatsMap);
                DictionaryPool<LocalizationKey, (LocalizationKey, LocalizationKey)>.Release(_doubleFormatsMap);
                DictionaryPool<LocalizationKey, List<LocalizationKey>>.Release(_arrayFormatsMap);
                DictionaryPool<LocalizationKey, LocalizationKey[]>.Release(_localizedVariablesMap);
                DictionaryPool<LocalizationKey, string[]>.Release(_stringVariablesMap);
            }

            public void Format(LocalizationKey key, Locale locale, ref string value) {
                if (_singleFormatsMap.TryGetValue(key, out var format)) {
                    value = string.Format(value, ResolveFormatKey(format));
                    return;
                }
                
                if (_doubleFormatsMap.TryGetValue(key, out var formatPair)) {
                    value = string.Format(value, ResolveFormatKey(formatPair.Item1), ResolveFormatKey(formatPair.Item2));
                    return;
                }

                if (_arrayFormatsMap.TryGetValue(key, out var formatArray)) {
                    object[] dest = new object[formatArray.Count];
                    ResolveFormatKeyArray(formatArray, dest);
                    value = string.Format(value, dest);
                }
            }

            private string ResolveFormatKey(LocalizationKey key) {
                if (_localizedVariablesMap.TryGetValue(key, out var variants)) {
                    return variants.GetRandom().GetValue();
                }
                
                if (_stringVariablesMap.TryGetValue(key, out string[] strings)) {
                    return strings.GetRandom();
                }

                return null;
            }
            
            private void ResolveFormatKeyArray(IReadOnlyList<LocalizationKey> formatArray, object[] dest) {
                for (int i = 0; i < formatArray.Count; i++) {
                    dest[i] = ResolveFormatKey(formatArray[i]);
                }
            }
        }
    }
    
}