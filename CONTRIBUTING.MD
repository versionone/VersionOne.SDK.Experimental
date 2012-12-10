# Contributing API Input & Output Plugins

In the project at [https://github.com/versionone/versionone.sdk.experimental](VersionOne.SDK.Experimental) are two interfaces:

`ITranslateApiInputToAssetXml` translates a given input format into the standard AssetXml input that the API takes now.
`ITranslateAssetXmlOutputToContentType` translates the AssetXml output into a different output format

```c#
public interface ITranslateApiInputToAssetXml
{
    bool CanTranslate(string contentType);
    XPathDocument Execute(string i

## [BaseTranslateApiInputToAssetXml](https://github.com/versionone/VersionOne.SDK.Experimental/blob/master/ApiInputTranslatorPlugins/VersionOne.Web.Plugins/Api/BaseTranslateApiHalInputToAssetXml.cs) 

Provides an abstract base that produces Asset XML, given simple string and list inputs.

*NOTE:* this could certainly use some generic strong typing to make it more concrete than `object`, but then again, the loose typing keeps it simple and the derived class just needs to cast the inputs.
nput);
}

public interface ITranslateAssetXmlOutputToContentType
{
    bool CanTranslate(string contentType);
    string Execute(string input);
}
```

## [TranslateHalJsonHalInputToAssetXml](https://github.com/versionone/VersionOne.SDK.Experimental/blob/master/ApiInputTranslatorPlugins/VersionOne.Web.Plugins/Api/TranslateHalJsonHalInputToAssetXml.cs) 

Translates incoming JSON that conforms to the proposed IETF HAL format (Hypertext Application Language) into Asset XML.

### HAL JSON Example Test Case:

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

[See all HAL JSON test cases here.](https://github.com/versionone/VersionOne.SDK.Experimental/blob/master/ApiInputTranslatorPlugins/VersionOne.Web.Plugins.Tests/Api/TranslateHalJsonInputToAssetXmlTests.cs)

## [TranslateHalYamlInputToAssetXml]https://github.com/versionone/VersionOne.SDK.Experimental/blob/master/ApiInputTranslatorPlugins/VersionOne.Web.Plugins/Api/TranslateHalYamlInputToAssetXml.cs)

Translates incoming YAML (YAML Ain’t Markup Language), conforming to HAL, into Asset XML.

### Example HAL YAML Test Case:

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