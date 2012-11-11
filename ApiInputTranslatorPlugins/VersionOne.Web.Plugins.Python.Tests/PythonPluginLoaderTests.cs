using System.Collections.Generic;
using System.Linq;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using NUnit.Framework;
using VersionOne.Web.Plugins.Api;

namespace VersionOne.Web.Plugins.Python.Tests
{
    public static class PythonPluginLoader
    {
        public static IEnumerable<T> LoadPlugins<T>(string path)
        {
            var engine = IronPython.Hosting.Python.CreateEngine();
            var script = engine.CreateScriptSourceFromFile(path);
            var code = script.Compile();
            var scope = engine.CreateScope();
            code.Execute(scope);

            var instances = (from obj in scope.GetItems().Where(kvp => kvp.Value is PythonType)
                    let value = obj.Value
                    where 
                        obj.Key != typeof (T).Name 
                        &&  PythonOps.IsSubClass(value, 
                            DynamicHelpers.GetPythonTypeFromType(typeof (T)))
                    select (T) value()).ToList();
            
            return instances;
        }
    }

    [TestFixture]
    public class PythonPluginLoaderTests
    {
        private ITranslateApiInputToAssetXml _subject;

        [TestFixtureSetUp]
        public void Setup()
        {
            var path = GetScriptPath();

            var plugins = PythonPluginLoader.LoadPlugins<ITranslateApiInputToAssetXml>(path).ToList();

            Assert.AreEqual(1, plugins.Count);

            _subject = plugins[0];
        }

        private static string GetScriptPath()
        {
            var path = System.Reflection.Assembly.GetExecutingAssembly().EscapedCodeBase;
            path = path.Substring(0, path.LastIndexOf("/") + 1);
            path = path.Substring(path.IndexOf("C:"));

            path += "VersionOne.Web.Plugins.Python.py";
            return path;
        }

        [TestCase("text/xml", false)]
        [TestCase("text/yaml", true)]
        [TestCase("yaml", true)]
        [TestCase("application/yaml", true)]
        [TestCase("AppLiCAtioN/yAMl", true)]
        [TestCase("", false)]
        //[TestCase(null, false)]
        public void CanProcess_correct_content_types(string contentType, bool expected)
        {
            var actual = _subject.CanTranslate(contentType);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Execute_returns_correctly()
        {
            var input = "Testing Now";
            var expected =
@"<Asset>
  <Attribute name=""Name"" act=""set"">" + input + @"</Attribute>
</Asset>";

            var actual = _subject.Execute(input).CreateNavigator().OuterXml;

            Assert.AreEqual(expected, actual);
        }
    }
}