using System;
using MisterGames.Common.Data;

namespace MisterGames.Character.Capsule {

    [Serializable]
    public struct CharacterPoseSettings {
        public CharacterCapsuleSize capsuleSize;
        public Pair<CharacterPoseType, CharacterPoseTransition>[] transitions;
    }

}
