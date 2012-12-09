namespace VersionOne.Web.Plugins.Api
{
    public class Attribute
    {
        public string Key { get; set; }
        public object Value { get; set; }
        public string Action { get; set; }

        public Attribute(string key, object value, string act = "set")
        {
            Key = key;
            Value = value;
            Action = act;
        }

        public static Attribute CreateForRemove(string  key)
        {
            return new Attribute(key, null, "remove");
        }
    }
}

