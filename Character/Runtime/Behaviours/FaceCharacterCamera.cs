using MisterGames.Actors;
using MisterGames.Character.Core;
using MisterGames.Common.Maths;
using MisterGames.Common.Tick;
using UnityEngine;

namespace MisterGames.Character.Behaviours {
    
    public sealed class FaceCharacterCamera : MonoBehaviour, IUpdate {

        [SerializeField] private Transform[] _targets;
        [SerializeField] private Vector3 _axes = Vector3.up;
        [SerializeField] private Vector3 _rotationOffset;
        [SerializeField] [Min(0f)] private float _smoothing = 5f;
        
        private IActor _actor;
        private Transform _camera;

        private void OnEnable() {
            PlayerLoopStage.LateUpdate.Subscribe(this);
        }

        private void OnDisable() {
            PlayerLoopStage.LateUpdate.Unsubscribe(this);
        
            _actor = null;
            _camera = null;
        }

        void IUpdate.OnUpdate(float dt) {
            if (CharacterSpawner.Instance.GetCharacter() is not { } actor) {
                _actor = null;
                _camera = null;
                return;
            }

            if (_actor == null || _actor != actor) {
                _actor = actor;
                _camera = actor.TryGetComponent(out Camera camera) ? camera.transform : null;
            }

            var cameraPos = _camera!.position;
            var rotOffset = Quaternion.Euler(_rotationOffset);

            for (int i = 0; i < _targets.Length; i++) {
                var target = _targets[i];
                target.GetPositionAndRotation(out var pos, out var rot);

                var dir = cameraPos - pos;
                if (dir == default) continue;

                var lookEuler = (Quaternion.LookRotation(dir) * rotOffset).eulerAngles;
                var euler = rot.eulerAngles;

                euler.x = Mathf.LerpAngle(euler.x, lookEuler.x, _axes.x);
                euler.y = Mathf.LerpAngle(euler.y, lookEuler.y, _axes.y);
                euler.z = Mathf.LerpAngle(euler.z, lookEuler.z, _axes.z);

                target.rotation = rot.SlerpNonZero(Quaternion.Euler(euler), _smoothing, dt);
            }
        }

#if UNITY_EDITOR
        private void Reset() {
            _targets = new[] { transform };
        }
#endif
    }
    
}