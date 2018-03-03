using Microsoft.Extensions.Configuration;
using System;

namespace Roxolan.Storage
{
    public interface IUriResolver
    {
        IStorageContainer CreateContainer(string location, IStorageConfig storageConfig = null);
        IStorageContainer CreateContainer(Uri uri, IStorageConfig storageConfig = null);
        IStorageItem CreateItem(string location, IStorageConfig storageConfig = null);
        IStorageItem CreateItem(Uri uri, IStorageConfig storageConfig = null);
        IStorageContainer CreateContainer(string location, IConfiguration config);
        IStorageContainer CreateContainer(Uri uri, IConfiguration config);
        IStorageItem CreateItem(string location, IConfiguration config);
        IStorageItem CreateItem(Uri uri, IConfiguration config);
    }
}