using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace osq2osb {
    public enum ErrorType {
        Message,
        ExecutionWarning,
        ExecutionError,
        ParseError
    };

    public interface IErrorHandler {
        void Trigger(ErrorType type, Location location, string message);
        void Trigger(ErrorType type, Location location, Exception exception);

        bool IsFatalErrorOccured();
    }
}
