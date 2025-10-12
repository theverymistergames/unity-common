using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Async;
using MisterGames.Common.Localization;
using MisterGames.Common.Service;
using MisterGames.Dialogues.Core;
using MisterGames.Input.Actions;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MisterGames.Dialogues.Components {
    
    public sealed class DialogueLauncher : MonoBehaviour {

        [SerializeField] private DialogueReference _dialogueReference;
        [SerializeField] private LaunchMode _launchMode = LaunchMode.OnEnable;
        [SerializeField] private InputActionRef _skipInput;
        
        private enum LaunchMode {
            OnAwake,
            OnEnable,
            Manual,
        }

        private CancellationTokenSource _cts;
        
        private void Awake() {
            if (_launchMode == LaunchMode.OnAwake) LaunchDialogue(_dialogueReference.AssetGUID);
        }

        private void OnEnable() {
            if (_skipInput.Get() is { } inputAction) inputAction.performed += OnSkipInput;
            
            if (_launchMode == LaunchMode.OnEnable) LaunchDialogue(_dialogueReference.AssetGUID);
        }

        private void OnDisable() {
            if (_skipInput.Get() is { } inputAction) inputAction.performed -= OnSkipInput;
            
            if (_launchMode == LaunchMode.OnEnable) StopDialogue();
        }

        private static void OnSkipInput(InputAction.CallbackContext obj) {
            Services.Get<IDialogueService>()?.CancelCurrentElementPrinting(DialogueCancelMode.PrintToEnd);
        }

        public void LaunchDialogue(string guid) {
            LaunchDialogueAsync(guid, default).Forget();
        }

        public void StopDialogue() {
            AsyncExt.DisposeCts(ref _cts);
        }

        public async UniTask LaunchDialogueAsync(string guid, CancellationToken cancellationToken) {
            AsyncExt.RecreateCts(ref _cts);
            cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, cancellationToken).Token;

            var service = Services.Get<IDialogueService>();
            
            service.ClearAllPrinters();
            
            var table = await service.LoadDialogueAsync(guid);
            
            if (cancellationToken.IsCancellationRequested) {
                service.UnloadDialogue(guid);
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
                
                if (role != lastRole) {
                    lastRole = role;
                    await service.AwaitDialogueEvents(branch, DialogueEvent.DialogueBranchStart, cancellationToken);
                    if (cancellationToken.IsCancellationRequested) break;
                }

                await service.AwaitDialogueEvents(branch, DialogueEvent.DialogueElementStart, cancellationToken);
                if (cancellationToken.IsCancellationRequested) break;
                
                await service.PrintElementAsync(table.DialogueId, element.key, cancellationToken);
            }
            
            if (cancellationToken.IsCancellationRequested) return;
            
            service.StopDialogue(table.DialogueId);
            
            await service.AwaitDialogueEvents(table.DialogueId, DialogueEvent.DialogueStart, cancellationToken);
        }
    }
    
}