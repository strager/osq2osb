using System;

namespace osq {
    public abstract class OsqException : Exception {
        public abstract Location Location {
            get;
        }

        protected OsqException() {
        }

        protected OsqException(string message) :
            base(message) {
        }
            
        protected OsqException(string message, Exception inner) :
            base(message, inner) {
        }
    }
}
