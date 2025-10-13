using System;
using System.Buffers;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Async;
using MisterGames.Common.Data;
using MisterGames.Common.Lists;
using MisterGames.Common.Localization;
using MisterGames.Common.Strings;
using MisterGames.Dialogues.Storage;
using Unity.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace MisterGames.Dialogues.Core {
    
    public sealed class DialogueService : IDialogueService, IDisposable {
        
        private const bool EnableLogs = true;
        private static readonly string LogPrefix = nameof(DialogueService).FormatColorOnlyForEditor(Color.white);

        public event IDialogueService.DialogueStart OnDialogueStart = delegate { };
        public event IDialogueService.DialogueStart OnDialogueStop = delegate { };
        public event IDialogueService.GroupStart OnDialogueBranchStart = delegate { };
        public event IDialogueService.GroupStart OnDialogueRoleStart = delegate { };
        public event IDialogueService.ElementStart OnDialogueElementStart = delegate { };
        public event IDialogueService.DialogueGenericEvent OnAnyDialogueEvent = delegate { };

        private readonly Dictionary<int, IDialogueTable> _tableMap = new();
        private readonly Dictionary<int, AsyncOperationHandle<DialogueTableStorage>> _tableStorageHandlesMap = new();
        private readonly Dictionary<LocalizationKey, DialogueElement> _startedDialogues = new();
        
        private readonly MultiValueDictionary<(LocalizationKey, DialogueEvent), Func<UniTask>> _dialogueEvents = new();
        private readonly HashSet<IDialoguePrinter> _printers = new();
        
        public void Initialize() {
            
        }

        public void Dispose() {
            foreach (var table in _tableMap.Values) {
                if (table is IDisposable disposable) disposable.Dispose();
            }
            
            foreach (var handle in _tableStorageHandlesMap.Values) {
                Addressables.Release(handle);
            }
            
            _tableMap.Clear();
            _tableStorageHandlesMap.Clear();
            _startedDialogues.Clear();
        }

        public void StartDialogue(LocalizationKey dialogue) {
            if (!_startedDialogues.TryAdd(dialogue, default)) return;
            
            OnDialogueStart.Invoke(dialogue);
            OnAnyDialogueEvent.Invoke(dialogue, DialogueEvent.DialogueStart);
        }

        public void StopDialogue(LocalizationKey dialogue) {
            if (!_startedDialogues.Remove(dialogue)) return;
            
            OnDialogueStop.Invoke(dialogue);
            OnAnyDialogueEvent.Invoke(dialogue, DialogueEvent.DialogueStop);
        }

        public void StartDialogueElement(LocalizationKey dialogue, DialogueElement element) {
            if (_startedDialogues.TryGetValue(dialogue, out var currentElement)) {
                _startedDialogues[dialogue] = element;

                if (currentElement.branchId != element.branchId) {
                    OnDialogueBranchStart.Invoke(dialogue, element.branchId, element.roleId);
                    OnAnyDialogueEvent.Invoke(element.branchId, DialogueEvent.DialogueBranchStart);
                }
                
                if (currentElement.roleId != element.roleId) {
                    OnDialogueRoleStart.Invoke(dialogue, element.branchId, element.roleId);
                    OnAnyDialogueEvent.Invoke(element.roleId, DialogueEvent.DialogueRoleStart);
                }
            }
            else {
                _startedDialogues[dialogue] = element;
                
                OnDialogueStart.Invoke(dialogue);
                OnAnyDialogueEvent.Invoke(dialogue, DialogueEvent.DialogueStart);
                
                OnDialogueBranchStart.Invoke(dialogue, element.branchId, element.roleId);
                OnAnyDialogueEvent.Invoke(element.branchId, DialogueEvent.DialogueBranchStart);
                
                OnDialogueRoleStart.Invoke(dialogue, element.branchId, element.roleId);
                OnAnyDialogueEvent.Invoke(element.roleId, DialogueEvent.DialogueRoleStart);
            }
            
            OnDialogueElementStart.Invoke(dialogue, element);
            OnAnyDialogueEvent.Invoke(element.key, DialogueEvent.DialogueElementStart);
        }

        public LocalizationKey GetBranch(LocalizationKey dialogue) {
            return _startedDialogues.GetValueOrDefault(dialogue).branchId;
        }

        public LocalizationKey GetRole(LocalizationKey dialogue) {
            return _startedDialogues.GetValueOrDefault(dialogue).roleId;
        }

        public DialogueElement GetElement(LocalizationKey dialogue) {
            return _startedDialogues.GetValueOrDefault(dialogue);
        }

        public IDialogueTable LoadDialogue(string guid) {
            if (string.IsNullOrWhiteSpace(guid)) return null;
            
            int hash = Animator.StringToHash(guid);
            
            if (_tableMap.TryGetValue(hash, out var table)) { 
                return table;
            }
            
            var handle = Addressables.LoadAssetAsync<DialogueTableStorage>(guid);
            _tableStorageHandlesMap[hash] = handle;
            
            handle.WaitForCompletion();

            switch (handle.Status) {
                case AsyncOperationStatus.Succeeded:
                    var storage = handle.Result;
                    table = new DialogueTable(storage);
            
                    _tableMap[hash] = table;
                    
                    return table;
                
                case AsyncOperationStatus.None:
                case AsyncOperationStatus.Failed:
                    _tableStorageHandlesMap.Remove(hash);
                    LogError($"table with guid {guid} is not found.");
                    return null;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        public async UniTask<IDialogueTable> LoadDialogueAsync(string guid) {
            if (string.IsNullOrWhiteSpace(guid)) return null;
            
            int hash = Animator.StringToHash(guid);
            
            if (_tableMap.TryGetValue(hash, out var table)) { 
                return table;
            }
            
            var handle = Addressables.LoadAssetAsync<DialogueTableStorage>(guid);
            _tableStorageHandlesMap[hash] = handle;
            
            await handle;

            switch (handle.Status) {
                case AsyncOperationStatus.Succeeded:
                    var storage = handle.Result;
                    table = new DialogueTable(storage);
            
                    _tableMap[hash] = table;
                    
                    return table;
                
                case AsyncOperationStatus.None:
                case AsyncOperationStatus.Failed:
                    _tableStorageHandlesMap.Remove(hash);
                    LogError($"table with guid {guid} is not found.");
                    return null;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void UnloadDialogue(string guid) {
            if (string.IsNullOrWhiteSpace(guid)) return;
            
            int hash = Animator.StringToHash(guid);
            
            if (_tableMap.Remove(hash, out var table) && table is IDisposable disposable) {
                disposable.Dispose();
            }

            if (_tableStorageHandlesMap.Remove(hash, out var handle)) {
                Addressables.Release(handle);
            }
        }

        public void AddDialogueEvent(LocalizationKey key, DialogueEvent eventType, Func<UniTask> action) {
            _dialogueEvents.AddValue((key, eventType), action);
        }

        public void RemoveDialogueEvent(LocalizationKey key, DialogueEvent eventType, Func<UniTask> action) {
            _dialogueEvents.RemoveValue((key, eventType), action);
        }

        public async UniTask AwaitDialogueEvents(LocalizationKey key, DialogueEvent eventType, CancellationToken cancellationToken) {
            var id = (key, eventType);
            int count = _dialogueEvents.GetCount(id);
            
            var tasks = ArrayPool<UniTask>.Shared.Rent(count);
            tasks.ResetArrayElements();
            
            for (int i = 0; i < count; i++) {
                tasks[i] = _dialogueEvents.GetValueAt(id, i)?.Invoke() ?? UniTask.CompletedTask;
            }
            
            await UniTask.WhenAll(tasks);
            
            ArrayPool<UniTask>.Shared.Return(tasks);
        }

        public void RegisterPrinter(IDialoguePrinter printer) {
            _printers.Add(printer);
        }

        public void UnregisterPrinter(IDialoguePrinter printer) {
            _printers.Remove(printer);
        }
        
        public async UniTask PrintElementAsync(LocalizationKey key, int roleIndex, bool instant, CancellationToken cancellationToken) {
            var tasks = ArrayPool<UniTask>.Shared.Rent(_printers.Count);
            tasks.ResetArrayElements();
            int count = 0;
            
            foreach (var dialoguePrinter in _printers) {
                tasks[count++] = dialoguePrinter.PrintElement(key, roleIndex, instant, cancellationToken);
            }

            await UniTask.WhenAll(tasks);
            
            tasks.ResetArrayElements();
            ArrayPool<UniTask>.Shared.Return(tasks);
        }

        public void CancelCurrentElementPrinting(DialogueCancelMode mode) {
            foreach (var dialoguePrinter in _printers) {
                dialoguePrinter.CancelCurrentElementPrinting(mode);
            }
        }

        public void ClearAllPrinters() {
            foreach (var dialoguePrinter in _printers) {
                dialoguePrinter.ClearAllText();
            }
        }

        private static void LogInfo(string message) {
            if (EnableLogs) Debug.Log($"{LogPrefix}: f {Time.frameCount}, {message}");
        }
        
        private static void LogWarning(string message) {
            if (EnableLogs) Debug.LogWarning($"{LogPrefix}: f {Time.frameCount}, {message}");
        }
        
        private static void LogError(string message) {
            if (EnableLogs) Debug.LogError($"{LogPrefix}: f {Time.frameCount}, {message}");
        }
    }
    
}