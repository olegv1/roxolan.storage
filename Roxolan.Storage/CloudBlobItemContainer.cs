using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Roxolan.Storage
{
    public class CloudBlobItemContainer : StorageContainer
    {
        private CloudBlob blob;
        private IStorageConfig storageConfig;
        private CloudBlobContainer blobContainer;


        public CloudBlobItemContainer(CloudBlobContainer blobContainer, IStorageConfig storageConfig)
        {
            this.blobContainer = blobContainer;
            this.storageConfig = storageConfig;
        }

        public override Task CreateIfNotExistsAsync()
        {
            throw new NotImplementedException();
        }

        public override Task DeleteAsync()
        {
            throw new NotImplementedException();
        }

        public override Task<bool> ExistsAsync()
        {
            throw new NotImplementedException();
        }

        public override Task FetchPropertiesAsync()
        {
            throw new NotImplementedException();
        }

        public override Task<IEnumerable<T>> ListContainedItemsAsync<T>(string filter, int maxResults, CancellationToken cancellationToken, TimeSpan timeout)
        {
            throw new NotImplementedException();
        }

        public override Task<T> ListContainedItemsSegmentedAsync<T>(string filter, int maxResults, IRequestOptions reqOps, IContinuationToken currentToken, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}