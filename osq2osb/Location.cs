using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace osq2osb.Parser {
    public static class LocationExceptionHelpers {
        // FIXME I really don't understand why exception.Data won't accept my objects.
        // Sure, I need to make them serializable, but when I create a class implementing
        // System.Runtime.Serialization.ISerializable, it still throws an ArgumentException
        // (Argument passed in is not serializable.).  If you can help, please do.  Cleaner
        // code follows, commented.  Hacked code follows that.  Thanks.

        /*
        private static object locationIdentifier = "location";

        public static TException AtLocation<TException>(this TException exception, Location location) where TException : Exception {
            if(location != null) {
                exception.Data[locationIdentifier] = location.Clone();
            }

            return exception;
        }

        public static Location GetLocation<TException>(this TException exception) where TException : Exception {
            if(!exception.Data.Contains(locationIdentifier)) {
                return null;
            }

            return exception.Data[locationIdentifier] as Location;
        }
        */

        private static object locationIdentifier = "x";

        public static TException AtLocation<TException>(this TException exception, Location location) where TException : Exception {
            if(location != null && exception.Data != null) {
                exception.Data[locationIdentifier] =  location.Clone();
            }

            return exception;
        }

        public static Location GetLocation<TException>(this TException exception) where TException : Exception {
            if(exception.Data == null || !exception.Data.Contains(locationIdentifier)) {
                return null;
            }

            return exception.Data[locationIdentifier] as Location;
        }
    }

    [Serializable()]
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
            this(1, 1) {
        }

        public Location(string filename) :
            this(filename, 1, 1) {
            Filename = filename;
        }

        public Location(int lineNumber, int column) :
            this(null, lineNumber, column) {
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
            string output = "";

            if(!string.IsNullOrEmpty(Filename)) {
                output += Filename + ": ";
            }

            output += "line " + LineNumber.ToString() + ", column " + Column.ToString();

            return output;
        }

        public Location Clone() {
            return new Location(Filename, LineNumber, Column);
        }
    }
}
