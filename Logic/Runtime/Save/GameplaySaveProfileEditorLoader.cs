using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors.Actions;
using MisterGames.Common.Async;
using MisterGames.Common.Attributes;
using MisterGames.Common.Save;
using MisterGames.Common.Service;
using MisterGames.Scenes.Core;
using UnityEngine;

namespace MisterGames.Common.Gameplay {
    
    public sealed class GameplaySaveProfileEditorLoader : MonoBehaviour {

        [SerializeField] private int _editorProfileIndex = -1000;
        [SerializeReference] [SubclassSelector] private IActorAction _action;

        private CancellationTokenSource _cts;
        
        private void Awake() {
            if (SceneLoader.GetApplicationLaunchMode() != ApplicationLaunchMode.FromCustomEditorScene ||
                !Services.TryGet(out IGameplaySaveService gameplaySaveService)) 
            {
                return;
            }
            
            AsyncExt.RecreateCts(ref _cts);
            gameplaySaveService.LoadOrCreateProfile(gameplaySaveService.GetProfileKey(_editorProfileIndex), makeCurrent: true);
            _action?.Apply(null, _cts.Token).Forget();
        }

        private void OnDestroy() {
            AsyncExt.DisposeCts(ref _cts);
        }
    }
    
}