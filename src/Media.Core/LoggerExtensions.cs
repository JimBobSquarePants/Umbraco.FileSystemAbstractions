// Copyright (c) James Jackson-South.
// See LICENSE for more details.

using System;
using Microsoft.Extensions.Logging;

namespace Media.Core
{
    /// <summary>
    /// Extensions methods for the <see cref="ILogger"/> interface.
    /// </summary>
    internal static class LoggerExtensions
    {
        private static readonly Action<ILogger, string, Exception> LogProcessingErrorAction;
        private static readonly Action<ILogger, string, Exception> LogResolveFailedAction;
        private static readonly Action<ILogger, string, string, Exception> LogServedAction;
        private static readonly Action<ILogger, string, Exception> LogPathNotModifiedAction;
        private static readonly Action<ILogger, string, Exception> LogPreconditionFailedAction;

        /// <summary>
        /// Initializes static members of the <see cref="LoggerExtensions"/> class.
        /// </summary>
        static LoggerExtensions()
        {
            LogProcessingErrorAction = LoggerMessage.Define<string>(
                logLevel: LogLevel.Error,
                eventId: 1,
                formatString: "The file '{Uri}' could not be processed");

            LogResolveFailedAction = LoggerMessage.Define<string>(
                logLevel: LogLevel.Error,
                eventId: 2,
                formatString: "The file '{Uri}' could not be resolved");

            LogServedAction = LoggerMessage.Define<string, string>(
                logLevel: LogLevel.Information,
                eventId: 3,
                formatString: "Sending file. Request uri: '{Uri}'. Cached Key: '{Key}'");

            LogPathNotModifiedAction = LoggerMessage.Define<string>(
                logLevel: LogLevel.Information,
                eventId: 4,
                formatString: "The file '{Uri}' was not modified");

            LogPreconditionFailedAction = LoggerMessage.Define<string>(
                logLevel: LogLevel.Information,
                eventId: 5,
                formatString: "Precondition for file '{Uri}' failed");
        }

        /// <summary>
        /// Logs that a given file request could not be processed.
        /// </summary>
        /// <param name="logger">The type used to perform logging.</param>
        /// <param name="uri">The full request uri.</param>
        /// <param name="exception">The captured exception.</param>
        public static void LogFileProcessingFailed(this ILogger logger, string uri, Exception exception)
            => LogProcessingErrorAction(logger, uri, exception);

        /// <summary>
        /// Logs that a given file could not be resolved.
        /// </summary>
        /// <param name="logger">The type used to perform logging.</param>
        /// <param name="uri">The full request uri.</param>
        public static void LogFileResolveFailed(this ILogger logger, string uri)
            => LogResolveFailedAction(logger, uri, null);

        /// <summary>
        /// Logs that a given file request has been served.
        /// </summary>
        /// <param name="logger">The type used to perform logging.</param>
        /// <param name="uri">The full request uri.</param>
        /// <param name="key">The cached file key.</param>
        public static void LogFileServed(this ILogger logger, string uri, string key)
            => LogServedAction(logger, uri, key, null);

        /// <summary>
        /// Logs that a given file request has not been modified.
        /// </summary>
        /// <param name="logger">The type used to perform logging.</param>
        /// <param name="uri">The full request uri.</param>
        public static void LogFileNotModified(this ILogger logger, string uri)
            => LogPathNotModifiedAction(logger, uri, null);

        /// <summary>
        /// Logs that access to a given file request has been denied.
        /// </summary>
        /// <param name="logger">The type used to perform logging.</param>
        /// <param name="uri">The full request uri.</param>
        public static void LogFilePreconditionFailed(this ILogger logger, string uri)
            => LogPreconditionFailedAction(logger, uri, null);
    }
}
