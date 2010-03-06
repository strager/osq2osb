using System.Linq;
using NUnit.Framework;

namespace osq.Tests {
    [TestFixture]
    class DefineNodeTests {
        [Test]
        public void TestParameterParsing() {
            string input = @"#define test(a, b, c) abc";

            TreeNode.NodeBase node;

            using(var reader = new LocatedTextReaderWrapper(input)) {
                node = Parser.ReadNode(reader);
            }

            Assert.IsInstanceOf(typeof(TreeNode.DefineNode), node);

            TreeNode.DefineNode define = node as TreeNode.DefineNode;

            Assert.IsTrue(define.FunctionParameters.SequenceEqual(new string[] { "a", "b", "c" }));
        }
    }
}
