using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace VersionOne.Web.Plugins.Composition
{
    public class PartsList<TTypeToPopulateWith>
    {

#pragma warning disable 649
        [ImportMany]
        public IEnumerable<TTypeToPopulateWith> Items { get; set; }
#pragma warning restore 649

        public PartsList(string path)
        {
            new PartsAssembler(path).ComposeParts(this);
        }

        public PartsList(Action<TTypeToPopulateWith> initializeAction, string path)
        {
            new PartsAssembler(path).ComposeParts(this);

            foreach (var item in Items)
            {
                initializeAction(item);
            }
        }
    }
}