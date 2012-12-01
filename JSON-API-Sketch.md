# Thoughts on JSON API

This text comes from the existing [Data API Docs](http://community.versionone.com/sdk/Documentation/DataAPI.aspx).

I've added one or more JSON samples as suggestions after the XML fragments.

As of now, I've only added examples for INPUT, not the output as JSON yet.

Comments very welcome!

# Learn By Example: Updates

Updating assets through the API involves sending POST requests to the URL of the asset to be updated, with an XML payload. These examples can be tested in your browser by using the page [http://localhost/VersionOne/http.html](http://localhost/VersionOne/http.html).

## How to update a scalar attribute on an asset

Updating a scalar attribute on an asset is accomplished by marking the attribute with act="set", and filling in the new value in the element's text. This post will update the Phone attribute of the Member with ID 20:

```
POST /VersionOne/rest-1.v1/Data/Member/20 HTTP/1.1
Content-Type: text/xml; charset=utf-8
Content-Length: 78
```
```xml
<Asset>
	<Attribute name="Phone" act="set">555-555-1212</Attribute>
</Asset>
The response is:

<Asset href="/VersionOne/rest-1.v1/Data/Member/20/173" id="Member:20:173">
	<Attribute name="Phone">555-555-1212</Attribute>
</Asset>
```
### Remarks

The response includes the URL of the exact version of the asset that was just updated.

How to update a single-value relation on an asset

Updating a single-value relation is accomplished by marking the attribute with `act="set"`, and filling the idref of the enclosed Asset element. This post will change the Owner of the Scope with ID 0 to be the Member with ID 20:

```
POST /VersionOne/rest-1.v1/Data/Scope/0 HTTP/1.1
Content-Type: text/xml; charset=utf-8
Content-Length: 98
```
```xml
<Asset>
	<Relation name="Owner" act="set">
		<Asset idref="Member:20" />
	</Relation>
</Asset>
```
Whereas this post will change the Owner of the Scope with ID 0 to nobody (that is, NULL):

```
POST /VersionOne/rest-1.v1/Data/Scope/0 HTTP/1.1
Content-Type: text/xml; charset=utf-8
Content-Length: 67
```
```xml
<Asset>
	<Relation name="Owner" act="set">
	</Relation>
</Asset>
```
## How to add and remove values from a multi-value relation

Updating a multi-value relation is accomplished by marking enclosed Asset elements with either `act="add"` or `act="remove"`. This post will add one Member and remove another Member from the Scope with ID 0:

```
POST /VersionOne/rest-1.v1/Data/Scope/0 HTTP/1.1
Content-Type: text/xml; charset=utf-8
Content-Length: 197
```
```xml
<Asset>
	<Relation name="Members">
		<Asset idref="Member:1000" act="add"/>
		<Asset idref="Member:1001" act="remove"/>
	</Relation>
</Asset>
```
## How to create a new asset

Creating a new asset is accomplished by posting to the asset type URL (without an ID). This post will create a Story with the name "New Story", within the Scope with ID 0.

```
POST /VersionOne/rest-1.v1/Data/Story HTTP/1.1
Content-Type: text/xml; charset=utf-8
Content-Length: 221 
```
```xml
<Asset>
	<Attribute name="Name" act="set">New Story</Attribute>
	<Relation name="Scope" act="set">
		<Asset idref="Scope:0" />
	</Relation>
</Asset>
```

The response is:

```xml
<Asset href="/VersionOne/rest-1.v1/Data/Story/1072/214" id="Story:1072:214">
	<Attribute name="Name">New Story</Attribute>
	<Relation name="Scope">
		<Asset href="/VersionOne.Web/rest-1.v1/Data/Scope/0" idref="Scope:0" />
	</Relation>
</Asset>
```

### Remarks

When creating a new asset, all required attributes must be provided, or the save will fail. In the above example, the Name and Scope attributes are the minimum required attributes for a Story.

# Learn By Example: New Asset Templates

A new asset template is an XML response that contains suggested attribute values for a new asset. These values can be changed, and new attributes added; then the whole template can be posted for saving.

Often a new asset should be created in the "context" of another asset. For example, if you ask for a new Story asset in the context of an Issue asset, the new Story asset will carry over some attributes from the Issue. A few of these attributes are the Name, Description, Scope, and a relationship back to the originating Issue. The same can be done with a Story in the context of a Request with similar attributes being carried across.

One example of a new asset template is asking for a new Timebox asset. When a new Timebox asset is asked for in the context of a Scope asset, it will automatically come with a suggested Name, as well as a StartDate and EndDate. This saves the client from having to calculate the StartDate and EndDate attributes.

These examples demonstrate how to ask for new assets. They should be viewable using a web browser capable of displaying XML data.

How to get a new Story asset template in the context of a Scope asset

Use this URL to get a new Story template.

`http://localhost/VersionOne/rest-1.v1/New/Story?ctx=Scope:1000`

The XML Response will look something like this.

```xml
<Asset href="/VersionOne/rest-1.v1/New/Story">
	<Relation name="Scope" act="set">
		<Asset href="/VersionOne/rest-1.v1/Data/Scope/1000" idref="Scope:1000" /> 
	</Relation>
</Asset>
```
### Remarks

Note how the response contains a pending change for the Scope attribute.

This Story asset is not saved yet. To save it (creating a new Story asset), you must add a Name attribute, and POST it to `http://localhost/VersionOne/rest-1.v1/Data/Story` as described in How to create a new asset.

## How to get a new Story asset in the context of a Request asset

Use this URL to get a new Story template.

`http://localhost/VersionOne/rest-1.v1/New/Story?ctx=Request:1684`

The XML Response will look something like this.

```xml
<Asset href="/VersionOne/rest-1.v1/New/Story">
	<Relation name="Scope" act="set">
		<Asset href="/VersionOne/rest-1.v1/Data/Scope/1009" idref="Scope:1009" /> 
	</Relation>
	<Attribute name="Description" act="set"><P>When an item's status is updated, alert the owner of the change in status value.</P></Attribute> 
	<Attribute name="RequestedBy" act="set">Joe Customer</Attribute> 
	<Attribute name="Name" act="set">Integrate with Palm Handheld</Attribute> 
	<Relation name="Requests">
		<Asset href="/VersionOne/rest-1.v1/Data/Request/1684" idref="Request:1684" act="add" /> 
	</Relation>
</Asset>
```

Note how the response contains many attributes carried over from the Request asset into the pending Story asset. Also note how the Story will have a relationship set to point back to the Request that it was created from.