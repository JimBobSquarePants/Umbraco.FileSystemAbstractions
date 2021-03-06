// Copyright (c) James Jackson-South.
// See LICENSE for more details.

using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using FileSystem.Abstractions;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace PhysicalFileSystem
{
    /// <summary>
    /// Provides control of files from within an on-disk file system.
    /// </summary>
    public class PhysicalFileSystem : IFileSystem
    {
        // Extended version of Path.GetInvalidPathChars
        private static readonly char[] InvalidPathChars = new char[] { '\0', '/', '+', ' ' };

        private readonly IFileProvider fileProvider;
        private readonly IFileContentTypeProvider contentTypeProvider;
        private readonly string root;

        /// <summary>
        /// Initializes a new instance of the <see cref="PhysicalFileSystem"/> class.
        /// </summary>
        /// <param name="options">The file system options.</param>
        /// <param name="contentTypeProvider">The content type provider.</param>
        /// <param name="environment">The host evironment.</param>
        public PhysicalFileSystem(
            IOptions<PhysicalFileSystemOptions> options,
            IFileContentTypeProvider contentTypeProvider,
            IHostEnvironment environment)
        {
            this.root = GetContentRoot(options.Value, environment.ContentRootPath);

            if (!Directory.Exists(this.root))
            {
                Directory.CreateDirectory(this.root);
            }

            this.fileProvider = new PhysicalFileProvider(this.root);
            this.contentTypeProvider = contentTypeProvider;
        }

        /// <inheritdoc/>
        public ValueTask<IFileSystemEntry> GetFileAsync(string name, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return ValueTaskEx.FromCanceled<IFileSystemEntry>(cancellationToken);
            }

            IFileInfo file = this.fileProvider.GetFileInfo(ToFilePath(name));
            if (!file.Exists)
            {
                return ValueTaskEx.FromResult<IFileSystemEntry>(null);
            }

            return ValueTaskEx.FromResult<IFileSystemEntry>(new PhysicalFileSystemEntry(file, this.contentTypeProvider));
        }

        /// <inheritdoc/>
        public async ValueTask PutFileAsync(IFileSystemEntry entry, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                await ValueTaskEx.FromCanceled(cancellationToken);
            }

            string name = entry.Name;
            string path = Path.Combine(this.root, ToFilePath(name));
            string directory = Path.GetDirectoryName(path);

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using Stream stream = await entry.CreateReadStreamAsync();
            using FileStream fileStream = File.Create(path);
            await stream.CopyToAsync(fileStream, cancellationToken);
        }

        /// <inheritdoc/>
        public ValueTask<bool> TryDeleteFileAsync(string name, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return ValueTaskEx.FromCanceled<bool>(cancellationToken);
            }

            IFileInfo file = this.fileProvider.GetFileInfo(ToFilePath(name));

            if (!file.Exists)
            {
                return ValueTaskEx.FromResult(false);
            }

            File.Delete(file.PhysicalPath);
            return ValueTaskEx.FromResult(true);
        }

        internal static string GetContentRoot(PhysicalFileSystemOptions options, string contentRootPath)
        {
            string root = options.RootPath;

            return Path.IsPathFullyQualified(root)
                ? root
                : Path.GetFullPath(root, contentRootPath);
        }

        /// <summary>
        /// Converts the name into a nested file path.
        /// NTFS directories can handle up to 10,000 files in the directory before slowing down.
        /// This will help us to ensure that don't ever go over that limit.
        /// <see href="http://stackoverflow.com/questions/197162/ntfs-performance-and-large-volumes-of-files-and-directories"/>
        /// <see href="http://stackoverflow.com/questions/115882/how-do-you-deal-with-lots-of-small-files"/>
        /// <see href="http://stackoverflow.com/questions/1638219/millions-of-small-graphics-files-and-how-to-overcome-slow-file-system-access-on"/>
        /// </summary>
        /// <param name="name">The name of the file.</param>
        /// <returns>The <see cref="string"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe string ToFilePath(string name)
        {
            name = Uri.UnescapeDataString(name.Replace("%2520", " "));

            // Each key substring char + separator + key
            int nameLength = Path.GetFileNameWithoutExtension(name.AsSpan()).Length;
            int length = (nameLength * 2) + name.Length;
            fixed (char* namePtr = name)
            {
                return string.Create(length, (Ptr: (IntPtr)namePtr, name.Length), (chars, args) =>
                {
                    const char separator = '/';
                    const char dash = '-';
                    var nameSpan = new ReadOnlySpan<char>((char*)args.Ptr, args.Length);
                    ref char nameBase = ref MemoryMarshal.GetReference(nameSpan);
                    ref char charRef = ref MemoryMarshal.GetReference(chars);

                    // Folder
                    int index = 0;
                    for (int i = 0; i < nameLength; i++)
                    {
                        char n = Unsafe.Add(ref nameBase, i);
                        if (Array.IndexOf(InvalidPathChars, n) > -1)
                        {
                            n = dash;
                        }

                        Unsafe.Add(ref charRef, index++) = n;
                        Unsafe.Add(ref charRef, index++) = separator;
                    }

                    // File
                    for (int i = 0; i < nameSpan.Length; i++)
                    {
                        Unsafe.Add(ref charRef, index++) = Unsafe.Add(ref nameBase, i);
                    }
                });
            }
        }
    }
}
