using Microsoft.WindowsAzure.Storage;
using System;
using System.Collections.Generic;
using System.Text;

namespace Roxolan.Storage
{
    public interface IStorageConfig
    {
        CloudStorageAccount DefaultAccount { get; set; }
        IDictionary<string, CloudStorageAccount> AccountDictionary { get; }

        void AddOrUpdateAccount(CloudStorageAccount sa);

        void AddOrUpdateAccount(string conn);

        CloudStorageAccount GetStorageAccountByName(string accountName);
        CloudStorageAccount GetStorageAccountByFilePath(string value);
        CloudStorageAccount GetStorageAccountByUri(Uri value);
        CloudStorageAccount GetStorageAccountByHostName(string host);
    }
}
