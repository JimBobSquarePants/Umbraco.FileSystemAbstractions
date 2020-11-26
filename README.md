# Umbraco.FileSystemAbstractions
An experimental design for a new `IFileSystem` for the Umbraco CMS VNext.

The goal is to design a lightweight, environment agnostic implementation that does not require
additional implementation detail for Umbraco Deploy.

TODO:
- [x] Design `IFileSystem` and `IFileSystemEntry`
- [x] Design`IFileContentTypeProvider` and implementation.
- [x] `PhysicalFileSytem` implementation
- [x] `AzureBlobFileSytem` implementation
- [x] Refactor `Udi` into efficient struct format.
- [ ] Implement `MediaFileResolverMiddleware`(_partially done_)
- [ ] Wire up sample site.
