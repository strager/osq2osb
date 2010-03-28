using NUnit.Framework;

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
    }
}
