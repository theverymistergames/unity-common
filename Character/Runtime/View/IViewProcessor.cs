using MisterGames.Common.Inputs;
using UnityEngine;

namespace MisterGames.Character.View {
    
    public interface IViewProcessor {
        Vector2 GetViewSensitivity(InputDeviceType deviceType) => Vector2.one;
    }
    
}