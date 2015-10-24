using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bibtex.Bib
{

    [Serializable]
    public class BibException : Exception
    {
        public BibException() { }
        public BibException(string message) : base(message) { }
        public BibException(string message, Exception inner) : base(message, inner) { }
        protected BibException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context)
        { }
    }
}
