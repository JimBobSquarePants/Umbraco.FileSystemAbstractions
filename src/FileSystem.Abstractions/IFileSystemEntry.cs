// Copyright (c) James Jackson-South.
// See LICENSE for more details.

using System;
using System.IO;
using System.Threading.Tasks;

namespace FileSystem.Abstractions
{
    /// <summary>
    /// Defines the contract for individual files in a given file system.
    /// </summary>
    public interface IFileSystemEntry
    {
        /// <summary>
        /// Gets the name of the file.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the MIME type of the file.
        /// </summary>
        string ContentType { get; }

        /// <summary>
        /// Gets the length of the file in bytes.
        /// </summary>
        long ContentLength { get; }

        /// <summary>
        /// Gets the date and time in coordinated universal time (UTC) since
        /// the file was last modified.
        /// </summary>
        DateTime LastModifiedUtc { get; }

        /// <summary>
        /// Returns the file contents as a readonly stream.
        /// The caller should dispose of the stream when complete.
        /// </summary>
        /// <returns>The <see cref="Stream"/>.</returns>
        Task<Stream> CreateReadStreamAsync();
    }
}
