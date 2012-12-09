using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.XPath;

namespace VersionOne.Web.Plugins.Api
{
    public abstract class BaseTranslateApiHalInputToAssetXml : ITranslateApiInputToAssetXml
    {
        protected abstract string[] GetContentTypes();

        public bool CanTranslate(string contentType)
        {
            if (!string.IsNullOrWhiteSpace(contentType))
            {
                contentType = contentType.Trim();
                return GetContentTypes().Any(c => c.Equals(contentType, StringComparison.OrdinalIgnoreCase));
            }

            return false;
        }

        public abstract XPathDocument Execute(string input);

        protected readonly XmlAssetBuilder Builder = new XmlAssetBuilder();

        protected void AddRelationsFromLinks(string name, object links)
        {
            if (name.Equals("_links", StringComparison.OrdinalIgnoreCase))
            {
                var relationList = new RelationList();
                var linkGroups = GetLinkGroupsFromRootProperty(links);
                foreach (var linkGroup in linkGroups)
                {
                    var relation = new Relation(GetLinkGroupKeyFromProperty(linkGroup));
                    var linkGroupRelations = GetLinkGroupRelations(linkGroup);

                    foreach (var linkGroupRelation in linkGroupRelations)
                    {
                        var relationAttributes = new List<Attribute>();
                        foreach (var relationItem in linkGroupRelation)
                        {
                            var attribute = CreateAttributeFromRelationItem(relationItem);
                            relationAttributes.Add(attribute);
                        }
                        relation.Add(relationAttributes);
                    }
                    relationList.Add(relation);
                }
                Builder.AddRelationsFromRelationList(relationList);
            }
        }

        protected abstract IEnumerable GetLinkGroupsFromRootProperty(object rootObject);

        protected abstract string GetLinkGroupKeyFromProperty(object property);

        protected abstract IEnumerable<IEnumerable> GetLinkGroupRelations(object linkGroup);

        protected abstract Attribute CreateAttributeFromRelationItem(object obj);

        protected void AddAttributesWithExplicitActionFromArray(string name, object obj)
        {
            var array = GetArrayFromObject(obj);

            var act = array[0].ToString();
            if (new[] { "set", "add" }.Any(a => a.Equals(act, StringComparison.OrdinalIgnoreCase)))
            {
                var value = array[1];
                var attr = new Attribute(name, value, act);
                Builder.AddAttributeFromArray(attr);
            }
            else if (act.Equals("remove", StringComparison.OrdinalIgnoreCase))
            {
                var attr = Attribute.CreateForRemove(name);
                Builder.AddAttributeFromArray(attr);
            }
        }

        protected abstract object[] GetArrayFromObject(object obj);

        protected abstract void AddAttributeFromScalarProperty(string name, object scalar);

        protected XPathDocument GetAssetXml()
        {
            return Builder.GetAssetXml();
        }
    }
}