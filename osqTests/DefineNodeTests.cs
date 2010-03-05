using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using osq;
using osq.Parser;

using NUnit.Framework;

namespace osq.Tests {
    [TestFixture]
    class DefineNodeTests {
        [Test]
        public void TestParameterParsing() {
            string input = @"#define test(a, b, c) abc";

            Parser.TreeNode.NodeBase node;

            using(var reader = new LocatedTextReaderWrapper(input)) {
                node = Parser.Parser.ReadNode(reader);
            }

            Assert.IsInstanceOf(typeof(Parser.TreeNode.DefineNode), node);

            Parser.TreeNode.DefineNode define = node as Parser.TreeNode.DefineNode;

            Assert.IsTrue(define.FunctionParameters.SequenceEqual(new string[] { "a", "b", "c" }));
        }
    }
}
