using UnityEngine;
#if STEAMWORKS_NET && !DISABLESTEAMWORKS
using Steamworks;
#endif

namespace MisterGames.Common.Steam {

    [DefaultExecutionOrder(-100_000)]
    public sealed class SteamBootstrap : MonoBehaviour {

        [SerializeField] private uint _appId = 480;
        [SerializeField] private bool _restartAppIfNecessary = true;

        public static bool IsInitialized => _instance != null && _instance._isInitialized;

        private static SteamBootstrap _instance;
        private bool _isInitialized;

        private void Awake() {
            if (_instance != null && _instance != this) {
                Debug.LogWarning($"SteamBootstrap.Awake: f {Time.frameCount}, another instance [{_instance.name}] is already active, destroying [{name}]");
                Destroy(gameObject);
                return;
            }

            _instance = this;

#if STEAMWORKS_NET && !DISABLESTEAMWORKS
            if (_restartAppIfNecessary && SteamAPI.RestartAppIfNecessary(new AppId_t(_appId))) {
                Debug.Log($"SteamBootstrap.Awake: f {Time.frameCount}, game was not launched through Steam, restarting via Steam with app id {_appId}");
                Application.Quit();
                return;
            }

            _isInitialized = SteamAPI.Init();

            if (_isInitialized) {
                Debug.Log($"SteamBootstrap.Awake: f {Time.frameCount}, Steam API initialized, app id {SteamUtils.GetAppID()}");
            }
            else {
                Debug.LogError($"SteamBootstrap.Awake: f {Time.frameCount}, SteamAPI.Init failed: Steam is not running, or steam_appid.txt is missing, or app id {_appId} is not owned");
            }
#else
            Debug.LogWarning($"SteamBootstrap.Awake: f {Time.frameCount}, Steamworks.NET is not available or disabled, Steam API is not initialized");
#endif
        }

        private void Update() {
#if STEAMWORKS_NET && !DISABLESTEAMWORKS
            if (_isInitialized) SteamAPI.RunCallbacks();
#endif
        }

        private void OnDestroy() {
            if (_instance != this) return;

            Shutdown();
            _instance = null;
        }

        private void OnApplicationQuit() {
            Shutdown();
        }

        private void Shutdown() {
#if STEAMWORKS_NET && !DISABLESTEAMWORKS
            if (!_isInitialized) return;

            SteamAPI.Shutdown();
            Debug.Log($"SteamBootstrap.Shutdown: f {Time.frameCount}, Steam API shut down");
#endif
            _isInitialized = false;
        }
    }

}
