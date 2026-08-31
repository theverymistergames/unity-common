using System.Collections.Generic;

namespace MisterGames.Feedback.Editor {

    /// <summary>
    /// A custom view of the downloaded feedback logs, drawn in the analyzer window above the log tree.
    /// Implementations are picked in the window with a subclass selector, so several views can be shown
    /// at the same time, one under another, in the order they are set.
    /// Implementations must be serializable classes and can have their own serialized settings.
    /// </summary>
    public interface IFeedbackViewProvider {

        /// <summary>
        /// Name of the view, drawn as its header.
        /// </summary>
        string Title { get; }

        void OnGUI(in FeedbackViewContext context);
    }

    /// <summary>
    /// Logs the view is asked to draw, already narrowed down by the player filter of the window.
    /// </summary>
    public readonly struct FeedbackViewContext {

        /// <summary>
        /// Players to draw: all of them, or the only selected one.
        /// </summary>
        public readonly IReadOnlyList<FeedbackLogPlayer> players;

        /// <summary>
        /// Player selected in the window, or null when all players are shown.
        /// </summary>
        public readonly FeedbackLogPlayer selectedPlayer;

        /// <summary>
        /// Text typed into the search field of the window, empty when nothing is searched for.
        /// </summary>
        public readonly string search;

        /// <summary>
        /// The window is switched to errors only.
        /// </summary>
        public readonly bool errorsOnly;

        public bool HasSelectedPlayer => selectedPlayer != null;

        public FeedbackViewContext(
            IReadOnlyList<FeedbackLogPlayer> players,
            FeedbackLogPlayer selectedPlayer,
            string search,
            bool errorsOnly)
        {
            this.players = players;
            this.selectedPlayer = selectedPlayer;
            this.search = search;
            this.errorsOnly = errorsOnly;
        }
    }

}
