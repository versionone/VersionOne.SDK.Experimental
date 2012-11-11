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
        # Add some more white space trimming and null checking here...
        return contentType.lower() in map(str.lower, ['text/yaml', 'application/yaml', 'yaml'])
    
    def Execute(self, input):
        output = '<Asset><Attribute name="Name" act="set">' + input + '</Attribute></Asset>'
        reader = StringReader(output)
        doc = XPathDocument(reader)
        return doc