using System.Collections.Generic;
using System.Xml.Serialization;

namespace Prism
{
    // Define your root element that corresponds to <KeyValueListWrapper>
    [XmlRoot("KeyValueListWrapper")]
    public class KeyValueListWrapper
    {
        // Define the collection of KeyValueItem within the <Items> container
        [XmlArray("Items")]
        [XmlArrayItem("KeyValueItem")]
        public List<KeyValueItem> Items { get; set; }
    }

    // Define the structure of your <KeyValueItem> elements
    public class KeyValueItem
    {
        // Element <Key> within <KeyValueItem>
        public string Key { get; set; }

        // Element <Value> within <KeyValueItem>, assuming it's always an integer
        public int Value { get; set; }
    }
}
