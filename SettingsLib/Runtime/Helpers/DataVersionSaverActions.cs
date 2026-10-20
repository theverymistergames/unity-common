using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Async;
using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.SettingsLib.Versioning {
    
    [RequireComponent(typeof(DataVersionSaver))]
    public sealed class DataVersionSaverActions : MonoBehaviour, IActorComponent {
        
        [SerializeReference] [SubclassSelector] private IActorAction _onActualVersion;
        [SerializeReference] [SubclassSelector] private IActorAction _onIncorrectOrNotSavedVersion;

        private DataVersionSaver _dataVersionSaver;
        private CancellationTokenSource _cts;
        private IActor _actor;

        void IActorComponent.OnAwake(IActor actor) {
            _actor = actor;
        }

        private void Awake() {
            AsyncExt.RecreateCts(ref _cts);
            
            _dataVersionSaver = GetComponent<DataVersionSaver>();
            _dataVersionSaver.OnStateChanged += CheckState;
            CheckState(_dataVersionSaver.CurrentState);
        }

        private void OnDestroy() {
            AsyncExt.DisposeCts(ref _cts);
            
            _dataVersionSaver.OnStateChanged -= CheckState;
        }

        private void CheckState(DataVersionInfo.State state) {
            switch (state) {
                case DataVersionInfo.State.NoSavedData:
                case DataVersionInfo.State.IncorrectSavedData:
                    _onIncorrectOrNotSavedVersion?.Apply(_actor, _cts.Token).Forget();
                    break;
                
                case DataVersionInfo.State.ActualSavedData:
                    _onActualVersion?.Apply(_actor, _cts.Token).Forget();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }
    }
    
}