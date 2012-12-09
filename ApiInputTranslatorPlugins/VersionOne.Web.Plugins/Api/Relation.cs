using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VersionOne.Web.Plugins.Api
{
    public class Relation : List<IEnumerable<Attribute>>
    {
        public string Name { get; set; }

        public Relation(string name, IEnumerable<Attribute> values = null)
        {
            Name = name;

            if (values != null)
            {
                Add(values);
            }
        }

        public Relation(string name, IEnumerable<IEnumerable<Attribute>> list)
        {
            Name = name;

            AddRange(list);
        }
    }
}
