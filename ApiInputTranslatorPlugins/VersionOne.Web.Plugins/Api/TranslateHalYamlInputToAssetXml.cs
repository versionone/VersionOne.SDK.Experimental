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
    public class TranslateHalYamlInputToAssetXml : BaseTranslateApiHalInputToAssetXml, ITranslateApiInputToAssetXml
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
                    AddRelationsFromLinks(name, entry.Value);
                }
                else if (entry.Value is YamlSequenceNode)
                {
                    AddAttributesWithExplicitActionFromArray(name, entry.Value);
                }
                else if (entry.Value is YamlScalarNode)
                {
                    AddAttributeFromScalarProperty(name, entry.Value);
                }
            }

            return Builder.GetAssetXml();
        }

        protected override IEnumerable GetLinkGroupsFromRootProperty(object rootObject)
        {
            var entry = rootObject as YamlMappingNode;
            return entry;
        }

        protected override string GetLinkGroupKeyFromProperty(object property)
        {
            var node = (KeyValuePair<YamlNode, YamlNode>)property;
            return node.Key.ToString();
        }

        protected override Attribute CreateAttributeFromRelationItem(object obj)
        {
            var relationItem = (YamlMappingNode)obj;
            var attribute = relationItem.Children.Select(x => new Attribute(x.Key.ToString(),
                x.Value)).FirstOrDefault();
            return attribute;
        }

        protected override IEnumerable<IEnumerable> GetLinkGroupRelations(object linkGroup)
        {
            var link = (KeyValuePair<YamlNode, YamlNode>)linkGroup;

            var relationItems = new List<IEnumerable>();

            if (link.Value is YamlMappingNode)
            {
                var value = (link.Value as YamlMappingNode);
                relationItems.Add(value);
            }
            else if (link.Value is YamlSequenceNode)
            {
                var value = (link.Value as YamlSequenceNode);
                var list = new List<YamlNode>();
                list.AddRange(value.Children.ToList());
                relationItems.Add(list);
            }

            return relationItems;
        }

        protected override void AddAttributeFromScalarProperty(string name, object scalar)
        {
            var obj = (YamlScalarNode) scalar;
            var value = obj.Value;
            Builder.AddAssetAttributeFromScalar(name, value);
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