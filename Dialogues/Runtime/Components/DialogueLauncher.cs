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

        private CancellationTokenSource _cts;
        private float _skipRequestedTime = -1f;

        private void Awake() {
            if (_launchMode == LaunchMode.OnAwake) LaunchDialogue(_dialogueReference.AssetGUID);
        }

        private void OnDestroy() {
            if (_launchMode == LaunchMode.OnAwake) StopDialogue();
        }

        private void OnEnable() {
            if (_skipInput.Get() is { } inputAction) inputAction.performed += OnSkipInput;
            
            if (_launchMode == LaunchMode.OnEnable) LaunchDialogue(_dialogueReference.AssetGUID);
        }

        private void OnDisable() {
            if (_skipInput.Get() is { } inputAction) inputAction.performed -= OnSkipInput;

            if (_launchMode == LaunchMode.OnEnable) StopDialogue();
        }

        private void OnSkipInput(InputAction.CallbackContext obj) {
            if (!IsRunning) return;

            _skipRequestedTime = Time.realtimeSinceStartup;
            Services.Get<IDialogueService>()?.CancelCurrentElementPrinting(DialogueCancelMode.PrintToEnd);
        }

        public void LaunchDialogue(string guid) {
            LaunchDialogueAsync(guid, default).Forget();
        }

        [Button(mode: ButtonAttribute.Mode.Runtime)]
        public void PauseDialogue() {
            if (!IsRunning) return;
            
            _skipRequestedTime = -1f;
            IsPaused = true;
        }
        
        [Button(mode: ButtonAttribute.Mode.Runtime)]
        public void ResumeDialogue() {
            if (!IsRunning) return;
            
            _skipRequestedTime = -1f;
            IsPaused = false;
        }
        
        [Button(mode: ButtonAttribute.Mode.Runtime)]
        public void StopDialogue() {
            AsyncExt.DisposeCts(ref _cts);

            IsRunning = false;
            IsPaused = false;
            _skipRequestedTime = -1f;
            
            Services.Get<IDialogueService>()?.UnloadDialogue(_dialogueReference.AssetGUID);
        }

        public async UniTask LaunchDialogueAsync(string guid, CancellationToken cancellationToken) {
            AsyncExt.RecreateCts(ref _cts);
            cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, cancellationToken).Token;

            IsRunning = true;
            IsPaused = false;
            _skipRequestedTime = -1f;
            
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
                
                if (Time.realtimeSinceStartup - _skipRequestedTime < _skipDuration && elementDelay > 0f) {
                    await UniTask.Delay(TimeSpan.FromSeconds(elementDelay), cancellationToken: cancellationToken)
                        .SuppressCancellationThrow();
                    if (cancellationToken.IsCancellationRequested) break;
                }
                
                await service.AwaitDialogueEvents(branch, DialogueEvent.DialogueElementStart, cancellationToken);
                if (cancellationToken.IsCancellationRequested) break;
                
                if (element.key.IsNotNull()) {
                    int roleIndex = Mathf.Max(0, table.Roles.TryFindIndex(element.roleId, (k0, k1) => k0 == k1));
                    bool instant = Time.realtimeSinceStartup - _skipRequestedTime < _skipDuration;
                    
                    await service.PrintElementAsync(element.key, roleIndex, instant, cancellationToken);
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