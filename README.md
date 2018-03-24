# roxolan.storage
.NET Core library provides repository pattern abstraction of Azure blob, Azure file, and local file storage.  It allows seamless URL and stream based interactions across Azure storage, or any other data streams,
by providing low memory footprint streaming implementation to copy, delete, list contents of directory/container, etc.

Static constructors and methods load and parse azure storage accounts out of the 'default' app configuration, or one can add accounts at run time, 
either via configuration load or individual accounts.

## Usage
Reference the `roxolan.storage.tests` project for a quick understanding of how to setup and use the service.

1. Reference the following libraries:
```
roxolan.storage

```

2. Supposing you have configuration in appsettings.xml as:
```
<configuration>
  <x>
    <y>
      <a name ="x">DefaultEndpointsProtocol=https;AccountName=youraccount;AccountKey=yourkey==;BlobEndpoint=https://youraccount.blob.core.windows.net/;QueueEndpoint=https://youraccount.queue.core.windows.net/;TableEndpoint=https://youraccount.table.core.windows.net/;FileEndpoint=https://youraccount.file.core.windows.net/;</a>
    </y>
  </x>
</configuration>
```
Extension factory methods CreateItem() create appropriate derived storage class to handle local file, azure blob, or azure file operations.  Based on the type of the URI or string file path,
factory methods create a respective instance, but all classes implement IStorageItem interface:

Local file copy to local file example: 
```
            IStorageItem f = @".\PluginManager_v1.4.9_x64.zip".CreateItem(); //creates a derived storage class for local file based on the value of ".\PluginManager_v1.4.9_x64.zip"
            f.CopyToLocation(@".\x.zip",true);  //will create a derived storage class for local file based on the value of ".\x.zip" and will copy the source as a stream to the destination stream for x.zip 
```

Local file copy to azure blob example: 
```
            IStorageItem f = @".\PluginManager_v1.4.9_x64.zip".CreateItem();	//creates a derived storage class for local file based on the value of ".\PluginManager_v1.4.9_x64.zip"
            Uri uri = new Uri($"https://youraccount.blob.core.windows.net/tmp/x.zip");
            var cfg = new ConfigurationBuilder()
                                .AddJsonFile("appsettings.json", true)
                                .AddXmlFile("appsettings.xml", true)
                                .Build();								
            var destItem = uri.CreateItem(new StorageConfig(cfg));  //uses accounts from configuration to resolve URI to the corresponding storage class for the referenced item 
            destItem.Parent.CreateIfNotExists();
            f.CopyToLocation(uri.AbsoluteUri, true); //will create a derived storage class for local file based on the value of 'uri' parameter and will copy from the source stream to the destination stream
```

azure blob to azure file example: 
```
            Uri srcuri = new Uri( $"https://youraccount.blob.core.windows.net/tmp/x.zip" );
            Uri dest = new Uri( $"https://youraccount.file.core.windows.net/temp/x.zip" );

            IStorageItem f = srcuri.CreateItem();
            var destItem = dest.CreateItem();
            destItem.Parent.CreateIfNotExists();
            f.CopyToLocation(dest.AbsoluteUri, true);	//will create a derived storage class for local file based on the value of 'dest' parameter and will copy from the source stream to the destination stream
```

### Prerequisites

* .NET Core 2.0+ installed

## Running the tests

The tests in this project use the **xUnit** testing framework.


## To Do:


## Authors

- **Oleg Semenov** - *Initial work*