// Copyright (c) James Jackson-South.
// See LICENSE for more details.

using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using FileSystem.Abstractions;

namespace AzureBlobFileSystem
{
    /// <summary>
    /// Represents a single file entry in an Azure Blob Storage file system.
    /// </summary>
    public class AzureBlobFileSystemEntry : IFileSystemEntry
    {
        private readonly BlobClient client;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureBlobFileSystemEntry"/> class.
        /// </summary>
        /// <param name="client">The blob client.</param>
        /// <param name="properties">The blob properties.</param>
        public AzureBlobFileSystemEntry(BlobClient client, BlobProperties properties)
        {
            this.client = client;
            this.Name = client.Name;
            this.ContentType = properties.ContentType;
            this.ContentLength = properties.ContentLength;
            this.LastModifiedUtc = properties.LastModified.UtcDateTime;
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
        // TODO: Check file size limit here.
        public async Task<Stream> CreateReadStreamAsync()
            => (await this.client.DownloadAsync()).Value.Content;
    }
}
