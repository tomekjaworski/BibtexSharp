using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace BibtexSharp
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            BBLGenerator bg = new BBLGenerator();
            bg.LoadAUXFile(args[0]+".aux");
            bg.LoadBIBFile(bg.BibDataFile+".bib");
            bg.Stats();
            bg.Generate(args[0] + ".bbl");
        }
    }
}
