using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;

namespace Roxolan.Storage
{
    public class FileItemDirectory : StorageContainer
    {
        private System.IO.DirectoryInfo _dir = null;
        private IStorageContainer _parent = null;

        public FileItemDirectory(string localPath):this(new Uri(System.IO.Path.GetFullPath(localPath)))
        {
        }

        public FileItemDirectory(Uri uri) : this(uri, new StorageConfig())
        {
        }

        public FileItemDirectory(System.IO.DirectoryInfo dir) : this(dir, new StorageConfig())
        {
        }

        public FileItemDirectory(System.IO.DirectoryInfo dir, IStorageConfig configuration)
        {
            Configuration = configuration;
            URI = new Uri(dir.FullName);
            _dir = dir;
            NativeObject = _dir;
        }

        public FileItemDirectory(Uri uri, IStorageConfig configuration)
        {
            Configuration = configuration;
            URI = uri;
            if (IsCloudLocation) { throw new ArgumentException($"Directory cannot be instantiated for an invalid uri {uri}"); }
            _dir = new System.IO.DirectoryInfo(URI.LocalPath);
            NativeObject = _dir;
        }
        public override IStorageContainer Parent
        {
            get
            {
                if (_parent == null && _dir?.Parent != null && _dir.FullName != _dir.Parent?.FullName) { _parent = new FileItemDirectory(_dir.Parent, Configuration); };
                return _parent;
            }
        }

        public async override Task CreateIfNotExistsAsync()
        {
            System.IO.Directory.CreateDirectory(URI.LocalPath);
        }

        public override async Task DeleteAsync()
        {
            System.IO.Directory.Delete(URI.LocalPath, true);
        }

        public override async Task<bool> ExistsAsync()
        {
            return _dir.Exists;
        }

        public override async Task<IEnumerable<T>> ListContainedItemsAsync<T>(string filter, int maxResults, CancellationToken cancellationToken, TimeSpan timeout)
        {
            var result = System.IO.Directory.GetFiles(URI.LocalPath);

            return result as IEnumerable<T>;
        }

        public override async Task<T> ListContainedItemsSegmentedAsync<T>(string filter, int maxResults, IRequestOptions reqOps, IContinuationToken currentToken, CancellationToken cancellationToken)
        {
            var result = System.IO.Directory.GetFiles(URI.LocalPath);
            return result as T;
        }

        public async override Task FetchPropertiesAsync()
        {
            Type type = typeof(System.IO.DirectoryInfo);
            Properties = new Dictionary<string, object>();
            foreach (System.Reflection.PropertyInfo pi in type.GetProperties() )
            {
                Properties[pi.Name] = pi.GetValue(_dir);
            }            
        }
    }
}