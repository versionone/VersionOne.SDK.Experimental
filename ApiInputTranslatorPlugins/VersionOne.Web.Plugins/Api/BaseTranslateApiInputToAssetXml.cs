using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Xml.XPath;

namespace VersionOne.Web.Plugins.Api
{
    [Export(typeof(ITranslateApiInputToAssetXml))]
    public abstract class BaseTranslateApiInputToAssetXml
    {
        protected readonly XmlAssetBuilder _builder = new XmlAssetBuilder();

        protected void AddRelations(string name, object obj)
        {
            if (name.Equals("_links", StringComparison.OrdinalIgnoreCase))
            {
                var relationList = new RelationList();
                var links = GetObjectItems(obj);
                foreach (var link in links)
                {
                    var relation = new Relation(GetKey(link));
                    var relationItems = GetRelationItems(link);

                    foreach (var item in relationItems)
                    {
                        var relationObjects = GetRelationItemEnumerable(item);
                        var relationAttributes = new List<Attribute>();
                        foreach (var relItem in relationObjects)
                        {
                            var attr = CreateAttributeFromRelationItem(relItem);
                            relationAttributes.Add(attr);
                        }
                        relation.Add(relationAttributes);
                    }
                    relationList.Add(relation);
                }
                _builder.AddRelationsFromRelationList(relationList);
            }
        }

        protected abstract IEnumerable GetObjectItems(object obj);

        protected abstract string GetKey(object item);

        protected abstract IEnumerable GetRelationItemEnumerable(object obj);

        protected abstract Attribute CreateAttributeFromRelationItem(object obj);

        protected abstract IEnumerable<object> GetRelationItems(object linkObj);

        protected void AddAttributesWithExplicitActions(string name, object obj)
        {
            var array = GetArrayFromObject(obj);

            var act = array[0].ToString();
            if (new[] { "set", "add" }.Any(a => a.Equals(act, StringComparison.OrdinalIgnoreCase)))
            {
                var value = array[1];
                var attr = new Attribute(name, value, act);
                _builder.AddAttributeFromArray(attr);
            }
            else if (act.Equals("remove", StringComparison.OrdinalIgnoreCase))
            {
                var attr = Attribute.CreateForRemove(name);
                _builder.AddAttributeFromArray(attr);
            }
        }

        protected abstract object[] GetArrayFromObject(object obj);

        protected abstract void AddAttributeFromScalar(string name, object scalar);

        protected XPathDocument GetAssetXml()
        {
            return _builder.GetAssetXml();
        }
    }
}