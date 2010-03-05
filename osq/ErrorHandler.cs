using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace osq2osb {
    public class ErrorHandler : IErrorHandler {
        public void Trigger(ErrorType type, Location location, string message) {
            string locstring = location == null ? "" : " at " + location.ToString();

            Console.WriteLine(type.ToString() + locstring + ": " + message);

            fatalErrorOccured |= IsFatalError(type);
        }

        public void Trigger(ErrorType type, Location location, Exception exception) {
            Trigger(type, location, exception.ToString());
        }

        public bool IsFatalError(ErrorType type) {
            return true;
        }

        public bool IsFatalErrorOccured() {
            return fatalErrorOccured;
        }

        private bool fatalErrorOccured = false;
    }
}
