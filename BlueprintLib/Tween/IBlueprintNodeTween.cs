using System.Collections.Generic;
using MisterGames.Blueprints.Compile;
using MisterGames.Tweens.Core;

namespace MisterGames.BlueprintLib {

    public interface IBlueprintNodeTween {
        ITween Tween { get; }
        List<RuntimeLink> NextLinks { get; }
    }

}
