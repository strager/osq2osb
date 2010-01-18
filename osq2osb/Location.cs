using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace osq2osb.Parser {
    public class Location {
        public string Filename {
            get;
            private set;
        }

        public int LineNumber {
            get;
            private set;
        }

        public int Column {
            get;
            private set;
        }

        private int lastChar;

        public Location() : 
            this(null, 1, 1) {
        }

        public Location(string filename) :
            this(filename, 1, 1) {
            Filename = filename;
        }

        public Location(string filename, int lineNumber, int column) {
            Filename = filename;
            LineNumber = lineNumber;
            Column = column;
        }

        public void AdvanceString(string s) {
            foreach(char c in s) {
                AdvanceCharacter(c);
            }
        }

        public void AdvanceCharacter(char c) {
            if(c == '\n' || c == '\r') {
                if((lastChar == '\n' || lastChar == '\r') && c != lastChar) {
                    lastChar = -1;

                    return;
                }

                ++LineNumber;
                Column = 1;
                lastChar = c;
            }

            lastChar = c;
        }

        public void AdvanceLine() {
            ++LineNumber;
            Column = 1;
            lastChar = -1;
        }

        public override string ToString() {
            if(string.IsNullOrEmpty(Filename)) {
                return "line " + LineNumber.ToString();
            } else {
                return Filename + ":line " + LineNumber.ToString();
            }
        }

        public Location Clone() {
            return new Location(Filename, LineNumber, Column);
        }
    }
}
