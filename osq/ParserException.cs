using System;

namespace osq {
    public class ParserException : OsqException {
        public override Location Location {
            get {
                return this.location;
            }
        }

        private readonly Location location;

        public ParserException() {
        }

        public ParserException(string message) :
            base(message) {
        }

        public ParserException(string message, Location location) :
            base(message) {
            this.location = location;
        }

        public ParserException(string message, Exception inner) :
            base(message, inner) {
        }

        public ParserException(string message, Location location, Exception inner) :
            base(message, inner) {
            this.location = location;
        }
    }
}
