using System;

namespace osq.TreeNode {
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = false)]
    public class DirectiveAttribute : Attribute {
        public string NameExpression {
            get;
            private set;
        }

        public DirectiveAttribute(string nameExpression) {
            NameExpression = nameExpression;
        }
    }
}