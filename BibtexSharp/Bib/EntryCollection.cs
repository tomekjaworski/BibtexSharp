using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bibtex.Bib
{

    public class EntryCollection
    {
        Dictionary<string, Entry> set;


        public EntryCollection()
        {
            this.set = new Dictionary<string, Entry>();
        }



        public void AddEntry(Entry ent)
        {
            string key = ent.Key;
            if (this.set.ContainsKey(key))
                throw new BibException(string.Format("Cannot add given field; {0} already exists", key));

            this.set.Add(key, ent);
        }

        public Entry Get(string key)
        {
            key = key.Trim().ToLower();
            if (!this.set.ContainsKey(key))
                return null;

            return this.set[key];
        }

        public int Count { get { return this.set.Count; } }

        internal Entry[] ToArray()
        {
            Entry[] ent = new Entry[this.set.Values.Count];
            int i = 0;
            foreach (Entry e in this.set.Values)
                ent[i++] = e;
            return ent;
        }
    }

}
