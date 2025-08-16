using System;
using UnityEngine;

namespace MisterGames.Scenes.Loading {
    
    public sealed class LoadingScreen : MonoBehaviour, ILoadingScreen {
        
        [Header("Layers")]
        [SerializeField] private GameObject _root;
        [SerializeField] private GameObject _background;
        [SerializeField] private GameObject _overlay;
        
        private void Awake() {
            LoadingService.Instance.RegisterLoadingScreen(this);
        }

        private void OnDestroy() {
            LoadingService.Instance.UnregisterLoadingScreen(this);
        }

        void ILoadingScreen.SetState(LoadingScreenState state) {
            switch (state) {
                case LoadingScreenState.Off:
                    _root.SetActive(false);
                    break;
                
                case LoadingScreenState.Background:
                    _root.SetActive(true);
                    _background.SetActive(true);
                    _overlay.SetActive(false);
                    break;
                
                case LoadingScreenState.Full:
                    _root.SetActive(true);
                    _background.SetActive(true);
                    _overlay.SetActive(true);
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }
    }
    
}