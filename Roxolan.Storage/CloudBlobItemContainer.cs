using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Roxolan.Storage
{
    public class CloudBlobItemContainer : StorageContainer
    {
        private CloudBlobDirectory _dir = null;
        private IStorageContainer _parent = null;

        public CloudBlobItemContainer(CloudBlobContainer blobContainer, IStorageConfig configuration)
        {
            Configuration = configuration;
            CloudStorageAccount sa = new CloudStorageAccount(blobContainer.ServiceClient.Credentials, true);
            Configuration.AddOrUpdateAccount(sa);
            StorageAccount = sa;
            _dir = blobContainer.GetDirectoryReference("notvaliddir").Parent;
            URI = blobContainer.Uri;
            NativeObject = _dir;
        }

        public CloudBlobItemContainer(Uri uri) : this(uri, new StorageConfig())
        {
        }

        public CloudBlobItemContainer(CloudBlobDirectory dir) : this(dir, new StorageConfig())
        {
        }

        public CloudBlobItemContainer(CloudBlobDirectory dir, IStorageConfig configuration)
        {
            Configuration = configuration;
            CloudStorageAccount sa = new CloudStorageAccount(dir.ServiceClient.Credentials, true);
            Configuration.AddOrUpdateAccount(sa);
            StorageAccount = sa;
            _dir = dir;
            URI = dir.Uri;
            NativeObject = dir;
        }

        public CloudBlobItemContainer(Uri uri, IStorageConfig configuration)
        {
            Configuration = configuration;
            StorageAccount = configuration.GetStorageAccountByUri(uri);
            URI = uri;
            if (!IsBlobContainer) { throw new ArgumentException($"CloudDirectory cannot be instantiated for an invalid uri {uri}"); }
            CloudBlob blob = new CloudBlob( new Uri(URI.AbsoluteUri.TrimEnd('/') + "/notvaliddir"), StorageAccount.Credentials);
            _dir = blob.Parent;
            NativeObject = _dir;
        }
        public override IStorageContainer Parent
        {
            get
            {
                if (_parent == null && _dir?.Parent != null && _dir.Uri.AbsoluteUri != _dir.Parent?.Uri.AbsoluteUri) { _parent = new CloudBlobItemContainer(_dir.Parent, Configuration); };
                return _parent;
            }
        }

        public async override Task CreateIfNotExistsAsync()
        {
            CloudStorageAccount sa = Configuration.GetStorageAccountByUri(URI);
            CloudBlob blob = new CloudBlob(new Uri(URI.AbsoluteUri.TrimEnd('/') + "/notvaliddir"), StorageAccount.Credentials);
            CloudBlobContainer blobContainer = blob.Container;
            var dir = blob.Parent;
            await blobContainer.CreateIfNotExistsAsync();
        }

        public override async Task DeleteAsync()
        {
        }

        public override async Task<bool> ExistsAsync()
        {
            return await _dir.Container.ExistsAsync();
        }

        public override async Task<IEnumerable<T>> ListContainedItemsAsync<T>(string filter, int maxResults, CancellationToken cancellationToken, TimeSpan timeout)
        {
            DateTime start = DateTime.UtcNow;
            DateTime end = start.Add(timeout);
            List<IListBlobItem> result = new List<IListBlobItem>();
            CloudBlobDirectory d = _dir;

            BlobContinuationToken continuationToken = null;
            BlobResultSegment resultSegment = null;
            //convert filename filter to regex
            string filterRegEx = filter.Replace("*", "(.*)").Replace("?","(.)");
            do
            {
                //resultSegment = await _dir.ListBlobsSegmentedAsync(filter, maxResults, continuationToken, null, null, cancellationToken);
                resultSegment = await _dir.ListBlobsSegmentedAsync(true, BlobListingDetails.None, maxResults, continuationToken, null, null, cancellationToken);
                ///todo apply filter after the fact
                if (!string.IsNullOrEmpty(filter))
                {
                    result.AddRange( resultSegment.Results.Where( bi => Regex.IsMatch(bi.Uri.Segments.Last(),filterRegEx, RegexOptions.Compiled) ) );
                }
                else
                {
                    result.AddRange(resultSegment.Results);
                }
                if (result.Count > maxResults) { return result.Take(maxResults) as IEnumerable<T>; }
                continuationToken = resultSegment.ContinuationToken;
            }
            while (continuationToken != null && !cancellationToken.IsCancellationRequested && DateTime.UtcNow < end);
            return result as IEnumerable<T>;
        }

        public override async Task<T> ListContainedItemsSegmentedAsync<T>(string filter, int maxResults, IRequestOptions reqOps, IContinuationToken currentToken, CancellationToken cancellationToken)
        {
            //convert filename filter to regex
            string filterRegEx = filter.Replace("*", "(.*)").Replace("?","(.)");
            var result = await _dir.ListBlobsSegmentedAsync(true, BlobListingDetails.None, maxResults, (BlobContinuationToken)currentToken, null, null, cancellationToken);
            if (!string.IsNullOrEmpty(filter))
            {
                ///todo apply filter after the fact
                var r = new BlobResultSegment(result.Results.Where(bi => Regex.IsMatch(bi.Uri.Segments.Last(), filterRegEx, RegexOptions.Compiled)), result.ContinuationToken);
                return r as T;
            }
            return result as T;
        }

        public async override Task FetchPropertiesAsync()
        {
            await _dir.Container.FetchAttributesAsync();
            Properties = new Dictionary<string, object>()
            {
                ["ETag"] = _dir.Container.Properties.ETag, //          Gets the file's ETag value.        
                ["LeaseDuration"] = _dir.Container.Properties.LeaseDuration, //          Gets the container's lease duration.        
                ["LeaseState"] = _dir.Container.Properties.LeaseState, //          Gets the container's lease state.        
                ["LeaseStatus"] = _dir.Container.Properties.LeaseStatus, //          Gets the container's LeaseStatus.        
                ["PublicAccess"] = _dir.Container.Properties.PublicAccess, //          Gets the container's PublicAccess.        
                ["LastModified"] = _dir.Container.Properties.LastModified //          Gets the the last - modified time for the file, expressed as a UTC value.
            };
        }
    }
}