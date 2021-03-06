﻿using Bibtex.Bib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace BibtexSharp
{
    public class BBLGenerator
    {
        private const int EOF = -1;
        private StringReader sr;
        private int row, col;
        private string bibdata;
        public string BibDataFile { get { return this.bibdata; } }


        EntryCollection entries;
        List<string> citations;
        public BBLGenerator()
        {
            this.entries = new EntryCollection();
            this.citations = new List<string>();
        }

        #region LoadBIBFile

        public void LoadBIBFile(string file)
        {
            Console.WriteLine("Loading the BIB file: {0}", file);

            this.sr = new StringReader(File.ReadAllText(file));
            this.row = 1;
            this.col = 1;

            while (true)
            {
                this.ReadWhiteSpace();
                int ch = this.PeekChar();
                if (ch == '%')
                {
                    // readout the comment
                    this.ReadUntilEOL();
                    continue;
                }

                if (ch == '@')
                {
                    // read an entry
                    this.ReadChar();
                    Entry ent = this.ReadBibtexEntry();
                    if (ent != null)
                        this.entries.AddEntry(ent);
                }

                if (ch == -1)
                    break; // koniec pliku
            }

            Console.WriteLine("Number of entries: {0}", this.entries.Count);
        }

        private Entry ReadBibtexEntry()
        {

            //@INPROCEEDINGS{DBLP:conf/ipcv/Shamir06,
            //  author = {Lior Shamir},
            //  title = {Human Perception-based Color Segmentation Using Fuzzy Logic},
            //  booktitle = {IPCV},
            //  year = {2006},
            //  pages = {496-502},
            //  bibsource = {DBLP, http://dblp.uni-trier.de},
            //  crossref = {DBLP:conf/ipcv/2006-2},
            //}

            string entry_type = this.ReadIdentifier().ToLower().Trim();
            this.ReadWhiteSpace();

            if (entry_type.ToLower() == "comment")
            {
                // comment entry is generated by the JabRef editor. Ignore this
                string content = this.ReadBetweenBraces('{', '}').Trim();
                return null;
            }

            this.MatchChar('{');
            this.ReadWhiteSpace();

            // get bibkey
            string bibkey = this.ReadUntilChar(',').Trim();
            this.ReadChar(); // ,

            Entry ent = new Entry(entry_type, bibkey);

            // read all values
            while (true)
            {
                this.ReadWhiteSpace();
                string name = this.ReadIdentifier();

                this.ReadWhiteSpace();
                this.MatchChar('=');

                this.ReadWhiteSpace();
                int ch = this.PeekChar();
                string value = string.Empty;
                if (ch == '{')
                {
                    // this part is probably highly incompatible with bibtex formats.
                    // However when using JabRef as citation manager, it normalizes the content so this
                    // code should (i hope) run correctly
                    value = this.ReadBetweenBraces('{', '}').Trim();
                }
                else
                    if (ch == '"')
                    value = this.ReadAsString();
                else
                {
                    this.ReadWhiteSpace();
                    value = this.ReadUntilChar(',').Trim();
                    this.MatchChar(',');
                }
                ent.AddField(name, value);


                this.ReadWhiteSpace();
                ch = this.PeekChar();
                if (ch == ',')
                {
                    this.ReadChar();
                    // nastepna pozycja
                }
                else
                    if (ch == '}')
                {
                    // koniec bloku
                    this.ReadChar();
                    break;
                }
            }

            return ent;
        }

        private string ReadBetweenBraces(char start, char stop)
        {
            string value = string.Empty;
            int counter = 0;
            while (true)
            {
                int ch = this.ReadChar();
                if (ch == start)
                {
                    if (counter != 0)
                        value += start;

                    counter++;
                }
                else
                    if (ch == stop)
                {
                    counter--;
                    if (counter != 0)
                        value += stop;
                }
                else value += (char)ch;

                if (counter == 0)
                    return value;
            }
        }

        private string ReadAsString()
        {
            string value = string.Empty;
            this.ReadChar(); // "
            while (true)
            {
                int ch = this.PeekChar();
                if (ch == '"')
                {
                    this.ReadChar();
                    return value;
                }

                value += (char)this.ReadChar();
            }

        }

        private string ReadUntilChar(char end_char)
        {
            string str = string.Empty;
            while (true)
            {
                int ch = this.PeekChar();
                if (ch == end_char)
                    return str;
                if (ch == -1)
                    return str;

                str += (char)ch;
                this.ReadChar();
            }
        }

        private void MatchChar(params char[] chars)
        {
            int ch = this.PeekChar();
            foreach (char c in chars)
                if (ch == (int)c)
                {
                    this.ReadChar();
                    return;
                }
            throw new Exception(string.Format("Unexpected character '{0}' at line {1} col {2}", (char)ch, this.row, this.col));
        }

        private string ReadIdentifier()
        {
            string name = string.Empty;
            while (true)
            {
                int ch = this.PeekChar();
                if (char.IsLetter((char)ch) || char.IsDigit((char)ch) || (ch == '_') || (ch == '-') || (ch == '+'))
                {
                    name += (char)ch;
                    this.ReadChar();
                    continue;
                }
                return name;
            }
        }

        private string ReadBibkey()
        {
            string name = string.Empty;
            string allowed = "_-+:.";
            while (true)
            {
                int ch = this.PeekChar();
                if (char.IsLetter((char)ch) || char.IsDigit((char)ch) || allowed.IndexOf((char)ch) != -1)
                {
                    name += (char)ch;
                    this.ReadChar();
                    continue;
                }
                return name;
            }
        }

        private void ReadUntilEOL()
        {
            while (true)
            {
                int ch = this.ReadChar();
                if (ch == -1 || ch == '\n')
                    return;
            }
        }

        private void ReadWhiteSpace()
        {
            int ch = this.PeekChar();
            while (ch == ' ' || ch == '\t' || ch == '\n')
            {
                this.ReadChar();
                ch = this.PeekChar();
                if (ch == EOF)
                    return;
            }
        }

        private int PeekChar()
        {
            if (sr.Peek() == '\r')
                sr.Read();
            return this.sr.Peek();
        }

        private int ReadChar()
        {
            if (sr.Peek() == '\r')
                sr.Read();

            int ch = this.sr.Read();
            if (ch == '\n')
            {
                this.row++;
                this.col = 1;
            }
            else
                this.col++;
            return ch;
        }

        #endregion

        public void LoadAUXFile(string file)
        {
            Console.WriteLine("Parsing AUX: {0}", file);
            this.sr = new StringReader(File.ReadAllText(file));
            this.row = 1;
            this.col = 1;

            while (true)
            {
                ReadWhiteSpace();

                int ch = PeekChar();
                if (ch == -1)
                    break;

                MatchChar('\\');
                string tag = ReadIdentifier();

                if (tag != "citation" && tag != "bibstyle" && tag != "bibdata")
                    throw new BibException(string.Format("Unexpected tag: {0}", tag));

                if (tag == "bibdata")
                {
                    // read bib filename
                    ReadWhiteSpace();
                    this.bibdata = this.ReadBetweenBraces('{', '}');
                    ReadWhiteSpace();
                }

                if (tag == "citation")
                {
                    // read citation, given as a bibtex key
                    ReadWhiteSpace();
                    MatchChar('{');
                    ReadWhiteSpace();
                    string key = ReadBibkey();
                    ReadWhiteSpace();
                    MatchChar('}');
                    ReadWhiteSpace();

                    this.citations.Add(key.Trim().ToLower());
                }

                if (tag != "citation") // any other case
                {
                    ReadWhiteSpace();
                    string dummy = this.ReadBetweenBraces('{', '}');
                    ReadWhiteSpace();
                }

            }

            Console.WriteLine("Number of citations in aux file: {0}", this.citations.Count);
        }

        internal void Stats()
        {
            Dictionary<string, int> stat = new Dictionary<string, int>();

            foreach (string cit in this.citations)
            {
                Entry ent = this.entries.Get(cit);
                if (ent == null)
                    Console.WriteLine("Bibkey {0} not found", cit);
                else
                if (stat.ContainsKey(ent.Type))
                    stat[ent.Type] += 1;
                else
                    stat[ent.Type] = 1;
            }

            foreach (string key in stat.Keys)
                Console.WriteLine("Key {0} referenced {1} time(s)", key, stat[key]);
        }

        internal void Generate(string bbl_file_name)
        {
            Console.WriteLine("Generating BBL file: {0}", bbl_file_name);

            List<Entry> ents = new List<Entry>(this.entries.ToArray());
            List<string> cits = new List<string>(this.citations);
            ents = this.SelectCitedEntries(ents, cits);
            ents = this.SortByAuthors(ents);

            using (FileStream fs = new FileStream(bbl_file_name + ".txt", FileMode.Create))
            using (StreamWriter sw = new StreamWriter(fs, Encoding.GetEncoding("Windows-1250")))
            {
                foreach (string type in ents.Select(n => n.Type).Distinct().ToArray())
                {
                    sw.WriteLine("TYPE = " + type);
                    string[] keys = ents.Where(n => n.Type == type).Select(n => n.Key).ToArray();
                    sw.WriteLine("   " + string.Join(",", keys));
                }
            }

            using (FileStream fs = new FileStream(bbl_file_name, FileMode.Create))
            using (StreamWriter sw = new StreamWriter(fs, Encoding.GetEncoding("Windows-1250")))
            {
                sw.WriteLine(@"\begin{thebibliography}{100}");

                GenerateMissingCitations(sw, ents, cits);

                foreach (Entry entry in ents)
                {
                    sw.WriteLine(@"\bibitem{{{0}}}", entry.Key);
                    switch (entry.Type)
                    {
                        case "book": this.Book(sw, entry); break;
                        case "incollection": this.InCollection(sw, entry); break;
                        case "patent": this.Patent(sw, entry); break;

                        case "mastersthesis": this.Thesis_Masters_PhD(sw, entry); break;
                        case "phdthesis": this.Thesis_Masters_PhD(sw, entry); break;
                        case "manual": this.Manual(sw, entry); break;
                        case "online": this.Online(sw, entry); break;
                        case "misc": this.Misc(sw, entry); break;

                        case "article": this.Article(sw, entry); break;


                        case "conference": this.Conference_InProceedings(sw, entry); break;
                        case "inproceedings": this.Conference_InProceedings(sw, entry); break;

                        default:
                            sw.WriteLine(entry.Key + @"{\bf Unknown Entry Type}" + entry.Type);
                            break;
                    }

                    sw.WriteLine();
                }

                sw.WriteLine(@"\end{thebibliography}");
            }

        }

        private List<Entry> SelectCitedEntries(List<Entry> ents, List<string> citations)
        {
            List<Entry> cited = new List<Entry>();
            foreach (Entry en in ents)
                if (citations.Contains(en.Key))
                    cited.Add(en);

            return cited;
        }


        private void GenerateMissingCitations(StreamWriter sw, List<Entry> entries, List<string> citations)
        {
            foreach (String key in citations)
            {
                if (entries.Count(n => n.Key == key) != 0)
                    continue;

                sw.WriteLine(@"\bibitem{{{0}}}", key);
                sw.WriteLine(Boldify("No data for bibkey " + key));
                sw.WriteLine();
            }
        }

        #region citation emiters

        private void Book(StreamWriter sw, Entry ent)
        {
            string author = ent.Get("author", null);
            if (string.IsNullOrEmpty(author))
                author = ent.Get("editor", null);
            if (string.IsNullOrEmpty(author))
                author = "???????? (author unknown) ????????";

            // authors
            sw.WriteLine(@"{{\sc {0}}}: ", FormatNames(author, true));

            // title
            sw.WriteLine(@"\newblock {{{0}}}.", Italicize(ent.Get("title", Error("title"))));

            // volume and number
            if (ent.Get("series", null) != null)
                sw.WriteLine(@"\newblock {{{0} {1}}}.", ent.Get("series", ""), ent.Get("volume", Error("volume")));

            // adress of publcation and the publisher
            sw.WriteLine(GenerateBlock(ent.Get("publisher", ""), ent.Get("address", ""), ent.Get("year", "")));
        }

        private void Conference_InProceedings(StreamWriter sw, Entry ent)
        {
            string author = ent.Get("author", null);
            if (string.IsNullOrEmpty(author))
                author = "??????????????????????";

            // authors
            //sw.WriteLine(@"{{#{2}#}} {{\sc {0}}}: ", FormatNames(author, true), ent.Type, Boldify(ent.Key));
            sw.WriteLine(@"{{\sc {0}}}: ", FormatNames(author, true), ent.Type, Boldify(ent.Key));

            // title
            sw.WriteLine(@"\newblock {{{0}}}.", ent.Get("title", Error("title")));

            // book issue data
            sw.WriteLine(@"\newblock {{{0}{1}{2}{3}{4}{5}{6}.}}",
                Italicize(ent.Get("booktitle", Error("booktitle"))),
                FormatIfNotEmpty(", tom {0}", ent.Get("volume", ""/*Error("volume")*/)),
                FormatIfNotEmpty(", nr {0}", ent.Get("number", ""/*Error("number")*/)),
                FormatIfNotEmpty(", {0}", ent.Get("publisher", ""/*Error("publisher")*/)),
                FormatIfNotEmpty(", {0}", ent.Get("address", ""/*Error("address")*/)),
                FormatIfNotEmpty(", {0}", ent.Get("year", ""/*Error("year")*/)),
                FormatIfNotEmpty(", str. {0}", ent.Get("pages", ""/*Error("pages")*/))
                );



        }


        private void Article(StreamWriter sw, Entry ent)
        {
            string author = ent.Get("author", null);
            if (string.IsNullOrEmpty(author))
                author = "??????????????????????";

            // authors and title
            sw.WriteLine(@"\newblock {{{0}: {1}}}.", SmallCaps(FormatNames(author, true)), ent.Get("title", Error("title")));

            // journal data
            sw.WriteLine(@"\newblock {{{0} {1}{2}{3}{4}.}}",
                Italicize(ent.Get("journal", Error("journal"))), ent.Get("year", Error("year")),
                FormatIfNotEmpty(", tom {0}", ent.Get("volume", Error("volume"))),
                FormatIfNotEmpty(", nr {0}", ent.Get("number", ""/*Error("number")*/)),
                FormatIfNotEmpty(", str. {0}", ent.Get("pages", Error("pages")))
                );
            
        }


        private void InCollection(StreamWriter sw, Entry ent)
        {
            string author = ent.Get("author", null);
            if (string.IsNullOrEmpty(author))
                author = "??????????????????????";

            // authors
            sw.WriteLine(@"{{\sc {0}}}: ", FormatNames(author, true));

            // title
            sw.WriteLine(@"\newblock {{{0}.}}", ent.Get("title", Error("title")));

            // book - editors/authors
            string editor = "";
            if (ent.HasField("editor"))
                editor = @"{\sc " + FormatNames(ent.Get("editor", null), true) + "}, ed. ";
            else
                editor = Error("editor") + " ";

            sw.WriteLine(@"\newblock {{W: {0}{1}.}}", editor, Italicize(ent.Get("booktitle", Error("booktitle"))));

            // place of publication and the name of the publisher
            sw.WriteLine(GenerateBlock(ent.Get("publisher", ""), ent.Get("address", ""), ent.Get("year", "")));

            // pages
            sw.WriteLine(@"\newblock {{str. {0}}}.", ent.Get("pages", Error("pages")));
        }


        private void Patent(StreamWriter sw, Entry ent)
        {
            string author = ent.Get("author", null);
            if (string.IsNullOrEmpty(author))
                author = "??????????????????????";

            // authors
            sw.WriteLine(@"{{\sc {0}}}: ", FormatNames(author, true));

            // number and title
            sw.WriteLine(@"\newblock {{{0}: {1}.}}", ent.Get("number", Error("number")), ent.Get("title", Error("title")));


            // place of publication and the name of the publisher
            sw.WriteLine(GenerateBlock(ent.Get("publisher", ""), ent.Get("address", ""), ent.Get("year", "")));

        }


        void Thesis_Masters_PhD(StreamWriter sw, Entry ent)
        {
            string author = ent.Get("author", null);
            if (string.IsNullOrEmpty(author))
                author = "??????????????????????";

            // authors
            sw.WriteLine(@"{{\sc {0}}}: ", FormatNames(author, true));

            // issu and title
            sw.WriteLine(@"\newblock {{{0}}}.", ent.Get("title", Error("title")));

            // university and year
            sw.WriteLine(@"\newblock {{{0}, {1}}}.", ent.Get("school", Error("school")), ent.Get("year", Error("year")));


        }

        void Manual(StreamWriter sw, Entry ent)
        {

            string l = "";
            l += AddIfNotEmpty(ent.Get("note", null), " ");
            l += (ent.Get("title", Error("title")));

            sw.WriteLine(@"\newblock {{{0}}}.", l);
            sw.WriteLine(GenerateBlock(ent.Get("organization", ""), ent.Get("year", "")));



        }

        void Online(StreamWriter sw, Entry ent)
        {
            // string l = "";

            string auth = "";
            if (ent.Get("author", null) != null)
                auth = FormatNames(ent.Get("author", null), true);
            else
                auth = ent.Get("organization", Error("organizatio/author"));

            sw.WriteLine(@"{{\sc {0}}}: {1}.", auth, ent.Get("title", ""));
            sw.WriteLine(@"\newblock {{{0}.}}", ent.Get("url", ""));

            if (ent.HasField("title") && ent.HasField("organization"))
                sw.WriteLine(@"\newblock {{{0}.}}", ent.Get("organization", ""));

            if (ent.HasField("note"))
                sw.WriteLine(@"\newblock {{{0}.}}", ent.Get("note", ""));
        }

        void Misc(StreamWriter sw, Entry ent)
        {
            string auth = "";
            if (ent.Get("author", null) != null)
                auth = FormatNames(ent.Get("author", null), true);
            else
                auth = ent.Get("organization", Error("organizatio/author"));

            sw.WriteLine(@"{{\sc {0}}}: {1}.", auth, ent.Get("title", ""));

            if (ent.HasField("note"))
                sw.WriteLine(@"\newblock {{{0}.}}", ent.Get("note", ""));
        }

        #endregion


        string GenerateBlock(params string[] texts)
        {
            string s = string.Join(", ", texts.Where(str => str.Length > 0).ToArray());
            if (s.Length == 0)
                return @"\newblock {}";

            return string.Format(@"\newblock {{{0}}}.", s);
        }

        private string Error(string p)
        {
            return string.Format(@"{{\bf\it !!!!!!!{0}!!!!!!!}}", p.ToUpper());
        }

        private string FormatNames(string s, bool bold_me)
        {
            string[] authors = Regex.Split(s, "[ ]and[ ]", RegexOptions.IgnoreCase);

            List<string> fauthors = new List<string>();
            foreach (string author in authors)
            {
                string[] names;
                string last_name = "";
                string first_names = "";
                if (author.Contains(","))
                {
                    names = author.Split(',').Select(str => str.Trim()).ToArray();
                    last_name = names[0];
                    first_names = string.Join(" ", names, 1, names.Length - 1);
                }
                else
                {
                    names = author.Split(' ').Select(str => str.Trim()).ToArray();
                    last_name = names[names.Length - 1];
                    first_names = string.Join(" ", names, 0, names.Length - 1);
                }

                // initials instead of full names
                names = first_names.Split(' ').Select(str => str.Trim()).ToArray();
                for (int i = 0; i < names.Length; i++)
                    if (!names[i].EndsWith("."))
                        if (names[i].Length > 0)
                            names[i] = names[i].Substring(0, 1) + ".";

                string author_name = last_name + "~" + string.Join("~", names);

                // The following code boldifies selected author. I wanted my name to
                // be visible among all citations. Especially if they are written with
                // a smaller font.
                if (bold_me)
                    if (author_name.ToLower().Replace('~', ' ') == "jaworski t.")
                        author_name = Boldify(author_name);
                fauthors.Add(author_name);
            }


            return string.Join(", ", fauthors.ToArray());
        }

        private List<Entry> SortByAuthors(List<Entry> ents)
        {

            foreach (Entry e in ents)
            {
                if (e.Type == "misc")
                    e.sort_tag = e.Get("author", "") + e.Get("organization", "") + e.Get("title", "") + e.Get("note", "");
                else
                    if (e.Type == "online")
                    e.sort_tag = e.Get("author", "") + e.Get("organization", "") + e.Get("title", "") + e.Get("note", "");
                else
                        if (e.Type == "manual")
                    e.sort_tag = e.Get("note", "") + e.Get("organization", "") + e.Get("title", "");
                else
                {
                    e.sort_tag = e.Get("author", null);
                    if (string.IsNullOrEmpty(e.sort_tag))
                        e.sort_tag = e.Get("editor", null);
                    if (string.IsNullOrEmpty(e.sort_tag))
                        e.sort_tag = "??????????????????????";

                    e.sort_tag = FormatNames(e.sort_tag, false);
                }
                e.sort_tag = e.sort_tag.ToLower();
            }

            ents = ents.OrderBy(ee => ee.sort_tag).ToList();

            return ents;
        }

        string Boldify(string s)
        {
            if (String.IsNullOrEmpty(s))
                return "";
            return string.Format(@"{{\bf {0}}}", s);
        }
        string Italicize(string s)
        {
            if (String.IsNullOrEmpty(s))
                return "";
            return string.Format(@"{{\it {0}}}", s);
        }

        string SmallCaps(string s)
        {
            if (String.IsNullOrEmpty(s))
                return "";
            return string.Format(@"{{\sc {0}}}", s);
        }
        string AddIfNotEmpty(string a, string b)
        {
            if (string.IsNullOrEmpty(a))
                return "";
            else
                return a + b;
        }


        private string FormatIfNotEmpty(string p1, string p2)
        {
            if (string.IsNullOrEmpty(p2))
                return "";

            return string.Format(p1, p2);
        }

    }


}