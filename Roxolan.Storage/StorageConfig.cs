using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;

namespace Roxolan.Storage
{
    public class StorageConfig: IStorageConfig
    {
        static IConfiguration _initial = null;
        IConfiguration _cfg;
        static CloudStorageAccount[] _cfgCloudStorageAccounts = null;

        IDictionary<string, CloudStorageAccount> _fqdnDictionary = new ConcurrentDictionary<string, CloudStorageAccount>();
        IDictionary<string, CloudStorageAccount> _accountDictionary = new ConcurrentDictionary<string, CloudStorageAccount>();

        static StorageConfig()
        {
            var cfg = new ConfigurationBuilder()
                            .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                            .AddJsonFile(DefaultAppSettingsName + ".json", optional: true)
                            .AddXmlFile(DefaultAppSettingsName + ".xml", optional: true)
                            //.AddAzureKeyVault()
                            .AddEnvironmentVariables()
                            .Build();
            _initial = cfg;
            _cfgCloudStorageAccounts = FindStorageConnections(cfg.AsEnumerable().Select(x => x.Value));
        }

        public StorageConfig()
        {
            foreach (var a in _cfgCloudStorageAccounts)
            {
                AddOrUpdateAccount(a);
                if (null == DefaultAccount && Regex.IsMatch(a?.Credentials.AccountName ?? "", DefaultAccountSelector, RegexOptions.Compiled | RegexOptions.IgnoreCase))
                {
                    DefaultAccount = a;
                }
            }
        }
        public StorageConfig(IConfiguration configuration)
        {
            _cfg = configuration ?? _initial;
            _cfgCloudStorageAccounts = FindStorageConnections(_cfg.AsEnumerable().Select(x => x.Value));
            foreach (var a in _cfgCloudStorageAccounts)
            {
                AddOrUpdateAccount(a);
                if (null == DefaultAccount && Regex.IsMatch(a?.Credentials.AccountName ?? "", DefaultAccountSelector, RegexOptions.Compiled | RegexOptions.IgnoreCase))
                {
                    DefaultAccount = a;
                }
            }
        }
        private static Microsoft.WindowsAzure.Storage.CloudStorageAccount GetStorageAccount(string connStrValue)
        {
            Microsoft.WindowsAzure.Storage.CloudStorageAccount storageAccount = null;
            Microsoft.WindowsAzure.Storage.CloudStorageAccount.TryParse(connStrValue, out storageAccount);
            return storageAccount;
        }

        public static CloudStorageAccount[] FindStorageConnections(IEnumerable<string> cfgValues)
        {
            List<CloudStorageAccount> result = new List<CloudStorageAccount>();
            IEnumerable<string> values = cfgValues?.Where(v => Regex.IsMatch(v ?? "", CloudStoragePattern, RegexOptions.Compiled | RegexOptions.IgnoreCase)) ?? new string[] { };

            foreach (string conn in values)
            {
                var sa = GetStorageAccount(conn);
                if (sa != null) { result.Add(sa); }
            }
            return result.ToArray();
        }

        public CloudStorageAccount DefaultAccount { get; set; } = null;
        public IDictionary<string, CloudStorageAccount> AccountDictionary { get; protected set; } = new Dictionary<string, CloudStorageAccount>();

        public void SafeUpdateDictionary(IDictionary<string, CloudStorageAccount> dictionary, string key, CloudStorageAccount storageAccount)
        {
            try
            {
                dictionary[key] = storageAccount;
            }
            catch (Exception)
            {
                //suppress
            }
        }

        public void AddOrUpdateAccount(CloudStorageAccount sa)
        {
            if (null == sa) { throw new ArgumentNullException("sa", "CloudStorageAccount cannot be NULL"); }

            SafeUpdateDictionary(_fqdnDictionary, sa.FileEndpoint.DnsSafeHost, sa);
            SafeUpdateDictionary(_fqdnDictionary, sa.BlobEndpoint.DnsSafeHost, sa);
            SafeUpdateDictionary(_fqdnDictionary, sa.TableEndpoint.DnsSafeHost, sa);
            SafeUpdateDictionary(_fqdnDictionary, sa.QueueEndpoint.DnsSafeHost, sa);
            SafeUpdateDictionary(_accountDictionary, sa.Credentials.AccountName, sa);
        }

        public void AddOrUpdateAccount(string conn)
        {
            var sa = CloudStorageAccount.Parse(conn);
            AddOrUpdateAccount(sa);
        }

        public CloudStorageAccount GetStorageAccountByFilePath(string value)
        {
            try
            {
                string fqdn = value.Split(new string[] { @"\", "/" }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
                if (_fqdnDictionary.ContainsKey(fqdn))
                {
                    return _fqdnDictionary[fqdn];
                }
                string accnt = fqdn.Split(".".ToCharArray()).FirstOrDefault();
                if (_accountDictionary.ContainsKey(accnt))
                {
                    return _accountDictionary[accnt];
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Unable to find a configured storage account for path {value}", ex);
            }
            throw new Exception($"Unable to find a configured storage account for path {value}");
        }

        public CloudStorageAccount GetStorageAccountByHostName(string host)
        {
            try
            {
                if (_fqdnDictionary.ContainsKey(host))
                {
                    return _fqdnDictionary[host];
                }
                string accnt = host.Split(".".ToCharArray()).FirstOrDefault();
                if (_accountDictionary.ContainsKey(accnt))
                {
                    return _accountDictionary[accnt];
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Unable to find a configured storage account for host name {host}", ex);
            }
            throw new Exception($"Unable to find a configured storage account for host name {host}");
        }

        public CloudStorageAccount GetStorageAccountByName(string accountName)
        {
            try
            {
                if (_accountDictionary.ContainsKey(accountName))
                {
                    return _accountDictionary[accountName];
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Unable to find a configured storage account for account name {accountName}", ex);
            }
            throw new Exception($"Unable to find a configured storage account for account name {accountName}");
        }

        public CloudStorageAccount GetStorageAccountByUri(Uri value)
        {
            if (value.IsAbsoluteUri)
            {
                Uri uri = (value);
                if (_fqdnDictionary.ContainsKey(uri.DnsSafeHost))
                {
                    return _fqdnDictionary[uri.DnsSafeHost];
                }
                string accnt = uri.DnsSafeHost.Split(".".ToCharArray()).FirstOrDefault();
                if (_accountDictionary.ContainsKey(accnt))
                {
                    return _accountDictionary[accnt];
                }
            }
            else
            {
                throw new Exception($"Unable to find a configured storage account for malformed absolute URI {value}");
            }
            throw new Exception($"Unable to find a configured storage account for URI {value}");
        }
        public CloudStorageAccount GetStorageAccountByLocation(string value)
        {
            if (Uri.IsWellFormedUriString(value, UriKind.Absolute))
            {
                return GetStorageAccountByUri(new Uri(value));
            }
            else
            {
                return GetStorageAccountByFilePath(value);
            }
            throw new Exception($"Unable to find a configured storage account for reference {value}");
        }
        public static string CloudStoragePattern { get; set; } = "Account=|AccountName=";
        public static string DefaultAppSettingsName { get; protected set; } = "appsettings";
        public static string DefaultAccountSelector { get; protected set; } = ".";
        public static string CloudLocationPattern { get; set; } = @"\.file\.core\.windows.net|\.blob\.core\.windows.net";
        public static string CloudBlobPattern { get; set; } = @"\.blob\.core\.windows.net";
        public static string CloudFilePattern { get; set; } = @"\.file\.core\.windows.net";
    }
}
