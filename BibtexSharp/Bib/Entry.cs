using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bibtex.Bib
{
    public class Entry
    {
        public string Key { get { return this.key; } }
        public string Type { get { return this.type; } }
        public string sort_tag;

        string key;
        string type;

        Dictionary<string, string> fields;

        public Entry(string type, string key)
        {
            this.key = key.Trim().ToLower();
            this.type = type;
            this.fields = new Dictionary<string, string>();
        }

    
        public void AddField(string name, string value)
        {
            name = name.Trim().ToLower();
            if (fields.ContainsKey(name))
                throw new Exception("Cannot add the field");

            this.fields.Add(name, value);
        }

         public bool HasField(string field_name)
        {
            field_name = field_name.Trim().ToLower();
            return this.fields.ContainsKey(field_name);
        }

        public string Get(string field_name, string default_value)
        {
            field_name = field_name.Trim().ToLower();
            if (!this.fields.ContainsKey(field_name))
                return default_value;

            return this.fields[field_name];
        }
    }
}
