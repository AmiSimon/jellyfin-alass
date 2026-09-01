namespace SubtitleSync.Shared.Interfaces
{
    /// <summary>
    /// Abstraction for logging operations.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Logs a debug message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="args">The arguments.</param>
        void Debug(string message, params object[] args);

        /// <summary>
        /// Logs an information message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="args">The arguments.</param>
        void Info(string message, params object[] args);

        /// <summary>
        /// Logs a warning message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="args">The arguments.</param>
        void Warn(string message, params object[] args);

        /// <summary>
        /// Logs an error message.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="message">The message.</param>
        /// <param name="args">The arguments.</param>
        void Error(System.Exception exception, string message, params object[] args);

        /// <summary>
        /// Logs an error message without an exception.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="args">The arguments.</param>
        void Error(string message, params object[] args);
    }
}
