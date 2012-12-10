# Contributing VersionOne API Input & Output Translator Plugins

VersionOne's core HTTP API accepts and produces Asset XML documents in response to incoming queries and commands. The API has been stable for a number of years.

Read more about the [VersionOne Core Data API](http://community.versionone.com/sdk/Documentation/DataAPI.aspx) if you'd like more background information.

## Lighter Alternatives to XML Needed

But, XML is a very verbose and heavy syntax for data exchange. Alternative formats, such as [JSON](http://en.wikipedia.org/wiki/JSON) (JavaScript Object Notation) and [YAML](http://en.wikipedia.org/wiki/YAML) (YAML Ain't Markup Language) have become more popular in recent years.

To enable easier integration with external programs and even within VersionOne's core product, we're adding a mechanism for contributing **VersionOne API Input & Output Translator Plugins**.

These plugins are very simple to author and add to VersionOne. An **API Input Translator Plugin** is passed a string and must produce an `XPathDocument` result that conforms to the [VersionOne Core API XML format](http://community.versionone.com/sdk/Documentation/DataAPI.aspx). Similarly, an **API Output Translator Plugin** is passesd a complete Asset XML document and must translate it to an alternative output.

# How To Implement VersionOne API Input & Output Translator Plugins

Two interfaces in [VersionOne.SDK.Experimental](https://github.com/versionone/versionone.sdk.experimental) form the basis of plugins:

`ITranslateApiInputToAssetXml` defines a contract for translating a given input format into the standard [Asset Xml](http://community.versionone.com/sdk/Documentation/DataAPI.aspx) input that the API takes now.
`ITranslateAssetXmlOutputToContentType` defines a contract for translating the Asset Xml output into a different output format.

```c#
public interface ITranslateApiInputToAssetXml
{
    bool CanTranslate(string contentType);
    XPathDocument Execute(string input);
}

public interface ITranslateAssetXmlOutputToContentType
{
    bool CanTranslate(string contentType);
    string Execute(string input);
}
```

All you have to do is implement one of those interfaces and then add the binary assembly to application, described next, for VersionOne to recognize the plugin.

## Installing an API Plugin

Once you've written and compiled the translator plugin, simply add a copy of it, and any referenced DLLs it needs, to the `bin\Plugins` folder of your VersionOne instance.

# Built-In Plugins Suppoting HAL-compliant JSON and YAML

We've implemented a few built-in plugins that you can take advantage of already. These plugins utilize Hypertext Application Language format, HAL, as the basis of both input and output, as described next.

## HAL, The Hypertext Application Language Format

[HAL has been proposed as an IETF standard](http://tools.ietf.org/html/draft-kelly-json-hal-03) for *a media type for representing resources and
   their relations as hypermedia*. We feel it is a good format that provides a slimmer alternative than the existing VersionOne JSON output option.
   
It forms the basis of input and output formats for the rest of the plugins described in this document.   

## More About HAL
* [IETF Proposal](http://tools.ietf.org/html/draft-kelly-json-hal-03)
* [Annotated Specification](http://stateless.co/hal_specification.html)
* [GitHub Repo](https://github.com/mikekelly/hal_specification)
* [JSON Linking with HAL](http://blog.stateless.co/post/13296666138/json-linking-with-hal)

## [BaseTranslateApiInputToAssetXml](https://github.com/versionone/VersionOne.SDK.Experimental/blob/master/ApiInputTranslatorPlugins/VersionOne.Web.Plugins/Api/BaseTranslateApiHalInputToAssetXml.cs) 

While not a requirement for you to inherit from when writing your own plugin, this class provides an abstract base that produces Asset XML, given simple string and list inputs.

**NOTE:** this could certainly use some generic strong typing to make it more concrete than `object`, but then again, the loose typing keeps it simple and the derived class just needs to cast the inputs.

## [TranslateHalJsonHalInputToAssetXml](https://github.com/versionone/VersionOne.SDK.Experimental/blob/master/ApiInputTranslatorPlugins/VersionOne.Web.Plugins/Api/TranslateHalJsonHalInputToAssetXml.cs) 

Translates incoming HAL-compliant JSON into Asset XML.

### HAL JSON Example Test Case

```c#
        [Test]
        public void create_asset_with_multiple_attributes_and_single_relation()
        {
            const string input =
@"
{
    Name:""Commit"",
    URL:""http://jgough/apiservice/commits.html?id=1"",
    OnMenu:true,
    RemoveProp:[""remove""],
    AddToProp:[""add"",""addedValue""],
    _links: {
        ""Asset"":
            [
                {""idref"":""Story:1082""},
                {""idref"":""Story:9090""}
            ],
        ""Scope"" : { ""idref"" : ""Scope:0"" }        
    }
}
";

const string expected =
@"<Asset>
  <Attribute name=""Name"" act=""set"">Commit</Attribute>
  <Attribute name=""URL"" act=""set"">http://jgough/apiservice/commits.html?id=1</Attribute>
  <Attribute name=""OnMenu"" act=""set"">True</Attribute>
  <Attribute name=""RemoveProp"" act=""remove"" />
  <Attribute name=""AddToProp"" act=""add"">addedValue</Attribute>
  <Relation name=""Asset"" act=""set"">
    <Asset idref=""Story:1082"" />
    <Asset idref=""Story:9090"" />
  </Relation>
  <Relation name=""Scope"" act=""set"">
    <Asset idref=""Scope:0"" />
  </Relation>
</Asset>";
            _subject = new TranslateHalJsonHalInputToAssetXml();

            var actual = _subject.Execute(input).CreateNavigator().OuterXml;

            Assert.AreEqual(expected, actual);
        }
    }
````

### [See all HAL JSON test cases](https://github.com/versionone/VersionOne.SDK.Experimental/blob/master/ApiInputTranslatorPlugins/VersionOne.Web.Plugins.Tests/Api/TranslateHalJsonInputToAssetXmlTests.cs)

## [TranslateHalYamlInputToAssetXml](https://github.com/versionone/VersionOne.SDK.Experimental/blob/master/ApiInputTranslatorPlugins/VersionOne.Web.Plugins/Api/TranslateHalYamlInputToAssetXml.cs)

Translates incoming HAL-compliant [YAML](http://www.yaml.org/) into Asset XML.

### Example HAL YAML Test Case

```c#
        [Test]
        public void create_asset_with_multiple_attributes_and_single_relation()
        {
            const string input =
@"
_links:
    Asset:
        - idref: Story:1082
        - idref: Story:9090
    Scope:
        - idref: Scope:0
Name:   Commit
URL:    http://jgough/apiservice/commits.html?id=1
OnMenu: true
RemoveProp: [remove]
AddToProp:  [add, Added Value]
";

const string expected =
@"<Asset>
  <Relation name=""Asset"" act=""set"">
    <Asset idref=""Story:1082"" />
    <Asset idref=""Story:9090"" />
  </Relation>
  <Relation name=""Scope"" act=""set"">
    <Asset idref=""Scope:0"" />
  </Relation>
  <Attribute name=""Name"" act=""set"">Commit</Attribute>
  <Attribute name=""URL"" act=""set"">http://jgough/apiservice/commits.html?id=1</Attribute>
  <Attribute name=""OnMenu"" act=""set"">true</Attribute>
  <Attribute name=""RemoveProp"" act=""remove"" />
  <Attribute name=""AddToProp"" act=""add"">Added Value</Attribute>
</Asset>";
            _subject = new TranslateHalYamlInputToAssetXml();

            var actual = _subject.Execute(input).CreateNavigator().OuterXml;

            Assert.AreEqual(expected, actual);
        }
```