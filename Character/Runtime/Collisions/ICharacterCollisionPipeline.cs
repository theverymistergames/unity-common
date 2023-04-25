using MisterGames.Character.Core;
using MisterGames.Collisions.Core;

namespace MisterGames.Character.Collisions {

    public interface ICharacterCollisionPipeline : ICharacterPipeline {

        ICollisionDetector HitDetector { get; }
        CharacterCeilingDetector CeilingDetector { get; }
        CharacterGroundDetector GroundDetector { get; }
    }

}
