using MisterGames.Common.Service;
using MisterGames.Scenes.Core;
using UnityEngine;

namespace MisterGames.Scenes.Background {
    
    public sealed class BackgroundSceneLoader : MonoBehaviour {

        [SerializeField] private Mode _mode;
        [SerializeField] private bool _activateFirstScene = true;
        [SerializeField] private SceneReference[] _scenes;
        
        private enum Mode {
            AwakeDestroy,
            EnableDisable,
        }

        private void Awake() {
            if (_mode != Mode.AwakeDestroy) return;
            
            BindScenes();
        }

        private void OnDestroy() {
            if (_mode != Mode.AwakeDestroy) return;
            
            UnbindScenes();
        }

        private void OnEnable() {
            if (_mode != Mode.EnableDisable) return;
            
            BindScenes();
        }

        private void OnDisable() {
            if (_mode != Mode.EnableDisable) return;
            
            UnbindScenes();
        }
        
        private void BindScenes() {
            if (!Services.TryGet(out IBackgroundSceneService service)) return;

            for (int i = 0; i < _scenes?.Length; i++) {
                ref string scene = ref _scenes[i].scene;
                service.BindBackgroundScene(this, scene);
                
                if (i == 0 && _activateFirstScene) SceneLoader.SetActiveScene(scene);
            }
        }

        private void UnbindScenes() {
            if (!Services.TryGet(out IBackgroundSceneService service)) return;

            for (int i = 0; i < _scenes?.Length; i++) {
                ref string scene = ref _scenes[i].scene;
                service.UnbindBackgroundScene(this, scene);
            }
        }
    }
    
}