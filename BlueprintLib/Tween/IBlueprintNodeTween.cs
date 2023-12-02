using MisterGames.Blueprints.Runtime;
using MisterGames.Tweens.Core;

namespace MisterGames.BlueprintLib {

    public interface IBlueprintNodeTween {
        ITween Tween { get; }
        LinkIterator NextLinks { get; }
    }

}
