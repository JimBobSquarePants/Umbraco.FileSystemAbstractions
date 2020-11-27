// Copyright (c) James Jackson-South.
// See LICENSE for more details.

using System;

namespace Media.Core
{
    /// <summary>
    /// Provides options for the middleware.
    /// </summary>
    public class MediaFileResolverMiddlewareOptions
    {
        /// <summary>
        /// Gets or sets the duration to store images in the browser cache.
        /// <para>
        /// Defaults to 7 days.
        /// </para>
        /// </summary>
        public TimeSpan BrowserMaxAge { get; set; } = TimeSpan.FromDays(7);
    }
}
