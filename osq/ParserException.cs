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
            this.location = location.Clone();
        }

        public ParserException(string message, Exception inner) :
            base(message, inner) {
        }

        public ParserException(string message, Location location, Exception inner) :
            base(message, inner) {
            this.location = location.Clone();
        }
    }

    public class MissingDataException : ParserException {
        public MissingDataException() {
        }

        public MissingDataException(string message) :
            base(message) {
        }

        public MissingDataException(string message, Location location) :
            base(message, location) {
        }

        public MissingDataException(string message, Exception inner) :
            base(message, inner) {
        }

        public MissingDataException(string message, Location location, Exception inner) :
            base(message, location, inner) {
        }
    }

    public class BadDataException : ParserException {
        public BadDataException() {
        }

        public BadDataException(string message) :
            base(message) {
        }

        public BadDataException(string message, Location location) :
            base(message, location) {
        }

        public BadDataException(string message, Exception inner) :
            base(message, inner) {
        }

        public BadDataException(string message, Location location, Exception inner) :
            base(message, location, inner) {
        }
    }
}
