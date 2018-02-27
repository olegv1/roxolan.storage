using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Roxolan.Storage
{

    public class CloudBlobItem : StorageItem
    {
        CloudBlockBlob _blob = null;
        private IStorageContainer _parent = null;

        public CloudBlobItem(Uri uri) : this(uri, new StorageConfig())
        {
        }

        public CloudBlobItem(CloudBlockBlob blob) : this(blob, new StorageConfig())
        {
        }

        public CloudBlobItem(CloudBlockBlob blob, IStorageConfig configuration)
        {
            Configuration = configuration;
            CloudStorageAccount sa = new CloudStorageAccount(blob.ServiceClient.Credentials, true);
            Configuration.AddOrUpdateAccount(sa);
            StorageAccount = sa;
            URI = blob.Uri;
            _blob = blob;
            NativeObject = _blob;
        }

        public CloudBlobItem(Uri uri, IStorageConfig configuration)
        {
            Configuration = configuration;
            StorageAccount = configuration.GetStorageAccountByUri(uri);
            URI = uri;
            if (!IsBlob) { throw new ArgumentException($"CloudBlob cannot be instantiated for an invalid uri {uri}"); }
            _blob = new CloudBlockBlob(URI, StorageAccount.Credentials);
            NativeObject = _blob;
        }
        public override IStorageContainer Parent
        {
            get
            {
                if (_parent == null && _blob?.Parent != null && _blob.Uri.AbsoluteUri != _blob.Parent?.Uri.AbsoluteUri) { _parent = new CloudBlobItemContainer(_blob.Parent, Configuration); };
                return _parent;
            }
        }

        public override async Task CopyToLocationAsync(string destination, bool overwrite = false)
        {
            IStorageItem item = destination.CreateItem(Configuration);

            using (Stream source = OpenRead())
            {
                await _blob.FetchAttributesAsync();
                using (Stream dest = item.OpenWrite(overwrite, _blob.Properties.Length))
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
            await _blob.DeleteAsync();
        }

        public override async Task<bool> ExistsAsync()
        {
            return await _blob.ExistsAsync();
        }

        public override async Task<Stream> OpenReadAsync()
        {
            return await _blob.OpenReadAsync();
        }

        public override async Task<Stream> OpenWriteAsync(bool overwrite = false, long? destMaxSize = null)
        {
            if (!overwrite)
            {
                bool exists = await _blob.ExistsAsync();
                if (exists) { throw new Exception($"overwrite flag is set to 'false' and resource {_blob.Uri} already exists."); }
            }
            return await _blob.OpenWriteAsync();
        }

        public override async Task FetchPropertiesAsync()
        {
            await _blob.FetchAttributesAsync();
            Properties = new Dictionary<string, object>()
            {
                ["AppendBlobCommittedBlockCount"] = _blob.Properties.AppendBlobCommittedBlockCount,  // If the blob is an append blob, gets the number of committed blocks.
                ["BlobTierInferred"] = _blob.Properties.BlobTierInferred,  // Gets a value indicating if the tier of the blob has been inferred.
                ["BlobTierLastModifiedTime"] = _blob.Properties.BlobTierLastModifiedTime,  // Gets the time for when the tier of the blob was last-modified, expressed as a UTC value.
                ["BlobType"] = _blob.Properties.BlobType,  // Gets the type of the blob.
                ["CacheControl"] = _blob.Properties.CacheControl,  // Gets or sets the cache-control value stored for the blob.
                ["ContentDisposition"] = _blob.Properties.ContentDisposition,  // Gets or sets the content-disposition value stored for the blob.
                ["ContentEncoding"] = _blob.Properties.ContentEncoding,  // Gets or sets the content-encoding value stored for the blob.
                ["ContentLanguage"] = _blob.Properties.ContentLanguage,  // Gets or sets the content-language value stored for the blob.
                ["ContentMD5"] = _blob.Properties.ContentMD5,  // Gets or sets the content-MD5 value stored for the blob.
                ["ContentType"] = _blob.Properties.ContentType,  // Gets or sets the content-type value stored for the blob.
                ["DeletedTime"] = _blob.Properties.DeletedTime,  // If the blob is deleted, gets the the deletion time for the blob, expressed as a UTC value.
                ["ETag"] = _blob.Properties.ETag,  // Gets the blob's ETag value.
                ["IsIncrementalCopy"] = _blob.Properties.IsIncrementalCopy,  // Gets a value indicating whether or not this blob is an incremental copy.
                ["IsServerEncrypted"] = _blob.Properties.IsServerEncrypted,  // Gets the blob's server-side encryption state.
                ["LastModified"] = _blob.Properties.LastModified,  // Gets the the last-modified time for the blob, expressed as a UTC value.
                ["LeaseDuration"] = _blob.Properties.LeaseDuration,  // Gets the blob's lease duration.
                ["LeaseState"] = _blob.Properties.LeaseState,  // Gets the blob's lease state.
                ["LeaseStatus"] = _blob.Properties.LeaseStatus,  // Gets the blob's lease status.
                ["Length"] = _blob.Properties.Length,  // Gets the size of the blob, in bytes.
                ["PageBlobSequenceNumber"] = _blob.Properties.PageBlobSequenceNumber,  // If the blob is a page blob, gets the blob's current sequence number.
                ["PremiumPageBlobTier"] = _blob.Properties.PremiumPageBlobTier,  // Gets a value indicating the tier of the premium page blob.
                ["RehydrationStatus"] = _blob.Properties.RehydrationStatus,  // Gets a value indicating that the blob is being rehdrated and the tier of the blob once the rehydration from archive has completed.
                ["RemainingDaysBeforePermanentDelete"] = _blob.Properties.RemainingDaysBeforePermanentDelete,  // If the blob is an soft-deleted, gets the number of remaining days before the blob is permenantly deleted.
                ["StandardBlobTier"] = _blob.Properties.StandardBlobTier  // Gets a value indicating the tier of the block blob.
            };
        }

    }
}
