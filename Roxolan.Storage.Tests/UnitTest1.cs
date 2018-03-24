using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.File;
using System;
using System.IO;
using Xunit;

namespace Roxolan.Storage.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void TestCopyFileToFileLocation()
        {
            IStorageItem f = @".\PluginManager_v1.4.9_x64.zip".CreateItem();
            f.CopyToLocation(@".\x.zip",true);
            Assert.Equal(Convert.ToBase64String(File.ReadAllBytes(@".\PluginManager_v1.4.9_x64.zip")), Convert.ToBase64String(File.ReadAllBytes(@".\x.zip")));
        }
        [Fact]
        public void TestCopyFileToBlobLocation()
        {
            IStorageItem f = @".\PluginManager_v1.4.9_x64.zip".CreateItem();
            Uri uri = new Uri($"https://4wkg2mcjiyss43.blob.core.windows.net/tmp/x.zip");
            var cfg = new ConfigurationBuilder()
                                .AddJsonFile("appsettings.json", true)
                                .AddXmlFile("appsettings.xml", true)
                                .Build();
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(cfg["x:y:a:x"]);
            var destItem = uri.CreateItem(new StorageConfig(cfg));
            destItem.Parent.CreateIfNotExists();
            f.CopyToLocation(uri.AbsoluteUri, true);

            byte[] bytes = null;
            using (MemoryStream ms = new MemoryStream())
            {
                (new CloudBlob(uri, storageAccount.Credentials)).DownloadToStreamAsync(ms).GetAwaiter().GetResult();
                bytes = ms.ToArray();
            }
            Assert.Equal(Convert.ToBase64String(File.ReadAllBytes(@".\PluginManager_v1.4.9_x64.zip")), Convert.ToBase64String(bytes));
        }
        [Fact]
        public void TestBlobToCloudFileLocation()
        {
            Uri srcuri = new Uri( $"https://4wkg2mcjiyss43.blob.core.windows.net/tmp/x.zip" );
            Uri dest = new Uri( $"https://4wkg2mcjiyss43.file.core.windows.net/temp/x.zip" );
            var cfg = new ConfigurationBuilder()
                                .AddJsonFile("appsettings.json", true)
                                .AddXmlFile("appsettings.xml", true)
                                .Build();
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(cfg["x:y:a:x"]);
            IStorageItem f = srcuri.CreateItem();
            var destItem = dest.CreateItem(new StorageConfig(cfg));
            destItem.Parent.CreateIfNotExists();
            f.CopyToLocation(dest.AbsoluteUri, true);

            byte[] srcbytes = null;
            using (MemoryStream ms = new MemoryStream())
            {
                (new CloudBlob(srcuri, storageAccount.Credentials)).DownloadToStreamAsync(ms).GetAwaiter().GetResult();
                srcbytes = ms.ToArray();
            } 
            byte[] destbytes = null;
            using (MemoryStream ms = new MemoryStream())
            {
                (new CloudFile(srcuri, storageAccount.Credentials)).DownloadToStreamAsync(ms).GetAwaiter().GetResult();
                destbytes = ms.ToArray();
            } 
            Assert.Equal(Convert.ToBase64String(srcbytes), Convert.ToBase64String(destbytes));
        }
        [Fact]
        public void TestCloudFileToFileStream()
        {
            Uri uri = new Uri($"https://4wkg2mcjiyss43.file.core.windows.net/temp/x.zip");
            var dest = File.OpenWrite( @".\x.zip" );
            var cfg = new ConfigurationBuilder()
                                .AddJsonFile("appsettings.json", true)
                                .AddXmlFile("appsettings.xml", true)
                                .Build();
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(cfg["x:y:a:x"]);
            IStorageItem f = uri.CreateItem();
            f.CopyToStream(dest, false);

            byte[] srcbytes = null;
            using (MemoryStream ms = new MemoryStream())
            {
                (new CloudFile(uri, storageAccount.Credentials)).DownloadToStreamAsync(ms).GetAwaiter().GetResult();
                srcbytes = ms.ToArray();
            }
            byte[] destbytes = File.ReadAllBytes(@".\x.zip");
            Assert.Equal(Convert.ToBase64String(srcbytes), Convert.ToBase64String(destbytes));
        }
    }
}
