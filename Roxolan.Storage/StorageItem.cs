using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;

namespace Roxolan.Storage
{
    public abstract class StorageItem : IStorageItem
    {
        public IStorageContainer Parent { get; protected set; } = null;

        public IDictionary<string, object> Properties { get; protected set; } = null;

        public bool IsCloudLocation
        {
            get
            {
                return Regex.IsMatch(URI?.DnsSafeHost??"", CloudLocationPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            }
        }

        public bool IsCloudFile
        {
            get
            {
                return Regex.IsMatch(URI?.DnsSafeHost ?? "", CloudFilePattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            }
        }
        
        public bool IsBlob
        {
            get
            {
                return Regex.IsMatch(URI?.DnsSafeHost ?? "", CloudBlobPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            }
        }

        public bool IsFile { get { return URI?.IsFile ?? false; } }

        public object NativeObject { get; protected set; } = null;

        public void CopyToLocation(string destination, bool overwrite = false) => CopyToLocationAsync(destination,overwrite).GetAwaiter().GetResult();
        public void MoveToLocation(string destination, bool overwrite = false) => MoveToLocationAsync(destination,overwrite).GetAwaiter().GetResult();

        public void CopyToStream(Stream destination, bool keepDestinationOpen = false) => CopyToStreamAsync(destination, keepDestinationOpen).GetAwaiter().GetResult();
        public void Delete() => DeleteAsync().GetAwaiter().GetResult();
        public bool Exists() => ExistsAsync().GetAwaiter().GetResult();
        public void FetchProperties() => FetchPropertiesAsync().GetAwaiter().GetResult();

        public abstract Task CopyToLocationAsync(string destination, bool overwrite = false);

        public async Task MoveToLocationAsync(string destination, bool overwrite)
        {
            await CopyToLocationAsync(destination,overwrite);
            await DeleteAsync();
        }
        public abstract Task CopyToStreamAsync(Stream destination, bool keepDestinationOpen = false);
        public abstract Task DeleteAsync();
        public abstract Task<bool> ExistsAsync();
        public abstract Task FetchPropertiesAsync();

        public abstract Task<Stream> OpenReadAsync();

        public abstract Task<Stream> OpenWriteAsync(bool overwrite = false, long? destMaxSize = null);
        public Stream OpenRead() => OpenReadAsync().GetAwaiter().GetResult();

        public Stream OpenWrite(bool overwrite = false, long? destMaxSize = null) => OpenWriteAsync(overwrite,destMaxSize).GetAwaiter().GetResult();
        public Uri URI { get; protected set; } = null;
        public string CloudLocationPattern { get; set; } = StorageConfig.CloudLocationPattern;
        public string CloudBlobPattern { get;  set; } = StorageConfig.CloudBlobPattern;
        public string CloudFilePattern { get;  set; } =  StorageConfig.CloudFilePattern;
        public IStorageConfig Configuration { get; protected set; }

        public CloudStorageAccount StorageAccount { get; protected set; }
    }
}
