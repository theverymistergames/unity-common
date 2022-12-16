using System;
using System.Linq;

namespace MisterGames.Tick.Core {

    public enum PlayerLoopStage {
        PreUpdate,
        Update,
        UnscaledUpdate,
        LateUpdate,
        FixedUpdate,
    }

    public static class PlayerLoopStages {

        public static ReadOnlySpan<PlayerLoopStage> All => PlayerLoopStagesArray;

        private static readonly PlayerLoopStage[] PlayerLoopStagesArray = CreatePlayerLoopStagesArray();

        private static PlayerLoopStage[] CreatePlayerLoopStagesArray() {
            return typeof(PlayerLoopStage)
                .GetEnumValues()
                .Cast<PlayerLoopStage>()
                .ToArray();
        }
    }

}
