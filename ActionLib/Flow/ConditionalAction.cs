using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Async;
using MisterGames.Common.Attributes;
using MisterGames.Common.Tick;
using UnityEngine;

namespace MisterGames.ActionLib.Flow {

    public sealed class ConditionalAction : MonoBehaviour, IActorComponent, IUpdate {

        [SerializeReference] [SubclassSelector] private IActorCondition _condition;
        [SerializeField] [Min(0f)] private float _checkPeriod = 0.1f;
        [SerializeField] private bool _cancelOnNextAction;
        [SerializeReference] [SubclassSelector] private IActorAction _enableAction;
        [SerializeReference] [SubclassSelector] private IActorAction _disableAction;

        private CancellationTokenSource _cts;
        private IActor _actor;
        private bool _isActive;
        private float _checkTimer;

        void IActorComponent.OnAwake(IActor actor) {
            _actor = actor;
        }

        private void OnDestroy() {
            AsyncExt.DisposeCts(ref _cts);
        }

        private void OnEnable() {
            _isActive = false;
            _checkTimer = 0f;

            PlayerLoopStage.Update.Subscribe(this);
            CheckCondition();
        }

        private void OnDisable() {
            PlayerLoopStage.Update.Unsubscribe(this);
            SetActive(false);
        }

        void IUpdate.OnUpdate(float dt) {
            _checkTimer += dt;
            if (_checkTimer < _checkPeriod) return;

            _checkTimer = 0f;
            CheckCondition();
        }

        private void CheckCondition() {
            SetActive(_condition == null || _condition.IsMatch(_actor));
        }

        private void SetActive(bool active) {
            if (_isActive == active) return;

            _isActive = active;

            if (_cancelOnNextAction || _cts == null) {
                AsyncExt.RecreateCts(ref _cts);
            }

            (active ? _enableAction : _disableAction)?.Apply(_actor, _cts.Token).Forget();
        }
    }

}
