using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Xml.XPath;
using YamlDotNet.RepresentationModel;

namespace VersionOne.Web.Plugins.Api
{
    [Export(typeof(ITranslateApiInputToAssetXml))]
    public class TranslateYamlInputToAssetXml : BaseTranslateApiInputToAssetXml, ITranslateApiInputToAssetXml
    {          
        public XPathDocument Execute(string input)
        {
            var yamlDocument = new StringReader(input);

            var yaml = new YamlStream();
            yaml.Load(yamlDocument);

            var mapping = (YamlMappingNode)yaml.Documents[0].RootNode;

            foreach (var entry in mapping.Children)
            {
                var name = entry.Key.ToString();

                if (entry.Value is YamlMappingNode)
                {
                    AddRelations(name, entry.Value);
                }
                else if (entry.Value is YamlSequenceNode)
                {
                    AddAttributesWithExplicitActions(name, entry.Value);
                }
                else if (entry.Value is YamlScalarNode)
                {
                    AddAttributeFromScalar(name, entry.Value);
                }
            }

            return _builder.GetAssetXml();
        }

        protected override IEnumerable GetObjectItems(object obj)
        {
            var entry = obj as YamlMappingNode;
            return entry;
        }

        protected override string GetKey(object item)
        {
            var node = (KeyValuePair<YamlNode, YamlNode>)item;
            return node.Key.ToString();
        }

        protected override IEnumerable GetRelationItemEnumerable(object obj)
        {
            return obj as YamlMappingNode;
        }

        protected override Attribute CreateAttributeFromRelationItem(object obj)
        {
            var relItem = (KeyValuePair<YamlNode, YamlNode>)obj;
            return new Attribute(relItem.Key.ToString(), relItem.Value);
        }

        protected override IEnumerable<object> GetRelationItems(object linkObj)
        {
            var link = (KeyValuePair<YamlNode, YamlNode>)linkObj;

            var relationItems = new List<object>();

            if (link.Value is YamlMappingNode)
            {
                var value = (link.Value as YamlMappingNode);
                relationItems = value.Children.Cast<object>().ToList();
            }
            else if (link.Value is YamlSequenceNode)
            {
                var value = (link.Value as YamlSequenceNode);
                relationItems = value.Children.Cast<object>().ToList();
            }
            return relationItems;
        }

        protected override void AddAttributeFromScalar(string name, object scalar)
        {
            var obj = (YamlScalarNode) scalar;
            var value = obj.Value;
            _builder.AddAssetAttributeFromScalar(name, value);
        }

        protected override object[] GetArrayFromObject(object obj)
        {
            var pair = (YamlSequenceNode) obj;
            var array = pair.Children.Cast<object>().ToArray();
            return array;
        }

        private readonly string[] _contentTypes = new[]
            {
                "yaml",
                "application/yaml",
                "text/yaml"
            };

        public bool CanTranslate(string contentType)
        {
            if (!string.IsNullOrWhiteSpace(contentType))
            {
                contentType = contentType.Trim();
                return _contentTypes.Any(c => c.Equals(contentType, StringComparison.OrdinalIgnoreCase));
            }

            return false;
        }

    }
}