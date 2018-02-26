using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Roxolan.Storage
{
    internal class FileItem : StorageItem
    {
        public FileItem(string localPath)
        {
            URI =  new Uri(localPath);
        }
        FileInfo _file = null;
        public FileItem(Uri uri) : this(uri, new StorageConfig())
        {
        }

        public FileItem(FileInfo file) : this(file, new StorageConfig())
        {
        }

        public FileItem(FileInfo file, IStorageConfig configuration)
        {
            Configuration = configuration;
            URI = new Uri(file.FullName);
            _file = file;
            NativeObject = _file;
        }

        public FileItem(Uri uri, IStorageConfig configuration)
        {
            Configuration = configuration;
            StorageAccount = configuration.GetStorageAccountByUri(uri);
            URI = uri;
            if (IsCloudLocation) { throw new ArgumentException($"File cannot be instantiated for an invalid uri {uri}"); }
            _file = new FileInfo(URI.LocalPath);
            NativeObject = _file;
        }

        public override async Task CopyToLocationAsync(string destination, bool overwrite = false)
        {
            IStorageItem item = destination.CreateItem(Configuration);

            using (Stream source = OpenRead())
            {
                using (Stream dest = item.OpenWrite(overwrite))
                {
                    await source.CopyToAsync(dest);
                    await dest.FlushAsync();
                }
            }
        }

        public override async Task CopyToStreamAsync(Stream destination, bool keepDestinationOpen = false)
        {
            using (Stream source = OpenRead())
            {
                if (keepDestinationOpen)
                {
                    await source.CopyToAsync(destination);
                    await destination.FlushAsync();
                }
                else
                {
                    using (destination)
                    {
                        await source.CopyToAsync(destination);
                        await destination.FlushAsync();
                    }
                }
            }
        }

        public override async Task DeleteAsync()
        {
            _file.Delete();
        }

        public override async Task<bool> ExistsAsync()
        {
            return _file.Exists;
        }

        public override async Task<Stream> OpenReadAsync()
        {
            return _file.OpenRead();
        }

        public override async Task<Stream> OpenWriteAsync(bool overwrite = false, long? destMaxSize = null)
        {
            return _file.OpenWrite();
        }

        public override async Task FetchPropertiesAsync()
        {
            Type type = typeof(System.IO.FileInfo);
            Properties = new Dictionary<string, object>();
            foreach (System.Reflection.PropertyInfo pi in type.GetProperties())
            {
                Properties[pi.Name] = pi.GetValue(_file);
            }
        }
    }
}