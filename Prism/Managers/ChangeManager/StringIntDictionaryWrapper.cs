using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Prism.Managers.ChangeManager
{
    [XmlRoot("Root")]
    public class StringIntDictionaryWrapper
    {
        [XmlElement("PartMark")]
        public string PartMark { get; set; }

        [XmlElement("NumberOfParts")]
        public int NumberOfParts { get; set; }
    }
}
