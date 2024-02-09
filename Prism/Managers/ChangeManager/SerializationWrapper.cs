using System.Collections.Generic;
using System.Xml.Serialization;

namespace Prism
{
    [XmlRoot("Root")]
    public class SerializationWrapper
    {
        [XmlElement("Assembly")]
        public List<MyAssembly> Assemblies { get; set; }

        [XmlElement("FittingMarkCount")]
        public List<FittingMarkCount> FittingMarkCounts { get; set; }
    }
}
