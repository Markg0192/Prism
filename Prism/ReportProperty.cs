namespace Prism
{
    public class ReportProperty
    {
        public string Name;
        public PropertyTypeEnum Type;

        public enum PropertyTypeEnum { String, Double, Integer };

        public ReportProperty() { }

        public ReportProperty(string name, PropertyTypeEnum type)
        {
            Name = name;
            Type = type;
        }
    }
}
