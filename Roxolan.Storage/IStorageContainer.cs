using Microsoft.WindowsAzure.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Roxolan.Storage
{
    public interface IStorageContainer
    {
        Task<IEnumerable<T>> ListContainedItemsAsync<T>(string filter);
        Task<IEnumerable<T>> ListContainedItemsAsync<T>(string filter, int maxResults, CancellationToken cancellationToken, TimeSpan timeout);
        Task<T> ListContainedItemsSegmentedAsync<T>(string filter, int maxResults, IRequestOptions reqOps, IContinuationToken currentToken, CancellationToken cancellationToken) where T : class;
        IEnumerable<T> ListContainedItems<T>(string filter);
        IEnumerable<T> ListContainedItems<T>(string filter, int maxResults, TimeSpan timeout);
        T ListContainedItemsSegmented<T>(string filter, int maxResults, IRequestOptions reqOps, IContinuationToken currentToken) where T : class;
        Task FetchPropertiesAsync();
        Task DeleteAsync();
        Task<bool> ExistsAsync();
        void FetchProperties();
        void Delete();
        bool Exists();
        IStorageConfig  Configuration { get; }
        IStorageContainer Parent { get;  }
        IDictionary<string, object> Properties { get;  }
        bool IsCloudLocation { get;  }
        bool IsCloudFileDirectory { get;  }
        bool IsBlobContainer { get;  }
        bool IsDirectory { get;  }
        object NativeObject { get;  }
        Uri URI { get;  }
        string CloudLocationPattern { get; set; }
        string CloudBlobPattern { get; set; }
        string CloudFilePattern { get; set; }
        CloudStorageAccount StorageAccount { get; }

        Task CreateIfNotExistsAsync();
        void CreateIfNotExists();
    }
}
