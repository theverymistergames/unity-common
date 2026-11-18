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
    }

}
