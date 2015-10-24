using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bibtex.Bib
{

    /// <summary>
    /// Class for holding a bibtex .bib file entry
    /// </summary>
    public class Entry
    {
        /// <summary>
        /// Key of the entry
        /// </summary>
        public string Key { get { return this.key; } }

        /// <summary>
        /// Type (article, book, incollection, etc.)
        /// </summary>
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

        public override string ToString()
        {
            return string.Format("{0} ({1})", this.key, this.type);
        }

        public void AddField(string name, string value)
        {
            name = name.Trim().ToLower();
            if (fields.ContainsKey(name))
                throw new BibException(string.Format("Cannot add given field; {0} already exists"));

            this.fields.Add(name, value);
        }

        /// <summary>
        /// Checks if a field <paramref name="field_name"/> exists in this entry.
        /// </summary>
        /// <param name="field_name">A field name</param>
        /// <returns>True if the given field exists</returns>
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
