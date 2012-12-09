using System.Collections.Generic;

namespace VersionOne.Web.Plugins.Api
{
    public class RelationList : List<Relation>
    {
        public RelationList(IEnumerable<Relation> relations = null)
        {
            if (relations != null)
            {
                AddRange(relations);
            }
        }
    }
}