using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace osq2osb.Parser {
    public class Location {
        public string Filename {
            get;
            set;
        }

        public int LineNumber {
            get;
            set;
        }

        public Location() {
        }

        public Location(string filename, int lineNumber) {
            Filename = filename;
            LineNumber = lineNumber;
        }

        public override string ToString() {
            if(string.IsNullOrEmpty(Filename)) {
                return "line " + LineNumber.ToString();
            } else {
                return Filename + ":line " + LineNumber.ToString();
            }
        }

        public Location Clone() {
            return new Location(Filename, LineNumber);
        }
    }
}
