﻿namespace MisterGames.Blueprints.Core2 {

    /// <summary>
    /// Blueprint node has to implement this interface if it has an enter port.
    /// </summary>
    public interface IBlueprintEnter {
        void OnEnterPort(int port);
    }

}
