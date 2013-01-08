# Deployment

Supposing your VersionOne install dir is `c:\inetpub\wwwroot\VersionOne`:

* Copy `VersionOne.Web.Plugins.Interfaces\bin\Release\VersionOne.Web.Plugins.Interfaces.dll` into `<install dir>\bin`.
* Create a folder `<install dir>\bin\Plugins`.
* Copy all DLLs except the interfaces DLL from `VersionOne.Web.Plugins\bin\Release\` into the `Plugins` folder.
* Add this line to `Web.config` in the `<httpModules>` element after the `ErrorLog` module:

```xml
<add name="ApiTranslatorFilterModule" type="VersionOne.Web.Plugins.Api.ApiTranslatorFilterModule, VersionOne.Web.Plugins.Interfaces" />     
```

In context, it should then look something like this:

```xml
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

* Lastly, In IIS, for the specific app/virtual directory, select `HTTP Response Headers` and add these:

```text
Access-Control-Allow-Methods = GET, PUT, POST, DELETE, OPTIONS
Access-Control-Allow-Origin = *
Access-Control-Allow-Headers = Authorization, Content-Type
```

To learn more about this, visit the [Enable CORS web site](http://enable-cors.org/). In summary:

> JavaScript and the web programming has grown by leaps and bounds over the years, but the same-origin policy still remains. This prevents JavaScript from making requests across domain boundaries, and has spawned various hacks for making cross-domain requests.
>
> CORS introduces a standard mechanism that can be used by all browsers for implementing cross-domain requests. The spec defines a set of headers that allow the browser and server to communicate about which requests are (and are not) allowed. CORS continues the spirit of the open web by bringing API access to all.

# How to Test

Given your VersionOne instance is installed at `htp://localhost/VersionOne.Web`:

* Visit the site and authenticate normally, so that the browser gets the authentication cookie from the server.
* Next, visit `http://localhost/VersionOne.Web/rest-1.v1/Data/RequestPriority?acceptFormat=haljson`

That will return data like this in HAL-conformant JSON result:

```json
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
```
