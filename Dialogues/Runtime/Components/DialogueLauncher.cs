using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Async;
using MisterGames.Common.Attributes;
using MisterGames.Common.Lists;
using MisterGames.Common.Localization;
using MisterGames.Common.Service;
using MisterGames.Dialogues.Core;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MisterGames.Dialogues.Components {
    
    public sealed class DialogueLauncher : MonoBehaviour, IActorComponent {

        [Header("Launch")]
        [SerializeField] private DialogueReference _dialogueReference;
        [SerializeField] private DialoguePrinter _dialoguePrinter;
        [SerializeField] private LaunchMode _launchMode = LaunchMode.OnEnable;
        [SerializeField] private bool _clearOnFinish;
        
        [Header("Skip")]
        [SerializeField] private NextElementMode _nextElementMode = NextElementMode.WaitSkip;
        [SerializeField] [Min(-1f)] private float _maxTimeAfterSkipToMoveToNext = 0.1f;
        [SerializeField] [Min(-1f)] private float _skipSymbolDelay = -1f;
            
        [Header("Timings")]
        [SerializeField] [Min(0f)] private float _firstReplicaDelay = 0.5f;
        [SerializeField] [Min(0f)] private float _minReplicaDelaySameRole = 0.4f;
        [SerializeField] [Min(0f)] private float _maxReplicaDelaySameRole = 0.7f;
        [SerializeField] [Min(0f)] private float _minReplicaDelayChangedRole = 0.6f;
        [SerializeField] [Min(0f)] private float _maxReplicaDelayChangedRole = 1f;
        [SerializeField] private bool _useTimeScale = false;
        
        [Header("Actions")]
        [SerializeReference] [SubclassSelector] private IActorAction _beforeStartAction;
        [SerializeReference] [SubclassSelector] private IActorAction _afterFinishAction;
        
        private enum LaunchMode {
            OnEnable,
            Manual,
        }

        private enum NextElementMode {
            WaitSkip,
            AutoPlayNext,
        }

        public event Action<LocalizationKey, int> OnDialogueElementPrinted = delegate { }; 
        
        public bool IsPaused { get; private set; }
        public bool IsRunning { get; private set; }

        private CancellationTokenSource _destroyCts;
        private CancellationTokenSource _enableCts;
        private CancellationTokenSource _dialogueLaunchCts;
        private CancellationTokenSource _skipCts;
        private IActor _actor;
        private float _lastSkipTime;

        void IActorComponent.OnAwake(IActor actor) {
            _actor = actor;
        }

        private void Awake() {
            AsyncExt.RecreateCts(ref _destroyCts);
        }

        private void OnDestroy() {
            AsyncExt.DisposeCts(ref _destroyCts);
        }

        private void OnEnable() {
            AsyncExt.RecreateCts(ref _enableCts);

            if (_launchMode == LaunchMode.OnEnable) {
                LaunchDialogueAsync(_dialogueReference.AssetGUID, _enableCts.Token).Forget();
            }
        }

        private void OnDisable() {
            AsyncExt.DisposeCts(ref _enableCts);
            
            StopDialogue();
        }

        public void NotifySkip() {
            if (!IsRunning || IsPaused) return;
            
            _lastSkipTime = Time.realtimeSinceStartup;
            
            AsyncExt.DisposeCts(ref _skipCts);
            _dialoguePrinter.FinishLastPrinting(_skipSymbolDelay);
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
        }

        public async UniTask LaunchDialogueAsync(string guid, CancellationToken cancellationToken = default) {
            var service = Services.Get<IDialogueService>();
            var table = await service.LoadDialogueAsync(guid);

            if (!cancellationToken.IsCancellationRequested) {
                await LaunchDialogueAsync(table, cancellationToken);
            }
            
            service.UnloadDialogue(guid);
        }

        public async UniTask LaunchDialogueAsync(IDialogueTable table, CancellationToken cancellationToken = default) {
            AsyncExt.RecreateCts(ref _dialogueLaunchCts);
            AsyncExt.RecreateCts(ref _skipCts);
            
            cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(_dialogueLaunchCts.Token, cancellationToken).Token;

            IsRunning = true;
            IsPaused = false;
            
            var service = Services.Get<IDialogueService>();
            
            if (cancellationToken.IsCancellationRequested) {
                StopDialogue();
                return;
            }

            ClearAllText();
            
            if (_beforeStartAction != null) {
                await _beforeStartAction.Apply(_actor, cancellationToken);
                if (cancellationToken.IsCancellationRequested) return;
            }
            
            service.StartDialogue(table.DialogueId);

            await service.AwaitDialogueEvents(table.DialogueId, DialogueEvent.DialogueStart, cancellationToken);
            if (cancellationToken.IsCancellationRequested) return;
            
            var elements = table.Elements;
            LocalizationKey lastBranch = default;
            LocalizationKey lastRole = default;
            
            AsyncExt.RecreateCts(ref _skipCts);
            var skipToken = CancellationTokenSource.CreateLinkedTokenSource(_skipCts.Token, cancellationToken).Token;
            
            for (int i = 0; i < elements.Count && !cancellationToken.IsCancellationRequested; i++) {
                var element = elements[i];
                
                service.StartDialogueElement(table.DialogueId, element);

                if (element.branchId != lastBranch) {
                    lastBranch = element.branchId;
                    
                    await service.AwaitDialogueEvents(element.branchId, DialogueEvent.DialogueBranchStart, cancellationToken);
                    if (cancellationToken.IsCancellationRequested) break;
                }

                await WaitWhilePaused(cancellationToken);
                if (cancellationToken.IsCancellationRequested) break;
                
                float elementDelay;
                
                if (element.roleId != lastRole) {
                    lastRole = element.roleId;
                    
                    await service.AwaitDialogueEvents(element.branchId, DialogueEvent.DialogueRoleStart, cancellationToken);
                    if (cancellationToken.IsCancellationRequested) break;
                    
                    await WaitWhilePaused(cancellationToken);
                    if (cancellationToken.IsCancellationRequested) break;
                    
                    elementDelay = Random.Range(_minReplicaDelayChangedRole, _maxReplicaDelayChangedRole);
                }
                else {
                    elementDelay = Random.Range(_minReplicaDelaySameRole, _maxReplicaDelaySameRole);
                }

                if (i == 0) elementDelay = _firstReplicaDelay;
                
                if (elementDelay > 0f) {
                    await WaitDelay(elementDelay, skipToken);
                    if (cancellationToken.IsCancellationRequested) break;
                }

                await WaitWhilePaused(cancellationToken);
                if (cancellationToken.IsCancellationRequested) break;

                await service.AwaitDialogueEvents(element.branchId, DialogueEvent.DialogueElementStart, cancellationToken);
                if (cancellationToken.IsCancellationRequested) break;
                
                await WaitWhilePaused(cancellationToken);
                if (cancellationToken.IsCancellationRequested) break;
                
                if (_skipCts == null) {
                    AsyncExt.RecreateCts(ref _skipCts);
                    skipToken = CancellationTokenSource.CreateLinkedTokenSource(_skipCts.Token, cancellationToken).Token;
                }
                
                if (element.key.IsNotNull()) {
                    int roleIndex = Mathf.Max(0, table.Roles.TryFindIndex(element.roleId, (k0, k1) => k0 == k1));
                    
                    await _dialoguePrinter.PrintElement(element.key, roleIndex, cancellationToken);
                    if (cancellationToken.IsCancellationRequested) break;
                    
                    OnDialogueElementPrinted.Invoke(element.key, roleIndex);
                    
                    await WaitWhilePaused(cancellationToken);
                    if (cancellationToken.IsCancellationRequested) break;
                }
                
                switch (_nextElementMode) {
                    case NextElementMode.WaitSkip:
                        if (Time.realtimeSinceStartup - _lastSkipTime <= _maxTimeAfterSkipToMoveToNext) continue;
                        
                        if (_skipCts == null) {
                            AsyncExt.RecreateCts(ref _skipCts);
                            skipToken = CancellationTokenSource.CreateLinkedTokenSource(_skipCts.Token, cancellationToken).Token;
                        }

                        await WaitSkipInput(skipToken, cancellationToken);
                        continue;
                    
                    case NextElementMode.AutoPlayNext:
                        continue;
                    
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            if (cancellationToken.IsCancellationRequested) return;
            
            service.StopDialogue(table.DialogueId);
            
            await service.AwaitDialogueEvents(table.DialogueId, DialogueEvent.DialogueStop, cancellationToken);
            if (cancellationToken.IsCancellationRequested) return;
            
            if (_afterFinishAction != null) {
                await _afterFinishAction.Apply(_actor, cancellationToken);
                if (cancellationToken.IsCancellationRequested) return;
            }
            
            if (_clearOnFinish) ClearAllText();
            
            IsRunning = false;
            IsPaused = false;
        }

        public void ClearAllText() {
            _dialoguePrinter.ClearAllText();
        }
        
        public void CancelLastPrinting(bool clear = false) {
            _dialoguePrinter.CancelLastPrinting(clear);
        }

        public void FinishLastPrinting(float symbolDelay = -1) {
            _dialoguePrinter.FinishLastPrinting(symbolDelay);
        }

        public void ReprintLast(LocalizationKey key) {
            _dialoguePrinter.ReprintLast(key);
        }
        
        private async UniTask WaitWhilePaused(CancellationToken cancellationToken) {
            while (!cancellationToken.IsCancellationRequested && IsPaused) {
                await UniTask.Yield();
            }
        }
        
        private static async UniTask WaitSkipInput(CancellationToken skipToken, CancellationToken cancellationToken) {
            while (!cancellationToken.IsCancellationRequested && !skipToken.IsCancellationRequested) {
                await UniTask.Yield();
            }
        }
        
        private UniTask WaitDelay(float delay, CancellationToken cancellationToken) {
            return UniTask.Delay(TimeSpan.FromSeconds(delay), _useTimeScale ? DelayType.DeltaTime : DelayType.UnscaledDeltaTime, cancellationToken: cancellationToken)
                .SuppressCancellationThrow();
        }
    }
    
}