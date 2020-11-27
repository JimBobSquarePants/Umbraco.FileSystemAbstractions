// Copyright (c) James Jackson-South.
// See LICENSE for more details.

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FileSystem.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Media.Core
{
    /// <summary>
    /// Resolves and serves media files from the file system.
    /// </summary>
    public class MediaFileResolverMiddleware
    {
        private static readonly ConcurrentDictionary<string, Lazy<Task>> Workers
            = new ConcurrentDictionary<string, Lazy<Task>>(StringComparer.OrdinalIgnoreCase);

        private readonly RequestDelegate next;
        private readonly MediaFileResolverMiddlewareOptions options;
        private readonly ILogger logger;
        private readonly IFileSystem fileSystem;

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaFileResolverMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next middleware in the pipeline.</param>
        /// <param name="options">The middleware options.</param>
        /// <param name="logger">The logger instance.</param>
        /// <param name="fileSystem">The file system.</param>
        public MediaFileResolverMiddleware(
            RequestDelegate next,
            IOptions<MediaFileResolverMiddlewareOptions> options,
            ILogger<MediaFileResolverMiddleware> logger,
            IFileSystem fileSystem)
        {
            this.next = next;
            this.options = options.Value;
            this.logger = logger;
            this.fileSystem = fileSystem;
        }

        /// <summary>
        /// Performs operations upon the current request.
        /// </summary>
        /// <param name="context">The current HTTP request context.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        public async Task Invoke(HttpContext context)
        {
            // Support only Head requests or Get Requests.
            if (!HttpMethods.IsGet(context.Request.Method)
                && !HttpMethods.IsHead(context.Request.Method))
            {
                await this.next(context);
                return;
            }

            if (!Udi.TryParse(context.Request.Host + context.Request.Path, out Udi udi)
                || udi.EntityType != Constants.UdiEntityTypes.MediaFile
                || udi.EntityType.Type != UdiType.ClosedString)
            {
                await this.next(context);
                return;
            }

            // When multiple requests occur for the same file we use a Lazy<Task>
            // to initialize the file store request once.
            string id = udi.Id;
            await Workers.GetOrAdd(
                id,
                x => new Lazy<Task>(
                    async () =>
                    {
                        try
                        {
                            CancellationToken cancellationToken = context?.RequestAborted ?? CancellationToken.None;
                            IFileSystemEntry entry = await this.fileSystem.GetFileAsync(id, cancellationToken);

                            if (entry != null)
                            {
                                using Stream stream = await entry.CreateReadStreamAsync();
                                await this.SendResponseAsync(
                                    entry,
                                    new MediaContext(context),
                                    stream,
                                    this.options.BrowserMaxAge);
                            }
                            else
                            {
                                // TODO: Log 404?
                                await this.next(context);
                                return;
                            }
                        }
                        catch (Exception ex)
                        {
                            // Log the error, and pass to pipeline to handle as 404.
                            // Multiple requests at the same time will all recieve the same 404
                            // as we use LazyThreadSafetyMode.ExecutionAndPublication.
                            this.logger.LogError(ex, "Error retrieving file from media file store for request path {Path}", id);
                        }
                        finally
                        {
                            _ = Workers.TryRemove(id, out Lazy<Task> writeTask);
                        }
                    }, LazyThreadSafetyMode.ExecutionAndPublication)).Value;
        }

        private async Task SendResponseAsync(
            IFileSystemEntry entry,
            MediaContext mediaContext,
            Stream stream,
            TimeSpan maxAge)
        {
            mediaContext.ComprehendRequestHeaders(entry.LastModifiedUtc, entry.ContentLength);

            switch (mediaContext.GetPreconditionState())
            {
                case MediaContext.PreconditionState.Unspecified:
                case MediaContext.PreconditionState.ShouldProcess:
                    if (mediaContext.IsHeadRequest())
                    {
                        await mediaContext.SendStatusAsync(ResponseConstants.Status200Ok, entry.ContentType, maxAge);
                        return;
                    }

                    this.logger.LogFileServed(mediaContext.GetDisplayUrl(), entry.Name);
                    await mediaContext.SendAsync(stream, entry.ContentType, maxAge);
                    return;

                case MediaContext.PreconditionState.NotModified:
                    this.logger.LogFileNotModified(mediaContext.GetDisplayUrl());
                    await mediaContext.SendStatusAsync(ResponseConstants.Status304NotModified, entry.ContentType, maxAge);
                    return;

                case MediaContext.PreconditionState.PreconditionFailed:
                    this.logger.LogFilePreconditionFailed(mediaContext.GetDisplayUrl());
                    await mediaContext.SendStatusAsync(ResponseConstants.Status412PreconditionFailed, entry.ContentType, maxAge);
                    return;
                default:
                    var exception = new NotImplementedException(mediaContext.GetPreconditionState().ToString());
                    Debug.Fail(exception.ToString());
                    throw exception;
            }
        }
    }
}
