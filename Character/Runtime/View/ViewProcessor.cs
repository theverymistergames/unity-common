using MisterGames.Character.Configs;
using MisterGames.Character.Input;
using MisterGames.Character.Motion;
using MisterGames.Tick.Core;
using UnityEngine;

namespace MisterGames.Character.View {

    public class ViewProcessor : MonoBehaviour, IUpdate {
        
        [SerializeField] private PlayerLoopStage _timeSourceStage = PlayerLoopStage.Update;
        
        [Header("Input")]
        [SerializeField] private CharacterInput _input;

        [Header("Output")]
        [SerializeField] private CharacterAdapter _adapter;
        [SerializeField] private ViewSettings _viewSettings;

        private ITimeSource _timeSource => TimeSources.Get(_timeSourceStage);
        private Vector2 _targetView;
        private Vector2 _currentView;

        private void OnEnable() {
            _input.View += HandleViewInput;
            _timeSource.Subscribe(this);
        }

        private void OnDisable() {
            _input.View -= HandleViewInput;
            _timeSource.Unsubscribe(this);

            _targetView = default;
            _currentView = default;
        }

        void IUpdate.OnUpdate(float dt) {
            var prevView = _currentView;
            _currentView = Vector2.Lerp(_currentView, _targetView, dt * _viewSettings.viewSmoothFactor);

            var diff = _currentView - prevView;
            _adapter.RotateHead(diff.x);
            _adapter.RotateBody(diff.y);
        }

        private void HandleViewInput(Vector2 delta) {
            var local = ToLocalSpace(delta);
            local.x *= _viewSettings.sensitivityVertical;
            local.y *= _viewSettings.sensitivityHorizontal;
            
            _targetView += local;
            _targetView.x = Mathf.Clamp(_targetView.x, -90f, 90f);
        }

        private static Vector2 ToLocalSpace(Vector2 vector) {
            return new Vector2(-vector.y, vector.x);
        }
    }

}
