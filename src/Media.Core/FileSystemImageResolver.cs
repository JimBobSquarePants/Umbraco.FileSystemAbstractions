// Copyright (c) James Jackson-South.
// See LICENSE for more details.

using System.IO;
using System.Threading.Tasks;
using FileSystem.Abstractions;
using SixLabors.ImageSharp.Web;
using SixLabors.ImageSharp.Web.Resolvers;

namespace Media.Core
{
    /// <summary>
    /// An image resolver that uses an <see cref="IFileSystemEntry"/> as a source.
    /// </summary>
    public class FileSystemImageResolver : IImageResolver
    {
        private readonly IFileSystemEntry file;
        private readonly ImageMetadata meta;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileSystemImageResolver"/> class.
        /// </summary>
        /// <param name="file">The filesystem entry.</param>
        public FileSystemImageResolver(IFileSystemEntry file)
        {
            this.file = file;
            this.meta = new ImageMetadata(file.LastModifiedUtc, file.ContentLength);
        }

        /// <inheritdoc/>
        public Task<ImageMetadata> GetMetaDataAsync() => Task.FromResult(this.meta);

        /// <inheritdoc/>
        public Task<Stream> OpenReadAsync() => this.file.CreateReadStreamAsync();
    }
}
