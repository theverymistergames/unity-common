using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Async;
using MisterGames.Common.Attributes;
using MisterGames.Common.Lists;
using MisterGames.Common.Localization;
using MisterGames.Common.Service;
using MisterGames.Dialogues.Core;
using MisterGames.Input.Actions;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

namespace MisterGames.Dialogues.Components {
    
    public sealed class DialogueLauncher : MonoBehaviour {

        [SerializeField] private DialogueReference _dialogueReference;
        [SerializeField] private LaunchMode _launchMode = LaunchMode.OnEnable;
        
        [Header("Skip")]
        [SerializeField] private InputActionRef _skipInput;
        [SerializeField] [Min(0f)] private float _skipDuration = 0.1f;
        
        [Header("Timings")]
        [SerializeField] [Min(0f)] private float _minReplicaDelaySameRole = 0.4f;
        [SerializeField] [Min(0f)] private float _maxReplicaDelaySameRole = 0.7f;
        [SerializeField] [Min(0f)] private float _minReplicaDelayChangedRole = 0.6f;
        [SerializeField] [Min(0f)] private float _maxReplicaDelayChangedRole = 1f;
        
        private enum LaunchMode {
            OnAwake,
            OnEnable,
            Manual,
        }

        public bool IsPaused { get; private set; }
        public bool IsRunning { get; private set; }

        private CancellationTokenSource _destroyCts;
        private CancellationTokenSource _enableCts;
        private CancellationTokenSource _dialogueLaunchCts;
        private CancellationTokenSource _skipCts;
        
        private void Awake() {
            AsyncExt.RecreateCts(ref _destroyCts);

            if (_launchMode == LaunchMode.OnAwake) {
                LaunchDialogueAsync(_dialogueReference.AssetGUID, _destroyCts.Token).Forget();
            }
        }

        private void OnDestroy() {
            AsyncExt.DisposeCts(ref _destroyCts);

            if (_launchMode == LaunchMode.OnAwake) {
                StopDialogue();
            }
        }

        private void OnEnable() {
            AsyncExt.RecreateCts(ref _enableCts);
            
            if (_skipInput.Get() is { } inputAction) inputAction.performed += OnSkipInput;
            
            if (_launchMode == LaunchMode.OnEnable) {
                LaunchDialogueAsync(_dialogueReference.AssetGUID, _enableCts.Token).Forget();
            }
        }

        private void OnDisable() {
            AsyncExt.DisposeCts(ref _enableCts);
            
            if (_skipInput.Get() is { } inputAction) inputAction.performed -= OnSkipInput;

            if (_launchMode == LaunchMode.OnEnable) {
                StopDialogue();
            }
        }

        private void OnSkipInput(InputAction.CallbackContext obj) {
            if (!IsRunning) return;
            
            AsyncExt.DisposeCts(ref _skipCts);
            Services.Get<IDialogueService>()?.CancelCurrentElementPrinting(DialogueCancelMode.PrintToEnd);
        }

        [Button(mode: ButtonAttribute.Mode.Runtime)]
        public void PauseDialogue() {
            if (!IsRunning) return;
            
            IsPaused = true;
        }
        
        [Button(mode: ButtonAttribute.Mode.Runtime)]
        public void ResumeDialogue() {
            if (!IsRunning) return;
            
            IsPaused = false;
        }
        
        [Button(mode: ButtonAttribute.Mode.Runtime)]
        public void StopDialogue() {
            AsyncExt.DisposeCts(ref _dialogueLaunchCts);
            AsyncExt.DisposeCts(ref _skipCts);

            IsRunning = false;
            IsPaused = false;
            
            Services.Get<IDialogueService>()?.UnloadDialogue(_dialogueReference.AssetGUID);
        }

        public async UniTask LaunchDialogueAsync(string guid, CancellationToken cancellationToken) {
            AsyncExt.RecreateCts(ref _dialogueLaunchCts);
            AsyncExt.RecreateCts(ref _skipCts);
            
            cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(_dialogueLaunchCts.Token, cancellationToken).Token;

            IsRunning = true;
            IsPaused = false;
            
            var service = Services.Get<IDialogueService>();
            
            service.ClearAllPrinters();
            
            var table = await service.LoadDialogueAsync(guid);
            
            if (cancellationToken.IsCancellationRequested) {
                StopDialogue();
                return;
            }

            service.StartDialogue(table.DialogueId);

            await service.AwaitDialogueEvents(table.DialogueId, DialogueEvent.DialogueStart, cancellationToken);
            if (cancellationToken.IsCancellationRequested) return;
            
            var elements = table.Elements;
            LocalizationKey lastBranch = default;
            LocalizationKey lastRole = default;
            
            for (int i = 0; i < elements.Count && !cancellationToken.IsCancellationRequested; i++) {
                var element = elements[i];
                
                service.StartDialogueElement(table.DialogueId, element);
                var branch = element.branchId;
                var role = element.roleId;

                if (branch != lastBranch) {
                    lastBranch = branch;
                    await service.AwaitDialogueEvents(branch, DialogueEvent.DialogueBranchStart, cancellationToken);
                    if (cancellationToken.IsCancellationRequested) break;
                }

                float elementDelay;
                
                if (role != lastRole) {
                    lastRole = role;
                    await service.AwaitDialogueEvents(branch, DialogueEvent.DialogueRoleStart, cancellationToken);
                    if (cancellationToken.IsCancellationRequested) break;
                    elementDelay = Random.Range(_minReplicaDelayChangedRole, _maxReplicaDelayChangedRole);
                }
                else {
                    elementDelay = Random.Range(_minReplicaDelaySameRole, _maxReplicaDelaySameRole);
                }
                
                var skipToken = CancellationToken.None;
                
                if (_skipCts == null) {
                    AsyncExt.RecreateCts(ref _skipCts);
                    skipToken = CancellationTokenSource.CreateLinkedTokenSource(_skipCts.Token, cancellationToken).Token;
                }
                
                if (elementDelay > 0f) {
                    await UniTask.Delay(TimeSpan.FromSeconds(elementDelay), cancellationToken: skipToken)
                        .SuppressCancellationThrow();
                    if (cancellationToken.IsCancellationRequested) break;
                }
                
                while (!cancellationToken.IsCancellationRequested && IsPaused) {
                    await UniTask.Yield();
                }
                if (cancellationToken.IsCancellationRequested) break;

                await service.AwaitDialogueEvents(branch, DialogueEvent.DialogueElementStart, cancellationToken);
                if (cancellationToken.IsCancellationRequested) break;
                
                if (element.key.IsNotNull()) {
                    int roleIndex = Mathf.Max(0, table.Roles.TryFindIndex(element.roleId, (k0, k1) => k0 == k1));
                    
                    await service.PrintElementAsync(element.key, roleIndex, instant: skipToken.IsCancellationRequested, cancellationToken);
                    if (cancellationToken.IsCancellationRequested) break;
                }
                
                while (!cancellationToken.IsCancellationRequested && IsPaused) {
                    await UniTask.Yield();
                }
            }

            if (cancellationToken.IsCancellationRequested) return;
            
            service.StopDialogue(table.DialogueId);
            service.UnloadDialogue(_dialogueReference.AssetGUID);
            
            await service.AwaitDialogueEvents(table.DialogueId, DialogueEvent.DialogueStart, cancellationToken);
            
            if (cancellationToken.IsCancellationRequested) return;

            IsRunning = false;
            IsPaused = false;
        }
    }
    
}