using System;
using osq.TreeNode;

namespace osq {
    public class ExecutionException : OsqException {
        public override Location Location {
            get {
                return this.node.Location;
            }
        }

        public NodeBase Node {
            get {
                return this.node;
            }
        }

        private readonly NodeBase node;

        public ExecutionException() {
        }

        public ExecutionException(string message) :
            base(message) {
        }

        public ExecutionException(string message, NodeBase node) :
            base(message) {
            this.node = node;
        }

        public ExecutionException(string message, Exception inner) :
            base(message, inner) {
        }

        public ExecutionException(string message, NodeBase node, Exception inner) :
            base(message, inner) {
            this.node = node;
        }
    }

    public class DataTypeException : ExecutionException {
        public DataTypeException() {
        }

        public DataTypeException(string message) :
            base(message) {
        }

        public DataTypeException(string message, NodeBase node) :
            base(message, node) {
        }

        public DataTypeException(string message, Exception inner) :
            base(message, inner) {
        }

        public DataTypeException(string message, NodeBase node, Exception inner) :
            base(message, node, inner) {
        }
    }

    public class UnknownVariableException : ExecutionException {
        public UnknownVariableException() {
        }

        public UnknownVariableException(string message) :
            base(message) {
        }

        public UnknownVariableException(string message, NodeBase node) :
            base(message, node) {
        }

        public UnknownVariableException(string message, Exception inner) :
            base(message, inner) {
        }

        public UnknownVariableException(string message, NodeBase node, Exception inner) :
            base(message, node, inner) {
        }
    }
}
