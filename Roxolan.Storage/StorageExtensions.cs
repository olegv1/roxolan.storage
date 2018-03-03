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
    public static class StorageExtensions
    {
        public static IStorageItem CreateItem(this Uri uri, IStorageConfig storageConfig)
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

        public static IStorageItem CreateItem(this Uri uri, IUriResolver resolver = null, IStorageConfig storageConfig = null)
        {
            var r = resolver ?? new BaseUriResolver();
            return r.CreateItem(uri, storageConfig);
        }

        public static IStorageItem CreateItem(this Uri uri, IConfiguration config, IUriResolver resolver = null)
        {
            var r = resolver ?? new BaseUriResolver();
            return r.CreateItem(uri, config);
        }

        private static IStorageItem CreateCloudBlobItem(Uri uri, IStorageConfig storageConfig = null)
        {
            IStorageItem result = null;
            IStorageConfig scfg = storageConfig ?? new StorageConfig();
            result = new CloudBlobItem(uri, scfg);
            return result;
        }

        public static IStorageItem CreateCloudFileItem(this Uri uri, IStorageConfig storageConfig = null)
        {
            IStorageItem result = null;
            IStorageConfig scfg = storageConfig ?? new StorageConfig();
            result = new CloudFileItem(uri, scfg);
            return result;
        }

        private static IStorageItem CreateFileItem(Uri uri, IStorageConfig storageConfig = null)
        {
            IStorageItem result = null;
            //IStorageConfig scfg = storageConfig ?? new StorageConfig();
            result = new FileItem(uri.LocalPath);
            return result;
        }

        public static IStorageItem CreateItem(this string location, IStorageConfig storageConfig)
        {
            return CreateItem(location, new BaseUriResolver(), storageConfig);
        }
        public static IStorageItem CreateItem(this string location, IUriResolver resolver = null, IStorageConfig storageConfig = null)
        {
            var r = resolver ?? new BaseUriResolver();
            try
            {
                return r.CreateItem (new Uri(location), storageConfig);
            }
            catch (Exception ex)
            {
                return r.CreateItem(new Uri("file://" + System.IO.Path.GetFullPath(location)), storageConfig);
            }
        }
        public static IStorageItem CreateItem(this string location, IConfiguration config, IUriResolver resolver = null)
        {
            var r = resolver ?? new BaseUriResolver();
            try
            {
                return r.CreateItem (new Uri(location), config);
            }
            catch (Exception ex)
            {
                return r.CreateItem(new Uri("file://" + System.IO.Path.GetFullPath(location)), config);
            }
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
            result = new CloudFileItemDirectory(dir, scfg);
            return result;
        }
        public static IStorageContainer CreateContainer(this Uri uri, IUriResolver resolver = null, IStorageConfig storageConfig = null)
        {
            var r = resolver ?? new BaseUriResolver();
            return resolver.CreateContainer(uri, storageConfig);
        }
        public static IStorageContainer CreateContainer(this Uri uri, IConfiguration config, IUriResolver resolver = null)
        {
            var r = resolver ?? new BaseUriResolver();
            return resolver.CreateContainer(uri, config);
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

            result = new CloudBlobItemContainer(blobContainer, scfg);
            return result;
        }

        public static IStorageContainer CreateContainer(this string location, IStorageConfig storageConfig)
        {
            try
            {
                return CreateContainer(new Uri(location), new BaseUriResolver(), storageConfig);
            }
            catch (Exception ex)
            {
                    return CreateContainer(new Uri("file://" + System.IO.Path.GetFullPath(location)), new BaseUriResolver(), storageConfig);
            }
        }
        public static IStorageContainer CreateContainer(this string location, IUriResolver resolver = null, IStorageConfig storageConfig = null)
        {
            var r = resolver ?? new BaseUriResolver();
            try
            {
                return resolver.CreateContainer(new Uri(location), storageConfig);
            }
            catch (Exception ex)
            {
                return resolver.CreateContainer(new Uri("file://" + System.IO.Path.GetFullPath(location)), storageConfig);
            }
        }
        public static IStorageContainer CreateContainer(this string location, IConfiguration config, IUriResolver resolver = null)
        {
            var r = resolver ?? new BaseUriResolver();
            try
            {
                return resolver.CreateContainer(new Uri(location), config);
            }
            catch (Exception ex)
            {
                return resolver.CreateContainer(new Uri("file://" + System.IO.Path.GetFullPath(location)), config);
            }
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
        public static Uri GetParent(this Uri uri)
        {
            try
            {
                return new Uri(uri.AbsoluteUri.Remove(uri.AbsoluteUri.Length - uri.Segments.Last().Length));
            }
            catch (Exception)
            {
                return null;
            }
        }
        public static Uri AsUri(this string location)
        {
            try
            {
                return (new Uri(location));
            }
            catch (Exception ex)
            {
                return (new Uri("file://" + System.IO.Path.GetFullPath(location)));
            }
        }
    }

}
