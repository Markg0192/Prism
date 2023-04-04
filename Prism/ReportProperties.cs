using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tekla.Structures.Model;

namespace Prism
{
    public class ReportProperties
    {
        public static List<ReportProperty> ReportPropertyList = new List<ReportProperty>();
        public static ArrayList StringReportProperties = new ArrayList();
        public static ArrayList DoubleReportProperties = new ArrayList();
        public static ArrayList IntegerReportProperties = new ArrayList();

        private readonly Dictionary<string, string> _stringProperties;
        private readonly Dictionary<string, double> _doubleProperties;
        private readonly Dictionary<string, int> _integerProperties;

        public ReportProperties(ModelObject modelObject)
        {
            var stringProperties = new Hashtable();
            modelObject.GetStringReportProperties(StringReportProperties, ref stringProperties);
            _stringProperties = HashtableToDictionary<string, string>(stringProperties);

            var doubleProperties = new Hashtable();
            modelObject.GetDoubleReportProperties(DoubleReportProperties, ref doubleProperties);
            _doubleProperties = HashtableToDictionary<string, double>(doubleProperties);

            var integerProperties = new Hashtable();
            modelObject.GetIntegerReportProperties(IntegerReportProperties, ref integerProperties);
            _integerProperties = HashtableToDictionary<string, int>(integerProperties);
            PopulateTables();
        }

        private static void PopulateTables()
        {
            var reportPropertyList = Properties.Resources.ReportProperties.Split(
                new string[] { Environment.NewLine },
                StringSplitOptions.None
            ).ToList();

            ReportPropertyList = new List<ReportProperty>();
            foreach (var line in reportPropertyList.Where(line => !string.IsNullOrEmpty(line)))
            {
                if (line.Length >= 2)
                    if (line.Substring(0, 2) == "//")
                        continue;
                if (line.Contains("[ALL]")) continue;

                var data = line.Split(' ').ToList();
                for (var i = data.Count - 1; i >= 0; i--)
                {
                    if (data[i].Length == 0) data.RemoveAt(i);
                }

                if (data.Count <= 1) continue;

                var reportProp = new ReportProperty
                {
                    Name = data[0]
                };
                if (data[1] == "CHARACTER") reportProp.Type = ReportProperty.PropertyTypeEnum.String;
                if (data[1] == "INTEGER") reportProp.Type = ReportProperty.PropertyTypeEnum.Integer;
                if (data[1] == "FLOAT") reportProp.Type = ReportProperty.PropertyTypeEnum.Double;
                ReportPropertyList.Add(reportProp);
            }

            StringReportProperties = new ArrayList();
            DoubleReportProperties = new ArrayList();
            IntegerReportProperties = new ArrayList();
            foreach (var rp in ReportPropertyList)
            {
                switch (rp.Type)
                {
                    case ReportProperty.PropertyTypeEnum.String:
                        StringReportProperties.Add(rp.Name);
                        break;
                    case ReportProperty.PropertyTypeEnum.Double:
                        DoubleReportProperties.Add(rp.Name);
                        break;
                    case ReportProperty.PropertyTypeEnum.Integer:
                        IntegerReportProperties.Add(rp.Name);
                        break;
                }
            }
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
            if (current == double.NaN)
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
