using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;

namespace VersionOne.Web.Plugins.Composition
{
    public class PartsAssembler
    {
        private readonly string _path;

        public PartsAssembler(string path)
        {
            _path = path;
        }

        public void ComposeParts(object target)
        {
            var directoryCatalog = new DirectoryCatalog(_path);
            var container = new CompositionContainer(directoryCatalog);
            container.ComposeParts(target);
        }
    }
}