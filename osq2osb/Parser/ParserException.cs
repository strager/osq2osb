using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace osq2osb.Parser {
    public class ParserException : ApplicationException {
        public virtual Location Location {
            get {
                return location;
            }
        }

        public virtual Parser Parser {
            get {
                return parser;
            }
        }

        private Location location;
        private Parser parser;

        public ParserException() {
        }

        public ParserException(string message) :
            base(message) {
        }

        public ParserException(string message, Exception inner) :
            base(message, inner) {
        }

        public ParserException(string message, Parser parser, Location location) :
            this(message) {
            this.parser = parser;
            this.location = location;
        }

        public ParserException(string message, Parser parser, Location location, Exception inner) :
            this(message, inner) {
            this.parser = parser;
            this.location = location;
        }

        protected ParserException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) :
            base(info, context) {
            throw new NotImplementedException();
        }

        public override string ToString() {
            return GetType().Name + ": " + Message + " in " + Location.ToString() + Environment.NewLine + StackTrace;
        }
    }
}
