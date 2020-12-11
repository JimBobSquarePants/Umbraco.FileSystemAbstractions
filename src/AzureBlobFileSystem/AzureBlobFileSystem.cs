// Copyright (c) James Jackson-South.
// See LICENSE for more details.

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using FileSystem.Abstractions;
using Microsoft.Extensions.Options;

namespace AzureBlobFileSystem
{
    /// <summary>
    /// Provides control of files from within an Azure Blob Storage file system.
    /// </summary>
    public class AzureBlobFileSystem : IFileSystem
    {
        private readonly BlobContainerClient container;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureBlobFileSystem"/> class.
        /// </summary>
        /// <param name="options">The file system options.</param>
        public AzureBlobFileSystem(IOptions<AzureBlobFileSystemOptions> options)
        {
            AzureBlobFileSystemOptions blobOptions = options.Value;
            this.container = new BlobContainerClient(
                blobOptions.ConnectionString,
                blobOptions.ContainerName);
        }

        /// <inheritdoc/>
        public async ValueTask<IFileSystemEntry> GetFileAsync(string name, CancellationToken cancellationToken = default)
        {
            BlobClient client = this.container.GetBlobClient(name);

            if (!await client.ExistsAsync(cancellationToken))
            {
                return null;
            }

            // I've had a good read through the SDK source and I believe we cannot get
            // a 304 here since 'If-Modified-Since' header is not set by default.
            BlobProperties properties = (await client.GetPropertiesAsync(cancellationToken: cancellationToken)).Value;

            return new AzureBlobFileSystemEntry(client, properties);
        }

        /// <inheritdoc/>
        public async ValueTask PutFileAsync(IFileSystemEntry entry, CancellationToken cancellationToken = default)
        {
            var options = new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = entry.ContentType,
                }
            };

            // TODO: Check file size limit here.
            using Stream stream = await entry.CreateReadStreamAsync();
            _ = await this.container.GetBlobClient(entry.Name).UploadAsync(stream, options, cancellationToken);
        }

        /// <inheritdoc/>
        public async ValueTask<bool> TryDeleteFileAsync(string name, CancellationToken cancellationToken = default)
        {
            BlobClient client = this.container.GetBlobClient(name);
            return (await client.DeleteIfExistsAsync(cancellationToken: cancellationToken)).Value;
        }
    }
}
