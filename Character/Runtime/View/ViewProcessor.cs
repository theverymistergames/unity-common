using MisterGames.Character.Configs;
using MisterGames.Character.Input;
using MisterGames.Character.Motion;
using MisterGames.Tick.Core;
using UnityEngine;

namespace MisterGames.Character.View {

    public class ViewProcessor : MonoBehaviour, IUpdate {
        
        [SerializeField] private TimeDomain _timeDomain;
        
        [Header("Input")]
        [SerializeField] private CharacterInput _input;

        [Header("Output")]
        [SerializeField] private CharacterAdapter _adapter;
        [SerializeField] private ViewSettings _viewSettings;
        
        private Vector2 _targetView;
        private Vector2 _currentView;

        private void OnEnable() {
            _input.View += HandleView;
            _timeDomain.Source.Subscribe(this);
        }

        private void OnDisable() {
            _input.View -= HandleView;
            _timeDomain.Source.Unsubscribe(this);
        }

        void IUpdate.OnUpdate(float dt) {
            var prevView = _currentView;
            _currentView = GetSmoothedView(_targetView, dt);

            var diff = _currentView - prevView;
            _adapter.RotateHead(diff.x);
            _adapter.RotateBody(diff.y);
        }

        private Vector2 GetSmoothedView(Vector2 target, float dt) {
            return Vector2.Lerp(_currentView, target, dt * _viewSettings.viewSmoothFactor);
        }

        private void HandleView(Vector2 delta) {
            var local = ToLocalSpace(delta);
            local.x *= _viewSettings.sensitivityVertical;
            local.y *= _viewSettings.sensitivityHorizontal;
            
            _targetView += local;
            _targetView.x = Mathf.Clamp(_targetView.x, -90, 90);
        }

        private static Vector2 ToLocalSpace(Vector2 vector) {
            return new Vector2(-vector.y, vector.x);
        }
    }

}
