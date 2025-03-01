using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors.Actions;
using MisterGames.Common.Async;
using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.Common.Inputs {
    
    public class GamepadVibrationTest : MonoBehaviour {
        
        [SerializeField] private Vector2 _constantFreq = Vector2.one;
        [SerializeReference] [SubclassSelector] public IActorAction action;
        
        private CancellationTokenSource cts;
        
        [Button]
        public void PlayAction() {
            action?.Apply(null, destroyCancellationToken).Forget();
        }

        [Button]
        public async void PlayConstant() {
            AsyncExt.RecreateCts(ref cts);
            var token = cts.Token;
            
            DeviceService.Instance.GamepadVibration.Register(this, 0);
            
            while (!token.IsCancellationRequested) {
                DeviceService.Instance.GamepadVibration.SetTwoMotors(this, _constantFreq, weight: 1f);
                await UniTask.Yield();
            }
        }

        [Button]
        public void StopConstant() {
            AsyncExt.DisposeCts(ref cts);
            DeviceService.Instance.GamepadVibration.Unregister(this);
        }
    }
    
}