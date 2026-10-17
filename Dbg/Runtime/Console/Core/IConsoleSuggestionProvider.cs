using System.Collections.Generic;

namespace MisterGames.Dbg.Console.Core {

    /// <summary>
    /// Implement in a console module to suggest arguments of its commands in the console input field.
    /// </summary>
    public interface IConsoleSuggestionProvider {

        /// <summary>
        /// Collect suggestions for the last argument in <paramref name="args"/>.
        /// </summary>
        /// <param name="cmd">Command name in lower case, as it is registered.</param>
        /// <param name="args">Arguments typed so far. The last one is being typed now and can be empty.</param>
        /// <param name="suggestions">Full values for the last argument, the most relevant first.</param>
        void CollectSuggestions(string cmd, IReadOnlyList<string> args, ICollection<string> suggestions);
    }

}
