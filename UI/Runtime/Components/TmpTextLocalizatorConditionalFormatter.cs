using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Attributes;
using MisterGames.Common.Localization;
using MisterGames.Common.Tick;
using UnityEngine;

namespace MisterGames.UI.Components {

    public sealed class TmpTextLocalizatorConditionalFormatter : MonoBehaviour, IActorComponent, IUpdate {

        [SerializeField] private TmpTextLocalizator _localizator;
        [SerializeReference] [SubclassSelector] private IActorCondition _condition;
        [SerializeField] [Min(0f)] private float _checkPeriod = 0.25f;
        [SerializeReference] [SubclassSelector] private IArgumentResolver _formatterTrue;
        [SerializeReference] [SubclassSelector] private IArgumentResolver _formatterFalse;

        private IActor _actor;
        private float _lastCheckTime;
        private bool _lastResult;
        private IArgumentResolver _currentFormatter;

        void IActorComponent.OnAwake(IActor actor) {
            _actor = actor;
        }

        private void OnEnable() {
            PlayerLoopStage.LateUpdate.Subscribe(this);

            _lastCheckTime = TimeSources.unscaledTime;
            _lastResult = IsMatch();
            
            ApplyFormatter(_lastResult);
        }

        private void OnDisable() {
            PlayerLoopStage.LateUpdate.Unsubscribe(this);

            RemoveCurrentFormatter();
            _localizator.ForceUpdateValue();
        }

        void IUpdate.OnUpdate(float dt) {
            if (TimeSources.unscaledTime < _lastCheckTime + _checkPeriod) return;

            _lastCheckTime = TimeSources.unscaledTime;

            bool result = IsMatch();
            if (result == _lastResult) return;

            _lastResult = result;
            ApplyFormatter(result);
        }

        private bool IsMatch() {
            return _condition?.IsMatch(_actor) ?? false;
        }

        private void ApplyFormatter(bool result) {
            RemoveCurrentFormatter();

            _currentFormatter = result ? _formatterTrue : _formatterFalse;
            if (_currentFormatter != null) _localizator.AddFormatter(_currentFormatter);

            _localizator.ForceUpdateValue();
        }

        private void RemoveCurrentFormatter() {
            if (_currentFormatter != null) _localizator.RemoveFormatter(_currentFormatter);
            _currentFormatter = null;
        }

#if UNITY_EDITOR
        private void Reset() {
            _localizator = GetComponent<TmpTextLocalizator>();
        }
#endif
    }

}
