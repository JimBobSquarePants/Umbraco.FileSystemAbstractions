// Copyright (c) James Jackson-South.
// See LICENSE for more details.

namespace FileSystem.Abstractions
{
    /// <summary>
    /// Provides methods to resolve the MIME type for file entries.
    /// </summary>
    public interface IFileContentTypeProvider
    {
        /// <summary>
        /// Returns the content type for the given input filename.
        /// </summary>
        /// <param name="fileName">The filename to parse.</param>
        /// <returns>The <see cref="string"/>.</returns>
        string GetContentType(string fileName);

        /// <summary>
        /// Returns the file extension type for the given MIME type.
        /// </summary>
        /// <param name="contentType">The MIME type to parse.</param>
        /// <returns>The <see cref="string"/>.</returns>
        string GetFileExtension(string contentType);
    }
}
