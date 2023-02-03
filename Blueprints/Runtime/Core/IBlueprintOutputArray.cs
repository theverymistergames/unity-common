using System.Collections.Generic;

namespace MisterGames.Blueprints {

    /// <summary>
    /// Blueprint node has to implement this interface if it has an array output of type T port.
    /// </summary>
    public interface IBlueprintOutputArray<out T> {

        /// <summary>
        /// Called when linked node is trying to read value of type T.
        /// </summary>
        IReadOnlyList<T> GetOutputArrayPortValues(int port);
    }

}
