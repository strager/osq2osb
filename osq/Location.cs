using System;

namespace osq {
    /// <summary>
    /// Represents a location in a string or file.
    /// </summary>
    [Serializable]
    public class Location {
        /// <summary>
        /// Gets or sets the name of the file this <see cref="Location"/> refers to.
        /// </summary>
        /// <value>The name of the file.</value>
        public string FileName {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the line number this <see cref="Location"/> refers to.
        /// The first line in a file is 1.
        /// </summary>
        /// <value>The line number.</value>
        public int LineNumber {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the column this <see cref="Location"/> refers to.
        /// The first column in a line is 1.
        /// </summary>
        /// <value>The column number.</value>
        public int Column {
            get;
            private set;
        }

        /// <summary>
        /// Last character which was read.
        /// </summary>
        private int lastChar;

        /// <summary>
        /// Initializes a new instance of the <see cref="Location"/> class.
        /// </summary>
        public Location() :
            this(1, 1) {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Location"/> class.
        /// </summary>
        /// <param name="lineNumber">The line number.</param>
        /// <param name="column">The column.</param>
        public Location(int lineNumber, int column) :
            this(null, lineNumber, column) {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Location"/> class.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="lineNumber">The line number.</param>
        /// <param name="column">The column.</param>
        public Location(string fileName, int lineNumber = 1, int column = 1) {
            FileName = fileName;
            LineNumber = lineNumber;
            Column = column;
        }

        /// <summary>
        /// Advances the current location along a string.
        /// </summary>
        /// <param name="s">The string to advance the location along.</param>
        public void AdvanceString(string s) {
            foreach(char c in s) {
                AdvanceCharacter(c);
            }
        }

        /// <summary>
        /// Advances the current location along one character.
        /// </summary>
        /// <param name="c">The character to advance the location along.</param>
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

        /// <summary>
        /// Advances the current location to the next line.
        /// </summary>
        public void AdvanceLine() {
            ++LineNumber;
            Column = 1;
            this.lastChar = -1;
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString() {
            string output = "";

            if(!string.IsNullOrEmpty(FileName)) {
                output += FileName + ": ";
            }

            output += "line " + this.LineNumber + ", column " + this.Column;

            return output;
        }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns>A new <see cref="Location"/> instance which is equal to this <see cref="Location"/> instance.</returns>
        public Location Clone() {
            return new Location(FileName, LineNumber, Column);
        }
    }
}