using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Character.Core;
using MisterGames.Common.Async;
using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.ActionLib.Flow {

    public sealed class ActionOnEnableDisable : MonoBehaviour, IActorComponent {
        
        [SerializeField] private bool _useCharacterAsContext = true;
        [SerializeField] private bool _cancelOnNextAction;
        [SerializeReference] [SubclassSelector] private IActorAction _enableAction;
        [SerializeReference] [SubclassSelector] private IActorAction _disableAction;

        private CancellationTokenSource _cts;
        private IActor _actor;

        void IActorComponent.OnAwake(IActor actor) {
            _actor = actor;
        }

        private void OnDestroy() {
            AsyncExt.DisposeCts(ref _cts);
        }

        private void OnEnable() {
            if (_cancelOnNextAction || _cts == null) {
                AsyncExt.RecreateCts(ref _cts);
            }
            
            _enableAction?.Apply(GetContext(), _cts.Token).Forget();
        }

        private void OnDisable() {
            if (_cancelOnNextAction || _cts == null) {
                AsyncExt.RecreateCts(ref _cts);
            }
            
            _disableAction?.Apply(GetContext(), _cts.Token).Forget();
        }

        private IActor GetContext() {
            return _useCharacterAsContext ? CharacterSpawner.Instance.GetCharacter() : _actor;
        }
    }

}
