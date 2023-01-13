namespace MisterGames.Blueprints.Core2 {

    /// <summary>
    /// Blueprint node can implement this interface to receive start call from Blueprint runner.
    /// </summary>
    internal interface IBlueprintStart {
        void OnStart();
    }

}
