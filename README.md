VersionOne.SDK.Experimental
===========================

Contains experimental code related to the VersionOne SDK.

# How you can help

VersionOne has been opening up its development process to the community. We welcome your feedback and assistance in areas that
will make your jobs, as developers and users, easier and more productive. If you have some ideas or source code to contribute, send a pull request!

# What's inside

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
