namespace Prism
{
    public class UserProperty
    {
        public string Name;
        public PropertyTypeEnum Type;

        public enum PropertyTypeEnum { String, Double, Integer };

        public UserProperty() { }

        public UserProperty(string name, PropertyTypeEnum type)
        {
            Name = name;
            Type = type;
        }
    }
}