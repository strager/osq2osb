using NUnit.Framework;
using osq.Parser;
using osq.TreeNode;

namespace osq.Tests {
    [TestFixture]
    public class ExecutionContextTests {
        [Test]
        public void TestGetStringOfInt() {
            var context = new ExecutionContext();

            Assert.AreEqual("1234", context.GetStringOf(1234));
            Assert.AreEqual("532532643", context.GetStringOf(532532643));
        }

        [Test]
        public void TestGetStringOfDouble() {
            var context = new ExecutionContext();

            Assert.AreEqual("1234", context.GetStringOf(1234.0));
            Assert.AreEqual("5325326430000000000000000000000", context.GetStringOf(5325326430000000000000000000000.0));
            Assert.AreEqual("0.000000000532532643", context.GetStringOf(0.000000000532532643));
        }

        [Test]
        public void TestGetStringOfArray() {
            var context = new ExecutionContext();

            Assert.AreEqual("(1,2,3)", context.GetStringOf(new[] { 1, 2, 3 }));
            Assert.AreEqual("(5325326430000000000000000000000)", context.GetStringOf(new[] { 5325326430000000000000000000000.0 }));
        }

        [Test]
        public void TestGetStringOfString() {
            var context = new ExecutionContext();

            Assert.AreEqual("1,2,3", context.GetStringOf("1,2,3"));
            Assert.AreEqual("this is a really lame string", context.GetStringOf("this is a really lame string"));
        }

        [Test]
        public void TestGetStringOfBool() {
            var context = new ExecutionContext();

            Assert.AreEqual("1", context.GetStringOf(true));
            Assert.AreEqual("0", context.GetStringOf(false));
        }

        [Test]
        public void TestGetBoolOfInt() {
            var context = new ExecutionContext();

            Assert.IsTrue(context.GetBoolFrom(42));
            Assert.IsFalse(context.GetBoolFrom(0));
        }

        [Test]
        public void TestGetBoolOfDouble() {
            var context = new ExecutionContext();

            Assert.IsTrue(context.GetBoolFrom(1.234));
            Assert.IsFalse(context.GetBoolFrom(0.0));
            Assert.IsFalse(context.GetBoolFrom(double.NaN));
            Assert.IsTrue(context.GetBoolFrom(double.NegativeInfinity));
        }

        [Test]
        public void TestGetBoolOfArray() {
            var context = new ExecutionContext();

            Assert.IsTrue(context.GetBoolFrom(new object[] { 1, "2", 3 }));
            Assert.IsTrue(context.GetBoolFrom(new object[] { 0 }));
            Assert.IsFalse(context.GetBoolFrom(new object[] { }));
            Assert.IsFalse(context.GetBoolFrom((object[])null));
        }

        [Test]
        public void TestGetBoolOfString() {
            var context = new ExecutionContext();

            Assert.IsTrue(context.GetBoolFrom("test"));
            Assert.IsFalse(context.GetBoolFrom(""));
            Assert.IsFalse(context.GetBoolFrom((string)null));
        }

        [Test]
        public void TestGetBoolOfBool() {
            var context = new ExecutionContext();

            Assert.IsTrue(context.GetBoolFrom(true));
            Assert.IsFalse(context.GetBoolFrom(false));
        }

        [Test]
        public void TestGetBoolOfNull() {
            var context = new ExecutionContext();

            Assert.IsFalse(context.GetBoolFrom((object)null));
        }

        [Test]
        public void TestFormat() {
            var context = new ExecutionContext();

            var formatFunc = (ExecutionContext.OsqFunction)context.GetVariable("format");

            var ret = formatFunc(new TokenNode(null, new[] {
                new TokenNode(new Token(TokenType.String, "this is a {0} {1:D9}")),
                new TokenNode(new Token(TokenType.String, "test")),
                new TokenNode(new Token(TokenType.Number, 10)),
            }), context);

            Assert.AreEqual("this is a test 000000010", ret);
        }
    }
}
