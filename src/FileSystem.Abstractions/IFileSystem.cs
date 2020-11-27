// Copyright (c) James Jackson-South.
// See LICENSE for more details.

using System.Threading;
using System.Threading.Tasks;

namespace FileSystem.Abstractions
{
    /// <summary>
    /// Defines the contract for file system implementations.
    /// </summary>
    public interface IFileSystem
    {
        /// <summary>
        /// Returns the file entry matching the given name or <see langword="null"/>
        /// if no file is found.
        /// </summary>
        /// <param name="name">The file name to return.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>The <see cref="IFileSystemEntry"/>.</returns>
        ValueTask<IFileSystemEntry> GetFileAsync(string name, CancellationToken cancellationToken);

        /// <summary>
        /// Puts the file entry against the file system.
        /// </summary>
        /// <param name="entry">The entry containing the file to put.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        ValueTask PutFileAsync(IFileSystemEntry entry, CancellationToken cancellationToken);

        /// <summary>
        /// Attempts to delete the file matching the given name.
        /// </summary>
        /// <param name="name">The name of the file to delete.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>The <see cref="bool"/> indicating the result.</returns>
        ValueTask<bool> TryDeleteFileAsync(string name, CancellationToken cancellationToken);
    }
}
