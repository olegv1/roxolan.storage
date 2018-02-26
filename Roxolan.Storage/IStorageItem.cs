using Microsoft.WindowsAzure.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Roxolan.Storage
{
    public interface IStorageItem
    {
        Task CopyToLocationAsync(string destination, bool overwrite = false);
        Task CopyToStreamAsync(Stream destination, bool keepDestinationOpen = false);
        Task DeleteAsync();
        Task<bool> ExistsAsync();
        void CopyToLocation(string destination, bool overwrite = false);
        void CopyToStream(Stream destination, bool keepDestinationOpen = false);
        Task<Stream> OpenReadAsync();
        Task<Stream> OpenWriteAsync(bool overwrite = false, long? destMaxSize = null);
        Task FetchPropertiesAsync();
        void Delete();
        bool Exists();
        void FetchProperties();
        IStorageContainer Parent { get;  }
        IDictionary<string, object> Properties { get;  }
        bool IsCloudLocation { get;  }
        bool IsCloudFile { get;  }
        bool IsBlob { get;  }
        bool IsFile { get;  }
        Stream OpenRead();
        Stream OpenWrite(bool overwrite= false, long? destMaxSize = null);
        object NativeObject { get;  }
        Uri URI { get; }
        string CloudLocationPattern { get; set; }
        string CloudBlobPattern { get; set; }
        string CloudFilePattern { get; set; }
        IStorageConfig Configuration { get; }
        CloudStorageAccount StorageAccount { get; }
    }
}
