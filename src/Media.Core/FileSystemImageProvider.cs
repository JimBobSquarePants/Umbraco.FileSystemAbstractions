// Copyright (c) James Jackson-South.
// See LICENSE for more details.

using System;
using System.Threading.Tasks;
using FileSystem.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using SixLabors.ImageSharp.Web;
using SixLabors.ImageSharp.Web.Providers;
using SixLabors.ImageSharp.Web.Resolvers;

namespace Media.Core
{
    /// <summary>
    /// An image provider that uses an <see cref="IFileSystem"/> as a source.
    /// </summary>
    public class FileSystemImageProvider : IImageProvider
    {
        private readonly IFileSystem fileSystem;
        private readonly FormatUtilities formatUtilities;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileSystemImageProvider"/> class.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="formatUtilities">The format utilities.</param>
        public FileSystemImageProvider(IFileSystem fileSystem, FormatUtilities formatUtilities)
        {
            this.fileSystem = fileSystem;
            this.formatUtilities = formatUtilities;
        }

        /// <inheritdoc/>
        public ProcessingBehavior ProcessingBehavior { get; } = ProcessingBehavior.CommandOnly;

        /// <inheritdoc/>
        public Func<HttpContext, bool> Match { get; set; } = _ => true;

        /// <inheritdoc/>
        public async Task<IImageResolver> GetAsync(HttpContext context)
        {
            if (!Udi.TryParse(context.Request.Host + context.Request.Path, out Udi udi)
                || udi.EntityType != Constants.UdiEntityTypes.MediaFile
                || udi.EntityType.Type != UdiType.ClosedString)
            {
                return null;
            }

            IFileSystemEntry file = await this.fileSystem.GetFileAsync(udi.Id, default);

            if (file is null)
            {
                return null;
            }

            return new FileSystemImageResolver(file);
        }

        /// <inheritdoc/>
        public bool IsValidRequest(HttpContext context)
        {
            // In addition to parsing the request we would either:
            // 1. Fire a validation event that a handler could use to read
            //    the media service and check if the file is in recycling - Preferred.
            // 2. Directly inject the media service into this middleware.
            return this.formatUtilities.GetExtensionFromUri(context.Request.GetDisplayUrl()) != null;
        }
    }
}
