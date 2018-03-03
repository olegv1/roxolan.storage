using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.File;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Roxolan.Storage
{
    public class BaseUriResolver : IUriResolver
    {
        public BaseUriResolver()
        {

        }
        public IStorageItem CreateItem(Uri uri, IStorageConfig storageConfig = null)
        {
            if (Regex.IsMatch(uri?.DnsSafeHost ?? "", StorageConfig.CloudFilePattern, RegexOptions.Compiled | RegexOptions.IgnoreCase))
            {
                return CreateCloudFileItem(uri, storageConfig);
            }
            if (Regex.IsMatch(uri?.DnsSafeHost ?? "", StorageConfig.CloudBlobPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase))
            {
                return CreateCloudBlobItem(uri, storageConfig);
            }
            return CreateFileItem(uri);
        }

        protected IStorageItem CreateCloudBlobItem(Uri uri, IStorageConfig storageConfig = null)
        {
            IStorageItem result = null;
            IStorageConfig scfg = storageConfig ?? new StorageConfig();
            result = new CloudBlobItem(uri, scfg);
            return result;
        }

        protected IStorageItem CreateCloudFileItem(Uri uri, IStorageConfig storageConfig = null)
        {
            IStorageItem result = null;
            IStorageConfig scfg = storageConfig ?? new StorageConfig();
            result = new CloudFileItem(uri, scfg);
            return result;
        }

        protected IStorageItem CreateFileItem(Uri uri, IStorageConfig storageConfig = null)
        {
            IStorageItem result = null;
            //IStorageConfig scfg = storageConfig ?? new StorageConfig();
            result = new FileItem(uri.LocalPath);
            return result;
        }

        public IStorageItem CreateItem(string location, IStorageConfig storageConfig = null)
        {
            try
            {
                return CreateItem(new Uri(location));
            }
            catch (Exception ex)
            {
                if (!System.IO.Path.IsPathRooted(location))
                {
                    return CreateItem(new Uri(System.IO.Path.GetFullPath(location)));
                }
                throw ex;
            }
        }
        protected IStorageContainer CreateCloudFileContainer(Uri uri, IStorageConfig storageConfig = null)
        {
            IStorageContainer result = null;
            IStorageConfig scfg = storageConfig ?? new StorageConfig();
            CloudStorageAccount sa = scfg.GetStorageAccountByUri(uri);
            CloudFileDirectory dir = new CloudFileDirectory(uri, sa.Credentials);
            CloudFileShare share = dir.Share;
            share.CreateIfNotExistsAsync().GetAwaiter().GetResult();
            dir = share.GetRootDirectoryReference();

            var directories = uri.Segments.Select(seg => seg.TrimEnd('/')).Where(str => !string.IsNullOrEmpty(str)).ToList();
            directories.RemoveAt(0); // remove the share, and leave only dirs
            var n = 0;
            while (n < directories.Count)
            {
                dir = dir.GetDirectoryReference(directories[n]);
                dir.CreateIfNotExistsAsync().GetAwaiter().GetResult();
                n = n + 1;
            }
            result = new CloudFileItemDirectory(dir, scfg);
            return result;
        }
        public IStorageContainer CreateContainer(Uri uri, IStorageConfig storageConfig = null)
        {
            if (Regex.IsMatch(uri?.DnsSafeHost ?? "", StorageConfig.CloudFilePattern, RegexOptions.Compiled | RegexOptions.IgnoreCase))
            {
                return CreateCloudFileContainer(uri, storageConfig);
            }
            if (Regex.IsMatch(uri?.DnsSafeHost ?? "", StorageConfig.CloudBlobPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase))
            {
                return CreateCloudBlobContainer(uri, storageConfig);
            }
            return CreateFileItemDirectory(uri);
        }

        protected IStorageContainer CreateFileItemDirectory(Uri uri)
        {
            IStorageContainer result = null;
            result = new FileItemDirectory(uri.LocalPath);
            return result;
        }

        protected IStorageContainer CreateCloudBlobContainer(Uri uri, IStorageConfig storageConfig = null)
        {
            IStorageContainer result = null;
            IStorageConfig scfg = storageConfig ?? new StorageConfig();
            CloudStorageAccount sa = scfg.GetStorageAccountByUri(uri);
            CloudBlob blob = new CloudBlob(uri, sa.Credentials);
            CloudBlobContainer blobContainer = blob.Container;
            blobContainer.CreateIfNotExistsAsync().GetAwaiter().GetResult();

            result = new CloudBlobItemContainer(blobContainer, scfg);
            return result;
        }

        public IStorageContainer CreateContainer(string location, IStorageConfig storageConfig = null)
        {
            return CreateContainer(new Uri(location), storageConfig);
        }

        public IStorageContainer CreateContainer(string location, IConfiguration config) => CreateContainer(location, new StorageConfig(config));

        public IStorageContainer CreateContainer(Uri uri, IConfiguration config) => CreateContainer(uri, new StorageConfig(config));

        public IStorageItem CreateItem(string location, IConfiguration config) => CreateItem(location, new StorageConfig(config));

        public IStorageItem CreateItem(Uri uri, IConfiguration config) => CreateItem(uri, new StorageConfig(config));
    }
}
