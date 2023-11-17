using System.Collections.Generic;
using MisterGames.Blueprints.Compile;
using MisterGames.Blueprints.Runtime;
using MisterGames.Tweens.Core;

namespace MisterGames.BlueprintLib {

    public interface IBlueprintNodeTween2 {
        ITween Tween { get; }
        LinkIterator NextLinks { get; }
    }

    public interface IBlueprintNodeTween {
        ITween Tween { get; }
        List<RuntimeLink> NextLinks { get; }
    }

}
