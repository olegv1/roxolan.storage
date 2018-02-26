using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.File;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Roxolan.Storage
{
    public class CloudFileItemDirectory:StorageContainer
    {
        private CloudFileDirectory _dir;
        public CloudFileItemDirectory(Uri uri) : this(uri, new StorageConfig())
        {
        }

        public CloudFileItemDirectory(CloudFileDirectory dir) : this(dir, new StorageConfig())
        {
        }

        public CloudFileItemDirectory(CloudFileDirectory dir, IStorageConfig configuration)
        {
            Configuration = configuration;
            CloudStorageAccount sa = new CloudStorageAccount(dir.ServiceClient.Credentials, true);
            Configuration.AddOrUpdateAccount(sa);
            StorageAccount = sa;
            URI = dir.Uri;
            _dir = dir;
            NativeObject = _dir;
        }

        public CloudFileItemDirectory(Uri uri, IStorageConfig configuration)
        {
            Configuration = configuration;
            StorageAccount = configuration.GetStorageAccountByUri(uri);
            URI = uri;
            if (!IsCloudFileDirectory) { throw new ArgumentException($"CloudDirectory cannot be instantiated for an invalid uri {uri}"); }
            _dir = new CloudFileDirectory(URI, StorageAccount.Credentials);
            NativeObject = _dir;
        }
        public async override Task CreateIfNotExistsAsync()
        {
            CloudStorageAccount sa = Configuration.GetStorageAccountByUri(URI);
            CloudFileDirectory dir = new CloudFileDirectory(URI, sa.Credentials);
            CloudFileShare share = dir.Share;
            await share.CreateIfNotExistsAsync();
            dir = share.GetRootDirectoryReference();

            var directories = URI.Segments.Select(seg => seg.TrimEnd('/')).Where(str => !string.IsNullOrEmpty(str)).ToList();
            directories.RemoveAt(0); // remove the share, and leave only dirs
            var n = 0;
            while (n < directories.Count)
            {
                dir = dir.GetDirectoryReference(directories[n]);
                await dir.CreateIfNotExistsAsync();
                n = n + 1;
            }
        }

        public override async Task DeleteAsync()
        {
            await _dir.DeleteAsync();
        }

        public override Task<bool> ExistsAsync()
        {
            return _dir.ExistsAsync();
        }

        public override async Task<IEnumerable<T>> ListContainedItemsAsync<T>(string filter, int maxResults, CancellationToken cancellationToken, TimeSpan timeout)
        {
            DateTime start = DateTime.UtcNow;
            DateTime end = start.Add(timeout);
            List<IListFileItem> result = new List<IListFileItem>();
            CloudFileDirectory d = new CloudFileDirectory(URI, StorageAccount.Credentials);

            FileContinuationToken continuationToken = null;
            FileResultSegment resultSegment = null;
            do
            {
                resultSegment = await _dir.ListFilesAndDirectoriesSegmentedAsync(filter, maxResults, continuationToken, null, null, cancellationToken);
                result.AddRange(resultSegment.Results);
                if (result.Count > maxResults) { return result.Take(maxResults) as IEnumerable<T>; }
                continuationToken = resultSegment.ContinuationToken;
            }
            while (continuationToken != null && !cancellationToken.IsCancellationRequested && DateTime.UtcNow < end);
            return result as IEnumerable<T>;
        }

        public override async Task<T> ListContainedItemsSegmentedAsync<T>(string filter, int maxResults, IRequestOptions reqOps, IContinuationToken currentToken, CancellationToken cancellationToken) 
        {
            var result = await _dir.ListFilesAndDirectoriesSegmentedAsync(filter, maxResults, (FileContinuationToken)currentToken,(FileRequestOptions) reqOps, null, cancellationToken);
            return result as T; 
        }

        public async override Task FetchPropertiesAsync()
        {
            await _dir.FetchAttributesAsync();
            Properties = new Dictionary<string, object>()
            {
                ["ETag"] = _dir.Properties.ETag, //          Gets the file's ETag value.        
                ["IsServerEncrypted"] = _dir.Properties.IsServerEncrypted, //          Gets the file's server-side encryption state.         
                ["LastModified"] = _dir.Properties.LastModified //          Gets the the last - modified time for the file, expressed as a UTC value.
            };
        }
    }
}
