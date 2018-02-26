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
    public static class StorageExtensions
    {
        public static IStorageItem CreateItem(this Uri uri, IStorageConfig storageConfig = null)
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

        private static IStorageItem CreateCloudBlobItem(Uri uri, IStorageConfig storageConfig = null)
        {
            IStorageItem result = null;
            IStorageConfig scfg = storageConfig ?? new StorageConfig();
            result = new CloudBlobItem(uri, storageConfig);
            return result;
        }

        public static IStorageItem CreateCloudFileItem(this Uri uri, IStorageConfig storageConfig = null)
        {
            IStorageItem result = null;
            IStorageConfig scfg = storageConfig ?? new StorageConfig();
            result = new CloudFileItem(uri, storageConfig);
            return result;
        }

        private static IStorageItem CreateFileItem(Uri uri, IStorageConfig storageConfig = null)
        {
            IStorageItem result = null;
            IStorageConfig scfg = storageConfig ?? new StorageConfig();
            result = new FileItem(uri.LocalPath);
            return result;
        }

        public static IStorageItem CreateItem(this string location, IStorageConfig storageConfig = null)
        {
            return CreateItem (new Uri(location));
        }
        private static IStorageContainer CreateCloudFileContainer(this Uri uri, IStorageConfig storageConfig = null)
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
            result = new CloudFileItemDirectory(dir, storageConfig);
            return result;
        }
        public static IStorageContainer CreateContainer(this Uri uri, IStorageConfig storageConfig = null)
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

        private static IStorageContainer CreateFileItemDirectory(Uri uri)
        {
            IStorageContainer result = null;
            result = new FileItemDirectory(uri.LocalPath);
            return result;
        }

        private static IStorageContainer CreateCloudBlobContainer(Uri uri, IStorageConfig storageConfig = null)
        {
            IStorageContainer result = null;
            IStorageConfig scfg = storageConfig ?? new StorageConfig();
            CloudStorageAccount sa = scfg.GetStorageAccountByUri(uri);
            CloudBlob blob = new CloudBlob(uri, sa.Credentials);
            CloudBlobContainer blobContainer = blob.Container;
            blobContainer.CreateIfNotExistsAsync().GetAwaiter().GetResult();

            result = new CloudBlobItemContainer(blobContainer, storageConfig);
            return result;
        }

        public static IStorageContainer CreateContainer(this string location, IStorageConfig storageConfig = null)
        {
            return CreateContainer(new Uri(location), storageConfig);
        }
        public static Uri ToCloudFileUri(this string strPath)
        {
            Uri uri = (Uri.IsWellFormedUriString(strPath, UriKind.Absolute)) ?
                                    new Uri(strPath) : new Uri(strPath.Replace(@"\\", "https://").Replace(@"\", "/"));
            return uri;
        }
        public static string ToCloudFileNetSharePath(this Uri uri)
        {
            return Uri.UnescapeDataString(uri.ToString().Replace("https://", @"\\").Replace("/", @"\"));
        }
    }

}
