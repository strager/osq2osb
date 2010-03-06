using System;

namespace osq {
    public static class LocationExceptionHelpers {
        private static readonly object LocationIdentifier = "x";

        public static TException AtLocation<TException>(this TException exception, Location location) where TException : Exception {
            if(location != null && exception.Data != null) {
                exception.Data[LocationIdentifier] = location.Clone();
            }

            return exception;
        }

        public static Location GetLocation<TException>(this TException exception) where TException : Exception {
            if(exception.Data == null || !exception.Data.Contains(LocationIdentifier)) {
                return null;
            }

            return exception.Data[LocationIdentifier] as Location;
        }
    }

    [Serializable()]
    public class Location {
        public string FileName {
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
            this(1, 1) {
        }

        public Location(string fileName) :
            this(fileName, 1, 1) {
        }

        public Location(int lineNumber, int column) :
            this(null, lineNumber, column) {
        }

        public Location(string fileName, int lineNumber, int column) {
            FileName = fileName;
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
                if((this.lastChar == '\n' || this.lastChar == '\r') && c != this.lastChar) {
                    this.lastChar = -1;

                    return;
                }

                ++LineNumber;
                Column = 1;
                this.lastChar = c;
            }

            this.lastChar = c;
        }

        public void AdvanceLine() {
            ++LineNumber;
            Column = 1;
            this.lastChar = -1;
        }

        public override string ToString() {
            string output = "";

            if(!string.IsNullOrEmpty(FileName)) {
                output += FileName + ": ";
            }

            output += "line " + this.LineNumber + ", column " + this.Column;

            return output;
        }

        public Location Clone() {
            return new Location(FileName, LineNumber, Column);
        }
    }
}