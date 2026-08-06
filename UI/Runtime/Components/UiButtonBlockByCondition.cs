using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Attributes;
using MisterGames.Common.Tick;
using UnityEngine;

namespace MisterGames.UI.Components {
    
    public sealed class UiButtonBlockByCondition : MonoBehaviour, IActorComponent, IUpdate {

        [SerializeField] private UiButton _button;
        [SerializeReference] [SubclassSelector] private IActorCondition _isBlockedCondition;
        [SerializeField] [Min(0f)] private float _checkPeriod = 0.25f;

        private IActor _actor;
        private float _lastCheckTime;
        
        void IActorComponent.OnAwake(IActor actor) {
            _actor = actor;
        }

        private void OnEnable() {
            PlayerLoopStage.LateUpdate.Subscribe(this);

            _lastCheckTime = Time.realtimeSinceStartup;
            _button.Block(this, NeedBlock());
        }

        private void OnDisable() {
            PlayerLoopStage.LateUpdate.Unsubscribe(this);
            _button.Block(this, false);
        }

        void IUpdate.OnUpdate(float dt) {
            if (Time.realtimeSinceStartup < _lastCheckTime + _checkPeriod) return;
            
            _lastCheckTime = Time.realtimeSinceStartup;
            _button.Block(this, NeedBlock());
        }

        private bool NeedBlock() {
            return _isBlockedCondition?.IsMatch(_actor) ?? false;
        }

#if UNITY_EDITOR
        private void Reset() {
            _button = GetComponent<UiButton>();
        }
#endif
    }
    
}