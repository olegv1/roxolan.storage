using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.File;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roxolan.Storage
{
    public class CloudFileItem : StorageItem
    {
        private IStorageContainer _parent = null;
        CloudFile _file = null;
        public CloudFileItem(Uri uri):this(uri,new StorageConfig())
        {
        }

        public CloudFileItem(CloudFile file):this(file,new StorageConfig())
        {
        }

        public CloudFileItem(CloudFile file, IStorageConfig configuration)
        {
            Configuration = configuration;
            CloudStorageAccount sa = new CloudStorageAccount(file.ServiceClient.Credentials, true);
            Configuration.AddOrUpdateAccount(sa);
            StorageAccount = sa;
            URI = file.Uri;
            _file = file;
            NativeObject = _file;
        }

        public CloudFileItem(Uri uri, IStorageConfig configuration)
        {
            Configuration = configuration;
            StorageAccount = configuration.GetStorageAccountByUri(uri);
            URI = uri;
            if (!IsCloudFile) { throw new ArgumentException($"CloudFile cannot be instantiated for an invalid uri {uri}"); }
            _file = new CloudFile(URI,StorageAccount.Credentials);
            NativeObject = _file;
        }
        public override IStorageContainer Parent
        {
            get
            {
                if (_parent == null && _file?.Parent != null && _file.Uri.AbsoluteUri != _file.Parent?.Uri.AbsoluteUri) { _parent = new CloudFileItemDirectory(_file.Parent, Configuration); };
                return _parent;
            }
        }

        public override async Task CopyToLocationAsync(string destination, bool overwrite = false)
        {
            IStorageItem item = destination.CreateItem(Configuration);

            using (Stream source = OpenRead())
            {
                await _file.FetchAttributesAsync();
                using (Stream dest = item.OpenWrite(overwrite, _file.Properties.Length))
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
            await _file.DeleteAsync();
        }

        public override async Task<bool> ExistsAsync()
        {
            return await _file.ExistsAsync();
        }

        public override async Task<Stream> OpenReadAsync()
        {
            return await _file.OpenReadAsync();
        }

        public override async Task<Stream> OpenWriteAsync(bool overwrite = false, long? destMaxSize = null)
        {
            if (!overwrite)
            {
                bool exists = await _file.ExistsAsync();
                if (exists) { throw new Exception($"overwrite flag is set to 'false' and resource {_file.Uri} already exists."); }
            }
            return await _file.OpenWriteAsync(destMaxSize);
        }

        public override async Task FetchPropertiesAsync()
        {
            await _file.FetchAttributesAsync();
            Properties = new Dictionary<string, object>()
            {
                ["CacheControl"] = _file.Properties.CacheControl, //Gets or sets the cache-control value stored for the file.
                ["ContentDisposition"] = _file.Properties.ContentDisposition, //Gets or sets the content - disposition value stored for the file.
                ["ContentEncoding"] = _file.Properties.ContentEncoding, //  Gets or sets the content - encoding value stored for the file.
                ["ContentLanguage"] = _file.Properties.ContentLanguage, //    Gets or sets the content - language value stored for the file.
                ["ContentMD5"] = _file.Properties.ContentMD5, //      Gets or sets the content - MD5 value stored for the file.
                ["ContentType"] = _file.Properties.ContentType, //        Gets or sets the content - type value stored for the file.
                  ["ETag"] = _file.Properties.ETag, //          Gets the file's ETag value.        
                  ["IsServerEncrypted"] = _file.Properties.IsServerEncrypted, //          Gets the file's server-side encryption state.         
                  ["LastModified"] = _file.Properties.LastModified, //          Gets the the last - modified time for the file, expressed as a UTC value.
                    ["Length"] = _file.Properties.Length //             Gets the size of the file, in bytes.            
            };
        }
    }
}
