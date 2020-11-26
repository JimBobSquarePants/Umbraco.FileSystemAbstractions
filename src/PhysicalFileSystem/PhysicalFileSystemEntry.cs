// Copyright (c) James Jackson-South.
// See LICENSE for more details.

using System;
using System.IO;
using System.Threading.Tasks;
using FileSystem.Abstractions;
using Microsoft.Extensions.FileProviders;

namespace PhysicalFileSystem
{
    /// <summary>
    /// Represents a single file entry in an on-disk file system.
    /// </summary>
    public class PhysicalFileSystemEntry : IFileSystemEntry
    {
        private readonly IFileInfo fileInfo;

        /// <summary>
        /// Initializes a new instance of the <see cref="PhysicalFileSystemEntry"/> class.
        /// </summary>
        /// <param name="fileInfo">The file info.</param>
        /// <param name="contentTypeProvider">The content type provider.</param>
        public PhysicalFileSystemEntry(
            IFileInfo fileInfo,
            IFileContentTypeProvider contentTypeProvider)
        {
            this.Name = fileInfo.Name;
            this.ContentType = contentTypeProvider.GetContentType(fileInfo.Name);
            this.ContentLength = fileInfo.Length;
            this.LastModifiedUtc = fileInfo.LastModified.UtcDateTime;
            this.fileInfo = fileInfo;
        }

        /// <inheritdoc/>
        public string Name { get; }

        /// <inheritdoc/>
        public string ContentType { get; }

        /// <inheritdoc/>
        public long ContentLength { get; }

        /// <inheritdoc/>
        public DateTime LastModifiedUtc { get; }

        /// <inheritdoc/>
        public Task<Stream> CreateReadStreamAsync()
            => Task.FromResult(this.fileInfo.CreateReadStream());
    }
}
