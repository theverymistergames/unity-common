namespace MisterGames.Feedback {

    public interface IFeedbackService {

        /// <summary>
        /// Appends an entry into the send queue. Can be called from any thread,
        /// including the threaded Application.logMessageReceivedThreaded callback.
        /// </summary>
        void AppendLog(string message);
    }

}
