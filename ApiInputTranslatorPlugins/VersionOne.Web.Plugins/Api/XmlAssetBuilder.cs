using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VersionOne.Web.Plugins.Api
{
    public class XmlAssetBuilder
    {
        private StringBuilder _buffer;

        private StringBuilder GetBuffer()
        {
            return _buffer ?? (_buffer = new StringBuilder());
        }

        public void AddRelationsFromRelationList(IEnumerable<Relation> relations)
        {
            foreach (var relation in relations)
            {
                var items = new List<string>();
                foreach (var relationItem in relation)
                {
                    items.AddRange(
                        from attribute
                        in relationItem
                        where attribute.Key.Equals("idref", StringComparison.OrdinalIgnoreCase)
                        select attribute.Value.ToString());
                }
                AddRelation(relation.Name, items);
            }
        }

        private void AddRelation(string relationName, IEnumerable<string> relationValues)
        {
            const string relationTemplate =
@"  <Relation name=""{0}"" act=""set"">
{1}  </Relation>
"; // <-- bizarre looking, gets filled in with new lines before it
            const string relationItem =
@"    <Asset idref=""{0}"" />
";
            var buff = new StringBuilder();
            foreach (var item in relationValues)
            {
                buff.Append(string.Format(relationItem, item));
            }
            var relation = string.Format(relationTemplate, relationName, buff);

            GetBuffer().Append(relation);
        }

        public void AddAssetAttributeFromScalar(string name, object value)
        {
            GetBuffer().Append(CreateAssetAttributeForUpdateOrAdd(new[] { name, "set", value.ToString() }));
        }

        public void AddAttributeFromArray(Attribute attr)
        {
            var attribute = string.Empty;
            var act = attr.Action;

            if (new[] { "set", "add" }.Any(a => a.Equals(act, StringComparison.OrdinalIgnoreCase)))
            {
                attribute = CreateAssetAttributeForUpdateOrAdd(new object[] { attr.Key, act, attr.Value });
            }
            else if (act.Equals("remove", StringComparison.OrdinalIgnoreCase))
            {
                attribute = CreateAssetAttributeForRemove(new[] { attr.Key, act });
            }
            GetBuffer().Append(attribute);
        }

        public string GetAssetXml()
        {
            const string xmlAsset = "<Asset>\r\n{0}</Asset>";

            var xml = string.Format(xmlAsset, GetBuffer());

            return xml;
        }

        private string CreateAssetAttributeForUpdateOrAdd(IList<object> attributeDef)
        {
            const string xmlAttribute =  "  <Attribute name=\"{0}\" act=\"{1}\">{2}</Attribute>\r\n";
            var attribute = string.Format(xmlAttribute, attributeDef[0], attributeDef[1], attributeDef[2]);
            return attribute;
        }

        private string CreateAssetAttributeForRemove(IList<object> attributeDef)
        {
            const string xmlAttribute = "  <Attribute name=\"{0}\" act=\"{1}\" />\r\n";
            var attribute = string.Format(xmlAttribute, attributeDef[0], attributeDef[1]);
            return attribute;
        }
    }
}
