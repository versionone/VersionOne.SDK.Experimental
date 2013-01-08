# Deployment

Supposing the VersionOne `<install dir>` is `c:\inetpub\wwwroot\VersionOne`:

1. Copy `VersionOne.Web.Plugins.Interfaces\bin\Release\VersionOne.Web.Plugins.Interfaces.dll` into `<install dir>\bin`.
2. Create a folder `<install dir>\bin\Plugins`.
3. Copy all DLLs except the interfaces DLL from `VersionOne.Web.Plugins\bin\Release\` into the `Plugins` folder.
4. Add this line to `Web.config` in the `<httpModules>` element after the `ErrorLog` module:

```xml
<add name="ApiTranslatorFilterModule" type="VersionOne.Web.Plugins.Api.ApiTranslatorFilterModule, VersionOne.Web.Plugins.Interfaces" />     
```

In context, it should then look something like this:

```
        <add namespace="VersionOne.Web.Models" />
      </namespaces>
    </pages>
    <httpModules>
      <add name="ActivityLogger" type="VersionOne.Web.Modules.ActivityLoggingModule,VersionOne.Web" />
      <add name="ErrorLog" type="Elmah.ErrorLogModule, Elmah" />
      <add name="ApiTranslatorFilterModule" type="VersionOne.Web.Plugins.Api.ApiTranslatorFilterModule, VersionOne.Web.Plugins.Interfaces" />      
    </httpModules>
    <httpHandlers>
      <add verb="GET" path="assetdetail.v1" type="VersionOne.Web.Tools.AssetDetailHandler,VersionOne.Web" />
```

6. In IIS, for the specific app/virtual directory, select "HTTP Response Headers", and add these:

Access-Control-Allow-Methods = GET, PUT, POST, DELETE, OPTIONS
Access-Control-Allow-Origin = *
Access-Control-Allow-Headers = Authorization, Content-Type

This is not a security risk, because the VersionOne API is protected by HTTP Basic Auth. For more info, see: http://enable-cors.org/

How to Test:

First you'll need to login to the instance to get past the authentication screen and have the cookie set. After that, a query like this will do:

http://localhost/VersionOne.Web/rest-1.v1/Data/RequestPriority?acceptFormat=haljson

That will return data like this in JSON, conformant with HAL:

[
  {
    "Name": "Low",
    "Description": "",
    "Order": "43",
    "AssetState": "64",
    "AssetType": "RequestPriority",
    "_links": {
      "self": {
        "href": "/VersionOne.Web/rest-1.v1/Data/RequestPriority/167",
        "id": "RequestPriority:167"
      }
    }
  },
  {
    "Name": "Medium",
    "Description": "",
    "Order": "44",
    "AssetState": "64",
    "AssetType": "RequestPriority",
    "_links": {
      "self": {
        "href": "/VersionOne.Web/rest-1.v1/Data/RequestPriority/168",
        "id": "RequestPriority:168"
      }
    }
  },
  {
    "Name": "High",
    "Description": "",
    "Order": "45",
    "AssetState": "64",
    "AssetType": "RequestPriority",
    "_links": {
      "self": {
        "href": "/VersionOne.Web/rest-1.v1/Data/RequestPriority/169",
        "id": "RequestPriority:169"
      }
    }
  }
]

