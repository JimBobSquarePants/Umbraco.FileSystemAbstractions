// Copyright (c) James Jackson-South.
// See LICENSE for more details.

using System.Diagnostics;
using System.Threading.Tasks;
using FileSystem.Abstractions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Sample.Models;

namespace Sample.Controllers
{
    public class HomeController : Controller
    {
        private readonly IFileSystem fileSystem;
        private readonly IFileContentTypeProvider contentTypeProvider;
        private readonly IFileProvider fileProvider;

        public HomeController(
            IFileSystem fileSystem,
            IFileContentTypeProvider contentTypeProvider,
            IWebHostEnvironment environment)
        {
            this.fileSystem = fileSystem;
            this.contentTypeProvider = contentTypeProvider;
            this.fileProvider = environment.ContentRootFileProvider;
        }

        public async Task<IActionResult> Index()
        {
            // Make sure all the files are in the system.
            IDirectoryContents dir = this.fileProvider.GetDirectoryContents("files");
            foreach (IFileInfo item in dir)
            {
                if (await this.fileSystem.GetFileAsync(item.Name) is null)
                {
                    var entry = new PhysicalFileSystem.PhysicalFileSystemEntry(item, this.contentTypeProvider);
                    await this.fileSystem.PutFileAsync(entry);
                }
            }

            return this.View();
        }

        public IActionResult Privacy() => this.View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() => this.View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? this.HttpContext.TraceIdentifier });
    }
}
