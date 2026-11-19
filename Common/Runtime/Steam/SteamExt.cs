using UnityEngine;
#if STEAMWORKS_NET && !DISABLESTEAMWORKS
using Steamworks;
#endif

namespace MisterGames.Common.Steam {

    public static class SteamExt {

        public static bool IsRunningOnSteamDeck() {
#if STEAMWORKS_NET && !DISABLESTEAMWORKS
            return SteamBootstrap.IsInitialized && SteamUtils.IsRunningOnSteamHardware() == ESteamHardwareType.k_ESteamHardwareTypeSteamDeck;
#else
            return false;
#endif
        }

        public static bool ShowFloatingKeyboard(Rect screenRect) {
#if STEAMWORKS_NET && !DISABLESTEAMWORKS
            return SteamBootstrap.IsInitialized && 
                   SteamUtils.ShowFloatingGamepadTextInput(
                       EFloatingGamepadTextInputMode.k_EFloatingGamepadTextInputModeModeSingleLine,
                       Mathf.RoundToInt(screenRect.x),
                       Mathf.RoundToInt(Screen.height - screenRect.yMax),
                       Mathf.RoundToInt(screenRect.width),
                       Mathf.RoundToInt(screenRect.height)
                   );
#else
            return false;
#endif
        }

        public static bool DismissFloatingKeyboard() {
#if STEAMWORKS_NET && !DISABLESTEAMWORKS
            return SteamBootstrap.IsInitialized && SteamUtils.DismissFloatingGamepadTextInput();
#else
            return false;
#endif
        }
    }

}
