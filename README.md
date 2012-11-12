VersionOne.SDK.Experimental
===========================

Contains experimental code related to the VersionOne SDK.

# How you can help

VersionOne has been opening up its development process to the community. We welcome your feedback and assistance in areas that
will make your jobs, as developers and users, easier and more productive. If you have some ideas or source code to contribute, send a pull request!

# What's inside

* Server side C# / .NET plugins
* Using jQuery.Ajax API against the VersionOne API
* Server side Python (IronPython) plugins

## ApiInputTranslatorPlugins

When ready, these assemblies will plug-in to the main VersionOne core application to provide support for processing other types of data than just XML passed to the API.
The initial design is to translate input from its native format into the asset XML for ease of integration and to leverage the existing backend architecture that's been well-tested for many years.

### VersionOne.Web.Plugins.Interfaces
Specifies an interface to implement for converting incoming POST data into the VersionOne SDK asset XML format for processing. The format is [documented here](http://community.versionone.com/sdk/Documentation/DataAPI.aspx).

### VersionOne.Web.Plugins
Contains an initial cut at a JSON translator that currently allows updates to single or multiple attributes on a single asset.

#### Implementation notes

An important aspect of these classes will be independence and testability. As such, so far the only dependencies the JSON translator takes is to [`NameValueCollection`](http://msdn.microsoft.com/en-us/library/system.collections.specialized.namevaluecollection.aspx).

### VersionOne.Web.Plugins.Tests

Here are the example test cases so far:

```c#
namespace VersionOne.Web.Plugins.Tests.Api
{
    [TestFixture]
    public class JsonInputStreamToAssetTranslatorTests
    {
        private JsonInputStreamToAssetXmlTranslator _subject;

        private readonly NameValueCollection _queryString = new NameValueCollection();

        [TestFixtureSetUp]
        public void Setup()
        {
            _queryString.Add("format", "json");    
        }

        [Test]
        public void single_attribute_update_creates_set_as_default_action()
        {
            const string input = 
@"
{ ""Name"" : ""Josh"" }
";

            const string expected = 
@"<Asset>
  <Attribute name=""Name"" act=""set"">Josh</Attribute>
</Asset>";

            _subject = new JsonInputStreamToAssetXmlTranslator(input, _queryString);

            var actual = _subject.Execute().CreateNavigator().OuterXml;

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void multiple_attribute_update_has_correct_actions()
        {
            const string input =
@"
{ ""Name"" : ""Josh"", ""Phone"" : [""set"", ""555""], ""Address"": [""remove""], ""Info"" : [""add"", ""newvalue""] }
";

            const string expected =
@"<Asset>
  <Attribute name=""Name"" act=""set"">Josh</Attribute>
  <Attribute name=""Phone"" act=""set"">555</Attribute>
  <Attribute name=""Address"" act=""remove"" />
  <Attribute name=""Info"" act=""add"">newvalue</Attribute>
</Asset>";

            _subject = new JsonInputStreamToAssetXmlTranslator(input, _queryString);

            var actual = _subject.Execute().CreateNavigator().OuterXml;

            Assert.AreEqual(expected, actual);
        }
    }
}
```
### Example use case

This example uses the simple command line tool [cURL](http://curl.haxx.se/) to modify the admin user, Member/20, on my local machine.
It sets the Phone to `555` and the name to `Josh`.  The default behavior is to set the value, 
but the example for Name uses array syntax to specify the command manually. Other valid
commands are `add` and `remove` as documented in the link above, but they only apply to certain types of assets.

#### Batch format

```batch
curl -X POST --basic -u admin:admin ^
http://localhost/VersionOne.Web/rest-1.v1/Data/Member/20?fmt=json ^
--data "{'Phone':'555', 'Name':['set','Josh']}"
```

#### Bash format

It's really the same thing. Just use `\` instead of `^` to separate onto new lines.

```bash
#!/usr/bin/bash
curl -X POST --basic -u admin:admin \
http://localhost/VersionOne.Web/rest-1.v1/Data/Member/20?fmt=json \
--data "{'Phone':'555', 'Name':['set','Josh']}"
```

#### Output

When successful, you should see results like this:

```xml
<?xml version="1.0" encoding="UTF-8"?>

<Asset href="/VersionOne.Web/rest-1.v1/Data/Member/20/1925" id="Member:20:1925">
        <Attribute name="Name">Josh</Attribute>
        <Attribute name="Phone">555</Attribute>
</Asset>
```
### Notes on JSON output

Currently, you can request that the output you get back is JSON, by appending **`?accept=text/json`** to the end of the query URL:

`http://localhost/versionone.web/rest-1.v1/Data/Member/20?accept=text/json`

However, this produces very verbose JSON, similar to XML:

```json
{
  "_type" : "Asset",
  "href" : "/versionone.web/rest-1.v1/Data/Member/20",
  "id" : "Member:20",
  "Attributes" : {
    "SecurityScope.Name" : {
      "_type" : "Attribute",
      "name" : "SecurityScope.Name",
      "value" : null
    },
    "Description" : {
      "_type" : "Attribute",
      "name" : "Description",
      "value" : null
    },
    "Nickname" : {
      "_type" : "Attribute",
      "name" : "Nickname",
      "value" : "Admin"
    },
    "DefaultRole.Name" : {
      "_type" : "Attribute",
      "name" : "DefaultRole.Name",
      "value" : "Role.Name'System Admin"
    },
    "Name" : {
      "_type" : "Attribute",
      "name" : "Name",
      "value" : "Jogo"
    },
    "IsLoginDisabled" : {
      "_type" : "Attribute",
      "name" : "IsLoginDisabled",
      "value" : false
    },
    "DefaultRole" : {
      "_type" : "Relation",
      "name" : "DefaultRole",
      "value" : {
        "_type" : "Asset",
        "href" : "/versionone.web/rest-1.v1/Data/Role/1",
        "idref" : "Role:1"
      }
    }
  }
}
```
From the command line, you'd need to escape the `&` character with a `^` in a Windows Batch file, or a `\` character in Bash:

```batch
curl -X POST --basic -u admin:admin ^
http://localhost/VersionOne.Web/rest-1.v1/Data/Member/20?fmt=json^&accept=text/json ^
--data "{'Phone':'555', 'Name':['set','Jogo']}"
```

The output is now:

```json
{
  "_type" : "Asset",
  "href" : "/VersionOne.Web/rest-1.v1/Data/Member/20/1930",
  "id" : "Member:20:1930",
  "Attributes" : {
    "Phone" : {
      "_type" : "Attribute",
      "name" : "Phone",
      "value" : "555"
    },
    "Name" : {
      "_type" : "Attribute",
      "name" : "Name",
      "value" : "Jogo"
    }
  }
}
```
Again, very verbose. There's a test case in the tests project called `AssetXmlToJsonTranslatorTests` which slims this JSON down quite a bit:

So, for an asset with this XML:

```xml
<?xml version="1.0" encoding="utf-16"?>
<Asset href="/versionone.web/rest-1.v1/Data/Member/20" id="Member:20">
    <Attribute name="DefaultRole.Name">Role.Name'System Admin</Attribute>
    <Attribute name="SecurityScope.Name" />
    <Attribute name="Ideas" />
    <Attribute name="AssetState">64</Attribute>
    <Attribute name="SendConversationEmails">true</Attribute>
    <Attribute name="Username">admin</Attribute>
    <Attribute name="Followers.Name" />
    <Attribute name="Description" />
    <Attribute name="Email">admin@company.com</Attribute>
</Asset>
```
This slimmed JSON will be produced:

```json
{
  "Asset": {
    "href": "\/versionone.web\/rest-1.v1\/Data\/Member\/20",
    "id": "Member:20"
  },
  "Data": {
    "DefaultRole.Name": "Role.Name'System Admin",
    "SecurityScope.Name": "",
    "Ideas": "",
    "AssetState": "64",
    "SendConversationEmails": "true",
    "Username": "admin",
    "Followers.Name": "",
    "Description": "",
    "Email": "admin@company.com"
  }
}
```
Are you worried about the `DefaultRole.Name` and others? Fortunately, in JavaScript, you can access all object properties by string name with array brackets.

Try out [this JSFiddle example](http://jsfiddle.net/UytTn/) that demonstrates the code below:

```javascript
// #data contains the data above...
var v1data = $("#data").text();

var obj = JSON.parse(v1data);

var userName = obj.Data.Username; // or even, crazily: obj.["Data"]['Username'];
var roleName = obj.Data["DefaultRole.Name"];

$("#output").html("Hello " + userName
 + "! Your Default role name is <span class=role>"
 + roleName + "</span>");
```

This, like the rest of this experimental repository, is under active development and we welcome your feedback and contributions!

## Using jQuery.Ajax API against the VersionOne API

Here's another quick trick you can do against the API using standard [jQuery `$.ajax`](http://api.jquery.com/jQuery.ajax/) calls.

1. Load your own instance of VersionOne in Chrome
2. Hit `F12` to open the developer tools, and select the Console tab
3. Type `$` and hit enter to verify that you're on a page that has jQuery loaded
4. If you get the correct script printed, you're good to go.

**Note**: in these examples, you do not have to pass any authentication information because your existing browser cookie already gets sent to VersionOne along with the HTTP request.

Now, type this into the console and hit enter. You can use `Shift + Enter` to navigate up and down without executing:

```javascript
$.ajax({
    url: "rest-1.v1/Data/Member/20",
}).done(function(data) {  
    console.log(data)
});​
```
This should show the resulting XML document for the admin user, if you can access that. Otherwise, try a different id.

You can also do this to update an asset:

```javascript
$.ajax({
  //data: "{ 'Name' : 'adminNameChange' }", // <-- when JSON support gets baked in!
  data: "<Asset><Attribute name='Name' act='set'>adminNameChange</Attribute></Asset>",
  type : 'POST',
  url: "rest-1.v1/Data/Member/20?format=text/json",
}).done(function(data) {  
    console.log(data);
});
```
As you can see, once we get JSON input as a supported format, it will be much less verbose, and coupled with
slimmified JSON output, it will be a powerful simplification.

This will tie in well with the concept of Teamroom UI plugins that is being worked on by other VersionOne developers. 

It will thus be very easy for a custom Teamroom plugin to subscribe to allow you to program against the VersionOne API directly using the simple jQuery `$.ajax` API.

## Using JSFiddle against your own instance

Here's slightly more interactive example you can [run at JSFiddle](http://jsfiddle.net/tTpSG/4/) against your own VersionOne instance.

You'll first have to modify your `Web.config` file to have these settings in it:

```xml
<system.webServer>
    <httpProtocol>
        <customHeaders>
            <add name="Access-Control-Allow-Origin" value="*" />
            <add name="Access-Control-Allow-Headers" value="Authorization" />
            <add name="Access-Control-Allow-Methods" value="POST, GET, PUT, DELETE, OPTIONS" />
        </customHeaders>
    </httpProtocol>
</system.webServer>
```
Code:

```
var assetUrl = "http://localhost/VersionOne.Web/rest-1.v1/Data/Member/20?accept=text/json";
// assetUrl += "&format=text/json"; // <-- not quite yet!
var headers = { "Authorization": "Basic " + btoa("admin:admin") };
var initialName;

$("#run").click(function() {
    $.ajax({
        url: assetUrl,
        headers: headers 
        }).done(function (data) {  
            console.log(data);
            initialName = data.Attributes.Name.value;
            $("#output").append("Initial value: " + initialName + "<hr/>");
    
            var newName = $("#newName").val();
    
            changeName(newName, function() {
                changeName(initialName);                
            });        
    });
    
    function changeName(name, nextFunc) {
        $.ajax({
            url: assetUrl,
            //data : JSON.stringify({'Name': name}), // <-- not quite yet!
            data : "<Asset><Attribute name='Name' act='set'>" + name + "</Attribute></Asset>",
            type : 'POST',
            headers: headers
            }).done(function (data) {  
                console.log(data);
                $("#output").append("Changed to: " + data.Attributes.Name.value + "<hr/>");
                if (nextFunc) {
                    nextFunc();
                }
        });
    }
});
```

## Server side Python plugins

We're also incorporating support for Python-based plugins via the [IronPython](http://ironpython.net/) dynamic language. 

Here's an example of a Python class that implements the .NET `ITranslateApiInputToAssetXml` interface, just like the C# code does:

```python
import clr
clr.AddReference('VersionOne.Web.Plugins.Interfaces')
clr.AddReference('System.Xml')
clr.AddReference('System')

from VersionOne.Web.Plugins.Api import (
    ITranslateApiInputToAssetXml
)
from System.Xml.XPath import (
    XPathDocument
)
from System.IO import (
    StringReader
)

class TranslateYamlInputToAssetXml (ITranslateApiInputToAssetXml):
    def CanTranslate(self, contentType):        
        return contentType.lower() in map(str.lower, ['text/yaml', 'application/yaml', 'yaml'])
    
    def Execute(self, input):
        output = '<Asset><Attribute name="Name" act="set">' + input + '</Attribute></Asset>'
        reader = StringReader(output)
        doc = XPathDocument(reader)
        return doc
```

Here's what we have so far to test this:

```C#
namespace VersionOne.Web.Plugins.Python.Tests
{
    public static class PythonPluginLoader
    {
        public static IEnumerable<T> LoadPlugins<T>(string path)
        {
            var engine = IronPython.Hosting.Python.CreateEngine();
            var script = engine.CreateScriptSourceFromFile(path);
            var code = script.Compile();
            var scope = engine.CreateScope();
            code.Execute(scope);

            var instances = (from obj in scope.GetItems().Where(kvp => kvp.Value is PythonType)
                    let value = obj.Value
                    where 
                        obj.Key != typeof (T).Name 
                        &&  PythonOps.IsSubClass(value, 
                            DynamicHelpers.GetPythonTypeFromType(typeof (T)))
                    select (T) value()).ToList();
            
            return instances;
        }
    }

    [TestFixture]
    public class PythonPluginLoaderTests
    {
        private ITranslateApiInputToAssetXml _subject;

        [TestFixtureSetUp]
        public void Setup()
        {
            var path = GetScriptPath();

            var plugins = PythonPluginLoader.LoadPlugins<ITranslateApiInputToAssetXml>(path).ToList();

            Assert.AreEqual(1, plugins.Count);

            _subject = plugins[0];
        }

        private static string GetScriptPath()
        {
            var path = System.Reflection.Assembly.GetExecutingAssembly().EscapedCodeBase;
            path = path.Substring(0, path.LastIndexOf("/") + 1);
            path = path.Substring(path.IndexOf("C:"));

            path += "VersionOne.Web.Plugins.Python.py";
            return path;
        }

        [TestCase("text/xml", false)]
        [TestCase("text/yaml", true)]
        [TestCase("yaml", true)]
        [TestCase("application/yaml", true)]
        [TestCase("AppLiCAtioN/yAMl", true)]
        [TestCase("", false)]
        //[TestCase(null, false)]
        public void CanProcess_correct_content_types(string contentType, bool expected)
        {
            var actual = _subject.CanTranslate(contentType);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Execute_returns_correctly()
        {
            var input = "Testing Now";
            var expected =
@"<Asset>
  <Attribute name=""Name"" act=""set"">" + input + @"</Attribute>
</Asset>";

            var actual = _subject.Execute(input).CreateNavigator().OuterXml;

            Assert.AreEqual(expected, actual);
        }
    }
}
```
