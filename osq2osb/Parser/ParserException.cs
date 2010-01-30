using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace osq2osb.Parser {
    public class ParserException : Exception {
        public virtual Location Location {
            get {
                return location;
            }
        }

        private Location location;

        public ParserException() {
        }

        public ParserException(string message) :
            base(message) {
        }

        public ParserException(string message, Exception inner) :
            base(message, inner) {
        }

        public ParserException(string message, Location location) :
            this(message) {
            this.location = location;
        }

        public ParserException(string message, Location location, Exception inner) :
            this(message, inner) {
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
