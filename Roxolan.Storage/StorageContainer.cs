using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.File;

namespace Roxolan.Storage
{
    public abstract class StorageContainer:IStorageContainer
    {
        public abstract IStorageContainer Parent { get; }

        public IDictionary<string, object> Properties  { get; protected set; }
        public string CloudLocationPattern { get; set; } = StorageConfig.CloudLocationPattern;
        public string CloudBlobPattern { get; set; } = StorageConfig.CloudBlobPattern;
        public string CloudFilePattern { get; set; } = StorageConfig.CloudFilePattern;
        public TimeSpan DefaultListingTimeout { get; protected set; } = TimeSpan.FromHours(1);
        public int DefaultMaxResults { get; private set; } = 10000000;

        public bool IsCloudLocation
        {
            get
            {
                return Regex.IsMatch(URI?.DnsSafeHost ?? "", CloudBlobPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            }
        }

        public bool IsCloudFileDirectory
        {
            get
            {
                return Regex.IsMatch(URI?.DnsSafeHost ?? "", CloudFilePattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            }
        }

        public bool IsBlobContainer
        {
            get
            {
                return Regex.IsMatch(URI?.DnsSafeHost ?? "", CloudBlobPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            }
        }
        public CloudStorageAccount StorageAccount { get; protected set; }

        public bool IsDirectory { get { return URI?.IsFile ?? false; } }
        public object NativeObject  { get; protected set; }

        public void Delete() => DeleteAsync().GetAwaiter().GetResult();

        public abstract Task DeleteAsync();

        public bool Exists() => ExistsAsync().GetAwaiter().GetResult();

        public abstract Task<bool> ExistsAsync();

        public IEnumerable<T> ListContainedItems<T>(string filter) => ListContainedItemsAsync<T>(filter).GetAwaiter().GetResult();

        public IEnumerable<T> ListContainedItems<T>(string filter, int maxResults, TimeSpan timeout) => ListContainedItemsAsync<T>(filter, maxResults, new CancellationTokenSource(timeout).Token, timeout).GetAwaiter().GetResult();

        public async Task<IEnumerable<T>> ListContainedItemsAsync<T>(string filter)
        { return await ListContainedItemsAsync<T>(filter, DefaultMaxResults, new CancellationTokenSource(DefaultListingTimeout).Token, DefaultListingTimeout); }

        public abstract Task<IEnumerable<T>> ListContainedItemsAsync<T>(string filter, int maxResults, CancellationToken cancellationToken, TimeSpan timeout);

        public T ListContainedItemsSegmented<T>(string filter, int maxResults, IRequestOptions reqOps, IContinuationToken currentToken) where T : class => ListContainedItemsSegmentedAsync<T>(filter, maxResults, reqOps, currentToken, new CancellationTokenSource(DefaultListingTimeout).Token).GetAwaiter().GetResult();

        public abstract Task<T> ListContainedItemsSegmentedAsync<T>(string filter, int maxResults, IRequestOptions reqOps, IContinuationToken currentToken, CancellationToken cancellationToken) where T : class;

        public void FetchProperties() => FetchPropertiesAsync().GetAwaiter().GetResult();

        public abstract Task FetchPropertiesAsync();

        public IStorageConfig Configuration { get; protected set; }

        public abstract Task CreateIfNotExistsAsync();

        public void CreateIfNotExists() => CreateIfNotExistsAsync().GetAwaiter().GetResult();

        public Uri URI { get; protected set; }
    }
}
