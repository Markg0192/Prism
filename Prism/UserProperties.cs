using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tekla.Structures.Model;

namespace Prism
{
    public class UserProperties
    {
        private readonly Dictionary<string, string> _stringProperties;
        private readonly Dictionary<string, double> _doubleProperties;
        private readonly Dictionary<string, int> _integerProperties;

        public UserProperties(ModelObject modelObject)
        {
            var stringProperties = new Hashtable();
            modelObject.GetStringUserProperties(ref stringProperties);
            _stringProperties = HashtableToDictionary<string, string>(stringProperties);

            var doubleProperties = new Hashtable();
            modelObject.GetDoubleUserProperties(ref doubleProperties);
            _doubleProperties = HashtableToDictionary<string, double>(doubleProperties);

            var integerProperties = new Hashtable();
            modelObject.GetIntegerUserProperties(ref integerProperties);
            _integerProperties = HashtableToDictionary<string, int>(integerProperties);
        }

        private static Dictionary<TK, TV> HashtableToDictionary<TK, TV>(Hashtable table)
        {
            return table
                .Cast<DictionaryEntry>()
                .ToDictionary(kvp => (TK)kvp.Key, kvp => (TV)kvp.Value);
        }

        public void AddStringProperty(string name, string value)
        {
            var current = GetStringProperty(name);
            if (current == string.Empty)
                _stringProperties.Add(name, value);
            else
                _stringProperties[name] = value;
        }

        public void AddDoubleProperty(string name, double value)
        {
            var current = GetDoubleProperty(name);
            if (double.IsNaN(current))
                _doubleProperties.Add(name, value);
            else
                _doubleProperties[name] = value;
        }

        public void AddIntegerProperty(string name, int value)
        {
            var current = GetIntegerProperty(name);
            if (current == int.MinValue)
                _integerProperties.Add(name, value);
            else
                _integerProperties[name] = value;
        }

        public string GetStringProperty(string name)
        {
            if (_stringProperties.ContainsKey(name)) return _stringProperties[name];
            return string.Empty;
        }

        public double GetDoubleProperty(string name)
        {
            if (_doubleProperties.ContainsKey(name)) return _doubleProperties[name];
            return double.NaN;
        }

        public int GetIntegerProperty(string name)
        {
            if (_integerProperties.ContainsKey(name)) return _integerProperties[name];
            return int.MinValue;
        }
    }
}
